using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EPOOutline;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Serializing;
using FishNet.Serializing.Generated;
using FishNet.Transporting;
using FluffyUnderware.DevTools.Extensions;
using ScheduleOne.AvatarFramework;
using ScheduleOne.AvatarFramework.Animation;
using ScheduleOne.AvatarFramework.Equipping;
using ScheduleOne.Combat;
using ScheduleOne.DevUtilities;
using ScheduleOne.Dialogue;
using ScheduleOne.Doors;
using ScheduleOne.Economy;
using ScheduleOne.GameTime;
using ScheduleOne.Interaction;
using ScheduleOne.ItemFramework;
using ScheduleOne.Map;
using ScheduleOne.Messaging;
using ScheduleOne.NPCs.Actions;
using ScheduleOne.NPCs.Behaviour;
using ScheduleOne.NPCs.Relation;
using ScheduleOne.NPCs.Responses;
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Persistence.Loaders;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Variables;
using ScheduleOne.Vehicles;
using ScheduleOne.Vehicles.AI;
using ScheduleOne.Vision;
using ScheduleOne.VoiceOver;
using UnityEngine;
using UnityEngine.AI;

namespace ScheduleOne.NPCs;

[RequireComponent(typeof(NPCHealth))]
public class NPC : NetworkBehaviour, IGUIDRegisterable, ISaveable, ICombatTargetable, IDamageable, ISightable
{
	public const float PANIC_DURATION = 20f;

	public const bool RequiresRegionUnlocked = true;

	[Header("Info Settings")]
	public string FirstName = string.Empty;

	public bool hasLastName = true;

	public string LastName = string.Empty;

	public string ID = string.Empty;

	public Sprite MugshotSprite;

	public EMapRegion Region = EMapRegion.Downtown;

	[Header("If true, NPC will respawn next day instead of waiting 3 days.")]
	public bool IsImportant;

	[Header("Personality")]
	[Range(0f, 1f)]
	public float Aggression;

	[Header("References")]
	[SerializeField]
	protected Transform modelContainer;

	[SerializeField]
	protected InteractableObject intObj;

	public NPCMovement Movement;

	public DialogueHandler DialogueHandler;

	public ScheduleOne.AvatarFramework.Avatar Avatar;

	public NPCAwareness Awareness;

	public NPCResponses Responses;

	public NPCActions Actions;

	public NPCBehaviour Behaviour;

	public NPCInventory Inventory;

	public VOEmitter VoiceOverEmitter;

	public NPCHealth Health;

	public EntityVisibility Visibility;

	public Action<LandVehicle> onEnterVehicle;

	public Action<LandVehicle> onExitVehicle;

	[Header("Summoning")]
	public bool CanBeSummoned = true;

	[Header("Relationship")]
	public NPCRelationData RelationData;

	public string NPCUnlockedVariable = string.Empty;

	public bool ShowRelationshipInfo = true;

	[Header("Messaging")]
	public List<EConversationCategory> ConversationCategories;

	public bool MessagingKnownByDefault = true;

	public bool ConversationCanBeHidden = true;

	public Action onConversationCreated;

	[Header("Other Settings")]
	public bool CanOpenDoors = true;

	public bool OverrideParent;

	public Transform OverriddenParent;

	public bool IgnoreImpacts;

	[SerializeField]
	protected List<GameObject> OutlineRenderers = new List<GameObject>();

	protected Outlinable OutlineEffect;

	[Header("GUID")]
	public string BakedGUID = string.Empty;

	public Action<bool> onVisibilityChanged;

	[HideInInspector]
	[SyncVar]
	public NetworkObject PlayerConversant;

	private Coroutine resetUnsettledCoroutine;

	private List<int> impactHistory = new List<int>();

	private int headlightStartTime = 1700;

	private int heaedLightsEndTime = 600;

	protected float defaultAggression;

	private Coroutine lerpScaleRoutine;

	public SyncVar<NetworkObject> syncVar___PlayerConversant;

	private bool NetworkInitialize___EarlyScheduleOne.NPCs.NPCAssembly-CSharp.dll_Excuted;

	private bool NetworkInitialize__LateScheduleOne.NPCs.NPCAssembly-CSharp.dll_Excuted;

	public string fullName
	{
		get
		{
			if (hasLastName)
			{
				return FirstName + " " + LastName;
			}
			return FirstName;
		}
	}

	public float Scale { get; private set; } = 1f;

	public bool IsConscious
	{
		get
		{
			if (Health.Health > 0f)
			{
				if (!Behaviour.UnconsciousBehaviour.Active)
				{
					return !Behaviour.DeadBehaviour.Active;
				}
				return false;
			}
			return false;
		}
	}

	public LandVehicle CurrentVehicle { get; protected set; }

	public bool IsInVehicle => CurrentVehicle != null;

	public bool isInBuilding => CurrentBuilding != null;

	public NPCEnterableBuilding CurrentBuilding { get; protected set; }

	public StaticDoor LastEnteredDoor { get; set; }

	public MSGConversation MSGConversation { get; protected set; }

	public string SaveFolderName => fullName;

	public string SaveFileName => "NPC";

	public Loader Loader => null;

	public bool ShouldSaveUnderFolder => true;

	public List<string> LocalExtraFiles { get; set; } = new List<string> { "Relationship", "MessageConversation" };

	public List<string> LocalExtraFolders { get; set; } = new List<string>();

	public bool HasChanged { get; set; }

	public Vector3 CenterPoint => CenterPointTransform.position;

	public Transform CenterPointTransform => Avatar.CenterPointTransform;

	public Vector3 LookAtPoint => Avatar.Eyes.transform.position;

	public bool IsCurrentlyTargetable
	{
		get
		{
			if (!Health.IsDead && !Health.IsKnockedOut)
			{
				return isVisible;
			}
			return false;
		}
	}

	public float RangedHitChanceMultiplier => 1f;

	public Vector3 Velocity => Movement.VelocityCalculator.Velocity;

	public VisionEvent HighestProgressionEvent { get; set; }

	public EntityVisibility VisibilityComponent => Visibility;

	public Guid GUID { get; protected set; }

	public bool isVisible { get; protected set; } = true;

	public bool isUnsettled { get; protected set; }

	public bool IsPanicked => TimeSincePanicked < 20f;

	public float TimeSincePanicked { get; protected set; } = 1000f;

	public NetworkObject SyncAccessor_PlayerConversant
	{
		get
		{
			return PlayerConversant;
		}
		set
		{
			if (value || !base.IsServerInitialized)
			{
				PlayerConversant = value;
			}
			if (Application.isPlaying)
			{
				syncVar___PlayerConversant.SetValue(value, value);
			}
		}
	}

	public void RecordLastKnownPosition(bool resetTimeSinceLastSeen)
	{
	}

	public float GetSearchTime()
	{
		return 30f;
	}

	public bool IsCurrentlySightable()
	{
		if (Health.IsDead)
		{
			return false;
		}
		if (Health.IsKnockedOut)
		{
			return false;
		}
		if (!isVisible)
		{
			return false;
		}
		return true;
	}

