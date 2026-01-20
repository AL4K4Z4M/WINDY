using System;
using System.Collections.Generic;
using System.Linq;
using EasyButtons;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Serializing;
using FishNet.Serializing.Generated;
using FishNet.Transporting;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.Interaction;
using ScheduleOne.ItemFramework;
using ScheduleOne.Money;
using ScheduleOne.Networking;
using ScheduleOne.PlayerScripts;
using ScheduleOne.UI;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.NPCs;

public class NPCInventory : NetworkBehaviour, IItemSlotOwner
{
	[Serializable]
	public class RandomInventoryItem
	{
		public StorableItemDefinition ItemDefinition;

		[Range(0f, 10f)]
		public float Weight = 1f;
	}

	public delegate bool ItemFilter(ItemInstance item);

	public InteractableObject PickpocketIntObj;

	public const float COOLDOWN = 30f;

	[Header("Settings")]
	public int SlotCount = 5;

	public bool CanBePickpocketed = true;

	public float PickpocketDifficultyMultiplier = 1f;

	public bool ClearInventoryEachNight = true;

	public ItemDefinition[] TestItems;

	public ItemDefinition[] StartupItems;

	[Header("Random cash")]
	public bool RandomCash = true;

	public int RandomCashMin;

	public int RandomCashMax = 100;

	[Header("Random items")]
	public bool RandomItems = true;

	public bool AllowDuplicateRandomItems = true;

	public RandomInventoryItem[] RandomInventoryItems;

	public int RandomItemMin = -1;

	public int RandomItemMax = 2;

	private NPC npc;

	public UnityEvent onContentsChanged;

	private float timeOnLastExpire = -100f;

	private bool NetworkInitialize___EarlyScheduleOne.NPCs.NPCInventoryAssembly-CSharp.dll_Excuted;

	private bool NetworkInitialize__LateScheduleOne.NPCs.NPCInventoryAssembly-CSharp.dll_Excuted;

	public List<ItemSlot> ItemSlots { get; set; } = new List<ItemSlot>();

