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
using ScheduleOne.Combat;
using ScheduleOne.DevUtilities;
using ScheduleOne.Doors;
using ScheduleOne.GameTime;
using ScheduleOne.ItemFramework;
using ScheduleOne.Map;
using ScheduleOne.Networking;
using ScheduleOne.Product;
using UnityEngine;

namespace ScheduleOne.NPCs.Behaviour;

public class NPCBehaviour : NetworkBehaviour
{
	public bool DEBUG_MODE;

	[Header("References")]
	public NPCScheduleManager ScheduleManager;

	[Header("Default Behaviours")]
	public CoweringBehaviour CoweringBehaviour;

	public RagdollBehaviour RagdollBehaviour;

	public CallPoliceBehaviour CallPoliceBehaviour;

	public GenericDialogueBehaviour GenericDialogueBehaviour;

	public HeavyFlinchBehaviour HeavyFlinchBehaviour;

	public FaceTargetBehaviour FaceTargetBehaviour;

	public DeadBehaviour DeadBehaviour;

	public UnconsciousBehaviour UnconsciousBehaviour;

	public Behaviour SummonBehaviour;

	public ConsumeProductBehaviour ConsumeProductBehaviour;

	public CombatBehaviour CombatBehaviour;

	public FleeBehaviour FleeBehaviour;

	public StationaryBehaviour StationaryBehaviour;

	public RequestProductBehaviour RequestProductBehaviour;

	[SerializeField]
	protected List<Behaviour> behaviourStack = new List<Behaviour>();

	private Coroutine summonRoutine;

	[SerializeField]
	private List<Behaviour> enabledBehaviours = new List<Behaviour>();

	private bool NetworkInitialize___EarlyScheduleOne.NPCs.Behaviour.NPCBehaviourAssembly-CSharp.dll_Excuted;

	private bool NetworkInitialize__LateScheduleOne.NPCs.Behaviour.NPCBehaviourAssembly-CSharp.dll_Excuted;

	public Behaviour activeBehaviour { get; set; }

	public NPC Npc { get; private set; }

