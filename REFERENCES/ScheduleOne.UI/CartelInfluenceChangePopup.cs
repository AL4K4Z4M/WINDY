using System;
using System.Collections;
using ScheduleOne.Cartel;
using ScheduleOne.DevUtilities;
using ScheduleOne.Map;
using ScheduleOne.Persistence;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI;

public class CartelInfluenceChangePopup : MonoBehaviour
{
	public const float SLIDER_ANIMATION_DURATION = 1.5f;

	public Animation Anim;

	public Slider Slider;

	public TextMeshProUGUI TitleLabel;

	public TextMeshProUGUI InfluenceCountLabel;

	private void Start()
	{
		CartelInfluence influence = NetworkSingleton<ScheduleOne.Cartel.Cartel>.Instance.Influence;
		influence.OnInfluenceChanged = (Action<EMapRegion, float, float>)Delegate.Combine(influence.OnInfluenceChanged, new Action<EMapRegion, float, float>(Show));
	}

	public void Show(EMapRegion region, float oldInfluence, float newInfluence)
	{
		if (!Singleton<LoadManager>.Instance.IsLoading && NetworkSingleton<ScheduleOne.Cartel.Cartel>.Instance.Status == ECartelStatus.Hostile && !(newInfluence >= oldInfluence))
		{
			StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			yield return new WaitUntil(() => !Singleton<DialogueCanvas>.Instance.isActive);
			yield return new WaitUntil(() => !Singleton<DealCompletionPopup>.Instance.IsPlaying);
			yield return new WaitUntil(() => !Singleton<NewCustomerPopup>.Instance.IsPlaying);
			yield return new WaitForSeconds(0.5f);
			SetDisplayedInfluence(oldInfluence);
			TitleLabel.text = "Benzies' Influence in " + region;
			Anim.Play();
			yield return new WaitForSeconds(0.8f);
			for (float i = 0f; i < 1.5f; i += Time.deltaTime)
			{
				float displayedInfluence = Mathf.Lerp(oldInfluence, newInfluence, i / 1.5f);
				SetDisplayedInfluence(displayedInfluence);
				yield return new WaitForEndOfFrame();
			}
		}
	}

	private void SetDisplayedInfluence(float influence)
	{
		InfluenceCountLabel.text = Mathf.RoundToInt(influence * 1000f) + " / 1000";
		Slider.value = influence;
	}
}
