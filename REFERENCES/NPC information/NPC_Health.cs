using System;
using System.Collections;
using System.Runtime.CompilerServices;
using FishNet;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.PlayerScripts;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.NPCs;

[RequireComponent(typeof(NPCHealth))]
[DisallowMultipleComponent]
public class NPCHealth : NetworkBehaviour
{
	public const int REVIVE_DAYS = 3;

	[CompilerGenerated]
	[SyncVar(WritePermissions = WritePermission.ClientUnsynchronized, Channel = Channel.Unreliable)]
	public float <Health>k__BackingField;

	[Header("Settings")]
	public bool Invincible;

	public float MaxHealth = 100f;

	public bool CanRevive = true;

	private NPC npc;

	public UnityEvent onDie;

	public UnityEvent onKnockedOut;

	public UnityEvent onDieOrKnockedOut;

	public UnityEvent onRevive;

	public Action<float> onTakeDamage;

	private bool AfflictedWithLethalEffect;

	public SyncVar<float> syncVar___<Health>k__BackingField;

	private bool NetworkInitialize___EarlyScheduleOne.NPCs.NPCHealthAssembly-CSharp.dll_Excuted;

	private bool NetworkInitialize__LateScheduleOne.NPCs.NPCHealthAssembly-CSharp.dll_Excuted;

	public float Health
	{
		[CompilerGenerated]
		get
		{
			return SyncAccessor_<Health>k__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			this.sync___set_value_<Health>k__BackingField(value, asServer: true);
		}
	}

	public float NormalizedHealth
	{
		get
		{
			if (MaxHealth <= 0f)
			{
				return 0f;
			}
			return SyncAccessor_<Health>k__BackingField / MaxHealth;
		}
	}

	public bool IsDead { get; private set; }

	public bool IsKnockedOut { get; private set; }

	public int DaysPassedSinceDeath { get; private set; }

	public int HoursSinceAttackedByPlayer { get; private set; } = 9999;

	public float SyncAccessor_<Health>k__BackingField
	{
		get
		{
			return Health;
		}
		set
		{
			if (value || !base.IsServerInitialized)
			{
				Health = value;
			}
			if (Application.isPlaying)
			{
				syncVar___<Health>k__BackingField.SetValue(value, value);
			}
		}
	}

