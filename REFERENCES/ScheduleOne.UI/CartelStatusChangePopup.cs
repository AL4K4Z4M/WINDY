using System;
using System.Collections;
using ScheduleOne.Cartel;
using ScheduleOne.DevUtilities;
using ScheduleOne.Persistence;
using TMPro;
using UnityEngine;

namespace ScheduleOne.UI;

public class CartelStatusChangePopup : MonoBehaviour
{
	public Animation Anim;

	public TextMeshProUGUI OldStatusLabel;

	public TextMeshProUGUI NewStatusLabel;

	public Color UnknownColor;

	public Color TrucedColor;

	public Color HostileColor;

	public Color DefeatedColor;

	private void Start()
	{
		ScheduleOne.Cartel.Cartel instance = NetworkSingleton<ScheduleOne.Cartel.Cartel>.Instance;
		instance.OnStatusChange = (Action<ECartelStatus, ECartelStatus>)Delegate.Combine(instance.OnStatusChange, new Action<ECartelStatus, ECartelStatus>(Show));
	}

	public void Show(ECartelStatus oldStatus, ECartelStatus newStatus)
	{
		if (!Singleton<LoadManager>.Instance.IsLoading)
		{
			OldStatusLabel.text = oldStatus.ToString().ToUpper();
			OldStatusLabel.color = GetColor(oldStatus);
			NewStatusLabel.text = newStatus.ToString().ToUpper();
			NewStatusLabel.color = GetColor(newStatus);
			StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			yield return new WaitUntil(() => !Singleton<DialogueCanvas>.Instance.isActive);
			yield return new WaitForSeconds(0.5f);
			Anim.Play();
		}
	}

	private Color GetColor(ECartelStatus status)
	{
		return status switch
		{
			ECartelStatus.Unknown => UnknownColor, 
			ECartelStatus.Truced => TrucedColor, 
			ECartelStatus.Hostile => HostileColor, 
			ECartelStatus.Defeated => DefeatedColor, 
			_ => Color.white, 
		};
	}
}
