using System.Collections;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.DevUtilities;
using ScheduleOne.Law;
using ScheduleOne.NPCs.Behaviour;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.NPCs.Actions;

public class NPCActions : NetworkBehaviour
{
	private NPC npc;

	private bool NetworkInitialize___EarlyScheduleOne.NPCs.Actions.NPCActionsAssembly-CSharp.dll_Excuted;

	private bool NetworkInitialize__LateScheduleOne.NPCs.Actions.NPCActionsAssembly-CSharp.dll_Excuted;

	protected NPCBehaviour behaviour => npc.Behaviour;

	public virtual void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne.NPCs.Actions.NPCActions_Assembly-CSharp.dll();
		NetworkInitialize__Late();
	}

	public void Cower()
	{
		behaviour.GetBehaviour("Cowering").Enable_Networked();
		StartCoroutine(Wait());
		IEnumerator Wait()
		{
			yield return new WaitForSeconds(10f);
			behaviour.GetBehaviour("Cowering").Disable_Networked(null);
		}
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void CallPolice_Networked(NetworkObject playerObj)
	{
		RpcWriter___Server_CallPolice_Networked_3323014238(playerObj);
		RpcLogic___CallPolice_Networked_3323014238(playerObj);
	}

	public void SetCallPoliceBehaviourCrime(Crime crime)
	{
		npc.Behaviour.CallPoliceBehaviour.ReportedCrime = crime;
	}

	public void FacePlayer(Player player)
	{
	}

	public virtual void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyScheduleOne.NPCs.Actions.NPCActionsAssembly-CSharp.dll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne.NPCs.Actions.NPCActionsAssembly-CSharp.dll_Excuted = true;
			RegisterServerRpc(0u, RpcReader___Server_CallPolice_Networked_3323014238);
		}
	}

	public virtual void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne.NPCs.Actions.NPCActionsAssembly-CSharp.dll_Excuted)
		{
			NetworkInitialize__LateScheduleOne.NPCs.Actions.NPCActionsAssembly-CSharp.dll_Excuted = true;
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Server_CallPolice_Networked_3323014238(NetworkObject playerObj)
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
			writer.WriteNetworkObject(playerObj);
			SendServerRpc(0u, writer, channel, DataOrderType.Default);
			writer.Store();
		}
	}

	public void RpcLogic___CallPolice_Networked_3323014238(NetworkObject playerObj)
	{
		if (NetworkSingleton<GameManager>.Instance.IsTutorial)
		{
			return;
		}
		Player component = playerObj.GetComponent<Player>();
		if (component == null || !npc.IsConscious)
		{
			return;
		}
		Console.Log(npc.fullName + " is calling the police on " + component.PlayerName);
		if (component.CrimeData.CurrentPursuitLevel != PlayerCrimeData.EPursuitLevel.None)
		{
			Console.LogWarning("Player is already being pursued, ignoring call police request.");
			return;
		}
		npc.Behaviour.CallPoliceBehaviour.Target = component;
		if (InstanceFinder.IsServer)
		{
			npc.Behaviour.CallPoliceBehaviour.Enable_Networked();
		}
	}

	private void RpcReader___Server_CallPolice_Networked_3323014238(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		NetworkObject playerObj = PooledReader0.ReadNetworkObject();
		if (base.IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___CallPolice_Networked_3323014238(playerObj);
		}
	}

	protected virtual void Awake_UserLogic_ScheduleOne.NPCs.Actions.NPCActions_Assembly-CSharp.dll()
	{
		npc = GetComponentInParent<NPC>();
	}
}
