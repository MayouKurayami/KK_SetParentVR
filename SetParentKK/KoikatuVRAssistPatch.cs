using System.Reflection;
using Harmony;
using static SetParentKK.SetParentLoader;
using static SetParentKK.SetParent;


namespace SetParentKK
{
	[HarmonyPatch]
	internal static class KoikatuVRAssistPatch
	{
		private static bool Prepare()
		{
			if (typeof(KoikatuVRAssistPlugin.GripMoveAssistObj) != null)
			{
				BepInEx.Logger.Log(BepInEx.Logging.LogLevel.Debug, PluginName + ": KoikatuVRAssist Patched for Compatibility");
				return true;
			}
			else
				return false;
		}

		private static MethodInfo TargetMethod()
		{
			return typeof(KoikatuVRAssistPlugin.GripMoveAssistObj).GetMethod("ProcSpeedUpClick", AccessTools.all);
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
