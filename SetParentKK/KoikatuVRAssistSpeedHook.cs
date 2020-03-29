using System.Reflection;
using System;
using Harmony;
using static SetParentKK.SetParentLoader;
using static SetParentKK.SetParent;


namespace SetParentKK
{
	[HarmonyPatch]
	internal static class KoikatuVRAssistSpeedHook
	{
		private static bool Prepare()
		{
			if (Type.GetType("KoikatuVRAssistPlugin.GripMoveAssistObj, KoikatuVRAssistPlugin") != null)
			{
				BepInEx.Logger.Log(BepInEx.Logging.LogLevel.Error, PluginName + ": KoikatuVRAssist patched for compatibility");
				return true;
			}
			else
			{
				BepInEx.Logger.Log(BepInEx.Logging.LogLevel.Error, PluginName + ": KoikatuVRAssist not found, not patched");
				return false;
			}
				
		}

		private static MethodInfo TargetMethod()
		{
			return Type.GetType("KoikatuVRAssistPlugin.GripMoveAssistObj, KoikatuVRAssistPlugin").GetMethod("ProcSpeedUpClick", AccessTools.all);
		}

		private static bool Prefix()
		{
			if (setParentObj.setFlag && (setParentObj.currentCtrlstate >= CtrlState.MaleControl || SetParentMode.Value >= ParentMode.PositionAndAnimation))
			{
				return false;
			}
			else
				return true;
		}
	}
 
}