	public virtual void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne.NPCs.NPC_Assembly-CSharp.dll();
		NetworkInitialize__Late();
	}

	protected virtual void CheckAndGetReferences()
	{
		if (Movement == null)
		{
			Movement = GetComponentInChildren<NPCMovement>();
		}
		if (DialogueHandler == null)
		{
			DialogueHandler = GetComponentInChildren<DialogueHandler>();
		}
		if (Avatar == null)
		{
			Avatar = GetComponentInChildren<ScheduleOne.AvatarFramework.Avatar>();
		}
		if (Awareness == null)
		{
			Awareness = GetComponentInChildren<NPCAwareness>();
		}
		if (Responses == null)
		{
			Responses = GetComponentInChildren<NPCResponses>();
		}
		if (Actions == null)
		{
			Actions = GetComponentInChildren<NPCActions>();
		}
		if (Behaviour == null)
		{
			Behaviour = GetComponentInChildren<NPCBehaviour>();
		}
		if (Inventory == null)
		{
			Inventory = GetComponentInChildren<NPCInventory>();
		}
		if (VoiceOverEmitter == null)
		{
			VoiceOverEmitter = Avatar.HeadBone.GetComponentInChildren<VOEmitter>();
		}
		if (Health == null)
		{
			Health = GetComponentInChildren<NPCHealth>();
		}
		if (Visibility == null)
		{
			Visibility = GetComponentInChildren<EntityVisibility>();
		}
	}

	public virtual void InitializeSaveable()
	{
		Singleton<SaveManager>.Instance.RegisterSaveable(this);
	}

	public void SetGUID(Guid guid)
	{
		GUID = guid;
		GUIDManager.RegisterObject(this);
	}

	private void PlayerSpawned()
	{
		CreateMessageConversation();
	}

	protected virtual void CreateMessageConversation()
	{
		if (MSGConversation != null)
		{
			Console.LogWarning("Message conversation already exists for " + fullName);
			return;
		}
		MSGConversation = new MSGConversation(this, GetMessagingName());
		MSGConversation.SetCategories(ConversationCategories);
		MSGConversation.SetIsKnown(MessagingKnownByDefault);
		if (onConversationCreated != null)
		{
			onConversationCreated();
		}
	}

	protected virtual string GetMessagingName()
	{
		return fullName;
	}

	public virtual Sprite GetMessagingIcon()
	{
		return MugshotSprite;
	}

	public void SendTextMessage(string message)
	{
		MSGConversation.SendMessage(new Message(message, Message.ESenderType.Other, _endOfGroup: true, UnityEngine.Random.Range(int.MinValue, int.MaxValue)));
	}

	protected override void OnValidate()
	{
		base.OnValidate();
		if (base.gameObject.scene.name != null && !(base.gameObject.scene.name == base.gameObject.name))
		{
			if (ID == string.Empty)
			{
				Console.LogWarning("NPC ID is empty (" + base.gameObject.name + ")");
			}
			CheckAndGetReferences();
		}
	}

	protected virtual void Start()
	{
		NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance.onMinutePass -= new Action(MinPass);
		NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance.onMinutePass += new Action(MinPass);
		NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance.onTick -= new Action(OnTick);
		NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance.onTick += new Action(OnTick);
		if (GUID == Guid.Empty)
		{
			if (!GUIDManager.IsGUIDValid(BakedGUID))
			{
				Console.LogWarning(base.gameObject.name + "'s baked GUID is not valid! Choosing random GUID");
				BakedGUID = GUIDManager.GenerateUniqueGUID().ToString();
			}
			GUID = new Guid(BakedGUID);
			GUIDManager.RegisterObject(this);
		}
		base.transform.SetParent(OverrideParent ? OverriddenParent : NetworkSingleton<NPCManager>.Instance.NPCContainer);
	}

	protected virtual void OnDestroy()
	{
		if (NetworkSingleton<ScheduleOne.GameTime.TimeManager>.InstanceExists)
		{
			NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance.onMinutePass -= new Action(MinPass);
			NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance.onTick -= new Action(OnTick);
		}
	}

	public override void OnSpawnServer(NetworkConnection connection)
	{
		base.OnSpawnServer(connection);
		if (!connection.IsLocalClient)
		{
			if (RelationData.Unlocked)
			{
				ReceiveRelationshipData(connection, RelationData.RelationDelta, unlocked: true);
			}
			if (IsInVehicle)
			{
				EnterVehicle(connection, CurrentVehicle);
			}
			if (isInBuilding)
			{
				EnterBuilding(connection, CurrentBuilding.GUID.ToString(), CurrentBuilding.Doors.IndexOf(LastEnteredDoor));
			}
			SetTransform(connection, base.transform.position, base.transform.rotation);
			if (Avatar.CurrentEquippable != null)
			{
				SetEquippable_Networked(connection, Avatar.CurrentEquippable.AssetPath);
			}
		}
	}

	[ObserversRpc]
	private void SetTransform(NetworkConnection conn, Vector3 position, Quaternion rotation)
	{
		RpcWriter___Observers_SetTransform_4260003484(conn, position, rotation);
	}

	protected virtual void MinPass()
	{
		if (InstanceFinder.IsServer)
		{
			float timeSincePanicked = TimeSincePanicked;
			TimeSincePanicked += 1f;
			if (TimeSincePanicked > 20f && timeSincePanicked <= 20f)
			{
				RemovePanicked();
			}
		}
		if (!(CurrentVehicle != null))
		{
			return;
		}
		VehicleLights component = CurrentVehicle.GetComponent<VehicleLights>();
		if (component != null)
		{
			if (NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance.IsCurrentTimeWithinRange(headlightStartTime, heaedLightsEndTime))
			{
				component.headLightsOn = true;
			}
			else
			{
				component.headLightsOn = false;
			}
		}
	}

	protected virtual void OnTick()
	{
	}

	public virtual void SetVisible(bool visible, bool networked = false)
	{
		if (networked)
		{
			SetVisible_Networked(visible);
			return;
		}
		isVisible = visible;
		modelContainer.gameObject.SetActive(isVisible);
		if (InstanceFinder.IsServer || !isVisible)
		{
			Movement.SetAgentEnabled(isVisible);
		}
		if (onVisibilityChanged != null)
		{
			onVisibilityChanged(visible);
		}
	}

	[ObserversRpc(RunLocally = true)]
	private void SetVisible_Networked(bool visible)
	{
		RpcWriter___Observers_SetVisible_Networked_1140765316(visible);
		RpcLogic___SetVisible_Networked_1140765316(visible);
	}

	public void SetScale(float scale)
	{
		Scale = scale;
		ApplyScale();
	}

	public void SetScale(float scale, float lerpTime)
	{
		if (lerpScaleRoutine != null)
		{
			StopCoroutine(lerpScaleRoutine);
		}
		float startScale = Scale;
		lerpScaleRoutine = Singleton<CoroutineService>.Instance.StartCoroutine(LerpScale());
		IEnumerator LerpScale()
		{
			for (float i = 0f; i < lerpTime; i += Time.deltaTime)
			{
				SetScale(Mathf.Lerp(startScale, scale, i / lerpTime));
				yield return new WaitForEndOfFrame();
			}
			SetScale(scale);
		}
	}

	protected virtual void ApplyScale()
	{
		base.transform.localScale = new Vector3(Scale, Scale, Scale);
	}

	[ServerRpc(RequireOwnership = false)]
	public virtual void AimedAtByPlayer(NetworkObject player)
	{
		RpcWriter___Server_AimedAtByPlayer_3323014238(player);
	}

	public void OverrideAggression(float aggression)
	{
		Aggression = aggression;
	}

	public void ResetAggression()
	{
		Aggression = defaultAggression;
	}

	protected virtual void OnDie()
	{
		Visibility.ClearStates();
	}

	protected virtual void OnKnockedOut()
	{
		if (Visibility != null)
		{
			Visibility.ClearStates();
		}
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public virtual void SendImpact(Impact impact)
	{
		RpcWriter___Server_SendImpact_427288424(impact);
		RpcLogic___SendImpact_427288424(impact);
	}

	[ObserversRpc(RunLocally = true)]
	public virtual void ReceiveImpact(Impact impact)
	{
		RpcWriter___Observers_ReceiveImpact_427288424(impact);
		RpcLogic___ReceiveImpact_427288424(impact);
	}

	public virtual void ProcessImpactForce(Vector3 forcePoint, Vector3 forceDirection, float force)
	{
		if (Avatar.Ragdolled)
		{
			Movement.ApplyRagdollForce(forcePoint, forceDirection, force);
		}
		else if (force >= 150f)
		{
			if (!Avatar.Ragdolled)
			{
				Movement.ActivateRagdoll(forcePoint, forceDirection, force);
			}
		}
		else if (force >= 100f)
		{
			if (!Movement.IsOnLadder)
			{
				Avatar.Animation.Flinch(forceDirection, AvatarAnimation.EFlinchType.Heavy);
			}
		}
		else if (force >= 50f && !Movement.IsOnLadder)
		{
			Avatar.Animation.Flinch(forceDirection, AvatarAnimation.EFlinchType.Light);
		}
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	public virtual void EnterVehicle(NetworkConnection connection, LandVehicle veh)
	{
		if ((object)connection == null)
		{
			RpcWriter___Observers_EnterVehicle_3321926803(connection, veh);
			RpcLogic___EnterVehicle_3321926803(connection, veh);
		}
		else
		{
			RpcWriter___Target_EnterVehicle_3321926803(connection, veh);
		}
	}

	[ObserversRpc(RunLocally = true)]
	public virtual void ExitVehicle()
	{
		RpcWriter___Observers_ExitVehicle_2166136261();
		RpcLogic___ExitVehicle_2166136261();
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void SendWorldspaceDialogueReaction(string key, float duration)
	{
		RpcWriter___Server_SendWorldspaceDialogueReaction_606697822(key, duration);
		RpcLogic___SendWorldspaceDialogueReaction_606697822(key, duration);
	}

	[ObserversRpc(RunLocally = true)]
	private void PlayWorldspaceDialogueReaction(string key, float duration)
	{
		RpcWriter___Observers_PlayWorldspaceDialogueReaction_606697822(key, duration);
		RpcLogic___PlayWorldspaceDialogueReaction_606697822(key, duration);
	}

	[ServerRpc(RequireOwnership = false)]
	public void SendWorldSpaceDialogue(string text, float duration)
	{
		RpcWriter___Server_SendWorldSpaceDialogue_606697822(text, duration);
	}

	[ObserversRpc(RunLocally = true)]
	public void ShowWorldSpaceDialogue(string text, float duration)
	{
		RpcWriter___Observers_ShowWorldSpaceDialogue_606697822(text, duration);
		RpcLogic___ShowWorldSpaceDialogue_606697822(text, duration);
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void SetConversant(NetworkObject player)
	{
		RpcWriter___Server_SetConversant_3323014238(player);
		RpcLogic___SetConversant_3323014238(player);
	}

	private void Hovered_Internal()
	{
		Hovered();
	}

	private void Interacted_Internal()
	{
		Interacted();
	}

	protected virtual void Hovered()
	{
	}

	protected virtual void Interacted()
	{
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	public void EnterBuilding(NetworkConnection connection, string buildingGUID, int doorIndex)
	{
		if ((object)connection == null)
		{
			RpcWriter___Observers_EnterBuilding_3905681115(connection, buildingGUID, doorIndex);
			RpcLogic___EnterBuilding_3905681115(connection, buildingGUID, doorIndex);
		}
		else
		{
			RpcWriter___Target_EnterBuilding_3905681115(connection, buildingGUID, doorIndex);
		}
	}

	protected virtual void EnterBuilding(string buildingGUID, int doorIndex)
	{
		NPCEnterableBuilding nPCEnterableBuilding = GUIDManager.GetObject<NPCEnterableBuilding>(new Guid(buildingGUID));
		if (nPCEnterableBuilding == null)
		{
			Console.LogWarning(fullName + ".EnterBuilding: building not found with given GUID");
			return;
		}
		if (nPCEnterableBuilding == CurrentBuilding)
		{
			if (InstanceFinder.IsServer)
			{
				Movement.Stop();
				Movement.Warp(nPCEnterableBuilding.Doors[doorIndex].AccessPoint);
				Movement.FaceDirection(-nPCEnterableBuilding.Doors[doorIndex].AccessPoint.forward, 0f);
			}
			SetVisible(visible: false);
			return;
		}
		if (CurrentBuilding != null)
		{
			Console.LogWarning("NPC.EnterBuilding called but NPC is already in a building. New building will still be entered.");
			ExitBuilding();
		}
		CurrentBuilding = nPCEnterableBuilding;
		LastEnteredDoor = nPCEnterableBuilding.Doors[doorIndex];
		Awareness.SetAwarenessActive(active: false);
		nPCEnterableBuilding.NPCEnteredBuilding(this);
		SetVisible(visible: false);
		Movement.Stop();
		Movement.Warp(nPCEnterableBuilding.Doors[doorIndex].AccessPoint);
		Movement.FaceDirection(-nPCEnterableBuilding.Doors[doorIndex].AccessPoint.forward, 0f);
	}

	[ObserversRpc(RunLocally = true)]
	public void ExitBuilding(string buildingID = "")
	{
		RpcWriter___Observers_ExitBuilding_3615296227(buildingID);
		RpcLogic___ExitBuilding_3615296227(buildingID);
	}

	protected virtual void ExitBuilding(NPCEnterableBuilding building)
	{
		if (!(building == null))
		{
			if (LastEnteredDoor == null)
			{
				LastEnteredDoor = building.Doors[0];
			}
			Avatar.transform.localPosition = Vector3.zero;
			Avatar.transform.localRotation = Quaternion.identity;
			NavMeshHit hit;
			Vector3 position = (NavMeshUtility.SamplePosition(LastEnteredDoor.AccessPoint.transform.position, out hit, 2f, -1) ? hit.position : LastEnteredDoor.AccessPoint.transform.position);
			Movement.Warp(position);
			Movement.FaceDirection(-LastEnteredDoor.AccessPoint.transform.forward, 0f);
			Awareness.SetAwarenessActive(active: true);
			building.NPCExitedBuilding(this);
			CurrentBuilding = null;
			SetVisible(visible: true);
		}
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	public void SetEquippable_Networked(NetworkConnection conn, string assetPath)
	{
		if ((object)conn == null)
		{
			RpcWriter___Observers_SetEquippable_Networked_2971853958(conn, assetPath);
			RpcLogic___SetEquippable_Networked_2971853958(conn, assetPath);
		}
		else
		{
			RpcWriter___Target_SetEquippable_Networked_2971853958(conn, assetPath);
		}
	}

	public AvatarEquippable SetEquippable_Networked_Return(NetworkConnection conn, string assetPath)
	{
		SetEquippable_Networked_ExcludeServer(conn, assetPath);
		return Avatar.SetEquippable(assetPath);
	}

	public AvatarEquippable SetEquippable_Return(string assetPath)
	{
		return Avatar.SetEquippable(assetPath);
	}

	[ObserversRpc(RunLocally = false, ExcludeServer = true)]
	private void SetEquippable_Networked_ExcludeServer(NetworkConnection conn, string assetPath)
	{
		RpcWriter___Observers_SetEquippable_Networked_ExcludeServer_2971853958(conn, assetPath);
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	public void SendEquippableMessage_Networked(NetworkConnection conn, string message)
	{
		if ((object)conn == null)
		{
			RpcWriter___Observers_SendEquippableMessage_Networked_2971853958(conn, message);
			RpcLogic___SendEquippableMessage_Networked_2971853958(conn, message);
		}
		else
		{
			RpcWriter___Target_SendEquippableMessage_Networked_2971853958(conn, message);
		}
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	public void SendEquippableMessage_Networked_Vector(NetworkConnection conn, string message, Vector3 data)
	{
		if ((object)conn == null)
		{
			RpcWriter___Observers_SendEquippableMessage_Networked_Vector_4022222929(conn, message, data);
			RpcLogic___SendEquippableMessage_Networked_Vector_4022222929(conn, message, data);
		}
		else
		{
			RpcWriter___Target_SendEquippableMessage_Networked_Vector_4022222929(conn, message, data);
		}
	}

	[ServerRpc(RequireOwnership = false)]
	public void SendAnimationTrigger(string trigger)
	{
		RpcWriter___Server_SendAnimationTrigger_3615296227(trigger);
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	public void SetAnimationTrigger_Networked(NetworkConnection conn, string trigger)
	{
		if ((object)conn == null)
		{
			RpcWriter___Observers_SetAnimationTrigger_Networked_2971853958(conn, trigger);
			RpcLogic___SetAnimationTrigger_Networked_2971853958(conn, trigger);
		}
		else
		{
			RpcWriter___Target_SetAnimationTrigger_Networked_2971853958(conn, trigger);
		}
	}

	public void SetAnimationTrigger(string trigger)
	{
		Avatar.Animation.SetTrigger(trigger);
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	public void ResetAnimationTrigger_Networked(NetworkConnection conn, string trigger)
	{
		if ((object)conn == null)
		{
			RpcWriter___Observers_ResetAnimationTrigger_Networked_2971853958(conn, trigger);
			RpcLogic___ResetAnimationTrigger_Networked_2971853958(conn, trigger);
		}
		else
		{
			RpcWriter___Target_ResetAnimationTrigger_Networked_2971853958(conn, trigger);
		}
	}

	public void ResetAnimationTrigger(string trigger)
	{
		Avatar.Animation.ResetTrigger(trigger);
	}

	[ObserversRpc(RunLocally = true)]
	public void SetCrouched_Networked(bool crouched)
	{
		RpcWriter___Observers_SetCrouched_Networked_1140765316(crouched);
		RpcLogic___SetCrouched_Networked_1140765316(crouched);
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	public void SetAnimationBool_Networked(NetworkConnection conn, string id, bool value)
	{
		if ((object)conn == null)
		{
			RpcWriter___Observers_SetAnimationBool_Networked_619441887(conn, id, value);
			RpcLogic___SetAnimationBool_Networked_619441887(conn, id, value);
		}
		else
		{
			RpcWriter___Target_SetAnimationBool_Networked_619441887(conn, id, value);
		}
	}

	public void SetAnimationBool(string trigger, bool val)
	{
		Avatar.Animation.SetBool(trigger, val);
	}

	protected virtual void SetUnsettled_30s(Player player)
	{
		SetUnsettled(30f);
	}

	protected void SetUnsettled(float duration)
	{
		bool num = isUnsettled;
		isUnsettled = true;
		if (!num)
		{
			Avatar.EmotionManager.AddEmotionOverride("Concerned", "unsettled", 0f, 5);
		}
		if (resetUnsettledCoroutine != null)
		{
			StopCoroutine(resetUnsettledCoroutine);
		}
		resetUnsettledCoroutine = StartCoroutine(ResetUnsettled());
		IEnumerator ResetUnsettled()
		{
			Movement.SpeedController.AddSpeedControl(new NPCSpeedController.SpeedControl("unsetttled", 10, 0.2f));
			yield return new WaitForSeconds(duration);
			isUnsettled = false;
			Avatar.EmotionManager.RemoveEmotionOverride("unsettled");
			Movement.SpeedController.RemoveSpeedControl("unsettled");
		}
	}

	[ServerRpc(RequireOwnership = false)]
	public void SetPanicked()
	{
		RpcWriter___Server_SetPanicked_2166136261();
	}

	[ObserversRpc]
	private void ReceivePanicked()
	{
		RpcWriter___Observers_ReceivePanicked_2166136261();
	}

	[ObserversRpc]
	private void RemovePanicked()
	{
		RpcWriter___Observers_RemovePanicked_2166136261();
	}

	public virtual string GetNameAddress()
	{
		return FirstName;
	}

	public void PlayVO(EVOLineType lineType, bool network = false)
	{
		if (network)
		{
			PlayVO_Server(lineType);
		}
		else
		{
			VoiceOverEmitter.Play(lineType);
		}
	}

	[ServerRpc(RequireOwnership = false)]
	private void PlayVO_Server(EVOLineType lineType)
	{
		RpcWriter___Server_PlayVO_Server_1710085680(lineType);
	}

	[ObserversRpc(RunLocally = true)]
	private void PlayVO_Client(EVOLineType lineType)
	{
		RpcWriter___Observers_PlayVO_Client_1710085680(lineType);
		RpcLogic___PlayVO_Client_1710085680(lineType);
	}

	[TargetRpc]
	public void ReceiveRelationshipData(NetworkConnection conn, float relationship, bool unlocked)
	{
		RpcWriter___Target_ReceiveRelationshipData_4052192084(conn, relationship, unlocked);
	}

	[ServerRpc(RequireOwnership = false)]
	public void SetIsBeingPickPocketed(bool pickpocketed)
	{
		RpcWriter___Server_SetIsBeingPickPocketed_1140765316(pickpocketed);
	}

	[ServerRpc(RequireOwnership = false)]
	public void SendRelationship(float relationship)
	{
		RpcWriter___Server_SendRelationship_431000436(relationship);
	}

	[ObserversRpc]
	private void SetRelationship(float relationship)
	{
		RpcWriter___Observers_SetRelationship_431000436(relationship);
	}

	public void ShowOutline(Color color)
	{
		if (OutlineEffect == null)
		{
			OutlineEffect = base.gameObject.AddComponent<Outlinable>();
			OutlineEffect.OutlineParameters.BlurShift = 0f;
			OutlineEffect.OutlineParameters.DilateShift = 0.5f;
			OutlineEffect.OutlineParameters.FillPass.Shader = Resources.Load<Shader>("Easy performant outline/Shaders/Fills/ColorFill");
			foreach (GameObject outlineRenderer in OutlineRenderers)
			{
				SkinnedMeshRenderer[] array = new SkinnedMeshRenderer[0];
				array = new SkinnedMeshRenderer[1] { outlineRenderer.GetComponent<SkinnedMeshRenderer>() };
				for (int i = 0; i < array.Length; i++)
				{
					OutlineTarget target = new OutlineTarget(array[i]);
					OutlineEffect.TryAddTarget(target);
				}
			}
		}
		OutlineEffect.OutlineParameters.Color = color;
		Color32 color2 = color;
		color2.a = 9;
		OutlineEffect.OutlineParameters.FillPass.SetColor("_PublicColor", color2);
		OutlineEffect.enabled = true;
	}

	public void HideOutline()
	{
		if (OutlineEffect != null)
		{
			OutlineEffect.enabled = false;
		}
	}

	public virtual bool ShouldSave()
	{
		if (ShouldSaveRelationshipData())
		{
			return true;
		}
		if (ShouldSaveMessages())
		{
			return true;
		}
		if (ShouldSaveInventory())
		{
			return true;
		}
		if (ShouldSaveHealth())
		{
			return true;
		}
		return HasChanged;
	}

	protected virtual bool ShouldSaveRelationshipData()
	{
		if (RelationData.Unlocked)
		{
			return true;
		}
		if (2f != RelationData.RelationDelta)
		{
			return true;
		}
		return false;
	}

	protected bool ShouldSaveMessages()
	{
		if (MSGConversation == null)
		{
			return false;
		}
		if (MSGConversation.messageHistory.Count > 0)
		{
			return true;
		}
		return false;
	}

	protected virtual bool ShouldSaveInventory()
	{
		return ((IItemSlotOwner)Inventory).GetQuantitySum() > 0;
	}

	protected virtual bool ShouldSaveHealth()
	{
		if (!(Health.Health < Health.MaxHealth) && !Health.IsDead)
		{
			return Health.DaysPassedSinceDeath > 0;
		}
		return true;
	}

	public string GetSaveString()
	{
		return GetSaveData().GetJson();
	}

	public virtual NPCData GetNPCData()
	{
		return new NPCData(ID);
	}

	public virtual DynamicSaveData GetSaveData()
	{
		DynamicSaveData dynamicSaveData = new DynamicSaveData(GetNPCData());
		if (ShouldSaveRelationshipData())
		{
			dynamicSaveData.AddData("Relationship", RelationData.GetSaveData().GetJson());
		}
		if (ShouldSaveMessages())
		{
			dynamicSaveData.AddData("MessageConversation", MSGConversation.GetSaveData().GetJson());
		}
		if (ShouldSaveInventory())
		{
			dynamicSaveData.AddData("Inventory", new ItemSet(Inventory.ItemSlots).GetJSON());
		}
		if (ShouldSaveHealth())
		{
			dynamicSaveData.AddData("Health", new NPCHealthData(Health.Health, Health.IsDead, Health.DaysPassedSinceDeath, Health.HoursSinceAttackedByPlayer).GetJson());
		}
		Customer component = GetComponent<Customer>();
		if (component != null)
		{
			dynamicSaveData.AddData("CustomerData", component.GetSaveString());
		}
		return dynamicSaveData;
	}

	public virtual List<string> WriteData(string parentFolderPath)
	{
		return new List<string>();
	}

	public virtual void Load(NPCData data, string containerPath)
	{
	}

	public virtual void Load(DynamicSaveData dynamicData, NPCData npcData)
	{
	}

	NetworkObject ICombatTargetable.get_NetworkObject()
	{
		return base.NetworkObject;
	}

	GameObject IDamageable.get_gameObject()
	{
		return base.gameObject;
	}

	NetworkObject ISightable.get_NetworkObject()
	{
		return base.NetworkObject;
	}

	public virtual void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyScheduleOne.NPCs.NPCAssembly-CSharp.dll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne.NPCs.NPCAssembly-CSharp.dll_Excuted = true;
			syncVar___PlayerConversant = new SyncVar<NetworkObject>(this, 0u, WritePermission.ServerOnly, ReadPermission.Observers, -1f, Channel.Reliable, PlayerConversant);
			RegisterObserversRpc(0u, RpcReader___Observers_SetTransform_4260003484);
			RegisterObserversRpc(1u, RpcReader___Observers_SetVisible_Networked_1140765316);
			RegisterServerRpc(2u, RpcReader___Server_AimedAtByPlayer_3323014238);
			RegisterServerRpc(3u, RpcReader___Server_SendImpact_427288424);
			RegisterObserversRpc(4u, RpcReader___Observers_ReceiveImpact_427288424);
			RegisterObserversRpc(5u, RpcReader___Observers_EnterVehicle_3321926803);
			RegisterTargetRpc(6u, RpcReader___Target_EnterVehicle_3321926803);
			RegisterObserversRpc(7u, RpcReader___Observers_ExitVehicle_2166136261);
			RegisterServerRpc(8u, RpcReader___Server_SendWorldspaceDialogueReaction_606697822);
			RegisterObserversRpc(9u, RpcReader___Observers_PlayWorldspaceDialogueReaction_606697822);
			RegisterServerRpc(10u, RpcReader___Server_SendWorldSpaceDialogue_606697822);
			RegisterObserversRpc(11u, RpcReader___Observers_ShowWorldSpaceDialogue_606697822);
			RegisterServerRpc(12u, RpcReader___Server_SetConversant_3323014238);
			RegisterObserversRpc(13u, RpcReader___Observers_EnterBuilding_3905681115);
			RegisterTargetRpc(14u, RpcReader___Target_EnterBuilding_3905681115);
			RegisterObserversRpc(15u, RpcReader___Observers_ExitBuilding_3615296227);
			RegisterObserversRpc(16u, RpcReader___Observers_SetEquippable_Networked_2971853958);
			RegisterTargetRpc(17u, RpcReader___Target_SetEquippable_Networked_2971853958);
			RegisterObserversRpc(18u, RpcReader___Observers_SetEquippable_Networked_ExcludeServer_2971853958);
			RegisterObserversRpc(19u, RpcReader___Observers_SendEquippableMessage_Networked_2971853958);
			RegisterTargetRpc(20u, RpcReader___Target_SendEquippableMessage_Networked_2971853958);
			RegisterObserversRpc(21u, RpcReader___Observers_SendEquippableMessage_Networked_Vector_4022222929);
			RegisterTargetRpc(22u, RpcReader___Target_SendEquippableMessage_Networked_Vector_4022222929);
			RegisterServerRpc(23u, RpcReader___Server_SendAnimationTrigger_3615296227);
			RegisterObserversRpc(24u, RpcReader___Observers_SetAnimationTrigger_Networked_2971853958);
			RegisterTargetRpc(25u, RpcReader___Target_SetAnimationTrigger_Networked_2971853958);
			RegisterObserversRpc(26u, RpcReader___Observers_ResetAnimationTrigger_Networked_2971853958);
			RegisterTargetRpc(27u, RpcReader___Target_ResetAnimationTrigger_Networked_2971853958);
			RegisterObserversRpc(28u, RpcReader___Observers_SetCrouched_Networked_1140765316);
			RegisterObserversRpc(29u, RpcReader___Observers_SetAnimationBool_Networked_619441887);
			RegisterTargetRpc(30u, RpcReader___Target_SetAnimationBool_Networked_619441887);
			RegisterServerRpc(31u, RpcReader___Server_SetPanicked_2166136261);
			RegisterObserversRpc(32u, RpcReader___Observers_ReceivePanicked_2166136261);
			RegisterObserversRpc(33u, RpcReader___Observers_RemovePanicked_2166136261);
			RegisterServerRpc(34u, RpcReader___Server_PlayVO_Server_1710085680);
			RegisterObserversRpc(35u, RpcReader___Observers_PlayVO_Client_1710085680);
			RegisterTargetRpc(36u, RpcReader___Target_ReceiveRelationshipData_4052192084);
			RegisterServerRpc(37u, RpcReader___Server_SetIsBeingPickPocketed_1140765316);
			RegisterServerRpc(38u, RpcReader___Server_SendRelationship_431000436);
			RegisterObserversRpc(39u, RpcReader___Observers_SetRelationship_431000436);
			RegisterSyncVarRead(ReadSyncVar___ScheduleOne.NPCs.NPC);
		}
	}

	public virtual void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne.NPCs.NPCAssembly-CSharp.dll_Excuted)
		{
			NetworkInitialize__LateScheduleOne.NPCs.NPCAssembly-CSharp.dll_Excuted = true;
			syncVar___PlayerConversant.SetRegistered();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Observers_SetTransform_4260003484(NetworkConnection conn, Vector3 position, Quaternion rotation)
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
			writer.WriteNetworkConnection(conn);
			writer.WriteVector3(position);
			writer.WriteQuaternion(rotation);
			SendObserversRpc(0u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
			writer.Store();
		}
	}

	private void RpcLogic___SetTransform_4260003484(NetworkConnection conn, Vector3 position, Quaternion rotation)
	{
		base.transform.position = position;
		base.transform.rotation = rotation;
	}

	private void RpcReader___Observers_SetTransform_4260003484(PooledReader PooledReader0, Channel channel)
	{
		NetworkConnection conn = PooledReader0.ReadNetworkConnection();
		Vector3 position = PooledReader0.ReadVector3();
		Quaternion rotation = PooledReader0.ReadQuaternion();
		if (base.IsClientInitialized)
		{
			RpcLogic___SetTransform_4260003484(conn, position, rotation);
		}
	}

	private void RpcWriter___Observers_SetVisible_Networked_1140765316(bool visible)
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
			writer.WriteBoolean(visible);
			SendObserversRpc(1u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
			writer.Store();
		}
	}

	private void RpcLogic___SetVisible_Networked_1140765316(bool visible)
	{
		SetVisible(visible);
	}

	private void RpcReader___Observers_SetVisible_Networked_1140765316(PooledReader PooledReader0, Channel channel)
	{
		bool visible = PooledReader0.ReadBoolean();
		if (base.IsClientInitialized && !base.IsHost)
		{
			RpcLogic___SetVisible_Networked_1140765316(visible);
		}
	}

	private void RpcWriter___Server_AimedAtByPlayer_3323014238(NetworkObject player)
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
			writer.WriteNetworkObject(player);
			SendServerRpc(2u, writer, channel, DataOrderType.Default);
			writer.Store();
		}
	}

	public virtual void RpcLogic___AimedAtByPlayer_3323014238(NetworkObject player)
	{
		Responses.RespondToAimedAt(player.GetComponent<Player>());
	}

	private void RpcReader___Server_AimedAtByPlayer_3323014238(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		NetworkObject player = PooledReader0.ReadNetworkObject();
		if (base.IsServerInitialized)
		{
			RpcLogic___AimedAtByPlayer_3323014238(player);
		}
	}

	private void RpcWriter___Server_SendImpact_427288424(Impact impact)
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
			GeneratedWriters___Internal.Write___ScheduleOne.Combat.ImpactFishNet.Serializing.Generated(writer, impact);
			SendServerRpc(3u, writer, channel, DataOrderType.Default);
			writer.Store();
		}
	}

	public virtual void RpcLogic___SendImpact_427288424(Impact impact)
	{
		ReceiveImpact(impact);
	}

	private void RpcReader___Server_SendImpact_427288424(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		Impact impact = GeneratedReaders___Internal.Read___ScheduleOne.Combat.ImpactFishNet.Serializing.Generateds(PooledReader0);
		if (base.IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SendImpact_427288424(impact);
		}
	}

	private void RpcWriter___Observers_ReceiveImpact_427288424(Impact impact)
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
			GeneratedWriters___Internal.Write___ScheduleOne.Combat.ImpactFishNet.Serializing.Generated(writer, impact);
			SendObserversRpc(4u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
			writer.Store();
		}
	}

	public virtual void RpcLogic___ReceiveImpact_427288424(Impact impact)
	{
		if (!IgnoreImpacts && !impactHistory.Contains(impact.ImpactID))
		{
			impactHistory.Add(impact.ImpactID);
			float num = 1f;
			switch (Movement.Stance)
			{
			case NPCMovement.EStance.None:
				num = 1f;
				break;
			case NPCMovement.EStance.Stanced:
				num = 0.5f;
				break;
			}
			if (impact.IsPlayerImpact(out var player))
			{
				Health.NotifyAttackedByPlayer(player);
			}
			Health.TakeDamage(impact.ImpactDamage, Impact.IsLethal(impact.ImpactType));
			ProcessImpactForce(impact.HitPoint, impact.ImpactForceDirection, impact.ImpactForce * num);
			Responses.ImpactReceived(impact);
		}
	}

	private void RpcReader___Observers_ReceiveImpact_427288424(PooledReader PooledReader0, Channel channel)
	{
		Impact impact = GeneratedReaders___Internal.Read___ScheduleOne.Combat.ImpactFishNet.Serializing.Generateds(PooledReader0);
		if (base.IsClientInitialized && !base.IsHost)
		{
			RpcLogic___ReceiveImpact_427288424(impact);
		}
	}

	private void RpcWriter___Observers_EnterVehicle_3321926803(NetworkConnection connection, LandVehicle veh)
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
			GeneratedWriters___Internal.Write___ScheduleOne.Vehicles.LandVehicleFishNet.Serializing.Generated(writer, veh);
			SendObserversRpc(5u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
			writer.Store();
		}
	}

	public virtual void RpcLogic___EnterVehicle_3321926803(NetworkConnection connection, LandVehicle veh)
	{
		if (!(veh == CurrentVehicle))
		{
			CurrentVehicle = veh;
			SetVisible(visible: false);
			Movement.SetAgentEnabled(enabled: false);
			base.transform.SetParent(veh.transform);
			veh.AddNPCOccupant(this);
			base.transform.position = CurrentVehicle.Seats[CurrentVehicle.OccupantNPCs.ToList().IndexOf(this)].transform.position;
			base.transform.localRotation = Quaternion.identity;
			if (onEnterVehicle != null)
			{
				onEnterVehicle(veh);
			}
		}
	}

	private void RpcReader___Observers_EnterVehicle_3321926803(PooledReader PooledReader0, Channel channel)
	{
		LandVehicle veh = GeneratedReaders___Internal.Read___ScheduleOne.Vehicles.LandVehicleFishNet.Serializing.Generateds(PooledReader0);
		if (base.IsClientInitialized && !base.IsHost)
		{
			RpcLogic___EnterVehicle_3321926803(null, veh);
		}
	}

	private void RpcWriter___Target_EnterVehicle_3321926803(NetworkConnection connection, LandVehicle veh)
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
			GeneratedWriters___Internal.Write___ScheduleOne.Vehicles.LandVehicleFishNet.Serializing.Generated(writer, veh);
			SendTargetRpc(6u, writer, channel, DataOrderType.Default, connection, excludeServer: false);
			writer.Store();
		}
	}

	private void RpcReader___Target_EnterVehicle_3321926803(PooledReader PooledReader0, Channel channel)
	{
		LandVehicle veh = GeneratedReaders___Internal.Read___ScheduleOne.Vehicles.LandVehicleFishNet.Serializing.Generateds(PooledReader0);
		if (base.IsClientInitialized)
		{
			RpcLogic___EnterVehicle_3321926803(base.LocalConnection, veh);
		}
	}

	private void RpcWriter___Observers_ExitVehicle_2166136261()
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
			SendObserversRpc(7u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
			writer.Store();
		}
	}

	public virtual void RpcLogic___ExitVehicle_2166136261()
	{
		if (!(CurrentVehicle == null))
		{
			int seatIndex = CurrentVehicle.OccupantNPCs.ToList().IndexOf(this);
			CurrentVehicle.RemoveNPCOccupant(this);
			CurrentVehicle.Agent.Flags.ResetFlags();
			if (CurrentVehicle.GetComponent<VehicleLights>() != null)
			{
				CurrentVehicle.GetComponent<VehicleLights>().headLightsOn = false;
			}
			Transform exitPoint = CurrentVehicle.GetExitPoint(seatIndex);
			base.transform.SetParent(OverrideParent ? OverriddenParent : NetworkSingleton<NPCManager>.Instance.NPCContainer);
			base.transform.position = exitPoint.position - exitPoint.up * 1f;
			Movement.FaceDirection(exitPoint.forward, 0f);
			if (InstanceFinder.IsServer)
			{
				Movement.SetAgentEnabled(enabled: true);
			}
			SetVisible(visible: true);
			if (onExitVehicle != null)
			{
				onExitVehicle(CurrentVehicle);
			}
			CurrentVehicle = null;
		}
	}

	private void RpcReader___Observers_ExitVehicle_2166136261(PooledReader PooledReader0, Channel channel)
	{
		if (base.IsClientInitialized && !base.IsHost)
		{
			RpcLogic___ExitVehicle_2166136261();
		}
	}

	private void RpcWriter___Server_SendWorldspaceDialogueReaction_606697822(string key, float duration)
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
			writer.WriteString(key);
			writer.WriteSingle(duration);
			SendServerRpc(8u, writer, channel, DataOrderType.Default);
			writer.Store();
		}
	}

	public void RpcLogic___SendWorldspaceDialogueReaction_606697822(string key, float duration)
	{
		PlayWorldspaceDialogueReaction(key, duration);
	}

	private void RpcReader___Server_SendWorldspaceDialogueReaction_606697822(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		string key = PooledReader0.ReadString();
		float duration = PooledReader0.ReadSingle();
		if (base.IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SendWorldspaceDialogueReaction_606697822(key, duration);
		}
	}

	private void RpcWriter___Observers_PlayWorldspaceDialogueReaction_606697822(string key, float duration)
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
			writer.WriteString(key);
			writer.WriteSingle(duration);
			SendObserversRpc(9u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
			writer.Store();
		}
	}

	private void RpcLogic___PlayWorldspaceDialogueReaction_606697822(string key, float duration)
	{
		DialogueHandler.PlayReaction(key, duration, network: false);
	}

	private void RpcReader___Observers_PlayWorldspaceDialogueReaction_606697822(PooledReader PooledReader0, Channel channel)
	{
		string key = PooledReader0.ReadString();
		float duration = PooledReader0.ReadSingle();
		if (base.IsClientInitialized && !base.IsHost)
		{
			RpcLogic___PlayWorldspaceDialogueReaction_606697822(key, duration);
		}
	}

	private void RpcWriter___Server_SendWorldSpaceDialogue_606697822(string text, float duration)
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
			writer.WriteString(text);
			writer.WriteSingle(duration);
			SendServerRpc(10u, writer, channel, DataOrderType.Default);
			writer.Store();
		}
	}

	public void RpcLogic___SendWorldSpaceDialogue_606697822(string text, float duration)
	{
		ShowWorldSpaceDialogue(text, duration);
	}

	private void RpcReader___Server_SendWorldSpaceDialogue_606697822(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		string text = PooledReader0.ReadString();
		float duration = PooledReader0.ReadSingle();
		if (base.IsServerInitialized)
		{
			RpcLogic___SendWorldSpaceDialogue_606697822(text, duration);
		}
	}

	private void RpcWriter___Observers_ShowWorldSpaceDialogue_606697822(string text, float duration)
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
			writer.WriteString(text);
			writer.WriteSingle(duration);
			SendObserversRpc(11u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
			writer.Store();
		}
	}

	public void RpcLogic___ShowWorldSpaceDialogue_606697822(string text, float duration)
	{
		DialogueHandler.ShowWorldspaceDialogue(text, duration);
	}

	private void RpcReader___Observers_ShowWorldSpaceDialogue_606697822(PooledReader PooledReader0, Channel channel)
	{
		string text = PooledReader0.ReadString();
		float duration = PooledReader0.ReadSingle();
		if (base.IsClientInitialized && !base.IsHost)
		{
			RpcLogic___ShowWorldSpaceDialogue_606697822(text, duration);
		}
	}

	private void RpcWriter___Server_SetConversant_3323014238(NetworkObject player)
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
			writer.WriteNetworkObject(player);
			SendServerRpc(12u, writer, channel, DataOrderType.Default);
			writer.Store();
		}
	}

	public void RpcLogic___SetConversant_3323014238(NetworkObject player)
	{
		this.sync___set_value_PlayerConversant(player, asServer: true);
	}

	private void RpcReader___Server_SetConversant_3323014238(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		NetworkObject player = PooledReader0.ReadNetworkObject();
		if (base.IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SetConversant_3323014238(player);
		}
	}

	private void RpcWriter___Observers_EnterBuilding_3905681115(NetworkConnection connection, string buildingGUID, int doorIndex)
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
			writer.WriteString(buildingGUID);
			writer.WriteInt32(doorIndex);
			SendObserversRpc(13u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
			writer.Store();
		}
	}

	public void RpcLogic___EnterBuilding_3905681115(NetworkConnection connection, string buildingGUID, int doorIndex)
	{
		EnterBuilding(buildingGUID, doorIndex);
	}

	private void RpcReader___Observers_EnterBuilding_3905681115(PooledReader PooledReader0, Channel channel)
	{
		string buildingGUID = PooledReader0.ReadString();
		int doorIndex = PooledReader0.ReadInt32();
		if (base.IsClientInitialized && !base.IsHost)
		{
			RpcLogic___EnterBuilding_3905681115(null, buildingGUID, doorIndex);
		}
	}

	private void RpcWriter___Target_EnterBuilding_3905681115(NetworkConnection connection, string buildingGUID, int doorIndex)
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
			writer.WriteString(buildingGUID);
			writer.WriteInt32(doorIndex);
			SendTargetRpc(14u, writer, channel, DataOrderType.Default, connection, excludeServer: false);
			writer.Store();
		}
	}

	private void RpcReader___Target_EnterBuilding_3905681115(PooledReader PooledReader0, Channel channel)
	{
		string buildingGUID = PooledReader0.ReadString();
		int doorIndex = PooledReader0.ReadInt32();
		if (base.IsClientInitialized)
		{
			RpcLogic___EnterBuilding_3905681115(base.LocalConnection, buildingGUID, doorIndex);
		}
	}

	private void RpcWriter___Observers_ExitBuilding_3615296227(string buildingID = "")
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
			writer.WriteString(buildingID);
			SendObserversRpc(15u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
			writer.Store();
		}
	}

	public void RpcLogic___ExitBuilding_3615296227(string buildingID = "")
	{
		if (buildingID == "" && CurrentBuilding != null)
		{
			buildingID = CurrentBuilding.GUID.ToString();
		}
		if (!(buildingID == ""))
		{
			ExitBuilding(GUIDManager.GetObject<NPCEnterableBuilding>(new Guid(buildingID)));
		}
	}

	private void RpcReader___Observers_ExitBuilding_3615296227(PooledReader PooledReader0, Channel channel)
	{
		string buildingID = PooledReader0.ReadString();
		if (base.IsClientInitialized && !base.IsHost)
		{
			RpcLogic___ExitBuilding_3615296227(buildingID);
		}
	}

	private void RpcWriter___Observers_SetEquippable_Networked_2971853958(NetworkConnection conn, string assetPath)
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
			writer.WriteString(assetPath);
			SendObserversRpc(16u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
			writer.Store();
		}
	}

	public void RpcLogic___SetEquippable_Networked_2971853958(NetworkConnection conn, string assetPath)
	{
		Avatar.SetEquippable(assetPath);
	}

	private void RpcReader___Observers_SetEquippable_Networked_2971853958(PooledReader PooledReader0, Channel channel)
	{
		string assetPath = PooledReader0.ReadString();
		if (base.IsClientInitialized && !base.IsHost)
		{
			RpcLogic___SetEquippable_Networked_2971853958(null, assetPath);
		}
	}

	private void RpcWriter___Target_SetEquippable_Networked_2971853958(NetworkConnection conn, string assetPath)
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
			writer.WriteString(assetPath);
			SendTargetRpc(17u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
			writer.Store();
		}
	}

	private void RpcReader___Target_SetEquippable_Networked_2971853958(PooledReader PooledReader0, Channel channel)
	{
		string assetPath = PooledReader0.ReadString();
		if (base.IsClientInitialized)
		{
			RpcLogic___SetEquippable_Networked_2971853958(base.LocalConnection, assetPath);
		}
	}

	private void RpcWriter___Observers_SetEquippable_Networked_ExcludeServer_2971853958(NetworkConnection conn, string assetPath)
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
			writer.WriteNetworkConnection(conn);
			writer.WriteString(assetPath);
			SendObserversRpc(18u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: true, excludeOwner: false);
			writer.Store();
		}
	}

	private void RpcLogic___SetEquippable_Networked_ExcludeServer_2971853958(NetworkConnection conn, string assetPath)
	{
		Avatar.SetEquippable(assetPath);
	}

	private void RpcReader___Observers_SetEquippable_Networked_ExcludeServer_2971853958(PooledReader PooledReader0, Channel channel)
	{
		NetworkConnection conn = PooledReader0.ReadNetworkConnection();
		string assetPath = PooledReader0.ReadString();
		if (base.IsClientInitialized)
		{
			RpcLogic___SetEquippable_Networked_ExcludeServer_2971853958(conn, assetPath);
		}
	}

	private void RpcWriter___Observers_SendEquippableMessage_Networked_2971853958(NetworkConnection conn, string message)
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
			writer.WriteString(message);
			SendObserversRpc(19u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
			writer.Store();
		}
	}

	public void RpcLogic___SendEquippableMessage_Networked_2971853958(NetworkConnection conn, string message)
	{
		Avatar.ReceiveEquippableMessage(message, null);
	}

	private void RpcReader___Observers_SendEquippableMessage_Networked_2971853958(PooledReader PooledReader0, Channel channel)
	{
		string message = PooledReader0.ReadString();
		if (base.IsClientInitialized && !base.IsHost)
		{
			RpcLogic___SendEquippableMessage_Networked_2971853958(null, message);
		}
	}

	private void RpcWriter___Target_SendEquippableMessage_Networked_2971853958(NetworkConnection conn, string message)
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
			writer.WriteString(message);
			SendTargetRpc(20u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
			writer.Store();
		}
	}

	private void RpcReader___Target_SendEquippableMessage_Networked_2971853958(PooledReader PooledReader0, Channel channel)
	{
		string message = PooledReader0.ReadString();
		if (base.IsClientInitialized)
		{
			RpcLogic___SendEquippableMessage_Networked_2971853958(base.LocalConnection, message);
		}
	}

	private void RpcWriter___Observers_SendEquippableMessage_Networked_Vector_4022222929(NetworkConnection conn, string message, Vector3 data)
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
			writer.WriteString(message);
			writer.WriteVector3(data);
			SendObserversRpc(21u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
			writer.Store();
		}
	}

	public void RpcLogic___SendEquippableMessage_Networked_Vector_4022222929(NetworkConnection conn, string message, Vector3 data)
	{
		Avatar.ReceiveEquippableMessage(message, data);
	}

	private void RpcReader___Observers_SendEquippableMessage_Networked_Vector_4022222929(PooledReader PooledReader0, Channel channel)
	{
		string message = PooledReader0.ReadString();
		Vector3 data = PooledReader0.ReadVector3();
		if (base.IsClientInitialized && !base.IsHost)
		{
			RpcLogic___SendEquippableMessage_Networked_Vector_4022222929(null, message, data);
		}
	}

	private void RpcWriter___Target_SendEquippableMessage_Networked_Vector_4022222929(NetworkConnection conn, string message, Vector3 data)
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
			writer.WriteString(message);
			writer.WriteVector3(data);
			SendTargetRpc(22u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
			writer.Store();
		}
	}

	private void RpcReader___Target_SendEquippableMessage_Networked_Vector_4022222929(PooledReader PooledReader0, Channel channel)
	{
		string message = PooledReader0.ReadString();
		Vector3 data = PooledReader0.ReadVector3();
		if (base.IsClientInitialized)
		{
			RpcLogic___SendEquippableMessage_Networked_Vector_4022222929(base.LocalConnection, message, data);
		}
	}

	private void RpcWriter___Server_SendAnimationTrigger_3615296227(string trigger)
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
			writer.WriteString(trigger);
			SendServerRpc(23u, writer, channel, DataOrderType.Default);
			writer.Store();
		}
	}

	public void RpcLogic___SendAnimationTrigger_3615296227(string trigger)
	{
		SetAnimationTrigger_Networked(null, trigger);
	}

	private void RpcReader___Server_SendAnimationTrigger_3615296227(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		string trigger = PooledReader0.ReadString();
		if (base.IsServerInitialized)
		{
			RpcLogic___SendAnimationTrigger_3615296227(trigger);
		}
	}

	private void RpcWriter___Observers_SetAnimationTrigger_Networked_2971853958(NetworkConnection conn, string trigger)
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
			writer.WriteString(trigger);
			SendObserversRpc(24u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
			writer.Store();
		}
	}

	public void RpcLogic___SetAnimationTrigger_Networked_2971853958(NetworkConnection conn, string trigger)
	{
		SetAnimationTrigger(trigger);
	}

	private void RpcReader___Observers_SetAnimationTrigger_Networked_2971853958(PooledReader PooledReader0, Channel channel)
	{
		string trigger = PooledReader0.ReadString();
		if (base.IsClientInitialized && !base.IsHost)
		{
			RpcLogic___SetAnimationTrigger_Networked_2971853958(null, trigger);
		}
	}

	private void RpcWriter___Target_SetAnimationTrigger_Networked_2971853958(NetworkConnection conn, string trigger)
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
			writer.WriteString(trigger);
			SendTargetRpc(25u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
			writer.Store();
		}
	}

	private void RpcReader___Target_SetAnimationTrigger_Networked_2971853958(PooledReader PooledReader0, Channel channel)
	{
		string trigger = PooledReader0.ReadString();
		if (base.IsClientInitialized)
		{
			RpcLogic___SetAnimationTrigger_Networked_2971853958(base.LocalConnection, trigger);
		}
	}

	private void RpcWriter___Observers_ResetAnimationTrigger_Networked_2971853958(NetworkConnection conn, string trigger)
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
			writer.WriteString(trigger);
			SendObserversRpc(26u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
			writer.Store();
		}
	}

	public void RpcLogic___ResetAnimationTrigger_Networked_2971853958(NetworkConnection conn, string trigger)
	{
		ResetAnimationTrigger(trigger);
	}

	private void RpcReader___Observers_ResetAnimationTrigger_Networked_2971853958(PooledReader PooledReader0, Channel channel)
	{
		string trigger = PooledReader0.ReadString();
		if (base.IsClientInitialized && !base.IsHost)
		{
			RpcLogic___ResetAnimationTrigger_Networked_2971853958(null, trigger);
		}
	}

	private void RpcWriter___Target_ResetAnimationTrigger_Networked_2971853958(NetworkConnection conn, string trigger)
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
			writer.WriteString(trigger);
			SendTargetRpc(27u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
			writer.Store();
		}
	}

	private void RpcReader___Target_ResetAnimationTrigger_Networked_2971853958(PooledReader PooledReader0, Channel channel)
	{
		string trigger = PooledReader0.ReadString();
		if (base.IsClientInitialized)
		{
			RpcLogic___ResetAnimationTrigger_Networked_2971853958(base.LocalConnection, trigger);
		}
	}

	private void RpcWriter___Observers_SetCrouched_Networked_1140765316(bool crouched)
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
			writer.WriteBoolean(crouched);
			SendObserversRpc(28u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
			writer.Store();
		}
	}

	public void RpcLogic___SetCrouched_Networked_1140765316(bool crouched)
	{
		Avatar.Animation.SetCrouched(crouched);
	}

	private void RpcReader___Observers_SetCrouched_Networked_1140765316(PooledReader PooledReader0, Channel channel)
	{
		bool crouched = PooledReader0.ReadBoolean();
		if (base.IsClientInitialized && !base.IsHost)
		{
			RpcLogic___SetCrouched_Networked_1140765316(crouched);
		}
	}

	private void RpcWriter___Observers_SetAnimationBool_Networked_619441887(NetworkConnection conn, string id, bool value)
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
			writer.WriteString(id);
			writer.WriteBoolean(value);
			SendObserversRpc(29u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
			writer.Store();
		}
	}

	public void RpcLogic___SetAnimationBool_Networked_619441887(NetworkConnection conn, string id, bool value)
	{
		Avatar.Animation.SetBool(id, value);
	}

	private void RpcReader___Observers_SetAnimationBool_Networked_619441887(PooledReader PooledReader0, Channel channel)
	{
		string id = PooledReader0.ReadString();
		bool value = PooledReader0.ReadBoolean();
		if (base.IsClientInitialized && !base.IsHost)
		{
			RpcLogic___SetAnimationBool_Networked_619441887(null, id, value);
		}
	}

	private void RpcWriter___Target_SetAnimationBool_Networked_619441887(NetworkConnection conn, string id, bool value)
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
			writer.WriteString(id);
			writer.WriteBoolean(value);
			SendTargetRpc(30u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
			writer.Store();
		}
	}

	private void RpcReader___Target_SetAnimationBool_Networked_619441887(PooledReader PooledReader0, Channel channel)
	{
		string id = PooledReader0.ReadString();
		bool value = PooledReader0.ReadBoolean();
		if (base.IsClientInitialized)
		{
			RpcLogic___SetAnimationBool_Networked_619441887(base.LocalConnection, id, value);
		}
	}

	private void RpcWriter___Server_SetPanicked_2166136261()
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
			SendServerRpc(31u, writer, channel, DataOrderType.Default);
			writer.Store();
		}
	}

	public void RpcLogic___SetPanicked_2166136261()
	{
		float timeSincePanicked = TimeSincePanicked;
		TimeSincePanicked = 0f;
		if (timeSincePanicked > 20f)
		{
			ReceivePanicked();
		}
	}

	private void RpcReader___Server_SetPanicked_2166136261(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		if (base.IsServerInitialized)
		{
			RpcLogic___SetPanicked_2166136261();
		}
	}

	private void RpcWriter___Observers_ReceivePanicked_2166136261()
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
			SendObserversRpc(32u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
			writer.Store();
		}
	}

	private void RpcLogic___ReceivePanicked_2166136261()
	{
		Avatar.EmotionManager.AddEmotionOverride("Scared", "panicked", 0f, 10);
		if (CurrentVehicle != null)
		{
			CurrentVehicle.Agent.Flags.OverriddenSpeed = 50f;
			CurrentVehicle.Agent.Flags.OverriddenReverseSpeed = 20f;
			CurrentVehicle.Agent.Flags.OverrideSpeed = true;
			CurrentVehicle.Agent.Flags.IgnoreTrafficLights = true;
			CurrentVehicle.Agent.Flags.ObstacleMode = DriveFlags.EObstacleMode.IgnoreOnlySquishy;
		}
		else
		{
			Behaviour.CoweringBehaviour.Enable();
		}
	}

	private void RpcReader___Observers_ReceivePanicked_2166136261(PooledReader PooledReader0, Channel channel)
	{
		if (base.IsClientInitialized)
		{
			RpcLogic___ReceivePanicked_2166136261();
		}
	}

	private void RpcWriter___Observers_RemovePanicked_2166136261()
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
			SendObserversRpc(33u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
			writer.Store();
		}
	}

	private void RpcLogic___RemovePanicked_2166136261()
	{
		Avatar.EmotionManager.RemoveEmotionOverride("panicked");
		if (CurrentVehicle != null)
		{
			CurrentVehicle.Agent.Flags.ResetFlags();
		}
		Behaviour.CoweringBehaviour.Disable();
	}

	private void RpcReader___Observers_RemovePanicked_2166136261(PooledReader PooledReader0, Channel channel)
	{
		if (base.IsClientInitialized)
		{
			RpcLogic___RemovePanicked_2166136261();
		}
	}

	private void RpcWriter___Server_PlayVO_Server_1710085680(EVOLineType lineType)
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
			GeneratedWriters___Internal.Write___ScheduleOne.VoiceOver.EVOLineTypeFishNet.Serializing.Generated(writer, lineType);
			SendServerRpc(34u, writer, channel, DataOrderType.Default);
			writer.Store();
		}
	}

	private void RpcLogic___PlayVO_Server_1710085680(EVOLineType lineType)
	{
		PlayVO_Client(lineType);
	}

	private void RpcReader___Server_PlayVO_Server_1710085680(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		EVOLineType lineType = GeneratedReaders___Internal.Read___ScheduleOne.VoiceOver.EVOLineTypeFishNet.Serializing.Generateds(PooledReader0);
		if (base.IsServerInitialized)
		{
			RpcLogic___PlayVO_Server_1710085680(lineType);
		}
	}

	private void RpcWriter___Observers_PlayVO_Client_1710085680(EVOLineType lineType)
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
			GeneratedWriters___Internal.Write___ScheduleOne.VoiceOver.EVOLineTypeFishNet.Serializing.Generated(writer, lineType);
			SendObserversRpc(35u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
			writer.Store();
		}
	}

	private void RpcLogic___PlayVO_Client_1710085680(EVOLineType lineType)
	{
		PlayVO(lineType);
	}

	private void RpcReader___Observers_PlayVO_Client_1710085680(PooledReader PooledReader0, Channel channel)
	{
		EVOLineType lineType = GeneratedReaders___Internal.Read___ScheduleOne.VoiceOver.EVOLineTypeFishNet.Serializing.Generateds(PooledReader0);
		if (base.IsClientInitialized && !base.IsHost)
		{
			RpcLogic___PlayVO_Client_1710085680(lineType);
		}
	}

	private void RpcWriter___Target_ReceiveRelationshipData_4052192084(NetworkConnection conn, float relationship, bool unlocked)
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
			writer.WriteSingle(relationship);
			writer.WriteBoolean(unlocked);
			SendTargetRpc(36u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
			writer.Store();
		}
	}

	public void RpcLogic___ReceiveRelationshipData_4052192084(NetworkConnection conn, float relationship, bool unlocked)
	{
		RelationData.SetRelationship(relationship);
		Console.Log("Received relationship data for " + fullName + " Unlocked: " + unlocked);
		if (unlocked)
		{
			RelationData.Unlock(NPCRelationData.EUnlockType.DirectApproach, notify: false);
		}
	}

	private void RpcReader___Target_ReceiveRelationshipData_4052192084(PooledReader PooledReader0, Channel channel)
	{
		float relationship = PooledReader0.ReadSingle();
		bool unlocked = PooledReader0.ReadBoolean();
		if (base.IsClientInitialized)
		{
			RpcLogic___ReceiveRelationshipData_4052192084(base.LocalConnection, relationship, unlocked);
		}
	}

	private void RpcWriter___Server_SetIsBeingPickPocketed_1140765316(bool pickpocketed)
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
			writer.WriteBoolean(pickpocketed);
			SendServerRpc(37u, writer, channel, DataOrderType.Default);
			writer.Store();
		}
	}

	public void RpcLogic___SetIsBeingPickPocketed_1140765316(bool pickpocketed)
	{
		if (pickpocketed)
		{
			Behaviour.StationaryBehaviour.Enable_Networked();
		}
		else
		{
			Behaviour.StationaryBehaviour.Disable_Networked(null);
		}
	}

	private void RpcReader___Server_SetIsBeingPickPocketed_1140765316(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		bool pickpocketed = PooledReader0.ReadBoolean();
		if (base.IsServerInitialized)
		{
			RpcLogic___SetIsBeingPickPocketed_1140765316(pickpocketed);
		}
	}

	private void RpcWriter___Server_SendRelationship_431000436(float relationship)
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
			writer.WriteSingle(relationship);
			SendServerRpc(38u, writer, channel, DataOrderType.Default);
			writer.Store();
		}
	}

	public void RpcLogic___SendRelationship_431000436(float relationship)
	{
		SetRelationship(relationship);
	}

	private void RpcReader___Server_SendRelationship_431000436(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		float relationship = PooledReader0.ReadSingle();
		if (base.IsServerInitialized)
		{
			RpcLogic___SendRelationship_431000436(relationship);
		}
	}

	private void RpcWriter___Observers_SetRelationship_431000436(float relationship)
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
			writer.WriteSingle(relationship);
			SendObserversRpc(39u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
			writer.Store();
		}
	}

	private void RpcLogic___SetRelationship_431000436(float relationship)
	{
		RelationData.SetRelationship(relationship);
	}

	private void RpcReader___Observers_SetRelationship_431000436(PooledReader PooledReader0, Channel channel)
	{
		float relationship = PooledReader0.ReadSingle();
		if (base.IsClientInitialized)
		{
			RpcLogic___SetRelationship_431000436(relationship);
		}
	}

	public virtual bool ReadSyncVar___ScheduleOne.NPCs.NPC(PooledReader PooledReader0, uint UInt321, bool Boolean2)
	{
		if (UInt321 == 0)
		{
			if (PooledReader0 == null)
			{
				this.sync___set_value_PlayerConversant(syncVar___PlayerConversant.GetValue(calledByUser: true), asServer: true);
				return true;
			}
			NetworkObject value = PooledReader0.ReadNetworkObject();
			this.sync___set_value_PlayerConversant(value, Boolean2);
			return true;
		}
		return false;
	}

	protected virtual void Awake_UserLogic_ScheduleOne.NPCs.NPC_Assembly-CSharp.dll()
	{
		intObj.onHovered.AddListener(Hovered_Internal);
		intObj.onInteractStart.AddListener(Interacted_Internal);
		CheckAndGetReferences();
		Player.onLocalPlayerSpawned = (Action)Delegate.Remove(Player.onLocalPlayerSpawned, new Action(PlayerSpawned));
		Player.onLocalPlayerSpawned = (Action)Delegate.Combine(Player.onLocalPlayerSpawned, new Action(PlayerSpawned));
		if (!NPCManager.NPCRegistry.Contains(this))
		{
			NPCManager.NPCRegistry.Add(this);
		}
		Awareness.onNoticedGeneralCrime.AddListener(SetUnsettled_30s);
		Awareness.onNoticedPettyCrime.AddListener(SetUnsettled_30s);
		Health.onDie.AddListener(OnDie);
		Health.onKnockedOut.AddListener(OnKnockedOut);
		SkinnedMeshRenderer[] bodyMeshes = Avatar.BodyMeshes;
		foreach (SkinnedMeshRenderer skinnedMeshRenderer in bodyMeshes)
		{
			OutlineRenderers.Add(skinnedMeshRenderer.gameObject);
		}
		if (VoiceOverEmitter == null)
		{
			VoiceOverEmitter = Avatar.HeadBone.GetComponentInChildren<VOEmitter>();
		}
		RelationData.Init(this);
		if (RelationData.Unlocked)
		{
			Unlocked(NPCRelationData.EUnlockType.DirectApproach, notify: false);
		}
		else
		{
			NPCRelationData relationData = RelationData;
			relationData.onUnlocked = (Action<NPCRelationData.EUnlockType, bool>)Delegate.Combine(relationData.onUnlocked, new Action<NPCRelationData.EUnlockType, bool>(Unlocked));
		}
		foreach (NPC connection in RelationData.Connections)
		{
			if (!(connection == null) && connection == this)
			{
				Console.LogWarning("NPC " + fullName + " has a connection to itself");
			}
		}
		headlightStartTime = 1700 + Mathf.RoundToInt(90f * Mathf.Clamp01((float)(fullName[0].GetHashCode() / 1000 % 10) / 10f));
		InitializeSaveable();
		defaultAggression = Aggression;
		void Unlocked(NPCRelationData.EUnlockType unlockType, bool notify)
		{
			if (NPCUnlockedVariable != string.Empty)
			{
				NetworkSingleton<VariableDatabase>.Instance.SetVariableValue(NPCUnlockedVariable, true.ToString());
			}
		}
	}
}
