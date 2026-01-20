using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.AvatarFramework.Animation;
using ScheduleOne.Combat;
using ScheduleOne.DevUtilities;
using ScheduleOne.Doors;
using ScheduleOne.Dragging;
using ScheduleOne.Management;
using ScheduleOne.Map;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Skating;
using ScheduleOne.Tools;
using ScheduleOne.Vehicles;
using ScheduleOne.VoiceOver;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace ScheduleOne.NPCs;

public class NPCMovement : NetworkBehaviour
{
	public enum EAgentType
	{
		Humanoid,
		BigHumanoid,
		IgnoreCosts
	}

	public enum EStance
	{
		None,
		Stanced
	}

	public enum WalkResult
	{
		Failed,
		Interrupted,
		Stopped,
		Partial,
		Success
	}

	public const float VEHICLE_RUNOVER_THRESHOLD = 10f;

	public const float SKATEBOARD_RUNOVER_THRESHOLD = 10f;

	public const float LIGHT_FLINCH_THRESHOLD = 50f;

	public const float HEAVY_FLINCH_THRESHOLD = 100f;

	public const float RAGDOLL_THRESHOLD = 150f;

	public const float MOMENTUM_ANNOYED_THRESHOLD = 10f;

	public const float MOMENTUM_LIGHT_FLINCH_THRESHOLD = 20f;

	public const float MOMENTUM_HEAVY_FLINCH_THRESHOLD = 40f;

	public const float MOMENTUM_RAGDOLL_THRESHOLD = 60f;

	public const bool USE_PATH_CACHE = true;

	public const float STUMBLE_DURATION = 0.66f;

	public const float STUMBLE_FORCE = 7f;

	public const float OBSTACLE_AVOIDANCE_RANGE = 25f;

	public const float PLAYER_DIST_IMPACT_THRESHOLD = 30f;

	public static Dictionary<Vector3, Vector3> cachedClosestReachablePoints = new Dictionary<Vector3, Vector3>();

	public static List<Vector3> cachedClosestPointKeys = new List<Vector3>();

	public const float CLOSEST_REACHABLE_POINT_CACHE_MAX_SQR_OFFSET = 1f;

	public bool DEBUG;

	[Header("Settings")]
	public float WalkSpeed = 1.8f;

	public float RunSpeed = 7f;

	public float MoveSpeedMultiplier = 1f;

	[Header("Obstacle Avoidance")]
	public bool ObstacleAvoidanceEnabled = true;

	public ObstacleAvoidanceType DefaultObstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;

	[Header("Slippery Mode")]
	public bool SlipperyMode;

	public float SlipperyModeMultiplier = 1f;

	[Header("References")]
	public NavMeshAgent Agent;

	public NPCSpeedController SpeedController;

	public CapsuleCollider CapsuleCollider;

	public NPCAnimation Animation;

	public SmoothedVelocityCalculator VelocityCalculator;

	public Draggable RagdollDraggable;

	public Collider RagdollDraggableCollider;

	protected NPC npc;

	public float MovementSpeedScale;

	private float ragdollStaticTime;

	public UnityEvent<LandVehicle> onHitByCar;

	public UnityEvent onRagdollStart;

	public UnityEvent onRagdollEnd;

	private bool cacheNextPath;

	private Vector3 currentDestination_Reachable = Vector3.zero;

	private Action<WalkResult> walkResultCallback;

	private float currentMaxDistanceForSuccess = 0.5f;

	private bool forceIsMoving;

	private Coroutine faceDirectionRoutine;

	private List<ConstantForce> ragdollForceComponents = new List<ConstantForce>();

	private float timeUntilNextStumble;

	private float timeSinceStumble = 1000f;

	private Vector3 stumbleDirection = Vector3.zero;

	private CircularQueue<Vector3> desiredVelocityHistory;

	private int desiredVelocityHistoryLength = 40;

	private float velocityHistorySpacing = 0.05f;

	private float timeSinceLastVelocityHistoryRecord;

	private NavMeshPath agentCurrentPath;

	private float agentCurrentSpeed;

	private Vector3[] agentCurrentPathCorners;

	private Coroutine ladderClimbRoutine;

	private bool NetworkInitialize___EarlyScheduleOne.NPCs.NPCMovementAssembly-CSharp.dll_Excuted;

	private bool NetworkInitialize__LateScheduleOne.NPCs.NPCMovementAssembly-CSharp.dll_Excuted;

	public bool HasDestination { get; protected set; }

	public bool IsMoving
	{
		get
		{
			if ((!Agent.hasPath && !Agent.pathPending) || !(Agent.remainingDistance > 0.25f))
			{
				return forceIsMoving;
			}
			return true;
		}
	}

	public bool IsPaused { get; protected set; }

	public Vector3 FootPosition => base.transform.position;

	public float GravityMultiplier { get; protected set; } = 1f;

	public EStance Stance { get; protected set; }

	public float TimeSinceHitByCar { get; protected set; }

	public bool FaceDirectionInProgress => faceDirectionRoutine != null;

	public bool IsOnLadder => CurrentLadder != null;

	public float CurrentLadderSpeed { get; protected set; }

	public bool IsClimbingUpwards => CurrentLadderSpeed > 0.1f;

	public Ladder CurrentLadder { get; protected set; }

	public Vector3 CurrentDestination { get; protected set; } = Vector3.zero;

	public NPCPathCache PathCache { get; private set; } = new NPCPathCache();

	public bool Disoriented { get; set; }

