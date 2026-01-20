using ScheduleOne.DevUtilities;
using UnityEngine;

namespace ScheduleOne.UI;

public static class UnitsUtility
{
	public enum ERoundingType
	{
		None,
		Nearest,
		Up,
		Down
	}

	public static string FormatShortDistance(float meters, ERoundingType roundingType = ERoundingType.Nearest, int decimalPoints = 0)
	{
		RoundValue(meters, ERoundingType.Nearest, decimalPoints);
		if (Singleton<ScheduleOne.DevUtilities.Settings>.InstanceExists && Singleton<ScheduleOne.DevUtilities.Settings>.Instance.UnitType == ScheduleOne.DevUtilities.Settings.EUnitType.Imperial)
		{
			float num = meters * 3.28084f;
			return $"{num:F0}ft";
		}
		return $"{meters:F0}m";
	}

	public static string FormatSpeed(float metersPerSecond, ERoundingType roundingType = ERoundingType.Nearest, int decimalPoints = 1)
	{
		if (Singleton<ScheduleOne.DevUtilities.Settings>.InstanceExists && Singleton<ScheduleOne.DevUtilities.Settings>.Instance.UnitType == ScheduleOne.DevUtilities.Settings.EUnitType.Imperial)
		{
			return RoundValue(metersPerSecond * 2.23694f, roundingType, decimalPoints).ToString($"F{decimalPoints}") + " mph";
		}
		return RoundValue(metersPerSecond * 3.6f, roundingType, decimalPoints).ToString($"F{decimalPoints}") + " km/h";
	}

	private static float RoundValue(float value, ERoundingType roundingType, int decimalPoints)
	{
		float num = Mathf.Pow(10f, decimalPoints);
		return roundingType switch
		{
			ERoundingType.None => value, 
			ERoundingType.Nearest => Mathf.Round(value * num) / num, 
			ERoundingType.Up => Mathf.Ceil(value * num) / num, 
			ERoundingType.Down => Mathf.Floor(value * num) / num, 
			_ => value, 
		};
	}
}
