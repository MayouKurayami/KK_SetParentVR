using System;
using Harmony;


namespace SetParentKK
{
	[HarmonyPatch]
	public static class KoikatuVRAssistPatch
	{
		public static bool Prepare()
		{
			if (Type.GetType("GripMoveAssistObj") != null)
			{
				Console.WriteLine("KoikatuVRAssistPlugin found, proceed to patch");
				return true;
			}
			else
			{
				Console.WriteLine("KoikatuVRAssistPlugin not found");
				return false;
			}
				
		}
	}
 
}