	public virtual void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne.NPCs.NPCMovement_Assembly-CSharp.dll();
		NetworkInitialize__Late();
	}

	private void Start()
	{
		string bakedGUID = npc.BakedGUID;
		if (bakedGUID != string.Empty)
		{
			bakedGUID = ((bakedGUID[bakedGUID.Length - 1] == '1') ? (bakedGUID.Substring(0, bakedGUID.Length - 1) + "2") : (bakedGUID.Substring(0, bakedGUID.Length - 1) + "1"));
			RagdollDraggable.SetGUID(new Guid(bakedGUID));
		}
	}

	public override void OnStartClient()
	{
		base.OnStartClient();
		if (!InstanceFinder.IsServer)
		{
			SetAgentEnabled(enabled: false);
		}
	}

	protected virtual void Update()
	{
		if (DEBUG)
		{
			Debug.Log(npc.fullName + " movement debug: ");
			Debug.Log("HasPath: " + Agent.hasPath);
			Debug.Log("PathPending: " + Agent.pathPending);
			Debug.Log("IsMoving: " + IsMoving);
			Debug.Log("IsPaused: " + IsPaused);
			Debug.Log("IsRagdolled: " + npc.Avatar.Ragdolled);
			Debug.Log("IsInVehicle: " + npc.IsInVehicle);
			Debug.Log("HasDestination: " + HasDestination);
			Debug.Log("CurrentDestination: " + CurrentDestination.ToString());
			Debug.Log("Movement Speed Scale: " + MovementSpeedScale);
		}
		if (!IsOnLadder && Agent.isOnOffMeshLink)
		{
			TraverseLadder(Agent.currentOffMeshLinkData.offMeshLink.GetComponent<Ladder>());
		}
	}

	public void SetAgentEnabled(bool enabled)
	{
		if (DEBUG)
		{
			Console.Log("Setting agent enabled: " + enabled + " for " + npc.fullName);
		}
		Agent.enabled = enabled;
	}

	private void UpdateRagdoll()
	{
		if (npc.IsConscious && ragdollStaticTime > 1f && npc.Avatar.Ragdolled)
		{
			DeactivateRagdoll();
		}
	}

	private void Stumble()
	{
		timeUntilNextStumble = UnityEngine.Random.Range(5f, 15f);
		if (UnityEngine.Random.Range(1f, 0f) < 0.1f)
		{
			ActivateRagdoll_Server();
			return;
		}
		timeSinceStumble = 0f;
		stumbleDirection = UnityEngine.Random.onUnitSphere;
		stumbleDirection.y = 0f;
		stumbleDirection.Normalize();
	}

	private void UpdateDestination()
	{
		if (!HasDestination)
		{
			return;
		}
		if (npc.IsInVehicle)
		{
			EndSetDestination(WalkResult.Interrupted);
		}
		else
		{
			if (IsMoving || Agent.pathPending || !CanMove() || IsOnLadder)
			{
				return;
			}
			if (IsAsCloseAsPossible(CurrentDestination))
			{
				if (Agent.hasPath)
				{
					Agent.ResetPath();
					agentCurrentPath = null;
					agentCurrentPathCorners = null;
				}
				if (Vector3.Distance(CurrentDestination, FootPosition) < currentMaxDistanceForSuccess || Vector3.Distance(CurrentDestination, base.transform.position) < currentMaxDistanceForSuccess)
				{
					EndSetDestination(WalkResult.Success);
				}
				else
				{
					EndSetDestination(WalkResult.Partial);
				}
			}
			else
			{
				SetDestination(CurrentDestination, walkResultCallback, interruptExistingCallback: false, currentMaxDistanceForSuccess);
			}
		}
	}

	protected virtual void FixedUpdate()
	{
		if (!InstanceFinder.IsServer)
		{
			return;
		}
		if (IsPaused)
		{
			Agent.isStopped = true;
		}
		TimeSinceHitByCar += Time.fixedDeltaTime;
		UpdateSpeed();
		UpdateStumble();
		UpdateRagdoll();
		UpdateDestination();
		RecordVelocity();
		UpdateSlippery();
		UpdateCache();
		if (npc.Avatar.Ragdolled && CanRecoverFromRagdoll())
		{
			if (npc.Avatar.MiddleSpineRB.velocity.magnitude < 0.15f)
			{
				ragdollStaticTime += Time.fixedDeltaTime;
			}
			else
			{
				ragdollStaticTime = 0f;
			}
		}
		else
		{
			ragdollStaticTime = 0f;
		}
	}

	private void UpdateStumble()
	{
		if (IsOnLadder)
		{
			return;
		}
		if (Disoriented && IsMoving)
		{
			timeUntilNextStumble -= Time.fixedDeltaTime;
			if (timeUntilNextStumble <= 0f)
			{
				Stumble();
			}
		}
		timeSinceStumble += Time.fixedDeltaTime;
		if (timeSinceStumble < 0.66f)
		{
			Agent.Move(stumbleDirection * (0.66f - timeSinceStumble) * Time.fixedDeltaTime * 7f);
		}
	}

	private void UpdateSpeed()
	{
		float num = 0f;
		if ((double)MovementSpeedScale >= 0.0)
		{
			num = Mathf.Lerp(WalkSpeed, RunSpeed, MovementSpeedScale) * MoveSpeedMultiplier;
		}
		if (!Mathf.Approximately(num, agentCurrentSpeed))
		{
			Agent.speed = num;
			agentCurrentSpeed = num;
		}
	}

	private void RecordVelocity()
	{
		if (timeSinceLastVelocityHistoryRecord > velocityHistorySpacing)
		{
			timeSinceLastVelocityHistoryRecord = 0f;
			desiredVelocityHistory.Enqueue(Agent.velocity);
		}
		else
		{
			timeSinceLastVelocityHistoryRecord += Time.fixedDeltaTime;
		}
	}

	private void UpdateSlippery()
	{
		if (SlipperyMode && Agent.enabled && Agent.isOnNavMesh)
		{
			BurstFunctions.Average(ref desiredVelocityHistory.q, out var result);
			float num = Vector3.Angle(result, base.transform.forward);
			Agent.Move(result * (SlipperyModeMultiplier * Time.fixedDeltaTime * Mathf.Clamp01(num / 90f)));
		}
	}

	private void UpdateCache()
	{
		if (cacheNextPath && agentCurrentPath != null && agentCurrentPathCorners.Length > 1)
		{
			cacheNextPath = false;
			PathCache.AddPath(agentCurrentPathCorners[0], agentCurrentPathCorners[^1], agentCurrentPath);
		}
	}

	public bool CanRecoverFromRagdoll()
	{
		if (npc.Behaviour.RagdollBehaviour.Seizure)
		{
			return false;
		}
		return true;
	}

	private void UpdateAvoidance()
	{
		Player.GetClosestPlayer(base.transform.position, out var distance);
		if (distance > 25f || !ObstacleAvoidanceEnabled)
		{
			Agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
		}
		else
		{
			Agent.obstacleAvoidanceType = DefaultObstacleAvoidanceType;
		}
	}

	public void OnTriggerEnter(Collider other)
	{
		CheckHit(other, CapsuleCollider, isCollision: false, other.transform.position);
	}

	public void OnCollisionEnter(Collision collision)
	{
		CheckHit(collision.collider, collision.contacts[0].thisCollider, isCollision: true, collision.contacts[0].point);
	}

	private void CheckHit(Collider other, Collider thisCollider, bool isCollision, Vector3 hitPoint)
	{
		if (npc.IgnoreImpacts)
		{
			return;
		}
		float distance;
		Player closestPlayer = Player.GetClosestPlayer(base.transform.position, out distance);
		if (distance > 30f)
		{
			return;
		}
		if (DEBUG)
		{
			Debug.Log("NPCMovement.CheckHit: " + other.gameObject.name + " hit by " + thisCollider.gameObject.name);
		}
		LandVehicle landVehicle = null;
		if (other.gameObject.layer == LayerMask.NameToLayer("Vehicle") || other.gameObject.layer == LayerMask.NameToLayer("Ignore Raycast"))
		{
			landVehicle = other.GetComponentInParent<LandVehicle>();
			if (landVehicle == null)
			{
				VehicleHumanoidCollider componentInParent = other.GetComponentInParent<VehicleHumanoidCollider>();
				if (componentInParent != null)
				{
					landVehicle = componentInParent.Vehicle;
				}
			}
		}
		if (landVehicle != null)
		{
			if (!npc.Avatar.Ragdolled && landVehicle != null && npc.CurrentVehicle != landVehicle && Mathf.Abs(landVehicle.Speed_Kmh) > 10f)
			{
				ActivateRagdoll_Server();
				if (onHitByCar != null)
				{
					onHitByCar.Invoke(other.GetComponentInParent<LandVehicle>());
				}
				TimeSinceHitByCar = 0f;
			}
		}
		else if (other.GetComponentInParent<Skateboard>() != null)
		{
			if (!npc.Avatar.Ragdolled && other.GetComponentInParent<Skateboard>().VelocityCalculator.Velocity.magnitude > 2.777778f)
			{
				ActivateRagdoll_Server();
				npc.PlayVO(EVOLineType.Hurt);
			}
		}
		else
		{
			if (!InstanceFinder.IsServer || other.isTrigger || !(other.GetComponentInParent<PhysicsDamageable>() != null))
			{
				return;
			}
			PhysicsDamageable componentInParent2 = other.GetComponentInParent<PhysicsDamageable>();
			float num = Mathf.Sqrt(componentInParent2.Rb.mass) * componentInParent2.Rb.velocity.magnitude;
			float magnitude = componentInParent2.Rb.velocity.magnitude;
			if (magnitude > 40f)
			{
				return;
			}
			if (magnitude > 1f)
			{
				magnitude = Mathf.Pow(magnitude, 1.5f);
			}
			else
			{
				magnitude = Mathf.Sqrt(magnitude);
			}
			if (num > 10f)
			{
				float num2 = 1f;
				switch (Stance)
				{
				case EStance.None:
					num2 = 1f;
					break;
				case EStance.Stanced:
					num2 = 0.5f;
					break;
				}
				float num3 = num * 2.5f;
				float num4 = num * 0.3f;
				if (num > 20f)
				{
					npc.Health.TakeDamage(num4, isLethal: false);
					npc.ProcessImpactForce(hitPoint, componentInParent2.Rb.velocity.normalized, num3 * num2);
				}
				Impact impact = new Impact(hitPoint, componentInParent2.Rb.velocity.normalized, num3, num4, EImpactType.PhysicsProp, (distance < 15f) ? closestPlayer.NetworkObject : null, UnityEngine.Random.Range(int.MinValue, int.MaxValue));
				npc.Responses.ImpactReceived(impact);
			}
		}
	}

	public void Warp(Transform target)
	{
		Warp(target.position);
	}

	public void Warp(Vector3 position)
	{
		if (!InstanceFinder.IsServer)
		{
			return;
		}
		if (!IsNPCPositionValid(position))
		{
			Vector3 vector = position;
			Console.LogWarning("NPCMovement.Warp called with invalid position: " + vector.ToString());
			return;
		}
		if (DEBUG)
		{
			string fullName = npc.fullName;
			Vector3 vector = position;
			Console.Log("Warping " + fullName + " to position: " + vector.ToString());
		}
		if (IsOnLadder)
		{
			CancelTraverseLadder();
		}
		if (Agent.enabled)
		{
			Agent.Warp(position);
		}
		else
		{
			base.transform.position = position;
		}
		ReceiveWarp(position);
	}

	[ObserversRpc(ExcludeServer = true)]
	private void ReceiveWarp(Vector3 position)
	{
		RpcWriter___Observers_ReceiveWarp_4276783012(position);
	}

	public void VisibilityChange(bool visible)
	{
		CapsuleCollider.gameObject.SetActive(visible);
	}

	public bool CanMove()
	{
		if (!npc.Avatar.Ragdolled && !npc.isInBuilding)
		{
			return !npc.IsInVehicle;
		}
		return false;
	}

	public void SetAgentType(EAgentType type)
	{
		string text = type.ToString();
		if (type == EAgentType.BigHumanoid)
		{
			text = "Big Humanoid";
		}
		if (type == EAgentType.IgnoreCosts)
		{
			text = "Ignore Costs";
		}
		Agent.agentTypeID = NavMeshUtility.GetNavMeshAgentID(text);
	}

	public void SetSeat(AvatarSeat seat)
	{
		npc.Avatar.Animation.SetSeat(seat);
		SetAgentEnabled(seat == null && InstanceFinder.IsServer);
	}

	public void SetStance(EStance stance)
	{
		Stance = stance;
	}

	public void SetGravityMultiplier(float multiplier)
	{
		GravityMultiplier = multiplier;
		foreach (ConstantForce ragdollForceComponent in ragdollForceComponents)
		{
			ragdollForceComponent.force = Physics.gravity * GravityMultiplier * ragdollForceComponent.GetComponent<Rigidbody>().mass;
		}
	}

	public void SetRagdollDraggable(bool draggable)
	{
		if (RagdollDraggable != null)
		{
			RagdollDraggable.enabled = draggable;
		}
		if (RagdollDraggableCollider != null)
		{
			RagdollDraggableCollider.enabled = draggable;
		}
	}

	[ServerRpc(RunLocally = true, RequireOwnership = false)]
	public void ActivateRagdoll_Server()
	{
		RpcWriter___Server_ActivateRagdoll_Server_2166136261();
		RpcLogic___ActivateRagdoll_Server_2166136261();
	}

	[ObserversRpc(RunLocally = true)]
	public void ActivateRagdoll(Vector3 forcePoint, Vector3 forceDir, float forceMagnitude)
	{
		RpcWriter___Observers_ActivateRagdoll_2690242654(forcePoint, forceDir, forceMagnitude);
		RpcLogic___ActivateRagdoll_2690242654(forcePoint, forceDir, forceMagnitude);
	}

	[ObserversRpc(RunLocally = true)]
	public void ApplyRagdollForce(Vector3 forcePoint, Vector3 forceDir, float forceMagnitude)
	{
		RpcWriter___Observers_ApplyRagdollForce_2690242654(forcePoint, forceDir, forceMagnitude);
		RpcLogic___ApplyRagdollForce_2690242654(forcePoint, forceDir, forceMagnitude);
	}

	[ObserversRpc(RunLocally = true)]
	public void DeactivateRagdoll()
	{
		RpcWriter___Observers_DeactivateRagdoll_2166136261();
		RpcLogic___DeactivateRagdoll_2166136261();
	}

	private bool SmartSampleNavMesh(Vector3 position, out NavMeshHit hit, float minRadius = 1f, float maxRadius = 10f, int steps = 3)
	{
		hit = default(NavMeshHit);
		NavMeshQueryFilter filter = new NavMeshQueryFilter
		{
			agentTypeID = NavMeshUtility.GetNavMeshAgentID("Humanoid"),
			areaMask = -1
		};
		for (int i = 0; i < steps; i++)
		{
			float maxDistance = Mathf.Lerp(minRadius, maxRadius, i / steps);
			if (NavMesh.SamplePosition(base.transform.position, out hit, maxDistance, filter))
			{
				return true;
			}
		}
		return false;
	}

	public void SetDestination(Transform target)
	{
		SetDestination(target.position);
	}

	public void SetDestination(Vector3 pos)
	{
		SetDestination(pos, null, 1f, 1f);
	}

	public void SetDestination(ITransitEntity entity)
	{
		SetDestination(NavMeshUtility.GetReachableAccessPoint(entity, npc).position);
	}

	public void SetDestination(Vector3 pos, Action<WalkResult> callback = null, float maximumDistanceForSuccess = 1f, float cacheMaxDistSqr = 1f)
	{
		SetDestination(pos, callback, interruptExistingCallback: true, maximumDistanceForSuccess, cacheMaxDistSqr);
	}

	private void SetDestination(Vector3 pos, Action<WalkResult> callback = null, bool interruptExistingCallback = true, float successThreshold = 1f, float cacheMaxDistSqr = 1f)
	{
		if (!IsNPCPositionValid(pos))
		{
			Vector3 vector = pos;
			Console.LogWarning("NPCMovement.SetDestination called with invalid position: " + vector.ToString());
			return;
		}
		if (npc.Avatar.Animation.IsSeated)
		{
			npc.Movement.SetSeat(null);
		}
		if (!InstanceFinder.IsServer)
		{
			Console.LogWarning("NPCMovement.SetDestination called on client");
			return;
		}
		if (npc.isInBuilding)
		{
			npc.ExitBuilding();
		}
		if (DEBUG)
		{
			string fullName = npc.fullName;
			Vector3 vector = pos;
			Console.Log(fullName + " SetDestination called: " + vector.ToString());
			Debug.DrawLine(FootPosition, pos, Color.green, 1f);
		}
		if (!CanMove())
		{
			Console.LogWarning("NPCMovement.SetDestination called but CanWalk == false (" + npc.fullName + ")");
			return;
		}
		if (!Agent.isOnNavMesh && !IsOnLadder)
		{
			Console.LogWarning("NPC is not on navmesh; warping to navmesh");
			if (!SmartSampleNavMesh(base.transform.position, out var hit))
			{
				Console.LogWarning("NavMesh sample failed at " + base.transform.position.ToString());
				return;
			}
			Warp(hit.position);
			SetAgentEnabled(enabled: false);
			SetAgentEnabled(enabled: true);
		}
		if (walkResultCallback != null && interruptExistingCallback)
		{
			EndSetDestination(WalkResult.Interrupted);
		}
		walkResultCallback = callback;
		currentMaxDistanceForSuccess = successThreshold;
		if (npc.IsInVehicle)
		{
			Console.LogWarning("SetDestination called but NPC is in a vehicle; returning WalkResult.Failed");
			EndSetDestination(WalkResult.Failed);
			return;
		}
		if (!GetClosestReachablePoint(pos, out var closestPoint))
		{
			if (DEBUG)
			{
				string fullName2 = npc.fullName;
				Vector3 vector = pos;
				Console.LogWarning(fullName2 + " failed to find closest reachable point for destination: " + vector.ToString());
				Debug.DrawLine(FootPosition, pos, Color.red, 1f);
			}
			EndSetDestination(WalkResult.Failed);
			return;
		}
		if (!IsNPCPositionValid(closestPoint))
		{
			string fullName3 = npc.fullName;
			Vector3 vector = pos;
			Console.LogWarning(fullName3 + " failed to find valid reachable point for destination: " + vector.ToString());
			EndSetDestination(WalkResult.Failed);
			return;
		}
		HasDestination = true;
		CurrentDestination = pos;
		currentDestination_Reachable = closestPoint;
		if (IsOnLadder)
		{
			return;
		}
		NavMeshPath path = PathCache.GetPath(Agent.transform.position, closestPoint, cacheMaxDistSqr);
		bool flag = false;
		if (path != null)
		{
			try
			{
				flag = path == agentCurrentPath || Agent.SetPath(path);
			}
			catch (Exception ex)
			{
				Console.LogWarning("Agent.SetDestination error: " + ex.Message);
				flag = false;
			}
		}
		if (!flag)
		{
			if (DEBUG)
			{
				Console.Log("No cached path for " + npc.fullName + "; calculating new path");
			}
			try
			{
				Agent.SetDestination(closestPoint);
				cacheNextPath = true;
			}
			catch (Exception ex2)
			{
				Console.LogWarning("Agent.SetDestination error: " + ex2.Message);
			}
		}
		agentCurrentPath = Agent.path;
		agentCurrentPathCorners = agentCurrentPath.corners;
		if (IsPaused)
		{
			Agent.isStopped = true;
		}
	}

	private bool IsNPCPositionValid(Vector3 position)
	{
		if (float.IsNaN(position.x) || float.IsNaN(position.y) || float.IsNaN(position.z))
		{
			return false;
		}
		if (float.IsInfinity(position.x) || float.IsInfinity(position.y) || float.IsInfinity(position.z))
		{
			return false;
		}
		if (position.magnitude > 10000f)
		{
			return false;
		}
		return true;
	}

	private void EndSetDestination(WalkResult result)
	{
		if (DEBUG)
		{
			Console.Log(npc.fullName + " EndSetDestination called: " + result);
		}
		if (walkResultCallback != null)
		{
			walkResultCallback(result);
			walkResultCallback = null;
		}
		HasDestination = false;
		CurrentDestination = Vector3.zero;
		currentDestination_Reachable = Vector3.zero;
	}

	public void Stop()
	{
		if (Agent.isOnNavMesh)
		{
			Agent.ResetPath();
			Agent.velocity = Vector3.zero;
			Agent.isStopped = true;
			Agent.isStopped = false;
			agentCurrentPath = null;
			agentCurrentPathCorners = null;
		}
		if (InstanceFinder.IsServer)
		{
			EndSetDestination(WalkResult.Stopped);
		}
	}

	public void WarpToNavMesh()
	{
	}

	public void FacePoint(Vector3 point, float lerpTime = 0.5f)
	{
		Vector3 forward = new Vector3(point.x, base.transform.position.y, point.z) - base.transform.position;
		if (faceDirectionRoutine != null)
		{
			StopCoroutine(faceDirectionRoutine);
		}
		if (DEBUG)
		{
			Vector3 vector = point;
			Debug.Log("Facing point: " + vector.ToString());
		}
		faceDirectionRoutine = StartCoroutine(FaceDirection_Process(forward, lerpTime));
	}

	public void FaceDirection(Vector3 forward, float lerpTime = 0.5f)
	{
		if (faceDirectionRoutine != null)
		{
			Singleton<CoroutineService>.Instance.StopCoroutine(faceDirectionRoutine);
		}
		if (DEBUG)
		{
			Vector3 vector = forward;
			Debug.Log("Facing dir: " + vector.ToString());
		}
		faceDirectionRoutine = Singleton<CoroutineService>.Instance.StartCoroutine(FaceDirection_Process(forward, lerpTime));
	}

	protected IEnumerator FaceDirection_Process(Vector3 forward, float lerpTime)
	{
		if (lerpTime > 0f)
		{
			Quaternion startRot = base.transform.rotation;
			for (float i = 0f; i < lerpTime; i += Time.deltaTime)
			{
				if (!IsOnLadder)
				{
					base.transform.rotation = Quaternion.Lerp(startRot, Quaternion.LookRotation(forward, Vector3.up), i / lerpTime);
				}
				yield return new WaitForEndOfFrame();
			}
		}
		base.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
		faceDirectionRoutine = null;
	}

	public void PauseMovement()
	{
		IsPaused = true;
		Agent.isStopped = true;
		Agent.velocity = Vector3.zero;
	}

	public void ResumeMovement()
	{
		IsPaused = false;
		if (Agent.isOnNavMesh)
		{
			Agent.isStopped = false;
		}
	}

	public bool IsAsCloseAsPossible(Vector3 location, float distanceThreshold = 0.5f)
	{
		if (Vector3.Distance(FootPosition, location) < distanceThreshold)
		{
			return true;
		}
		Vector3 closestPoint = Vector3.zero;
		if (!GetClosestReachablePoint(location, out closestPoint))
		{
			return false;
		}
		return Vector3.Distance(FootPosition, closestPoint) < distanceThreshold;
	}

	public bool GetClosestReachablePoint(Vector3 targetPosition, out Vector3 closestPoint)
	{
		closestPoint = Vector3.zero;
		bool flag = false;
		Vector3 vector = Vector3.zero;
		for (int i = 0; i < cachedClosestPointKeys.Count; i++)
		{
			if (Vector3.SqrMagnitude(cachedClosestPointKeys[i] - targetPosition) < 1f)
			{
				vector = cachedClosestReachablePoints[cachedClosestPointKeys[i]];
				flag = true;
				break;
			}
		}
		if (flag)
		{
			closestPoint = vector;
			return true;
		}
		if (!Agent.isOnNavMesh && !IsOnLadder)
		{
			return false;
		}
		NavMeshQueryFilter filter = new NavMeshQueryFilter
		{
			agentTypeID = Agent.agentTypeID,
			areaMask = Agent.areaMask
		};
		NavMeshPath navMeshPath = new NavMeshPath();
		float num = 3f;
		for (int j = 0; j < 3; j++)
		{
			if (NavMeshUtility.SamplePosition(targetPosition, out var hit, num * (float)(j + 1), -1))
			{
				if (DEBUG)
				{
					Console.Log("Hit!");
				}
				Vector3 sourcePosition = Agent.transform.position;
				if (IsOnLadder)
				{
					sourcePosition = ((!IsClimbingUpwards) ? CurrentLadder.OffMeshLink.startTransform.position : CurrentLadder.OffMeshLink.endTransform.position);
				}
				NavMesh.CalculatePath(sourcePosition, hit.position, filter, navMeshPath);
				if (navMeshPath != null && navMeshPath.corners.Length != 0)
				{
					Vector3 vector2 = navMeshPath.corners[navMeshPath.corners.Length - 1];
					closestPoint = vector2;
					return true;
				}
			}
		}
		return false;
	}

	public bool CanGetTo(Vector3 position, float proximityReq = 1f)
	{
		NavMeshPath path;
		return CanGetTo(position, proximityReq, out path);
	}

	public bool CanGetTo(ITransitEntity entity, float proximityReq = 1f)
	{
		if (entity == null)
		{
			return false;
		}
		Transform[] accessPoints = entity.AccessPoints;
		foreach (Transform transform in accessPoints)
		{
			if (!(transform == null) && CanGetTo(transform.position, proximityReq))
			{
				return true;
			}
		}
		return false;
	}

	public bool CanGetTo(Vector3 position, float proximityReq, out NavMeshPath path)
	{
		path = null;
		if (Vector3.Distance(position, base.transform.position) <= proximityReq)
		{
			return true;
		}
		if (!Agent.isOnNavMesh)
		{
			return false;
		}
		if (!NavMeshUtility.SamplePosition(position, out var hit, 2f, -1))
		{
			return false;
		}
		path = GetPathTo(hit.position, proximityReq);
		if (path == null)
		{
			Debug.DrawLine(FootPosition, hit.position, Color.red, 1f);
			return false;
		}
		if (path.corners.Length < 2)
		{
			Console.LogWarning("Path length < 2");
			return false;
		}
		float num = Vector3.Distance(path.corners[path.corners.Length - 1], hit.position);
		float num2 = Vector3.Distance(hit.position, position);
		if (num <= proximityReq)
		{
			return num2 <= proximityReq;
		}
		return false;
	}

	private NavMeshPath GetPathTo(Vector3 position, float proximityReq = 1f)
	{
		if (!Agent.isOnNavMesh)
		{
			Console.LogWarning("Agent not on nav mesh!");
			return null;
		}
		NavMeshPath navMeshPath = new NavMeshPath();
		NavMeshUtility.SamplePosition(position, out var hit, 2f, -1);
		if (!Agent.CalculatePath(hit.position, navMeshPath))
		{
			return null;
		}
		float num = Vector3.Distance(navMeshPath.corners[navMeshPath.corners.Length - 1], hit.position);
		float num2 = Vector3.Distance(hit.position, position);
		if (num <= proximityReq && num2 <= proximityReq)
		{
			return navMeshPath;
		}
		return null;
	}

	public void TraverseLadder(Ladder ladder)
	{
		CurrentLadder = ladder;
		SetAgentEnabled(enabled: false);
		if (ladderClimbRoutine != null)
		{
			StopCoroutine(ladderClimbRoutine);
			ladderClimbRoutine = null;
		}
		ladderClimbRoutine = StartCoroutine(Routine());
		IEnumerator OverrideLookDirection()
		{
			while (CurrentLadder != null)
			{
				npc.Avatar.LookController.BlockLookTargetOverrides();
				yield return new WaitForEndOfFrame();
			}
		}
		IEnumerator Routine()
		{
			bool startFromTop = false;
			if (Vector3.Distance(FootPosition, ladder.TopCenter) < Vector3.Distance(FootPosition, ladder.BottomCenter))
			{
				startFromTop = true;
			}
			Vector3 startLadderPos = (startFromTop ? (ladder.TopCenter - ladder.LadderTransform.up * 0.25f) : ladder.BottomCenter) - ladder.LadderTransform.forward * 0.42f;
			Vector3 endLadderPos = (startFromTop ? ladder.BottomCenter : (ladder.TopCenter - ladder.LadderTransform.up * 1f)) - ladder.LadderTransform.forward * 0.42f;
			Quaternion ladderRot = ladder.LadderTransform.rotation;
			Vector3 startNPCPos = base.transform.position;
			Quaternion startNPCRot = base.transform.rotation;
			Vector3 endNPCPos = (startFromTop ? CurrentLadder.OffMeshLink.startTransform.position : CurrentLadder.OffMeshLink.endTransform.position);
			Quaternion endPlayerRot = (startFromTop ? CurrentLadder.OffMeshLink.startTransform.rotation : CurrentLadder.OffMeshLink.endTransform.rotation);
			float mountLerpTime = Mathf.Max(Vector3.Distance(startNPCPos, startLadderPos) * 0.4f, startFromTop ? 0.7f : 0.3f);
			float climbLerpTime = Vector3.Distance(startLadderPos, endLadderPos) * 0.75f;
			float dismountLerpTime = Mathf.Max(Vector3.Distance(endLadderPos, endNPCPos) * 0.4f, startFromTop ? 0.4f : 0.3f);
			if (!startFromTop)
			{
				CurrentLadderSpeed = 1f;
			}
			StartCoroutine(OverrideLookDirection());
			if (startFromTop && ladder.LinkedManholeCover != null && InstanceFinder.IsServer && !ladder.LinkedManholeCover.IsOpen)
			{
				ladder.LinkedManholeCover.SetIsOpen_Server(open: true, EDoorSide.Exterior, openedForPlayer: false);
			}
			for (float t = 0f; t < mountLerpTime; t += Time.deltaTime)
			{
				Vector3 position = Vector3.Lerp(startNPCPos, startLadderPos, t / mountLerpTime);
				Quaternion rotation = Quaternion.Slerp(startNPCRot, ladderRot, t / mountLerpTime);
				base.transform.position = position;
				base.transform.rotation = rotation;
				yield return new WaitForEndOfFrame();
			}
			if (startFromTop)
			{
				CurrentLadderSpeed = -1f;
			}
			float timeUntilClimbSoundPlays = 0.3f;
			for (float t = 0f; t < climbLerpTime; t += Time.deltaTime)
			{
				Vector3 position2 = Vector3.Lerp(startLadderPos, endLadderPos, t / climbLerpTime);
				base.transform.position = position2;
				timeUntilClimbSoundPlays -= Time.deltaTime;
				if (timeUntilClimbSoundPlays <= 0f)
				{
					ladder.PlayClimbSound(npc.CenterPoint);
					timeUntilClimbSoundPlays = 0.3f;
				}
				if (!startFromTop && ladder.LinkedManholeCover != null && InstanceFinder.IsServer)
				{
					float num = Vector3.Distance(npc.CenterPoint, ladder.TopCenter);
					if (!ladder.LinkedManholeCover.IsOpen && num < 1.1f)
					{
						ladder.LinkedManholeCover.SetIsOpen_Server(open: true, EDoorSide.Interior, openedForPlayer: false);
					}
				}
				yield return new WaitForEndOfFrame();
			}
			CurrentLadderSpeed = 0f;
			for (float t = 0f; t < dismountLerpTime; t += Time.deltaTime)
			{
				Vector3 position3 = Vector3.Lerp(endLadderPos, endNPCPos, t / dismountLerpTime);
				Quaternion rotation2 = Quaternion.Slerp(ladderRot, endPlayerRot, t / dismountLerpTime);
				base.transform.position = position3;
				base.transform.rotation = rotation2;
				yield return new WaitForEndOfFrame();
			}
			SetAgentEnabled(enabled: true);
			CurrentLadder = null;
			Agent.CompleteOffMeshLink();
		}
	}

	private void CancelTraverseLadder()
	{
		if (ladderClimbRoutine != null)
		{
			StopCoroutine(ladderClimbRoutine);
			ladderClimbRoutine = null;
		}
		CurrentLadderSpeed = 0f;
		CurrentLadder = null;
	}

	public virtual void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyScheduleOne.NPCs.NPCMovementAssembly-CSharp.dll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne.NPCs.NPCMovementAssembly-CSharp.dll_Excuted = true;
			RegisterObserversRpc(0u, RpcReader___Observers_ReceiveWarp_4276783012);
			RegisterServerRpc(1u, RpcReader___Server_ActivateRagdoll_Server_2166136261);
			RegisterObserversRpc(2u, RpcReader___Observers_ActivateRagdoll_2690242654);
			RegisterObserversRpc(3u, RpcReader___Observers_ApplyRagdollForce_2690242654);
			RegisterObserversRpc(4u, RpcReader___Observers_DeactivateRagdoll_2166136261);
		}
	}

	public virtual void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne.NPCs.NPCMovementAssembly-CSharp.dll_Excuted)
		{
			NetworkInitialize__LateScheduleOne.NPCs.NPCMovementAssembly-CSharp.dll_Excuted = true;
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Observers_ReceiveWarp_4276783012(Vector3 position)
	{
		if (!base.IsServerInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			if ((object)networkManager == null)
			{
				networkManager = InstanceFinder.NetworkManager;
			}
			if ((object)networkManager != null)
			{
				networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			}
			else
			{
				Debug.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			}
		}
		else
		{
			Channel channel = Channel.Reliable;
			PooledWriter writer = WriterPool.GetWriter();
			writer.WriteVector3(position);
			SendObserversRpc(0u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: true, excludeOwner: false);
			writer.Store();
		}
	}

	private void RpcLogic___ReceiveWarp_4276783012(Vector3 position)
	{
		if (!IsNPCPositionValid(position))
		{
			Vector3 vector = position;
			Console.LogWarning("NPCMovement.Warp called with invalid position: " + vector.ToString());
			return;
		}
		if (DEBUG)
		{
			string fullName = npc.fullName;
			Vector3 vector = position;
			Console.Log("Warping " + fullName + " to position: " + vector.ToString());
		}
		if (Agent.enabled)
		{
			Agent.Warp(position);
		}
		else
		{
			base.transform.position = position;
		}
	}

	private void RpcReader___Observers_ReceiveWarp_4276783012(PooledReader PooledReader0, Channel channel)
	{
		Vector3 position = PooledReader0.ReadVector3();
		if (base.IsClientInitialized)
		{
			RpcLogic___ReceiveWarp_4276783012(position);
		}
	}

	private void RpcWriter___Server_ActivateRagdoll_Server_2166136261()
	{
		if (!base.IsClientInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			if ((object)networkManager == null)
			{
				networkManager = InstanceFinder.NetworkManager;
			}
			if ((object)networkManager != null)
			{
				networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			}
			else
			{
				Debug.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			}
		}
		else
		{
			Channel channel = Channel.Reliable;
			PooledWriter writer = WriterPool.GetWriter();
			SendServerRpc(1u, writer, channel, DataOrderType.Default);
			writer.Store();
		}
	}

	public void RpcLogic___ActivateRagdoll_Server_2166136261()
	{
		ActivateRagdoll(Vector3.zero, Vector3.zero, 0f);
	}

	private void RpcReader___Server_ActivateRagdoll_Server_2166136261(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		if (base.IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___ActivateRagdoll_Server_2166136261();
		}
	}

	private void RpcWriter___Observers_ActivateRagdoll_2690242654(Vector3 forcePoint, Vector3 forceDir, float forceMagnitude)
	{
		if (!base.IsServerInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			if ((object)networkManager == null)
			{
				networkManager = InstanceFinder.NetworkManager;
			}
			if ((object)networkManager != null)
			{
				networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			}
			else
			{
				Debug.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			}
		}
		else
		{
			Channel channel = Channel.Reliable;
			PooledWriter writer = WriterPool.GetWriter();
			writer.WriteVector3(forcePoint);
			writer.WriteVector3(forceDir);
			writer.WriteSingle(forceMagnitude);
			SendObserversRpc(2u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
			writer.Store();
		}
	}

	public void RpcLogic___ActivateRagdoll_2690242654(Vector3 forcePoint, Vector3 forceDir, float forceMagnitude)
	{
		if (DEBUG)
		{
			string[] obj = new string[8] { "Activating ragdoll for ", npc.fullName, " with forcePoint: ", null, null, null, null, null };
			Vector3 vector = forcePoint;
			obj[3] = vector.ToString();
			obj[4] = ", forceDir: ";
			vector = forceDir;
			obj[5] = vector.ToString();
			obj[6] = ", forceMagnitude: ";
			obj[7] = forceMagnitude.ToString();
			Console.Log(string.Concat(obj));
		}
		if (IsOnLadder)
		{
			CancelTraverseLadder();
		}
		Animation.SetRagdollActive(active: true);
		if (onRagdollStart != null)
		{
			onRagdollStart.Invoke();
		}
		if (InstanceFinder.IsServer)
		{
			EndSetDestination(WalkResult.Interrupted);
			SetAgentEnabled(enabled: false);
		}
		CapsuleCollider.gameObject.SetActive(value: false);
		if (forceMagnitude > 0f)
		{
			ApplyRagdollForce(forcePoint, forceDir, forceMagnitude);
		}
	}

	private void RpcReader___Observers_ActivateRagdoll_2690242654(PooledReader PooledReader0, Channel channel)
	{
		Vector3 forcePoint = PooledReader0.ReadVector3();
		Vector3 forceDir = PooledReader0.ReadVector3();
		float forceMagnitude = PooledReader0.ReadSingle();
		if (base.IsClientInitialized && !base.IsHost)
		{
			RpcLogic___ActivateRagdoll_2690242654(forcePoint, forceDir, forceMagnitude);
		}
	}

	private void RpcWriter___Observers_ApplyRagdollForce_2690242654(Vector3 forcePoint, Vector3 forceDir, float forceMagnitude)
	{
		if (!base.IsServerInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			if ((object)networkManager == null)
			{
				networkManager = InstanceFinder.NetworkManager;
			}
			if ((object)networkManager != null)
			{
				networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			}
			else
			{
				Debug.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			}
		}
		else
		{
			Channel channel = Channel.Reliable;
			PooledWriter writer = WriterPool.GetWriter();
			writer.WriteVector3(forcePoint);
			writer.WriteVector3(forceDir);
			writer.WriteSingle(forceMagnitude);
			SendObserversRpc(3u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
			writer.Store();
		}
	}

	public void RpcLogic___ApplyRagdollForce_2690242654(Vector3 forcePoint, Vector3 forceDir, float forceMagnitude)
	{
		(from x in npc.Avatar.RagdollRBs
			select new
			{
				rb = x,
				dist = Vector3.Distance(x.transform.position, forcePoint)
			} into x
			orderby x.dist
			select x).First().rb.AddForceAtPosition(forceDir.normalized * forceMagnitude, forcePoint, ForceMode.Impulse);
	}

	private void RpcReader___Observers_ApplyRagdollForce_2690242654(PooledReader PooledReader0, Channel channel)
	{
		Vector3 forcePoint = PooledReader0.ReadVector3();
		Vector3 forceDir = PooledReader0.ReadVector3();
		float forceMagnitude = PooledReader0.ReadSingle();
		if (base.IsClientInitialized && !base.IsHost)
		{
			RpcLogic___ApplyRagdollForce_2690242654(forcePoint, forceDir, forceMagnitude);
		}
	}

	private void RpcWriter___Observers_DeactivateRagdoll_2166136261()
	{
		if (!base.IsServerInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			if ((object)networkManager == null)
			{
				networkManager = InstanceFinder.NetworkManager;
			}
			if ((object)networkManager != null)
			{
				networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			}
			else
			{
				Debug.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			}
		}
		else
		{
			Channel channel = Channel.Reliable;
			PooledWriter writer = WriterPool.GetWriter();
			SendObserversRpc(4u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
			writer.Store();
		}
	}

	public void RpcLogic___DeactivateRagdoll_2166136261()
	{
		CapsuleCollider.gameObject.SetActive(npc.isVisible);
		Animation.SetRagdollActive(active: false);
		base.transform.position = npc.Avatar.transform.position;
		base.transform.rotation = npc.Avatar.transform.rotation;
		npc.Avatar.transform.localPosition = Vector3.zero;
		npc.Avatar.transform.localRotation = Quaternion.identity;
		VelocityCalculator.FlushBuffer();
		if (InstanceFinder.IsServer)
		{
			SetAgentEnabled(enabled: false);
			if (!Agent.isOnNavMesh)
			{
				NavMeshQueryFilter navMeshQueryFilter = new NavMeshQueryFilter
				{
					agentTypeID = NavMeshUtility.GetNavMeshAgentID("Humanoid"),
					areaMask = -1
				};
				if (SmartSampleNavMesh(base.transform.position, out var hit))
				{
					Warp(hit.position);
				}
				SetAgentEnabled(enabled: false);
				SetAgentEnabled(enabled: true);
			}
		}
		if (onRagdollEnd != null)
		{
			onRagdollEnd.Invoke();
		}
	}

	private void RpcReader___Observers_DeactivateRagdoll_2166136261(PooledReader PooledReader0, Channel channel)
	{
		if (base.IsClientInitialized && !base.IsHost)
		{
			RpcLogic___DeactivateRagdoll_2166136261();
		}
	}

	protected virtual void Awake_UserLogic_ScheduleOne.NPCs.NPCMovement_Assembly-CSharp.dll()
	{
		npc = GetComponent<NPC>();
		NPC nPC = npc;
		nPC.onVisibilityChanged = (Action<bool>)Delegate.Combine(nPC.onVisibilityChanged, new Action<bool>(VisibilityChange));
		VisibilityChange(npc.isVisible);
		InvokeRepeating("UpdateAvoidance", 0f, 0.5f);
		for (int i = 0; i < npc.Avatar.RagdollRBs.Length; i++)
		{
			ragdollForceComponents.Add(npc.Avatar.RagdollRBs[i].gameObject.AddComponent<ConstantForce>());
		}
		desiredVelocityHistory = new CircularQueue<Vector3>(desiredVelocityHistoryLength);
		SetRagdollDraggable(draggable: false);
		SetGravityMultiplier(1f);
	}
}
