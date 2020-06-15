using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;


namespace SetParentKK
{
	public partial class SetParent
	{
		private Dictionary<Side, GameObject> controllers = new Dictionary<Side, GameObject>();

		private FieldInfo f_device;

		private Dictionary<Side, VRViveController> viveControllers = new Dictionary<Side, VRViveController>();

		private Dictionary<Side, SteamVR_Controller.Device> steamVRDevices = new Dictionary<Side, SteamVR_Controller.Device>();


		private bool TrackPadPressing(Side side)
		{
			return (viveControllers[side]?.IsState(VRViveController.EViveButtonKind.Touchpad, -1) ?? false) || (steamVRDevices[side]?.GetPress((ulong)1 << 32) ?? false);
		}

		private bool TrackPadUp(Side side)
		{
			return (viveControllers[side]?.IsState(VRViveController.EViveButtonKind.Touchpad_Up, -1) ?? false) || steamVRDevices[side]?.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0).y > 0.7f;
		}

		private bool TrackPadDown(Side side)
		{
			return (viveControllers[side]?.IsState(VRViveController.EViveButtonKind.Touchpad_Down, -1) ?? false) || steamVRDevices[side]?.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0).y < -0.7f;
		}

		private bool MenuPressing(Side side)
		{
			return (viveControllers[side]?.IsState(VRViveController.EViveButtonKind.Menu, -1) ?? false) || (steamVRDevices[side]?.GetPress((ulong)1 << 1) ?? false);
		}

		private bool MenuPressDown(Side side)
		{
			return (viveControllers[side]?.IsPressDown(VRViveController.EViveButtonKind.Menu, -1) ?? false) || (steamVRDevices[side]?.GetPressDown((ulong)1 << 1) ?? false);
		}

		private bool TriggerPressDown(Side side)
		{
			return (viveControllers[side]?.IsPressDown(VRViveController.EViveButtonKind.Trigger, -1) ?? false) || (steamVRDevices[side]?.GetPressDown((ulong)1 << 33) ?? false);
		}

		private bool TriggerRelease(Side side)
		{
			return (viveControllers[side]?.IsPressUp(VRViveController.EViveButtonKind.Trigger, -1) ?? false) || (steamVRDevices[side]?.GetPressUp((ulong)1 << 33) ?? false);
		}

		private bool TriggerPressing(Side side)
		{
			return (viveControllers[side]?.IsState(VRViveController.EViveButtonKind.Trigger, -1) ?? false) || (steamVRDevices[side]?.GetPress((ulong)1 << 33) ?? false);
		}

		private bool GripPressing(Side side)
		{
			return (viveControllers[side]?.IsState(VRViveController.EViveButtonKind.Grip, -1) ?? false) || (steamVRDevices[side]?.GetPress((ulong)1 << 3) ?? false);
		}

		private bool GripPressDown(Side side)
		{
			return (viveControllers[side]?.IsPressDown(VRViveController.EViveButtonKind.Grip, -1) ?? false) || (steamVRDevices[side]?.GetPressDown((ulong)1 << 3) ?? false);
		}

		private bool IsDoubleClick(TriggerState input, float threshold)
		{
			if (Time.time - lastTriggerRelease[(int)input] > threshold)
			{
				lastTriggerRelease[(int)input] = Time.time;
				return false;
			}
			else
			{
				lastTriggerRelease[(int)input] = Time.time;
				return true;
			}
		}

		private enum TriggerState
		{
			Left,
			Right,
			LeftWhileGripped,
			RightWhileGripped
		}
	}
}