	public virtual void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne.NPCs.Behaviour.NPCBehaviour_Assembly-CSharp.dll();
		NetworkInitialize__Late();
	}

	protected virtual void Start()
	{
		Npc.Avatar.Animation.onHeavyFlinch.AddListener(HeavyFlinchBehaviour.Flinch);
		NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance.onTick -= new Action(OnTick);
		NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance.onTick += new Action(OnTick);
		for (int i = 0; i < behaviourStack.Count; i++)
		{
			Behaviour b = behaviourStack[i];
			if (b.Enabled)
			{
				enabledBehaviours.Add(b);
			}
			b.onEnable.AddListener(delegate
			{
				AddEnabledBehaviour(b);
			});
			b.onDisable.AddListener(delegate
			{
				RemoveEnabledBehaviour(b);
			});
		}
	}

	private void OnDestroy()
	{
		if (NetworkSingleton<ScheduleOne.GameTime.TimeManager>.InstanceExists)
		{
			NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance.onTick -= new Action(OnTick);
		}
	}

	protected override void OnValidate()
	{
		base.OnValidate();
		behaviourStack = GetComponentsInChildren<Behaviour>().ToList();
		SortBehaviourStack();
	}

	public override void OnSpawnServer(NetworkConnection connection)
	{
		base.OnSpawnServer(connection);
		if (!connection.IsHost)
		{
			ReplicationQueue.Enqueue(GetType().Name, connection, Replicate, 512);
		}
		void Replicate(NetworkConnection conn)
		{
			for (int i = 0; i < behaviourStack.Count; i++)
			{
				if (behaviourStack[i].Enabled)
				{
					EnableBehaviour_Client(conn, i);
				}
			}
			if (activeBehaviour != null)
			{
				activeBehaviour.Activate_Server(conn);
			}
		}
	}

	[ServerRpc(RequireOwnership = false)]
	public void Summon(string buildingGUID, int doorIndex, float duration)
	{
		RpcWriter___Server_Summon_900355577(buildingGUID, doorIndex, duration);
	}

	[ServerRpc(RequireOwnership = false)]
	public void ConsumeProduct(ProductItemInstance product)
	{
		RpcWriter___Server_ConsumeProduct_2622925554(product);
	}

	private void OnKnockOut()
	{
		foreach (Behaviour item in behaviourStack)
		{
			if (!(item == DeadBehaviour) && !(item == UnconsciousBehaviour))
			{
				item.Disable_Networked(null);
				if (item.Active)
				{
					item.Deactivate_Networked(null);
				}
			}
		}
	}

	private void OnRevive()
	{
		if (!InstanceFinder.IsServer)
		{
			return;
		}
		foreach (Behaviour item in behaviourStack)
		{
			if (item.EnabledOnAwake)
			{
				item.Enable_Server();
			}
		}
	}

	protected virtual void OnDie()
	{
		OnKnockOut();
		UnconsciousBehaviour.Disable_Networked(null);
	}

	public Behaviour GetBehaviour(string BehaviourName)
	{
		Behaviour behaviour = behaviourStack.Find((Behaviour x) => x.Name.ToLower() == BehaviourName.ToLower());
		if (behaviour == null)
		{
			Console.LogWarning("No behaviour found with name '" + BehaviourName + "'");
		}
		return behaviour;
	}

	public T GetBehaviour<T>() where T : Behaviour
	{
		return behaviourStack.FirstOrDefault((Behaviour x) => x is T) as T;
	}

	public virtual void Update()
	{
		if (DEBUG_MODE)
		{
			Debug.Log("Enabled behaviours: " + string.Join(", ", enabledBehaviours.Select((Behaviour x) => x.Name).ToArray()));
			if (activeBehaviour != null)
			{
				Debug.Log("Active behaviour: " + activeBehaviour.Name);
			}
		}
		if (InstanceFinder.IsHost)
		{
			Behaviour enabledBehaviour = GetEnabledBehaviour();
			if (enabledBehaviour != activeBehaviour)
			{
				if (activeBehaviour != null)
				{
					activeBehaviour.Pause_Server();
				}
				if (enabledBehaviour != null)
				{
					if (enabledBehaviour.Started)
					{
						enabledBehaviour.Resume_Server();
					}
					else
					{
						enabledBehaviour.Activate_Server(null);
					}
				}
			}
		}
		if (activeBehaviour != null && activeBehaviour.Active)
		{
			activeBehaviour.BehaviourUpdate();
		}
	}

	public virtual void LateUpdate()
	{
		if (activeBehaviour != null && activeBehaviour.Active)
		{
			activeBehaviour.BehaviourLateUpdate();
		}
	}

	protected virtual void OnTick()
	{
		if (activeBehaviour != null && activeBehaviour.Active)
		{
			activeBehaviour.OnActiveTick();
		}
	}

	public void SortBehaviourStack()
	{
		behaviourStack = behaviourStack.OrderByDescending((Behaviour x) => x.Priority).ToList();
		for (int num = 0; num < behaviourStack.Count; num++)
		{
			behaviourStack[num].BehaviourIndex = num;
		}
	}

	private Behaviour GetEnabledBehaviour()
	{
		return enabledBehaviours.FirstOrDefault();
	}

	private void AddEnabledBehaviour(Behaviour b)
	{
		if (!enabledBehaviours.Contains(b))
		{
			enabledBehaviours.Add(b);
			enabledBehaviours = enabledBehaviours.OrderByDescending((Behaviour x) => x.Priority).ToList();
		}
	}

	private void RemoveEnabledBehaviour(Behaviour b)
	{
		if (enabledBehaviours.Contains(b))
		{
			enabledBehaviours.Remove(b);
			enabledBehaviours = enabledBehaviours.OrderByDescending((Behaviour x) => x.Priority).ToList();
		}
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void EnableBehaviour_Server(int behaviourIndex)
	{
		RpcWriter___Server_EnableBehaviour_Server_3316948804(behaviourIndex);
		RpcLogic___EnableBehaviour_Server_3316948804(behaviourIndex);
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	public void EnableBehaviour_Client(NetworkConnection conn, int behaviourIndex)
	{
		if ((object)conn == null)
		{
			RpcWriter___Observers_EnableBehaviour_Client_2681120339(conn, behaviourIndex);
			RpcLogic___EnableBehaviour_Client_2681120339(conn, behaviourIndex);
		}
		else
		{
			RpcWriter___Target_EnableBehaviour_Client_2681120339(conn, behaviourIndex);
		}
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void DisableBehaviour_Server(int behaviourIndex)
	{
		RpcWriter___Server_DisableBehaviour_Server_3316948804(behaviourIndex);
		RpcLogic___DisableBehaviour_Server_3316948804(behaviourIndex);
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	public void DisableBehaviour_Client(NetworkConnection conn, int behaviourIndex)
	{
		if ((object)conn == null)
		{
			RpcWriter___Observers_DisableBehaviour_Client_2681120339(conn, behaviourIndex);
			RpcLogic___DisableBehaviour_Client_2681120339(conn, behaviourIndex);
		}
		else
		{
			RpcWriter___Target_DisableBehaviour_Client_2681120339(conn, behaviourIndex);
		}
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void ActivateBehaviour_Server(int behaviourIndex)
	{
		RpcWriter___Server_ActivateBehaviour_Server_3316948804(behaviourIndex);
		RpcLogic___ActivateBehaviour_Server_3316948804(behaviourIndex);
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	public void ActivateBehaviour_Client(NetworkConnection conn, int behaviourIndex)
	{
		if ((object)conn == null)
		{
			RpcWriter___Observers_ActivateBehaviour_Client_2681120339(conn, behaviourIndex);
			RpcLogic___ActivateBehaviour_Client_2681120339(conn, behaviourIndex);
		}
		else
		{
			RpcWriter___Target_ActivateBehaviour_Client_2681120339(conn, behaviourIndex);
		}
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void DeactivateBehaviour_Server(int behaviourIndex)
	{
		RpcWriter___Server_DeactivateBehaviour_Server_3316948804(behaviourIndex);
		RpcLogic___DeactivateBehaviour_Server_3316948804(behaviourIndex);
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	public void DeactivateBehaviour_Client(NetworkConnection conn, int behaviourIndex)
	{
		if ((object)conn == null)
		{
			RpcWriter___Observers_DeactivateBehaviour_Client_2681120339(conn, behaviourIndex);
			RpcLogic___DeactivateBehaviour_Client_2681120339(conn, behaviourIndex);
		}
		else
		{
			RpcWriter___Target_DeactivateBehaviour_Client_2681120339(conn, behaviourIndex);
		}
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void PauseBehaviour_Server(int behaviourIndex)
	{
		RpcWriter___Server_PauseBehaviour_Server_3316948804(behaviourIndex);
		RpcLogic___PauseBehaviour_Server_3316948804(behaviourIndex);
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	public void PauseBehaviour_Client(NetworkConnection conn, int behaviourIndex)
	{
		if ((object)conn == null)
		{
			RpcWriter___Observers_PauseBehaviour_Client_2681120339(conn, behaviourIndex);
			RpcLogic___PauseBehaviour_Client_2681120339(conn, behaviourIndex);
		}
		else
		{
			RpcWriter___Target_PauseBehaviour_Client_2681120339(conn, behaviourIndex);
		}
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void ResumeBehaviour_Server(int behaviourIndex)
	{
		RpcWriter___Server_ResumeBehaviour_Server_3316948804(behaviourIndex);
		RpcLogic___ResumeBehaviour_Server_3316948804(behaviourIndex);
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	public void ResumeBehaviour_Client(NetworkConnection conn, int behaviourIndex)
	{
		if ((object)conn == null)
		{
			RpcWriter___Observers_ResumeBehaviour_Client_2681120339(conn, behaviourIndex);
			RpcLogic___ResumeBehaviour_Client_2681120339(conn, behaviourIndex);
		}
		else
		{
			RpcWriter___Target_ResumeBehaviour_Client_2681120339(conn, behaviourIndex);
		}
	}

	public virtual void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyScheduleOne.NPCs.Behaviour.NPCBehaviourAssembly-CSharp.dll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne.NPCs.Behaviour.NPCBehaviourAssembly-CSharp.dll_Excuted = true;
			RegisterServerRpc(0u, RpcReader___Server_Summon_900355577);
			RegisterServerRpc(1u, RpcReader___Server_ConsumeProduct_2622925554);
			RegisterServerRpc(2u, RpcReader___Server_EnableBehaviour_Server_3316948804);
			RegisterObserversRpc(3u, RpcReader___Observers_EnableBehaviour_Client_2681120339);
			RegisterTargetRpc(4u, RpcReader___Target_EnableBehaviour_Client_2681120339);
			RegisterServerRpc(5u, RpcReader___Server_DisableBehaviour_Server_3316948804);
			RegisterObserversRpc(6u, RpcReader___Observers_DisableBehaviour_Client_2681120339);
			RegisterTargetRpc(7u, RpcReader___Target_DisableBehaviour_Client_2681120339);
			RegisterServerRpc(8u, RpcReader___Server_ActivateBehaviour_Server_3316948804);
			RegisterObserversRpc(9u, RpcReader___Observers_ActivateBehaviour_Client_2681120339);
			RegisterTargetRpc(10u, RpcReader___Target_ActivateBehaviour_Client_2681120339);
			RegisterServerRpc(11u, RpcReader___Server_DeactivateBehaviour_Server_3316948804);
			RegisterObserversRpc(12u, RpcReader___Observers_DeactivateBehaviour_Client_2681120339);
			RegisterTargetRpc(13u, RpcReader___Target_DeactivateBehaviour_Client_2681120339);
			RegisterServerRpc(14u, RpcReader___Server_PauseBehaviour_Server_3316948804);
			RegisterObserversRpc(15u, RpcReader___Observers_PauseBehaviour_Client_2681120339);
			RegisterTargetRpc(16u, RpcReader___Target_PauseBehaviour_Client_2681120339);
			RegisterServerRpc(17u, RpcReader___Server_ResumeBehaviour_Server_3316948804);
			RegisterObserversRpc(18u, RpcReader___Observers_ResumeBehaviour_Client_2681120339);
			RegisterTargetRpc(19u, RpcReader___Target_ResumeBehaviour_Client_2681120339);
		}
	}

	public virtual void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne.NPCs.Behaviour.NPCBehaviourAssembly-CSharp.dll_Excuted)
		{
			NetworkInitialize__LateScheduleOne.NPCs.Behaviour.NPCBehaviourAssembly-CSharp.dll_Excuted = true;
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Server_Summon_900355577(string buildingGUID, int doorIndex, float duration)
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
			writer.WriteString(buildingGUID);
			writer.WriteInt32(doorIndex);
			writer.WriteSingle(duration);
			SendServerRpc(0u, writer, channel, DataOrderType.Default);
			writer.Store();
		}
	}

	public void RpcLogic___Summon_900355577(string buildingGUID, int doorIndex, float duration)
	{
		NPCEnterableBuilding nPCEnterableBuilding = GUIDManager.GetObject<NPCEnterableBuilding>(new Guid(buildingGUID));
		if (nPCEnterableBuilding == null)
		{
			Console.LogError("Failed to find building with GUID: " + buildingGUID);
			return;
		}
		if (doorIndex >= nPCEnterableBuilding.Doors.Length || doorIndex < 0)
		{
			Console.LogError("Door index out of range: " + doorIndex + " / " + nPCEnterableBuilding.Doors.Length);
			return;
		}
		StaticDoor lastEnteredDoor = nPCEnterableBuilding.Doors[doorIndex];
		Npc.LastEnteredDoor = lastEnteredDoor;
		SummonBehaviour.Enable_Networked();
		if (summonRoutine != null)
		{
			StopCoroutine(summonRoutine);
		}
		summonRoutine = Singleton<CoroutineService>.Instance.StartCoroutine(Routine());
		IEnumerator Routine()
		{
			float t = 0f;
			while (Npc.IsConscious)
			{
				if (SummonBehaviour.Active)
				{
					t += Time.deltaTime;
					if (t >= duration)
					{
						break;
					}
				}
				yield return new WaitForEndOfFrame();
			}
			SummonBehaviour.Disable_Networked(null);
		}
	}

	private void RpcReader___Server_Summon_900355577(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		string buildingGUID = PooledReader0.ReadString();
		int doorIndex = PooledReader0.ReadInt32();
		float duration = PooledReader0.ReadSingle();
		if (base.IsServerInitialized)
		{
			RpcLogic___Summon_900355577(buildingGUID, doorIndex, duration);
		}
	}

	private void RpcWriter___Server_ConsumeProduct_2622925554(ProductItemInstance product)
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
			writer.WriteProductItemInstance(product);
			SendServerRpc(1u, writer, channel, DataOrderType.Default);
			writer.Store();
		}
	}

	public void RpcLogic___ConsumeProduct_2622925554(ProductItemInstance product)
	{
		ConsumeProductBehaviour.SendProduct(product);
		ConsumeProductBehaviour.Enable_Networked();
	}

	private void RpcReader___Server_ConsumeProduct_2622925554(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		ProductItemInstance product = PooledReader0.ReadProductItemInstance();
		if (base.IsServerInitialized)
		{
			RpcLogic___ConsumeProduct_2622925554(product);
		}
	}

	private void RpcWriter___Server_EnableBehaviour_Server_3316948804(int behaviourIndex)
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
			writer.WriteInt32(behaviourIndex);
			SendServerRpc(2u, writer, channel, DataOrderType.Default);
			writer.Store();
		}
	}

	public void RpcLogic___EnableBehaviour_Server_3316948804(int behaviourIndex)
	{
		EnableBehaviour_Client(null, behaviourIndex);
	}

	private void RpcReader___Server_EnableBehaviour_Server_3316948804(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		int behaviourIndex = PooledReader0.ReadInt32();
		if (base.IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___EnableBehaviour_Server_3316948804(behaviourIndex);
		}
	}

	private void RpcWriter___Observers_EnableBehaviour_Client_2681120339(NetworkConnection conn, int behaviourIndex)
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
			writer.WriteInt32(behaviourIndex);
			SendObserversRpc(3u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
			writer.Store();
		}
	}

	public void RpcLogic___EnableBehaviour_Client_2681120339(NetworkConnection conn, int behaviourIndex)
	{
		behaviourStack[behaviourIndex].Enable();
	}

	private void RpcReader___Observers_EnableBehaviour_Client_2681120339(PooledReader PooledReader0, Channel channel)
	{
		int behaviourIndex = PooledReader0.ReadInt32();
		if (base.IsClientInitialized && !base.IsHost)
		{
			RpcLogic___EnableBehaviour_Client_2681120339(null, behaviourIndex);
		}
	}

	private void RpcWriter___Target_EnableBehaviour_Client_2681120339(NetworkConnection conn, int behaviourIndex)
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
			writer.WriteInt32(behaviourIndex);
			SendTargetRpc(4u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
			writer.Store();
		}
	}

	private void RpcReader___Target_EnableBehaviour_Client_2681120339(PooledReader PooledReader0, Channel channel)
	{
		int behaviourIndex = PooledReader0.ReadInt32();
		if (base.IsClientInitialized)
		{
			RpcLogic___EnableBehaviour_Client_2681120339(base.LocalConnection, behaviourIndex);
		}
	}

	private void RpcWriter___Server_DisableBehaviour_Server_3316948804(int behaviourIndex)
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
			writer.WriteInt32(behaviourIndex);
			SendServerRpc(5u, writer, channel, DataOrderType.Default);
			writer.Store();
		}
	}

	public void RpcLogic___DisableBehaviour_Server_3316948804(int behaviourIndex)
	{
		DisableBehaviour_Client(null, behaviourIndex);
	}

	private void RpcReader___Server_DisableBehaviour_Server_3316948804(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		int behaviourIndex = PooledReader0.ReadInt32();
		if (base.IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___DisableBehaviour_Server_3316948804(behaviourIndex);
		}
	}

	private void RpcWriter___Observers_DisableBehaviour_Client_2681120339(NetworkConnection conn, int behaviourIndex)
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
			writer.WriteInt32(behaviourIndex);
			SendObserversRpc(6u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
			writer.Store();
		}
	}

	public void RpcLogic___DisableBehaviour_Client_2681120339(NetworkConnection conn, int behaviourIndex)
	{
		behaviourStack[behaviourIndex].Disable();
	}

	private void RpcReader___Observers_DisableBehaviour_Client_2681120339(PooledReader PooledReader0, Channel channel)
	{
		int behaviourIndex = PooledReader0.ReadInt32();
		if (base.IsClientInitialized && !base.IsHost)
		{
			RpcLogic___DisableBehaviour_Client_2681120339(null, behaviourIndex);
		}
	}

	private void RpcWriter___Target_DisableBehaviour_Client_2681120339(NetworkConnection conn, int behaviourIndex)
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
			writer.WriteInt32(behaviourIndex);
			SendTargetRpc(7u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
			writer.Store();
		}
	}

	private void RpcReader___Target_DisableBehaviour_Client_2681120339(PooledReader PooledReader0, Channel channel)
	{
		int behaviourIndex = PooledReader0.ReadInt32();
		if (base.IsClientInitialized)
		{
			RpcLogic___DisableBehaviour_Client_2681120339(base.LocalConnection, behaviourIndex);
		}
	}

	private void RpcWriter___Server_ActivateBehaviour_Server_3316948804(int behaviourIndex)
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
			writer.WriteInt32(behaviourIndex);
			SendServerRpc(8u, writer, channel, DataOrderType.Default);
			writer.Store();
		}
	}

	public void RpcLogic___ActivateBehaviour_Server_3316948804(int behaviourIndex)
	{
		ActivateBehaviour_Client(null, behaviourIndex);
	}

	private void RpcReader___Server_ActivateBehaviour_Server_3316948804(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		int behaviourIndex = PooledReader0.ReadInt32();
		if (base.IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___ActivateBehaviour_Server_3316948804(behaviourIndex);
		}
	}

	private void RpcWriter___Observers_ActivateBehaviour_Client_2681120339(NetworkConnection conn, int behaviourIndex)
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
			writer.WriteInt32(behaviourIndex);
			SendObserversRpc(9u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
			writer.Store();
		}
	}

	public void RpcLogic___ActivateBehaviour_Client_2681120339(NetworkConnection conn, int behaviourIndex)
	{
		behaviourStack[behaviourIndex].Activate();
	}

	private void RpcReader___Observers_ActivateBehaviour_Client_2681120339(PooledReader PooledReader0, Channel channel)
	{
		int behaviourIndex = PooledReader0.ReadInt32();
		if (base.IsClientInitialized && !base.IsHost)
		{
			RpcLogic___ActivateBehaviour_Client_2681120339(null, behaviourIndex);
		}
	}

	private void RpcWriter___Target_ActivateBehaviour_Client_2681120339(NetworkConnection conn, int behaviourIndex)
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
			writer.WriteInt32(behaviourIndex);
			SendTargetRpc(10u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
			writer.Store();
		}
	}

	private void RpcReader___Target_ActivateBehaviour_Client_2681120339(PooledReader PooledReader0, Channel channel)
	{
		int behaviourIndex = PooledReader0.ReadInt32();
		if (base.IsClientInitialized)
		{
			RpcLogic___ActivateBehaviour_Client_2681120339(base.LocalConnection, behaviourIndex);
		}
	}

	private void RpcWriter___Server_DeactivateBehaviour_Server_3316948804(int behaviourIndex)
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
			writer.WriteInt32(behaviourIndex);
			SendServerRpc(11u, writer, channel, DataOrderType.Default);
			writer.Store();
		}
	}

	public void RpcLogic___DeactivateBehaviour_Server_3316948804(int behaviourIndex)
	{
		DeactivateBehaviour_Client(null, behaviourIndex);
	}

	private void RpcReader___Server_DeactivateBehaviour_Server_3316948804(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		int behaviourIndex = PooledReader0.ReadInt32();
		if (base.IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___DeactivateBehaviour_Server_3316948804(behaviourIndex);
		}
	}

	private void RpcWriter___Observers_DeactivateBehaviour_Client_2681120339(NetworkConnection conn, int behaviourIndex)
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
			writer.WriteInt32(behaviourIndex);
			SendObserversRpc(12u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
			writer.Store();
		}
	}

	public void RpcLogic___DeactivateBehaviour_Client_2681120339(NetworkConnection conn, int behaviourIndex)
	{
		behaviourStack[behaviourIndex].Deactivate();
	}

	private void RpcReader___Observers_DeactivateBehaviour_Client_2681120339(PooledReader PooledReader0, Channel channel)
	{
		int behaviourIndex = PooledReader0.ReadInt32();
		if (base.IsClientInitialized && !base.IsHost)
		{
			RpcLogic___DeactivateBehaviour_Client_2681120339(null, behaviourIndex);
		}
	}

	private void RpcWriter___Target_DeactivateBehaviour_Client_2681120339(NetworkConnection conn, int behaviourIndex)
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
			writer.WriteInt32(behaviourIndex);
			SendTargetRpc(13u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
			writer.Store();
		}
	}

	private void RpcReader___Target_DeactivateBehaviour_Client_2681120339(PooledReader PooledReader0, Channel channel)
	{
		int behaviourIndex = PooledReader0.ReadInt32();
		if (base.IsClientInitialized)
		{
			RpcLogic___DeactivateBehaviour_Client_2681120339(base.LocalConnection, behaviourIndex);
		}
	}

	private void RpcWriter___Server_PauseBehaviour_Server_3316948804(int behaviourIndex)
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
			writer.WriteInt32(behaviourIndex);
			SendServerRpc(14u, writer, channel, DataOrderType.Default);
			writer.Store();
		}
	}

	public void RpcLogic___PauseBehaviour_Server_3316948804(int behaviourIndex)
	{
		PauseBehaviour_Client(null, behaviourIndex);
	}

	private void RpcReader___Server_PauseBehaviour_Server_3316948804(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		int behaviourIndex = PooledReader0.ReadInt32();
		if (base.IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___PauseBehaviour_Server_3316948804(behaviourIndex);
		}
	}

	private void RpcWriter___Observers_PauseBehaviour_Client_2681120339(NetworkConnection conn, int behaviourIndex)
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
			writer.WriteInt32(behaviourIndex);
			SendObserversRpc(15u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
			writer.Store();
		}
	}

	public void RpcLogic___PauseBehaviour_Client_2681120339(NetworkConnection conn, int behaviourIndex)
	{
		behaviourStack[behaviourIndex].Pause();
	}

	private void RpcReader___Observers_PauseBehaviour_Client_2681120339(PooledReader PooledReader0, Channel channel)
	{
		int behaviourIndex = PooledReader0.ReadInt32();
		if (base.IsClientInitialized && !base.IsHost)
		{
			RpcLogic___PauseBehaviour_Client_2681120339(null, behaviourIndex);
		}
	}

	private void RpcWriter___Target_PauseBehaviour_Client_2681120339(NetworkConnection conn, int behaviourIndex)
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
			writer.WriteInt32(behaviourIndex);
			SendTargetRpc(16u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
			writer.Store();
		}
	}

	private void RpcReader___Target_PauseBehaviour_Client_2681120339(PooledReader PooledReader0, Channel channel)
	{
		int behaviourIndex = PooledReader0.ReadInt32();
		if (base.IsClientInitialized)
		{
			RpcLogic___PauseBehaviour_Client_2681120339(base.LocalConnection, behaviourIndex);
		}
	}

	private void RpcWriter___Server_ResumeBehaviour_Server_3316948804(int behaviourIndex)
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
			writer.WriteInt32(behaviourIndex);
			SendServerRpc(17u, writer, channel, DataOrderType.Default);
			writer.Store();
		}
	}

	public void RpcLogic___ResumeBehaviour_Server_3316948804(int behaviourIndex)
	{
		ResumeBehaviour_Client(null, behaviourIndex);
	}

	private void RpcReader___Server_ResumeBehaviour_Server_3316948804(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		int behaviourIndex = PooledReader0.ReadInt32();
		if (base.IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___ResumeBehaviour_Server_3316948804(behaviourIndex);
		}
	}

	private void RpcWriter___Observers_ResumeBehaviour_Client_2681120339(NetworkConnection conn, int behaviourIndex)
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
			writer.WriteInt32(behaviourIndex);
			SendObserversRpc(18u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
			writer.Store();
		}
	}

	public void RpcLogic___ResumeBehaviour_Client_2681120339(NetworkConnection conn, int behaviourIndex)
	{
		behaviourStack[behaviourIndex].Resume();
	}

	private void RpcReader___Observers_ResumeBehaviour_Client_2681120339(PooledReader PooledReader0, Channel channel)
	{
		int behaviourIndex = PooledReader0.ReadInt32();
		if (base.IsClientInitialized && !base.IsHost)
		{
			RpcLogic___ResumeBehaviour_Client_2681120339(null, behaviourIndex);
		}
	}

	private void RpcWriter___Target_ResumeBehaviour_Client_2681120339(NetworkConnection conn, int behaviourIndex)
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
			writer.WriteInt32(behaviourIndex);
			SendTargetRpc(19u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
			writer.Store();
		}
	}

	private void RpcReader___Target_ResumeBehaviour_Client_2681120339(PooledReader PooledReader0, Channel channel)
	{
		int behaviourIndex = PooledReader0.ReadInt32();
		if (base.IsClientInitialized)
		{
			RpcLogic___ResumeBehaviour_Client_2681120339(base.LocalConnection, behaviourIndex);
		}
	}

	protected virtual void Awake_UserLogic_ScheduleOne.NPCs.Behaviour.NPCBehaviour_Assembly-CSharp.dll()
	{
		Npc = GetComponentInParent<NPC>();
		Npc.Health.onKnockedOut.AddListener(OnKnockOut);
		Npc.Health.onDie.AddListener(OnDie);
		Npc.Health.onRevive.AddListener(OnRevive);
		for (int i = 0; i < behaviourStack.Count; i++)
		{
			behaviourStack[i].BehaviourIndex = i;
		}
	}
}
