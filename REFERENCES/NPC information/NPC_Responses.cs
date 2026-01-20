using ScheduleOne.Combat;
using ScheduleOne.Law;
using ScheduleOne.NPCs.Actions;
using ScheduleOne.Noise;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Vehicles;
using ScheduleOne.VoiceOver;
using UnityEngine;

namespace ScheduleOne.NPCs.Responses;

public class NPCResponses : MonoBehaviour
{
	public const float ASSAULT_RELATIONSHIPCHANGE = -0.25f;

	public const float DEADLYASSAULT_RELATIONSHIPCHANGE = -1f;

	public const float AIMED_AT_RELATIONSHIPCHANGE = -0.5f;

	public const float PICKPOCKET_RELATIONSHIPCHANGE = -0.25f;

	private const float INITIALIZED_TIME_OFFSET = 100f;

	private const float TIME_THRESHOLD = 20f;

	protected float timeSinceLastImpact;

	protected float timeSinceAimedAt;

	protected NPC npc { get; private set; }

	protected NPCActions actions => npc.Actions;

	protected virtual void Awake()
	{
		npc = GetComponentInParent<NPC>();
		timeSinceLastImpact = Time.time - 100f;
		timeSinceAimedAt = Time.time - 100f;
	}

	public virtual void GunshotHeard(NoiseEvent gunshotSound)
	{
	}

	public virtual void ExplosionHeard(NoiseEvent explosionSound)
	{
	}

	public virtual void NoticedPettyCrime(Player player)
	{
	}

	public virtual void NoticedVandalism(Player player)
	{
	}

	public virtual void SawPickpocketing(Player player)
	{
	}

	public virtual void NoticePlayerBrandishingWeapon(Player player)
	{
	}

	public virtual void NoticePlayerDischargingWeapon(Player player)
	{
	}

	public virtual void PlayerFailedPickpocket(Player player)
	{
		if (npc.RelationData.Unlocked)
		{
			npc.RelationData.ChangeRelationship(0.25f);
		}
	}

	public virtual void NoticedDrugDeal(Player player)
	{
	}

	public virtual void NoticedViolatingCurfew(Player player)
	{
	}

	public virtual void NoticedWantedPlayer(Player player)
	{
	}

	public virtual void NoticedSuspiciousPlayer(Player player)
	{
	}

	public virtual void HitByCar(LandVehicle vehicle)
	{
		if (!(vehicle == null) && vehicle.DriverPlayer != null && npc.Movement.TimeSinceHitByCar > 2f)
		{
			if (vehicle.DriverPlayer.CrimeData.CurrentPursuitLevel > PlayerCrimeData.EPursuitLevel.None)
			{
				vehicle.DriverPlayer.CrimeData.AddCrime(new VehicularAssault());
			}
			else
			{
				vehicle.DriverPlayer.CrimeData.RecordVehicleCollision(npc);
			}
			npc.Avatar.EmotionManager.AddEmotionOverride("Angry", "hitbycar", 5f, 1);
			npc.Avatar.EmotionManager.AddEmotionOverride("Annoyed", "hitbycar1", 20f);
			npc.PlayVO(EVOLineType.Hurt);
		}
	}

	public virtual void ImpactReceived(Impact impact)
	{
		if (!npc.IsConscious)
		{
			timeSinceLastImpact = Time.time;
			return;
		}
		npc.VoiceOverEmitter.Play(EVOLineType.Hurt);
		Player player2;
		if (impact.ImpactForce > 50f || impact.ImpactDamage > 10f)
		{
			if (impact.IsPlayerImpact(out var player))
			{
				if (Impact.IsLethal(impact.ImpactType))
				{
					RespondToLethalAttack(player, impact);
				}
				else if (Time.time - timeSinceLastImpact < 20f)
				{
					RespondToRepeatedNonLethalAttack(player, impact);
				}
				else
				{
					RespondToFirstNonLethalAttack(player, impact);
				}
			}
		}
		else if (impact.IsPlayerImpact(out player2))
		{
			RespondToAnnoyingImpact(player2, impact);
		}
		timeSinceLastImpact = Time.time;
	}

	protected virtual void RespondToFirstNonLethalAttack(Player perpetrator, Impact impact)
	{
		if (Time.time - timeSinceLastImpact > 20f)
		{
			npc.RelationData.ChangeRelationship(0.25f);
		}
	}

	protected virtual void RespondToRepeatedNonLethalAttack(Player perpetrator, Impact impact)
	{
		if (Time.time - timeSinceLastImpact > 20f)
		{
			npc.RelationData.ChangeRelationship(-0.25f);
		}
	}

	protected virtual void RespondToLethalAttack(Player perpetrator, Impact impact)
	{
		if (Time.time - timeSinceLastImpact > 20f)
		{
			npc.RelationData.ChangeRelationship(-1f);
		}
	}

	protected virtual void RespondToAnnoyingImpact(Player perpetrator, Impact impact)
	{
	}

	public virtual void RespondToAimedAt(Player player)
	{
		if (Time.time - timeSinceAimedAt > 20f)
		{
			npc.RelationData.ChangeRelationship(-0.5f);
		}
		timeSinceAimedAt = Time.time;
	}
}
