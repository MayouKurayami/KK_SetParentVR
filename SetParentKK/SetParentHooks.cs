using Harmony;
using System.Collections.Generic;
using UnityEngine;
using static ItemObject;
using static SetParentKK.SetParentLoader;

namespace SetParentKK
{
	public static class SetParentHooks
	{
		//This should hook to a method that loads as late as possible in the loading phase
		//Hooking method "MapSameObjectDisable" because: "Something that happens at the end of H scene loading, good enough place to hook" - DeathWeasel1337/Anon11
		//https://github.com/DeathWeasel1337/KK_Plugins/blob/master/KK_EyeShaking/KK.EyeShaking.Hooks.cs#L20
		[HarmonyPostfix]
		[HarmonyPatch(typeof(VRHScene), "MapSameObjectDisable")]
		public static void VRHSceneLoadPostfix(VRHScene __instance)
		{
			if (setParentObj != null)
				UnityEngine.Object.Destroy(setParentObj);

			setParentObj = __instance.gameObject.AddComponent<SetParent>();
			setParentObj.Init(__instance.sprites[0], (List<MotionIK>)Traverse.Create(__instance).Field("lstMotionIK").GetValue());
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(VRHScene), "ChangeAnimator")]
		public static void ChangeAnimatorPrefix(ref bool _isForceCameraReset)
		{
			_isForceCameraReset = true;
			if (setParentObj != null)
			{
				setParentObj.UnsetP();
				setParentObj.male_p_cf_bodybone.transform.localPosition = Vector3.zero;
				setParentObj.male_p_cf_bodybone.transform.localRotation = Quaternion.identity;
				setParentObj.female_p_cf_bodybone.transform.localPosition = Vector3.zero;
				setParentObj.female_p_cf_bodybone.transform.localRotation = Quaternion.identity;
			}			
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(ItemObject), "LoadItem")]
		public static void LoadItemPostfix(ItemObject __instance)
		{
			if (setParentObj != null)
			{
				Dictionary<int, Item> dictItem = (Dictionary<int, Item>)Traverse.Create(__instance).Field("dicItem").GetValue();
				foreach (Item item in dictItem.Values)
				{
					setParentObj.SetObjectColliders(item.objItem.transform);
				}
			}
		}

		/// <summary>
		/// Disable controller input when controller is being used as parent
		/// </summary>
		[HarmonyPrefix]
		[HarmonyPatch(typeof(VRViveController), "IsPressDown")]
		public static bool IsPressDownPre(VRViveController __instance, ref bool __result)
		{
			if (!DisableParentInput.Value)
				return true;
			
			if ((setParentObj?.setFlag ?? false) && setParentObj.parentController == __instance.gameObject)
			{
				__result = false;
				return false;
			}
			else
				return true;
		}

		/// <summary>
		/// Disable controller input when controller is being used as parent
		/// </summary>
		[HarmonyPrefix]
		[HarmonyPatch(typeof(VRViveController), "IsPressUp")]
		public static bool IsPressUpPre(VRViveController __instance, ref bool __result)
		{
			if (!DisableParentInput.Value)
				return true;

			if ((setParentObj?.setFlag ?? false) && setParentObj.parentController == __instance.gameObject)
			{
				__result = false;
				return false;
			}
			else
				return true;
		}
	}
}
