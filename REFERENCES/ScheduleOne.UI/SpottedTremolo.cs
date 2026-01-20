using ScheduleOne.Audio;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Vision;
using UnityEngine;

namespace ScheduleOne.UI;

public class SpottedTremolo : MonoBehaviour
{
	[Range(0f, 1f)]
	public float Intensity;

	public AudioSourceController Loop;

	public EntityVisibility PlayerVisibility;

	[Header("Settings")]
	public float MinVolume;

	public float MaxVolume = 1f;

	public float MinPitch = 0.9f;

	public float MaxPitch = 1.2f;

	public float SmoothTime = 0.5f;

	[Range(0f, 1f)]
	[SerializeField]
	private float smoothedIntensity;

	public void Update()
	{
		Intensity = 0f;
		VisionEvent highestProgressionEvent = ((ISightable)Player.Local).HighestProgressionEvent;
		if (highestProgressionEvent != null && highestProgressionEvent.playTremolo)
		{
			Intensity = highestProgressionEvent.NormalizedNoticeLevel;
		}
		if (Intensity > smoothedIntensity)
		{
			smoothedIntensity = Mathf.MoveTowards(smoothedIntensity, Intensity, Time.deltaTime / SmoothTime);
		}
		else
		{
			smoothedIntensity = Mathf.MoveTowards(smoothedIntensity, Intensity, Time.deltaTime / 3f);
		}
		float num = Mathf.Lerp(MinVolume, MaxVolume, smoothedIntensity);
		Loop.volumeMultiplier = num;
		Loop.pitchMultiplier = Mathf.Lerp(MinPitch, MaxPitch, smoothedIntensity);
		Loop.ApplyPitch();
		if (num > 0f && !Loop.isPlaying)
		{
			Loop.Play();
		}
		else if (num <= 0f && Loop.isPlaying)
		{
			Loop.Stop();
		}
	}
}