	public virtual void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne.NPCs.NPCHealth_Assembly-CSharp.dll();
		NetworkInitialize__Late();
	}

	private void Start()
	{
		TimeManager instance = NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance;
		instance.onHourPass = (Action)Delegate.Remove(instance.onHourPass, new Action(OnHourPass));
		TimeManager instance2 = NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance;
		instance2.onHourPass = (Action)Delegate.Combine(instance2.onHourPass, new Action(OnHourPass));
	}

	private void OnDestroy()
	{
		if (NetworkSingleton<ScheduleOne.GameTime.TimeManager>.InstanceExists)
		{
			TimeManager instance = NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance;
			instance.onSleepStart = (Action)Delegate.Remove(instance.onSleepStart, new Action(SleepStart));
		}
	}

	public override void OnStartServer()
	{
		base.OnStartServer();
		RestoreHealth();
	}

	public void Load(NPCHealthData healthData)
	{
		Health = healthData.Health;
		DaysPassedSinceDeath = healthData.DaysPassedSinceDeath;
		if (healthData.DataVersion < 1)
		{
			healthData.HoursSinceAttackedByPlayer = 9999;
		}
		HoursSinceAttackedByPlayer = healthData.HoursSinceAttackedByPlayer;
		if (IsDead)
		{
			Die();
		}
		else if (SyncAccessor_<Health>k__BackingField == 0f)
		{
			KnockOut();
		}
	}

	private IEnumerator AfflictWithLethalEffect()
	{
		while (!IsDead)
		{
			if (AfflictedWithLethalEffect)
			{
				TakeDamage(15f * Time.deltaTime);
			}
			yield return null;
		}
	}

	protected virtual void OnHourPass()
	{
		HoursSinceAttackedByPlayer++;
	}

	public void SetAfflictedWithLethalEffect(bool value)
	{
		AfflictedWithLethalEffect = value;
		if (AfflictedWithLethalEffect)
		{
			StartCoroutine(AfflictWithLethalEffect());
		}
	}

	public void SleepStart()
	{
		if (!InstanceFinder.IsServer)
		{
			return;
		}
		if (!npc.IsConscious && CanRevive)
		{
			if (IsDead)
			{
				DaysPassedSinceDeath++;
				if (DaysPassedSinceDeath >= 3 || npc.IsImportant)
				{
					Revive();
				}
			}
			else
			{
				Revive();
			}
		}
		if (npc.IsConscious)
		{
			Health = MaxHealth;
		}
	}

	public virtual void NotifyAttackedByPlayer(Player player)
	{
		HoursSinceAttackedByPlayer = 0;
	}

	public void TakeDamage(float damage, bool isLethal = true)
	{
		if (IsDead)
		{
			return;
		}
		SyncAccessor_<Health>k__BackingField -= damage;
		if (damage > 10f)
		{
			npc.Avatar.BloodParticles.Play();
		}
		if (onTakeDamage != null)
		{
			onTakeDamage(damage);
		}
		if (!(SyncAccessor_<Health>k__BackingField <= 0f))
		{
			return;
		}
		Health = 0f;
		if (Invincible)
		{
			return;
		}
		if (isLethal)
		{
			if (!IsDead)
			{
				Die();
			}
		}
		else if (!IsKnockedOut)
		{
			KnockOut();
		}
	}

	public virtual void Die()
	{
		if (!Invincible)
		{
			Console.Log(npc.fullName + " has died.");
			IsDead = true;
			Health = 0f;
			npc.Behaviour.DeadBehaviour.Enable_Networked();
			if (onDie != null)
			{
				onDie.Invoke();
			}
			if (onDieOrKnockedOut != null)
			{
				onDieOrKnockedOut.Invoke();
			}
		}
	}

	public virtual void KnockOut()
	{
		if (!Invincible)
		{
			Console.Log(npc.fullName + " has been knocked out.");
			IsKnockedOut = true;
			npc.Behaviour.UnconsciousBehaviour.Enable_Networked();
			if (onKnockedOut != null)
			{
				onKnockedOut.Invoke();
			}
			if (onDieOrKnockedOut != null)
			{
				onDieOrKnockedOut.Invoke();
			}
		}
	}

	public virtual void Revive()
	{
		Console.Log(npc.fullName + " has been revived.");
		IsDead = false;
		IsKnockedOut = false;
		RestoreHealth();
		npc.Behaviour.DeadBehaviour.Disable_Server();
		npc.Behaviour.UnconsciousBehaviour.Disable_Server();
		if (onRevive != null)
		{
			onRevive.Invoke();
		}
	}

	public void RestoreHealth()
	{
		Health = MaxHealth;
	}

	public virtual void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyScheduleOne.NPCs.NPCHealthAssembly-CSharp.dll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne.NPCs.NPCHealthAssembly-CSharp.dll_Excuted = true;
			syncVar___<Health>k__BackingField = new SyncVar<float>(this, 0u, WritePermission.ClientUnsynchronized, ReadPermission.Observers, -1f, Channel.Unreliable, Health);
			RegisterSyncVarRead(ReadSyncVar___ScheduleOne.NPCs.NPCHealth);
		}
	}

	public virtual void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne.NPCs.NPCHealthAssembly-CSharp.dll_Excuted)
		{
			NetworkInitialize__LateScheduleOne.NPCs.NPCHealthAssembly-CSharp.dll_Excuted = true;
			syncVar___<Health>k__BackingField.SetRegistered();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	public virtual bool ReadSyncVar___ScheduleOne.NPCs.NPCHealth(PooledReader PooledReader0, uint UInt321, bool Boolean2)
	{
		if (UInt321 == 0)
		{
			if (PooledReader0 == null)
			{
				this.sync___set_value_<Health>k__BackingField(syncVar___<Health>k__BackingField.GetValue(calledByUser: true), asServer: true);
				return true;
			}
			float value = PooledReader0.ReadSingle();
			this.sync___set_value_<Health>k__BackingField(value, Boolean2);
			return true;
		}
		return false;
	}

	protected virtual void Awake_UserLogic_ScheduleOne.NPCs.NPCHealth_Assembly-CSharp.dll()
	{
		npc = GetComponent<NPC>();
		TimeManager instance = NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance;
		instance.onSleepStart = (Action)Delegate.Remove(instance.onSleepStart, new Action(SleepStart));
		TimeManager instance2 = NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance;
		instance2.onSleepStart = (Action)Delegate.Combine(instance2.onSleepStart, new Action(SleepStart));
		TimeManager instance3 = NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance;
		instance3.onHourPass = (Action)Delegate.Combine(instance3.onHourPass, new Action(OnHourPass));
	}
}