	public virtual void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne.NPCs.NPCInventory_Assembly-CSharp.dll();
		NetworkInitialize__Late();
	}

	protected virtual void Start()
	{
		TimeManager instance = NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance;
		instance.onSleepStart = (Action)Delegate.Remove(instance.onSleepStart, new Action(OnSleepStart));
		TimeManager instance2 = NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance;
		instance2.onSleepStart = (Action)Delegate.Combine(instance2.onSleepStart, new Action(OnSleepStart));
	}

	public override void OnSpawnServer(NetworkConnection connection)
	{
		base.OnSpawnServer(connection);
		if (!connection.IsLocalClient)
		{
			ReplicationQueue.Enqueue(GetType().Name, connection, ReplicateInventory, 50 + ((IItemSlotOwner)this).GetNonEmptySlotCount() * 80);
		}
		void ReplicateInventory(NetworkConnection conn)
		{
			((IItemSlotOwner)this).SendItemSlotDataToClient(connection);
		}
	}

	private void OnDestroy()
	{
		if (NetworkSingleton<ScheduleOne.GameTime.TimeManager>.InstanceExists)
		{
			TimeManager instance = NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance;
			instance.onSleepStart = (Action)Delegate.Remove(instance.onSleepStart, new Action(OnSleepStart));
		}
	}

	protected virtual void OnSleepStart()
	{
		if (!InstanceFinder.IsServer)
		{
			return;
		}
		if (ClearInventoryEachNight)
		{
			foreach (ItemSlot itemSlot in ItemSlots)
			{
				itemSlot.ClearStoredInstance();
			}
		}
		if (((IItemSlotOwner)this).GetQuantitySum() < 3)
		{
			if (RandomCash)
			{
				AddRandomCashInstance();
			}
			AddRandomItemsToInventory();
		}
	}

	public void AddRandomItemsToInventory()
	{
		if (!RandomItems)
		{
			return;
		}
		int num = UnityEngine.Random.Range(RandomItemMin, RandomItemMax + 1);
		List<string> list = new List<string>();
		for (int i = 0; i < num; i++)
		{
			StorableItemDefinition randomInventoryItem = GetRandomInventoryItem(list);
			if (!(randomInventoryItem == null))
			{
				if (randomInventoryItem is CashDefinition)
				{
					AddRandomCashInstance();
				}
				else
				{
					ItemInstance defaultInstance = randomInventoryItem.GetDefaultInstance();
					InsertItem(defaultInstance);
				}
				if (!AllowDuplicateRandomItems)
				{
					list.Add(randomInventoryItem.ID);
				}
				continue;
			}
			break;
		}
	}

	private void AddRandomCashInstance()
	{
		int num = UnityEngine.Random.Range(RandomCashMin, RandomCashMax);
		if (num > 0)
		{
			CashInstance cashInstance = NetworkSingleton<MoneyManager>.Instance.GetCashInstance(num);
			InsertItem(cashInstance);
		}
	}

	private StorableItemDefinition GetRandomInventoryItem(List<string> excludeIDs)
	{
		float num = UnityEngine.Random.Range(0f, GetTotalRandomInventoryItemWeight());
		float num2 = 0f;
		for (int i = 0; i < RandomInventoryItems.Length; i++)
		{
			num2 += RandomInventoryItems[i].Weight;
			if (num <= num2 && !excludeIDs.Contains(RandomInventoryItems[i].ItemDefinition.ID))
			{
				return RandomInventoryItems[i].ItemDefinition;
			}
		}
		return null;
	}

	[Button]
	public float GetTotalRandomInventoryItemWeight()
	{
		return RandomInventoryItems.Sum((RandomInventoryItem item) => item.Weight);
	}

	public int GetIdenticalItemAmount(ItemInstance item)
	{
		int num = 0;
		for (int i = 0; i < ItemSlots.Count; i++)
		{
			if (ItemSlots[i].Quantity != 0 && ItemSlots[i].ItemInstance.CanStackWith(item, checkQuantities: false))
			{
				num += ItemSlots[i].Quantity;
			}
		}
		return num;
	}

	public int GetMaxItemCount(string[] ids)
	{
		int[] array = new int[ids.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = ((IItemSlotOwner)this).GetQuantityOfItem(ids[i]);
		}
		if (array.Length == 0)
		{
			return 0;
		}
		return array.Max();
	}

	public bool CanItemFit(ItemInstance item)
	{
		if (item == null)
		{
			return false;
		}
		return GetCapacityForItem(item) >= item.Quantity;
	}

	public int GetCapacityForItem(ItemInstance item)
	{
		if (item == null)
		{
			return 0;
		}
		int num = 0;
		for (int i = 0; i < ItemSlots.Count; i++)
		{
			if (ItemSlots[i] != null && !ItemSlots[i].IsLocked && !ItemSlots[i].IsAddLocked)
			{
				if (ItemSlots[i].ItemInstance == null)
				{
					num += item.StackLimit;
				}
				else if (ItemSlots[i].ItemInstance.CanStackWith(item))
				{
					num += item.StackLimit - ItemSlots[i].ItemInstance.Quantity;
				}
			}
		}
		return num;
	}

	public void InsertItem(ItemInstance item, bool network = true)
	{
		if (!CanItemFit(item))
		{
			Console.LogWarning("StorageEntity InsertItem() called but CanItemFit() returned false");
			return;
		}
		int num = item.Quantity;
		for (int i = 0; i < ItemSlots.Count; i++)
		{
			if (!ItemSlots[i].IsLocked && !ItemSlots[i].IsAddLocked)
			{
				if (ItemSlots[i].ItemInstance != null && ItemSlots[i].ItemInstance.CanStackWith(item))
				{
					int num2 = Mathf.Min(item.StackLimit - ItemSlots[i].ItemInstance.Quantity, num);
					num -= num2;
					ItemSlots[i].ChangeQuantity(num2, network);
				}
				if (num <= 0)
				{
					return;
				}
			}
		}
		for (int j = 0; j < ItemSlots.Count; j++)
		{
			if (!ItemSlots[j].IsLocked && !ItemSlots[j].IsAddLocked)
			{
				if (ItemSlots[j].ItemInstance == null)
				{
					num -= item.StackLimit;
					ItemSlots[j].SetStoredItem(item, !network);
					break;
				}
				if (num <= 0)
				{
					break;
				}
			}
		}
	}

	public ItemInstance GetFirstItem(string id, ItemFilter filter = null)
	{
		for (int i = 0; i < ItemSlots.Count; i++)
		{
			if (ItemSlots[i].ItemInstance != null && ItemSlots[i].ItemInstance.ID == id && (filter == null || filter(ItemSlots[i].ItemInstance)))
			{
				return ItemSlots[i].ItemInstance;
			}
		}
		return null;
	}

	public ItemInstance GetFirstIdenticalItem(ItemInstance item, ItemFilter filter = null)
	{
		for (int i = 0; i < ItemSlots.Count; i++)
		{
			if (ItemSlots[i].ItemInstance != null && ItemSlots[i].ItemInstance.CanStackWith(item, checkQuantities: false) && (filter == null || filter(ItemSlots[i].ItemInstance)))
			{
				return ItemSlots[i].ItemInstance;
			}
		}
		return null;
	}

	protected virtual void InventoryContentsChanged()
	{
		if (onContentsChanged != null)
		{
			onContentsChanged.Invoke();
		}
	}

	public void Hovered()
	{
		if (CanPickpocket())
		{
			PickpocketIntObj.SetMessage(npc.IsConscious ? "Pickpocket" : "View inventory");
			PickpocketIntObj.SetInteractableState(InteractableObject.EInteractableState.Default);
		}
		else
		{
			PickpocketIntObj.SetInteractableState(InteractableObject.EInteractableState.Disabled);
		}
	}

	public void Interacted()
	{
		if (CanPickpocket())
		{
			StartPickpocket();
		}
	}

	private void StartPickpocket()
	{
		Singleton<PickpocketScreen>.Instance.Open(npc);
	}

	public void ExpirePickpocket()
	{
		timeOnLastExpire = Time.time;
	}

	private bool CanPickpocket()
	{
		if (!CanBePickpocketed)
		{
			return false;
		}
		if (GameManager.IS_TUTORIAL)
		{
			return false;
		}
		if (!npc.IsConscious)
		{
			return true;
		}
		if (!PlayerSingleton<PlayerMovement>.Instance.IsCrouched)
		{
			return false;
		}
		if (Player.Local.CrimeData.CurrentPursuitLevel != PlayerCrimeData.EPursuitLevel.None)
		{
			return false;
		}
		if (Time.time - timeOnLastExpire < 30f)
		{
			return false;
		}
		if (npc.Behaviour.CallPoliceBehaviour.Active)
		{
			return false;
		}
		if (npc.Behaviour.CombatBehaviour.Active)
		{
			return false;
		}
		if (npc.Behaviour.FaceTargetBehaviour.Active)
		{
			return false;
		}
		if (npc.Behaviour.FleeBehaviour.Active)
		{
			return false;
		}
		if (npc.Behaviour.GenericDialogueBehaviour.Active)
		{
			return false;
		}
		if (npc.Behaviour.StationaryBehaviour.Active)
		{
			return false;
		}
		if (npc.Behaviour.RequestProductBehaviour.Active)
		{
			return false;
		}
		return true;
	}

	[Button]
	public void PrintInventoryContents()
	{
		for (int i = 0; i < ItemSlots.Count; i++)
		{
			if (ItemSlots[i].Quantity != 0)
			{
				Console.Log("Slot " + i + ": " + ItemSlots[i].ItemInstance.Name + " x" + ItemSlots[i].Quantity);
			}
		}
	}

	public void Clear()
	{
		for (int i = 0; i < ItemSlots.Count; i++)
		{
			ItemSlots[i].ClearStoredInstance();
		}
	}

	public float GetCashInInventory()
	{
		float num = 0f;
		for (int i = 0; i < ItemSlots.Count; i++)
		{
			if (ItemSlots[i].ItemInstance is CashInstance cashInstance)
			{
				num += cashInstance.Balance;
			}
		}
		return num;
	}

	public void RemoveCash(float amountToRemove)
	{
		for (int i = 0; i < ItemSlots.Count; i++)
		{
			if (amountToRemove <= 0f)
			{
				break;
			}
			if (ItemSlots[i].ItemInstance is CashInstance cashInstance)
			{
				float num = Mathf.Min(amountToRemove, cashInstance.Balance);
				cashInstance.ChangeBalance(0f - num);
				amountToRemove -= num;
			}
		}
	}

	public void AddCash(float amountToAdd)
	{
		if (!(amountToAdd <= 0f))
		{
			while (amountToAdd > 0.1f)
			{
				float num = Mathf.Min(amountToAdd, 1000f);
				amountToAdd -= num;
				CashInstance cashInstance = NetworkSingleton<MoneyManager>.Instance.GetCashInstance(num);
				InsertItem(cashInstance);
			}
		}
	}

	[ServerRpc(RunLocally = true, RequireOwnership = false)]
	public void SetStoredInstance(NetworkConnection conn, int itemSlotIndex, ItemInstance instance)
	{
		RpcWriter___Server_SetStoredInstance_2652194801(conn, itemSlotIndex, instance);
		RpcLogic___SetStoredInstance_2652194801(conn, itemSlotIndex, instance);
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	private void SetStoredInstance_Internal(NetworkConnection conn, int itemSlotIndex, ItemInstance instance)
	{
		if ((object)conn == null)
		{
			RpcWriter___Observers_SetStoredInstance_Internal_2652194801(conn, itemSlotIndex, instance);
			RpcLogic___SetStoredInstance_Internal_2652194801(conn, itemSlotIndex, instance);
		}
		else
		{
			RpcWriter___Target_SetStoredInstance_Internal_2652194801(conn, itemSlotIndex, instance);
		}
	}

	[ServerRpc(RunLocally = true, RequireOwnership = false)]
	public void SetItemSlotQuantity(int itemSlotIndex, int quantity)
	{
		RpcWriter___Server_SetItemSlotQuantity_1692629761(itemSlotIndex, quantity);
		RpcLogic___SetItemSlotQuantity_1692629761(itemSlotIndex, quantity);
	}

	[ObserversRpc(RunLocally = true)]
	private void SetItemSlotQuantity_Internal(int itemSlotIndex, int quantity)
	{
		RpcWriter___Observers_SetItemSlotQuantity_Internal_1692629761(itemSlotIndex, quantity);
		RpcLogic___SetItemSlotQuantity_Internal_1692629761(itemSlotIndex, quantity);
	}

	[ServerRpc(RunLocally = true, RequireOwnership = false)]
	public void SetSlotLocked(NetworkConnection conn, int itemSlotIndex, bool locked, NetworkObject lockOwner, string lockReason)
	{
		RpcWriter___Server_SetSlotLocked_3170825843(conn, itemSlotIndex, locked, lockOwner, lockReason);
		RpcLogic___SetSlotLocked_3170825843(conn, itemSlotIndex, locked, lockOwner, lockReason);
	}

	[TargetRpc]
	[ObserversRpc(RunLocally = true)]
	private void SetSlotLocked_Internal(NetworkConnection conn, int itemSlotIndex, bool locked, NetworkObject lockOwner, string lockReason)
	{
		if ((object)conn == null)
		{
			RpcWriter___Observers_SetSlotLocked_Internal_3170825843(conn, itemSlotIndex, locked, lockOwner, lockReason);
			RpcLogic___SetSlotLocked_Internal_3170825843(conn, itemSlotIndex, locked, lockOwner, lockReason);
		}
		else
		{
			RpcWriter___Target_SetSlotLocked_Internal_3170825843(conn, itemSlotIndex, locked, lockOwner, lockReason);
		}
	}

	[ServerRpc(RunLocally = true, RequireOwnership = false)]
	public void SetSlotFilter(NetworkConnection conn, int itemSlotIndex, SlotFilter filter)
	{
		RpcWriter___Server_SetSlotFilter_527532783(conn, itemSlotIndex, filter);
		RpcLogic___SetSlotFilter_527532783(conn, itemSlotIndex, filter);
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	private void SetSlotFilter_Internal(NetworkConnection conn, int itemSlotIndex, SlotFilter filter)
	{
		if ((object)conn == null)
		{
			RpcWriter___Observers_SetSlotFilter_Internal_527532783(conn, itemSlotIndex, filter);
			RpcLogic___SetSlotFilter_Internal_527532783(conn, itemSlotIndex, filter);
		}
		else
		{
			RpcWriter___Target_SetSlotFilter_Internal_527532783(conn, itemSlotIndex, filter);
		}
	}

	public virtual void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyScheduleOne.NPCs.NPCInventoryAssembly-CSharp.dll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne.NPCs.NPCInventoryAssembly-CSharp.dll_Excuted = true;
			RegisterServerRpc(0u, RpcReader___Server_SetStoredInstance_2652194801);
			RegisterObserversRpc(1u, RpcReader___Observers_SetStoredInstance_Internal_2652194801);
			RegisterTargetRpc(2u, RpcReader___Target_SetStoredInstance_Internal_2652194801);
			RegisterServerRpc(3u, RpcReader___Server_SetItemSlotQuantity_1692629761);
			RegisterObserversRpc(4u, RpcReader___Observers_SetItemSlotQuantity_Internal_1692629761);
			RegisterServerRpc(5u, RpcReader___Server_SetSlotLocked_3170825843);
			RegisterTargetRpc(6u, RpcReader___Target_SetSlotLocked_Internal_3170825843);
			RegisterObserversRpc(7u, RpcReader___Observers_SetSlotLocked_Internal_3170825843);
			RegisterServerRpc(8u, RpcReader___Server_SetSlotFilter_527532783);
			RegisterObserversRpc(9u, RpcReader___Observers_SetSlotFilter_Internal_527532783);
			RegisterTargetRpc(10u, RpcReader___Target_SetSlotFilter_Internal_527532783);
		}
	}

	public virtual void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne.NPCs.NPCInventoryAssembly-CSharp.dll_Excuted)
		{
			NetworkInitialize__LateScheduleOne.NPCs.NPCInventoryAssembly-CSharp.dll_Excuted = true;
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Server_SetStoredInstance_2652194801(NetworkConnection conn, int itemSlotIndex, ItemInstance instance)
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
			writer.WriteNetworkConnection(conn);
			writer.WriteInt32(itemSlotIndex);
			writer.WriteItemInstance(instance);
			SendServerRpc(0u, writer, channel, DataOrderType.Default);
			writer.Store();
		}
	}

	public void RpcLogic___SetStoredInstance_2652194801(NetworkConnection conn, int itemSlotIndex, ItemInstance instance)
	{
		if (conn == null || conn.ClientId == -1)
		{
			SetStoredInstance_Internal(null, itemSlotIndex, instance);
		}
		else
		{
			SetStoredInstance_Internal(conn, itemSlotIndex, instance);
		}
	}

	private void RpcReader___Server_SetStoredInstance_2652194801(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		NetworkConnection conn2 = PooledReader0.ReadNetworkConnection();
		int itemSlotIndex = PooledReader0.ReadInt32();
		ItemInstance instance = PooledReader0.ReadItemInstance();
		if (base.IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SetStoredInstance_2652194801(conn2, itemSlotIndex, instance);
		}
	}

	private void RpcWriter___Observers_SetStoredInstance_Internal_2652194801(NetworkConnection conn, int itemSlotIndex, ItemInstance instance)
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
			writer.WriteInt32(itemSlotIndex);
			writer.WriteItemInstance(instance);
			SendObserversRpc(1u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
			writer.Store();
		}
	}

	private void RpcLogic___SetStoredInstance_Internal_2652194801(NetworkConnection conn, int itemSlotIndex, ItemInstance instance)
	{
		if (instance != null)
		{
			ItemSlots[itemSlotIndex].SetStoredItem(instance, _internal: true);
		}
		else
		{
			ItemSlots[itemSlotIndex].ClearStoredInstance(_internal: true);
		}
	}

	private void RpcReader___Observers_SetStoredInstance_Internal_2652194801(PooledReader PooledReader0, Channel channel)
	{
		int itemSlotIndex = PooledReader0.ReadInt32();
		ItemInstance instance = PooledReader0.ReadItemInstance();
		if (base.IsClientInitialized && !base.IsHost)
		{
			RpcLogic___SetStoredInstance_Internal_2652194801(null, itemSlotIndex, instance);
		}
	}

	private void RpcWriter___Target_SetStoredInstance_Internal_2652194801(NetworkConnection conn, int itemSlotIndex, ItemInstance instance)
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
			writer.WriteInt32(itemSlotIndex);
			writer.WriteItemInstance(instance);
			SendTargetRpc(2u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
			writer.Store();
		}
	}

	private void RpcReader___Target_SetStoredInstance_Internal_2652194801(PooledReader PooledReader0, Channel channel)
	{
		int itemSlotIndex = PooledReader0.ReadInt32();
		ItemInstance instance = PooledReader0.ReadItemInstance();
		if (base.IsClientInitialized)
		{
			RpcLogic___SetStoredInstance_Internal_2652194801(base.LocalConnection, itemSlotIndex, instance);
		}
	}

	private void RpcWriter___Server_SetItemSlotQuantity_1692629761(int itemSlotIndex, int quantity)
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
			writer.WriteInt32(itemSlotIndex);
			writer.WriteInt32(quantity);
			SendServerRpc(3u, writer, channel, DataOrderType.Default);
			writer.Store();
		}
	}

	public void RpcLogic___SetItemSlotQuantity_1692629761(int itemSlotIndex, int quantity)
	{
		SetItemSlotQuantity_Internal(itemSlotIndex, quantity);
	}

	private void RpcReader___Server_SetItemSlotQuantity_1692629761(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		int itemSlotIndex = PooledReader0.ReadInt32();
		int quantity = PooledReader0.ReadInt32();
		if (base.IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SetItemSlotQuantity_1692629761(itemSlotIndex, quantity);
		}
	}

	private void RpcWriter___Observers_SetItemSlotQuantity_Internal_1692629761(int itemSlotIndex, int quantity)
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
			writer.WriteInt32(itemSlotIndex);
			writer.WriteInt32(quantity);
			SendObserversRpc(4u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
			writer.Store();
		}
	}

	private void RpcLogic___SetItemSlotQuantity_Internal_1692629761(int itemSlotIndex, int quantity)
	{
		ItemSlots[itemSlotIndex].SetQuantity(quantity, _internal: true);
	}

	private void RpcReader___Observers_SetItemSlotQuantity_Internal_1692629761(PooledReader PooledReader0, Channel channel)
	{
		int itemSlotIndex = PooledReader0.ReadInt32();
		int quantity = PooledReader0.ReadInt32();
		if (base.IsClientInitialized && !base.IsHost)
		{
			RpcLogic___SetItemSlotQuantity_Internal_1692629761(itemSlotIndex, quantity);
		}
	}

	private void RpcWriter___Server_SetSlotLocked_3170825843(NetworkConnection conn, int itemSlotIndex, bool locked, NetworkObject lockOwner, string lockReason)
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
			writer.WriteNetworkConnection(conn);
			writer.WriteInt32(itemSlotIndex);
			writer.WriteBoolean(locked);
			writer.WriteNetworkObject(lockOwner);
			writer.WriteString(lockReason);
			SendServerRpc(5u, writer, channel, DataOrderType.Default);
			writer.Store();
		}
	}

	public void RpcLogic___SetSlotLocked_3170825843(NetworkConnection conn, int itemSlotIndex, bool locked, NetworkObject lockOwner, string lockReason)
	{
		if (conn == null || conn.ClientId == -1)
		{
			SetSlotLocked_Internal(null, itemSlotIndex, locked, lockOwner, lockReason);
		}
		else
		{
			SetSlotLocked_Internal(conn, itemSlotIndex, locked, lockOwner, lockReason);
		}
	}

	private void RpcReader___Server_SetSlotLocked_3170825843(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		NetworkConnection conn2 = PooledReader0.ReadNetworkConnection();
		int itemSlotIndex = PooledReader0.ReadInt32();
		bool locked = PooledReader0.ReadBoolean();
		NetworkObject lockOwner = PooledReader0.ReadNetworkObject();
		string lockReason = PooledReader0.ReadString();
		if (base.IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SetSlotLocked_3170825843(conn2, itemSlotIndex, locked, lockOwner, lockReason);
		}
	}

	private void RpcWriter___Target_SetSlotLocked_Internal_3170825843(NetworkConnection conn, int itemSlotIndex, bool locked, NetworkObject lockOwner, string lockReason)
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
			writer.WriteInt32(itemSlotIndex);
			writer.WriteBoolean(locked);
			writer.WriteNetworkObject(lockOwner);
			writer.WriteString(lockReason);
			SendTargetRpc(6u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
			writer.Store();
		}
	}

	private void RpcLogic___SetSlotLocked_Internal_3170825843(NetworkConnection conn, int itemSlotIndex, bool locked, NetworkObject lockOwner, string lockReason)
	{
		if (locked)
		{
			ItemSlots[itemSlotIndex].ApplyLock(lockOwner, lockReason, _internal: true);
		}
		else
		{
			ItemSlots[itemSlotIndex].RemoveLock(_internal: true);
		}
	}

	private void RpcReader___Target_SetSlotLocked_Internal_3170825843(PooledReader PooledReader0, Channel channel)
	{
		int itemSlotIndex = PooledReader0.ReadInt32();
		bool locked = PooledReader0.ReadBoolean();
		NetworkObject lockOwner = PooledReader0.ReadNetworkObject();
		string lockReason = PooledReader0.ReadString();
		if (base.IsClientInitialized)
		{
			RpcLogic___SetSlotLocked_Internal_3170825843(base.LocalConnection, itemSlotIndex, locked, lockOwner, lockReason);
		}
	}

	private void RpcWriter___Observers_SetSlotLocked_Internal_3170825843(NetworkConnection conn, int itemSlotIndex, bool locked, NetworkObject lockOwner, string lockReason)
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
			writer.WriteInt32(itemSlotIndex);
			writer.WriteBoolean(locked);
			writer.WriteNetworkObject(lockOwner);
			writer.WriteString(lockReason);
			SendObserversRpc(7u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
			writer.Store();
		}
	}

	private void RpcReader___Observers_SetSlotLocked_Internal_3170825843(PooledReader PooledReader0, Channel channel)
	{
		int itemSlotIndex = PooledReader0.ReadInt32();
		bool locked = PooledReader0.ReadBoolean();
		NetworkObject lockOwner = PooledReader0.ReadNetworkObject();
		string lockReason = PooledReader0.ReadString();
		if (base.IsClientInitialized && !base.IsHost)
		{
			RpcLogic___SetSlotLocked_Internal_3170825843(null, itemSlotIndex, locked, lockOwner, lockReason);
		}
	}

	private void RpcWriter___Server_SetSlotFilter_527532783(NetworkConnection conn, int itemSlotIndex, SlotFilter filter)
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
			writer.WriteNetworkConnection(conn);
			writer.WriteInt32(itemSlotIndex);
			GeneratedWriters___Internal.Write___ScheduleOne.ItemFramework.SlotFilterFishNet.Serializing.Generated(writer, filter);
			SendServerRpc(8u, writer, channel, DataOrderType.Default);
			writer.Store();
		}
	}

	public void RpcLogic___SetSlotFilter_527532783(NetworkConnection conn, int itemSlotIndex, SlotFilter filter)
	{
		if (conn == null || conn.ClientId == -1)
		{
			SetSlotFilter_Internal(null, itemSlotIndex, filter);
		}
		else
		{
			SetSlotFilter_Internal(conn, itemSlotIndex, filter);
		}
	}

	private void RpcReader___Server_SetSlotFilter_527532783(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		NetworkConnection conn2 = PooledReader0.ReadNetworkConnection();
		int itemSlotIndex = PooledReader0.ReadInt32();
		SlotFilter filter = GeneratedReaders___Internal.Read___ScheduleOne.ItemFramework.SlotFilterFishNet.Serializing.Generateds(PooledReader0);
		if (base.IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SetSlotFilter_527532783(conn2, itemSlotIndex, filter);
		}
	}

	private void RpcWriter___Observers_SetSlotFilter_Internal_527532783(NetworkConnection conn, int itemSlotIndex, SlotFilter filter)
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
			writer.WriteInt32(itemSlotIndex);
			GeneratedWriters___Internal.Write___ScheduleOne.ItemFramework.SlotFilterFishNet.Serializing.Generated(writer, filter);
			SendObserversRpc(9u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
			writer.Store();
		}
	}

	private void RpcLogic___SetSlotFilter_Internal_527532783(NetworkConnection conn, int itemSlotIndex, SlotFilter filter)
	{
		ItemSlots[itemSlotIndex].SetPlayerFilter(filter, _internal: true);
	}

	private void RpcReader___Observers_SetSlotFilter_Internal_527532783(PooledReader PooledReader0, Channel channel)
	{
		int itemSlotIndex = PooledReader0.ReadInt32();
		SlotFilter filter = GeneratedReaders___Internal.Read___ScheduleOne.ItemFramework.SlotFilterFishNet.Serializing.Generateds(PooledReader0);
		if (base.IsClientInitialized && !base.IsHost)
		{
			RpcLogic___SetSlotFilter_Internal_527532783(null, itemSlotIndex, filter);
		}
	}

	private void RpcWriter___Target_SetSlotFilter_Internal_527532783(NetworkConnection conn, int itemSlotIndex, SlotFilter filter)
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
			writer.WriteInt32(itemSlotIndex);
			GeneratedWriters___Internal.Write___ScheduleOne.ItemFramework.SlotFilterFishNet.Serializing.Generated(writer, filter);
			SendTargetRpc(10u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
			writer.Store();
		}
	}

	private void RpcReader___Target_SetSlotFilter_Internal_527532783(PooledReader PooledReader0, Channel channel)
	{
		int itemSlotIndex = PooledReader0.ReadInt32();
		SlotFilter filter = GeneratedReaders___Internal.Read___ScheduleOne.ItemFramework.SlotFilterFishNet.Serializing.Generateds(PooledReader0);
		if (base.IsClientInitialized)
		{
			RpcLogic___SetSlotFilter_Internal_527532783(base.LocalConnection, itemSlotIndex, filter);
		}
	}

	protected virtual void Awake_UserLogic_ScheduleOne.NPCs.NPCInventory_Assembly-CSharp.dll()
	{
		for (int i = 0; i < SlotCount; i++)
		{
			ItemSlot itemSlot = new ItemSlot();
			itemSlot.SetSlotOwner(this);
			itemSlot.onItemDataChanged = (Action)Delegate.Combine(itemSlot.onItemDataChanged, new Action(InventoryContentsChanged));
		}
		ItemDefinition[] testItems;
		if (Application.isEditor)
		{
			testItems = TestItems;
			for (int j = 0; j < testItems.Length; j++)
			{
				ItemInstance defaultInstance = testItems[j].GetDefaultInstance();
				InsertItem(defaultInstance);
			}
		}
		testItems = StartupItems;
		for (int j = 0; j < testItems.Length; j++)
		{
			ItemInstance defaultInstance2 = testItems[j].GetDefaultInstance();
			InsertItem(defaultInstance2);
		}
		npc = GetComponent<NPC>();
		PickpocketIntObj.onHovered.AddListener(Hovered);
		PickpocketIntObj.onInteractStart.AddListener(Interacted);
	}
}
