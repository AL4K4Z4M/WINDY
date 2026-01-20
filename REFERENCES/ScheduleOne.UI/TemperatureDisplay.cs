using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Temperature;
using TMPro;
using UnityEngine;

namespace ScheduleOne.UI;

public class TemperatureDisplay : MonoBehaviour
{
	public const float MaxCameraDistance = 8f;

	public const float MinCameraDistance = 0.5f;

	public const float FadeInDistance = 2f;

	public const float FadeOutDistance = 0.25f;

	public bool UseColor;

	[SerializeField]
	private Gradient _temperatureColorGradient;

	[SerializeField]
	private TextMeshPro _label;

	private Func<float> _getCelsiusTemperature;

	private Func<bool> _getIsVisible;

	private void Awake()
	{
		_label.enabled = false;
	}

	private void LateUpdate()
	{
		UpdateCanvas();
	}

	private void UpdateCanvas()
	{
		if (Player.Local == null)
		{
			return;
		}
		if (_getCelsiusTemperature == null)
		{
			_label.enabled = false;
			return;
		}
		if (_getIsVisible != null && !_getIsVisible())
		{
			_label.enabled = false;
			return;
		}
		float num = Vector3.Distance(_label.transform.position, PlayerSingleton<PlayerCamera>.Instance.transform.position);
		if (num > 8f)
		{
			_label.enabled = false;
			return;
		}
		_label.transform.rotation = Quaternion.LookRotation(_label.transform.position - PlayerSingleton<PlayerCamera>.Instance.transform.position, PlayerSingleton<PlayerCamera>.Instance.transform.up);
		float a = 1f - Mathf.Clamp01(Mathf.InverseLerp(6f, 8f, num));
		float b = Mathf.Clamp01(Mathf.InverseLerp(0.5f, 0.75f, num));
		float celsius = _getCelsiusTemperature();
		Color color = Color.white;
		if (UseColor)
		{
			color = _temperatureColorGradient.Evaluate(TemperatureUtility.NormalizeTemperature(celsius));
		}
		color.a = Mathf.Min(a, b);
		_label.color = color;
		_label.text = TemperatureUtility.FormatTemperatureWithAppropriateUnit(celsius);
		_label.enabled = true;
	}

	public void SetTemperatureGetter(Func<float> getCelsiusTemperature)
	{
		_getCelsiusTemperature = getCelsiusTemperature;
	}

	public void SetVisibilityGetter(Func<bool> getIsVisible)
	{
		_getIsVisible = getIsVisible;
	}

	public void SetEnabled(bool enabled)
	{
		base.gameObject.SetActive(enabled);
	}
}
