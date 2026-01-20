using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Vehicles;
using TMPro;
using UnityEngine;

namespace ScheduleOne.UI;

public class VehicleCanvas : Singleton<VehicleCanvas>
{
	[Header("References")]
	public Canvas Canvas;

	public TextMeshProUGUI SpeedText;

	public GameObject DriverPromptsContainer;

	private LandVehicle currentVehicle;

	protected override void Start()
	{
		base.Start();
		Player.onLocalPlayerSpawned = (Action)Delegate.Combine(Player.onLocalPlayerSpawned, new Action(Subscribe));
	}

	private void Subscribe()
	{
		Player local = Player.Local;
		local.onEnterVehicle = (Player.VehicleEvent)Delegate.Combine(local.onEnterVehicle, new Player.VehicleEvent(VehicleEntered));
		Player local2 = Player.Local;
		local2.onExitVehicle = (Player.VehicleTransformEvent)Delegate.Combine(local2.onExitVehicle, new Player.VehicleTransformEvent(VehicleExited));
	}

	private void Update()
	{
		if (!(Player.Local == null) && Player.Local.CurrentVehicle != null)
		{
			Canvas.enabled = !Singleton<GameplayMenu>.Instance.IsOpen;
		}
	}

	private void LateUpdate()
	{
		if (currentVehicle != null)
		{
			UpdateSpeedText();
		}
	}

	private void VehicleEntered(LandVehicle veh)
	{
		currentVehicle = veh;
		UpdateSpeedText();
		Canvas.enabled = true;
		DriverPromptsContainer.SetActive(currentVehicle.LocalPlayerIsDriver);
	}

	private void VehicleExited(LandVehicle veh, Transform exitPoint)
	{
		Canvas.enabled = false;
		currentVehicle = null;
	}

	private void UpdateSpeedText()
	{
		if (!(SpeedText == null))
		{
			SpeedText.text = UnitsUtility.FormatSpeed(Mathf.Abs(currentVehicle.VelocityCalculator.Velocity.magnitude * 1.4f), UnitsUtility.ERoundingType.Nearest, 0);
		}
	}
}
