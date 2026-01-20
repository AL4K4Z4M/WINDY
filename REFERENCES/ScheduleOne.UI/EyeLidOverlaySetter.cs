using ScheduleOne.DevUtilities;
using UnityEngine;

namespace ScheduleOne.UI;

public class EyeLidOverlaySetter : MonoBehaviour
{
	[Range(0f, 1f)]
	public float OpenOverride = 1f;

	private void OnEnable()
	{
		if (Singleton<EyelidOverlay>.InstanceExists)
		{
			Singleton<EyelidOverlay>.Instance.AutoUpdate = false;
		}
	}

	private void OnDisable()
	{
		if (Singleton<EyelidOverlay>.InstanceExists)
		{
			Singleton<EyelidOverlay>.Instance.AutoUpdate = true;
		}
	}

	private void Update()
	{
		if (Singleton<EyelidOverlay>.InstanceExists)
		{
			Singleton<EyelidOverlay>.Instance.SetOpen(OpenOverride);
		}
	}
}
