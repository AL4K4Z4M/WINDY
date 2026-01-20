using System;
using ScheduleOne.NPCs.Responses;
using ScheduleOne.Noise;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Vehicles;
using ScheduleOne.Vision;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.NPCs;

public class NPCAwareness : MonoBehaviour
{
	public const float PLAYER_AIM_DETECTION_RANGE = 15f;

	public bool AwarenessActiveByDefault = true;

	[Header("References")]
	public VisionCone VisionCone;

	public Listener Listener;

	public NPCResponses Responses;

	public UnityEvent<Player> onNoticedGeneralCrime;

	public UnityEvent<Player> onNoticedPettyCrime;

	public UnityEvent<Player> onNoticedDrugDealing;

	public UnityEvent<Player> onNoticedPlayerViolatingCurfew;

	public UnityEvent<Player> onNoticedSuspiciousPlayer;

	public UnityEvent<NoiseEvent> onGunshotHeard;

	public UnityEvent<NoiseEvent> onExplosionHeard;

	public UnityEvent<LandVehicle> onHitByCar;

	private NPC npc;

	protected virtual void Awake()
	{
		npc = GetComponentInParent<NPC>();
		if (Responses == null)
		{
			Console.LogError("NPCAwareness doesn't have a reference to NPCResponses - responses won't be automatically connected.");
		}
		VisionCone visionCone = VisionCone;
		visionCone.onVisionEventFull = (VisionCone.EventStateChange)Delegate.Combine(visionCone.onVisionEventFull, new VisionCone.EventStateChange(VisionEvent));
		Listener listener = Listener;
		listener.onNoiseHeard = (Listener.HearingEvent)Delegate.Combine(listener.onNoiseHeard, new Listener.HearingEvent(NoiseEvent));
		SetAwarenessActive(AwarenessActiveByDefault);
	}

	public void SetAwarenessActive(bool active)
	{
		Listener.enabled = active;
		VisionCone.enabled = active;
		base.enabled = active;
	}

	public void VisionEvent(VisionEventReceipt vEvent)
	{
		if (!base.enabled)
		{
			return;
		}
		switch (vEvent.State)
		{
		case EVisualState.DisobeyingCurfew:
			if (onNoticedPlayerViolatingCurfew != null)
			{
				onNoticedPlayerViolatingCurfew.Invoke(vEvent.Target.GetComponent<Player>());
			}
			if (Responses != null)
			{
				Responses.NoticedViolatingCurfew(vEvent.Target.GetComponent<Player>());
			}
			break;
		case EVisualState.PettyCrime:
			if (onNoticedPettyCrime != null)
			{
				onNoticedPettyCrime.Invoke(vEvent.Target.GetComponent<Player>());
			}
			if (onNoticedGeneralCrime != null)
			{
				onNoticedGeneralCrime.Invoke(vEvent.Target.GetComponent<Player>());
			}
			if (Responses != null)
			{
				Responses.NoticedPettyCrime(vEvent.Target.GetComponent<Player>());
			}
			break;
		case EVisualState.Vandalizing:
			if (Responses != null)
			{
				Responses.NoticedVandalism(vEvent.Target.GetComponent<Player>());
			}
			break;
		case EVisualState.Pickpocketing:
			if (Responses != null)
			{
				Responses.SawPickpocketing(vEvent.Target.GetComponent<Player>());
			}
			break;
		case EVisualState.DrugDealing:
			if (onNoticedDrugDealing != null)
			{
				onNoticedDrugDealing.Invoke(vEvent.Target.GetComponent<Player>());
			}
			if (onNoticedGeneralCrime != null)
			{
				onNoticedGeneralCrime.Invoke(vEvent.Target.GetComponent<Player>());
			}
			if (Responses != null)
			{
				Responses.NoticedDrugDeal(vEvent.Target.GetComponent<Player>());
			}
			break;
		case EVisualState.Wanted:
			if (Responses != null)
			{
				Responses.NoticedWantedPlayer(vEvent.Target.GetComponent<Player>());
			}
			break;
		case EVisualState.Suspicious:
			if (onNoticedSuspiciousPlayer != null)
			{
				onNoticedSuspiciousPlayer.Invoke(vEvent.Target.GetComponent<Player>());
			}
			if (Responses != null)
			{
				Responses.NoticedSuspiciousPlayer(vEvent.Target.GetComponent<Player>());
			}
			break;
		case EVisualState.Brandishing:
			if (Responses != null)
			{
				Responses.NoticePlayerBrandishingWeapon(vEvent.Target.GetComponent<Player>());
			}
			break;
		case EVisualState.DischargingWeapon:
			if (Responses != null)
			{
				Responses.NoticePlayerDischargingWeapon(vEvent.Target.GetComponent<Player>());
			}
			break;
		case EVisualState.Visible:
			break;
		}
	}

	public void NoiseEvent(NoiseEvent nEvent)
	{
		if (!base.enabled)
		{
			return;
		}
		if (nEvent.type == ENoiseType.Gunshot)
		{
			if (onGunshotHeard != null)
			{
				onGunshotHeard.Invoke(nEvent);
			}
			if (Responses != null)
			{
				Responses.GunshotHeard(nEvent);
			}
		}
		if (nEvent.type == ENoiseType.Explosion)
		{
			if (onExplosionHeard != null)
			{
				onExplosionHeard.Invoke(nEvent);
			}
			if (Responses != null)
			{
				Responses.ExplosionHeard(nEvent);
			}
		}
	}

	public void HitByCar(LandVehicle vehicle)
	{
		if (onHitByCar != null)
		{
			onHitByCar.Invoke(vehicle);
		}
		if (Responses != null)
		{
			Responses.HitByCar(vehicle);
		}
	}
}
