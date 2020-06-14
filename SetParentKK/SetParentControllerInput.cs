using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;


namespace SetParentKK
{
	public partial class SetParent
	{
		private Dictionary<Side, GameObject> controllers;

		private FieldInfo f_device;

		private VRViveController leftVVC;
		private VRViveController rightVVC;

		private SteamVR_Controller.Device leftDevice;
		private SteamVR_Controller.Device rightDevice;


		private bool RightTrackPadPressing()
		{
			return (rightVVC?.IsState(VRViveController.EViveButtonKind.Touchpad, -1) ?? false) || (rightDevice?.GetPress((ulong)1 << 32) ?? false);
		}

		private bool LeftTrackPadPressing()
		{
			return (leftVVC?.IsState(VRViveController.EViveButtonKind.Touchpad, -1) ?? false) || (leftDevice?.GetPress((ulong)1 << 32) ?? false);
		}

		private bool RightTrackPadUp()
		{
			return (rightVVC?.IsState(VRViveController.EViveButtonKind.Touchpad_Up, -1) ?? false) || rightDevice?.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0).y > 0.7f;
		}

		private bool LeftTrackPadUp()
		{
			return (leftVVC?.IsState(VRViveController.EViveButtonKind.Touchpad_Up, -1) ?? false) || leftDevice?.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0).y > 0.7f;
		}

		private bool RightTrackPadDown()
		{
			return (rightVVC?.IsState(VRViveController.EViveButtonKind.Touchpad_Down, -1) ?? false) || rightDevice?.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0).y < -0.7f;
		}

		private bool LeftTrackPadDown()
		{
			return (leftVVC?.IsState(VRViveController.EViveButtonKind.Touchpad_Down, -1) ?? false) || leftDevice?.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0).y < -0.7f;
		}

		private bool RightMenuPressing()
		{
			return (rightVVC?.IsState(VRViveController.EViveButtonKind.Menu, -1) ?? false) || (rightDevice?.GetPress((ulong)1 << 1) ?? false);
		}

		private bool RightMenuPressDown()
		{
			return (rightVVC?.IsPressDown(VRViveController.EViveButtonKind.Menu, -1) ?? false) || (rightDevice?.GetPressDown((ulong)1 << 1) ?? false);
		}

		private bool LeftMenuPressing()
		{
			return (leftVVC?.IsState(VRViveController.EViveButtonKind.Menu, -1) ?? false) || (leftDevice?.GetPress((ulong)1 << 1) ?? false);
		}

		private bool LeftMenuPressDown()
		{
			return (leftVVC?.IsPressDown(VRViveController.EViveButtonKind.Menu, -1) ?? false) || (leftDevice?.GetPressDown((ulong)1 << 1) ?? false);
		}

		private bool RightTriggerPressDown()
		{
			return (rightVVC?.IsPressDown(VRViveController.EViveButtonKind.Trigger, -1) ?? false) || (rightDevice?.GetPressDown((ulong)1 << 33) ?? false);
		}

		private bool LeftTriggerPressDown()
		{
			return (leftVVC?.IsPressDown(VRViveController.EViveButtonKind.Trigger, -1) ?? false) || (leftDevice?.GetPressDown((ulong)1 << 33) ?? false);
		}

		private bool RightTriggerRelease()
		{
			return (rightVVC?.IsPressUp(VRViveController.EViveButtonKind.Trigger, -1) ?? false) || (rightDevice?.GetPressUp((ulong)1 << 33) ?? false);
		}

		private bool LeftTriggerRelease()
		{
			return (leftVVC?.IsPressUp(VRViveController.EViveButtonKind.Trigger, -1) ?? false) || (leftDevice?.GetPressUp((ulong)1 << 33) ?? false);
		}

		private bool RightTriggerPressing()
		{
			return (rightVVC?.IsState(VRViveController.EViveButtonKind.Trigger, -1) ?? false) || (rightDevice?.GetPress((ulong)1 << 33) ?? false);
		}

		private bool LeftTriggerPressing()
		{
			return (leftVVC?.IsState(VRViveController.EViveButtonKind.Trigger, -1) ?? false) || (leftDevice?.GetPress((ulong)1 << 33) ?? false);
		}

		private bool LeftGripPressing()
		{
			return (leftVVC?.IsState(VRViveController.EViveButtonKind.Grip, -1) ?? false) || (leftDevice?.GetPress((ulong)1 << 3) ?? false);
		}

		private bool LeftGripPressDown()
		{
			return (leftVVC?.IsPressDown(VRViveController.EViveButtonKind.Grip, -1) ?? false) || (leftDevice?.GetPressDown((ulong)1 << 3) ?? false);
		}

		private bool RightGripPressing()
		{
			return (rightVVC?.IsState(VRViveController.EViveButtonKind.Grip, -1) ?? false) || (rightDevice?.GetPress((ulong)1 << 3) ?? false);
		}

		private bool RightGripPressDown()
		{
			return (rightVVC?.IsPressDown(VRViveController.EViveButtonKind.Grip, -1) ?? false) || (rightDevice?.GetPressDown((ulong)1 << 3) ?? false);
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
			LeftGripped,
			RightGripped
		}
	}
}
