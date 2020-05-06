using System.Reflection;
using System;
using Harmony;
using UnityEngine;
using KoikatuVRAssistPlugin;
using static SetParentKK.SetParentLoader;
using static SetParentKK.SetParent;


namespace SetParentKK
{
	//Workarounds for controller input conflicts with KoikatuVRAssistPlugin
	public static class KoikatuVRAssistPluginHooks
	{
		//Prevent KoikatuVRAssistPlugin from dragging the menu while SetParent is using the same controller input to adjust characters
		[HarmonyPrefix]
		[HarmonyPatch(typeof(GripMoveAssistObj), "PerformFloatingMainMenu")]
		public static void PerformFloatingMainMenuPre(object __instance)
		{
			if ((setParentObj?.setFlag ?? false) && (setParentObj.currentCtrlstate >= CtrlState.Following))
			{
				//Prevent KoikatuVRAssistPlugin from dragging the menu by resetting its timer for initiating the drag
				float[] gripDownTime = (float[]) Traverse.Create(__instance).Field("gripDownTime").GetValue();
				if (setParentObj.parentIsLeft)
					gripDownTime[1] = Time.time;
				else
					gripDownTime[0] = Time.time;
			}
		}

		//Prevent KoikatuVRAssist plugin from modifying the animation speed gauge when the thumbstick is used by SetParent
		[HarmonyPrefix]
		[HarmonyPatch(typeof(GripMoveAssistObj), "ProcSpeedUpClick")]
		public static bool ProcSpeedUpClickPre()
		{
			if ((setParentObj?.setFlag ?? false) && (setParentObj.currentCtrlstate >= CtrlState.MaleControl || SetParentMode.Value >= ParentMode.PositionAndAnimation))
			{
				return false;
			}
			else
				return true;
		}
	}

}
