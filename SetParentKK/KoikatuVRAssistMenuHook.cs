using System.Reflection;
using System;
using Harmony;
using UnityEngine;
using static SetParentKK.SetParentLoader;
using static SetParentKK.SetParent;


namespace SetParentKK
{
	[HarmonyPatch]
	internal static class KoikatuVRAssistMenuHook
	{
		private static bool Prepare()
		{
			if (Type.GetType("KoikatuVRAssistPlugin.GripMoveAssistObj, KoikatuVRAssistPlugin") != null)
			{
				BepInEx.Logger.Log(BepInEx.Logging.LogLevel.Debug, PluginName + ": KoikatuVRAssist patched for compatibility");
				return true;
			}
			else
			{
				BepInEx.Logger.Log(BepInEx.Logging.LogLevel.Debug, PluginName + ": KoikatuVRAssist not found, not patched");
				return false;
			}
		}

		private static MethodInfo TargetMethod()
		{
			return Type.GetType("KoikatuVRAssistPlugin.GripMoveAssistObj, KoikatuVRAssistPlugin").GetMethod("PerformFloatingMainMenu", AccessTools.all);
		}

		private static void Prefix(object __instance)
		{
			if ((setParentObj?.setFlag ?? false) && (setParentObj.currentCtrlstate >= CtrlState.Following))
			{
				float[] gripDownTime = (float[]) Traverse.Create(__instance).Field("gripDownTime").GetValue();
				if (setParentObj.parentIsLeft)
					gripDownTime[1] = Time.time;
				else
					gripDownTime[0] = Time.time;
			}
		}
	}

}
