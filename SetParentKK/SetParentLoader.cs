using System;
using System.Reflection;
using System.ComponentModel;
using UnityEngine;
using BepInEx;
using BepInEx.Logging;
using Harmony;

namespace SetParentKK
{
	[BepInPlugin(GUID, PluginName, Version)]
	public class SetParentLoader : BaseUnityPlugin
	{
		public const string GUID = "MK.KK_SetParentVR";
		public const string Version = "2.1.0";
		public const string PluginName = "SetParentVR";
		public const string AssembName = "KK_SetParentVR";
		internal static SetParent setParentObj;

		[DisplayName("Control Mode")]
		[Description("Use the controller to control the girl's position, animation, or both")]
		public static ConfigWrapper<ParentMode> SetParentMode { get; private set; }

		[DisplayName("Which Controller Controls Animation")]
		[Description("Select which controller affects animation speed and switching between weak/strong motion")]
		public static ConfigWrapper<ControllerAnimMode> CalcController { get; private set; }

		[DisplayName("Autofinish Timer")]
		[Description("When the girl's excitement gauge is above the cum threshold (70), she will cum after this set number of seconds \nSet this to 0 to disable the feature")]
		[AcceptableValueRange(0, float.MaxValue, false)]
		public static ConfigWrapper<float> Finishcount { get; private set; }

		[DisplayName("Synchronize Male's Head with Headset")]
		[Description("If enabled, the male body will rotate to align his head with the headset")]
		public static ConfigWrapper<bool> SetParentMale { get; private set; }

		[DisplayName("Synchronize Male's Hands with Controllers")]
		[Description("If enabled, the male's hands and arms will be synchronized to the controllers")]
		public static ConfigWrapper<bool> SyncMaleHands { get; private set; }

		[DisplayName("Enable Holding Girl's Limbs with Controllers")]
		[Description("If enabled, touching the girl's hands or feet with a controller will cause it to stick to the controller")]
		public static ConfigWrapper<bool> SetControllerCollider { get; private set; }

		[DisplayName("Distance to Detach Female Arms")]
		[Description("When stretched above this distance, the arms that are currently attached to objects will detach. This has no effect when the arms are attached manually via the floating menu button or by the controller. \nSet to 0 to disable automatic attachment of hands to objects")]
		[AcceptableValueRange(0, float.MaxValue, false)]
		public static ConfigWrapper<float> StretchLimitArms { get; private set; }

		[DisplayName("Distance to Detach Female Legs")]
		[Description("When stretched above this distance, the legs that are currently attached to objects will detach. This has no effect when the legs are attached manually via the floating menu button or by the controller. \nSet to 0 to disable automatic attachment of legs to objects")]
		[AcceptableValueRange(0, float.MaxValue, false)]
		public static ConfigWrapper<float> StretchLimitLegs { get; private set; }

		[DisplayName("Make Male's Feet Stick to Objects")]
		[Description("If enabled, male's feet will automatically grab onto objects when in contact. \nThis is useful to prevent male's feet from clipping into the ground")]
		public static ConfigWrapper<bool> SetMaleCollider { get; private set; }

		[DisplayName("Gaze Control")]
		[Description("Enables selecting any menu item by looking at it for more than 1 second")]
		public static ConfigWrapper<bool> GazeControl { get; private set; }

		[DisplayName("Hide Floating Menu by Default")]
		[Description("Hides floating menu by default. \nBring it up by holding the menu/B button for more than 1 second, or bring the controller close to the headset when SetParent is active")]
		public static ConfigWrapper<bool> MenuHideDefault { get; private set; }

		[DisplayName("Distance to Display Hidden Menu")]
		[Description("When the floating menu is hidden, bring the controller close to the headset within this distance to temporarily display the menu. Unit in meters. \nSet to 0 to disable this feature")]
		[AcceptableValueRange(0, float.MaxValue, false)]
		public static ConfigWrapper<float> MenuUpProximity { get; private set; }

		[DisplayName("Disable Input of Parent Controller")]
		[Description("If enabled, input for the parent controller will be turned off to prevent accidental input")]
		public static ConfigWrapper<bool> DisableParentInput { get; private set; }


		///
		//////////////////// Keyboard Shortcuts /////////////////////////// 
		///
		[DisplayName("Limb Release")]
		[Description("Press this key to release all limbs from attachment")]
		public static SavedKeyboardShortcut LimbReleaseKey { get; private set; }

		[DisplayName("Male Feet Fix/Release")]
		[Description("Press this key to fix or release male's feet in place")]
		public static SavedKeyboardShortcut MaleFeetToggle { get; private set; }

		[DisplayName("SetParent Toggle")]
		[Description("Press this key to enable/disable SetParent plugin using the left controller as parent")]
		public static SavedKeyboardShortcut SetParentToggle { get; private set; }


		///
		//////////////////// Advanced Settings /////////////////////////// 
		///
		[Category("Advanced Settings")]
		[DisplayName("Part of Girl's Body to Parent with The Controller")]
		[Description("Use this body part to act as the center/origin that will correspond to the movement and rotation of the controller")]
		public static ConfigWrapper<BodyPart> ParentPart { get; private set; }

		[Category("Advanced Settings")]
		[DisplayName("Smooth Tracking")]
		[Description("Enables smooth following of the girl's body to the controller. \nDisable to use strict and immediate following")]
		public static ConfigWrapper<bool> TrackingMode { get; private set; }

		[Category("Advanced Settings")]
		[DisplayName("Controller Movement Pool Size (frames)")]
		[Description("Movement amount of the controller will be calculated using the sum of distance moved in this number of frames")]
		[AcceptableValueRange(0, int.MaxValue, false)]
		public static ConfigWrapper<int> MoveDistancePoolSize { get; private set; }

		[Category("Advanced Settings")]
		[DisplayName("Animation Start Threshold")]
		[Description("Movement amount of the controller above this threshold will cause piston animation to start. Unit in meters")]
		[AcceptableValueRange(0, float.MaxValue, false)]
		public static ConfigWrapper<float> AnimStartThreshold { get; private set; }

		[Category("Advanced Settings")]
		[DisplayName("Animation Max Threshold")]
		[Description("When movement amount of the controller reaches this value, animation speed will be at max. Unit in meters")]
		[AcceptableValueRange(0, float.MaxValue, false)]
		public static ConfigWrapper<float> AnimMaxThreshold { get; private set; }

		[Category("Advanced Settings")]
		[DisplayName("Controller Average Position Pool Size (frames)")]
		[Description("Position of the controller will be calculated using the average position in this number of frames\n This value is used to calculate movement range of the controller")]
		[AcceptableValueRange(0, int.MaxValue, false)]
		public static ConfigWrapper<int> MoveCoordinatePoolSize { get; private set; }

		[Category("Advanced Settings")]
		[DisplayName("Strong Motion Threshold")]
		[Description("If the movement range of the controller exceeds this threshold, animation will switch to strong motion. Unit in meters")]
		[AcceptableValueRange(0, float.MaxValue, false)]
		public static ConfigWrapper<float> StrongMotionThreshold { get; private set; }

		[Category("Advanced Settings")]
		[DisplayName("Weak Motion Threshold")]
		[Description("If the movement range of the controller stays within this threshold for 1.5 seconds, animation will switch to weak motion. Unit in meters")]
		[AcceptableValueRange(0, float.MaxValue, false)]
		public static ConfigWrapper<float> WeakMotionThreshold { get; private set; }

		[Category("Advanced Settings")]
		[DisplayName("Strong Motion Speed Maximum Multiplier")]
		[Description("In strong motion, multiply the animation speed max threshold by this number to avoid reaching the maximum speed too easily due to the wide range of motion")]
		[AcceptableValueRange(0, float.MaxValue, false)]
		public static ConfigWrapper<float> StrongThresholdMultiplier { get; private set; }

		[Category("Advanced Settings")]
		[DisplayName("Male Yaw Rotation")]
		[Description("Enable/disable male body's yaw (left/right) rotation when male synchronization is enabled")]
		public static ConfigWrapper<bool> MaleYaw { get; private set; }

		[Category("Advanced Settings")]
		[DisplayName("Male Hands Display Mode")]
		[Description("When male hands are synchronized with the controllers, this controls whether additioanl hands will show on female body parts when you touch them. \nSet to auto to automatically hide them based on the proximity of your hands.")]
		public static ConfigWrapper<HideHandMode> MaleHandsDisplay { get; private set; }

		[Category("Advanced Settings")]
		[DisplayName("Hide Parent Controller")]
		[Description("Hides the parent controller in animation only mode")]
		public static ConfigWrapper<bool> HideParentCon { get; private set; }


		private void Start()
		{
			LoadFromModPref();

			if (!Application.dataPath.EndsWith("KoikatuVR_Data"))
				return;

			HarmonyInstance harmony = HarmonyInstance.Create(GUID);
			harmony.PatchAll(typeof(SetParentHooks));
			if (Type.GetType("KoikatuVRAssistPlugin.GripMoveAssistObj, KoikatuVRAssistPlugin") != null)
			{
				BepInEx.Logger.Log(LogLevel.Debug, PluginName + ": KoikatuVRAssist patched for compatibility");
				harmony.PatchAll(typeof(KoikatuVRAssistPluginHooks));
			}
		
		}

		private void LoadFromModPref()
		{
			SetParentMode = new ConfigWrapper<ParentMode>(nameof(SetParentMode), this, ParentMode.PositionAndAnimation);
			CalcController = new ConfigWrapper<ControllerAnimMode>(nameof(CalcController), this, ControllerAnimMode.SetParentController);
			Finishcount = new ConfigWrapper<float>(nameof(Finishcount), this, 0f);
			SetParentMale = new ConfigWrapper<bool>(nameof(SetParentMale), this, true);
			SyncMaleHands = new ConfigWrapper<bool>(nameof(SyncMaleHands), this, true);
			SetControllerCollider = new ConfigWrapper<bool>(nameof(SetControllerCollider), this, true);
			SetMaleCollider = new ConfigWrapper<bool>(nameof(SetMaleCollider), this, true);
			ParentPart = new ConfigWrapper<BodyPart>(nameof(ParentPart), this, BodyPart.Torso);
			TrackingMode = new ConfigWrapper<bool>(nameof(TrackingMode), this, true);
			GazeControl = new ConfigWrapper<bool>(nameof(GazeControl), this, false);
			MenuHideDefault = new ConfigWrapper<bool>(nameof(MenuHideDefault), this, true);
			MenuUpProximity = new ConfigWrapper<float>(nameof(MenuUpProximity), this, 0.25f);
			StretchLimitArms = new ConfigWrapper<float>(nameof(StretchLimitArms), this, 0.5f);
			StretchLimitLegs = new ConfigWrapper<float>(nameof(StretchLimitLegs), this, 0.7f);
			DisableParentInput = new ConfigWrapper<bool>(nameof(DisableParentInput), this, true);

			LimbReleaseKey = new SavedKeyboardShortcut(nameof(LimbReleaseKey), this, new KeyboardShortcut(KeyCode.None));
			SetParentToggle = new SavedKeyboardShortcut(nameof(SetParentToggle), this, new KeyboardShortcut(KeyCode.None));
			MaleFeetToggle = new SavedKeyboardShortcut(nameof(MaleFeetToggle), this, new KeyboardShortcut(KeyCode.None));

			MoveDistancePoolSize = new ConfigWrapper<int>(nameof(MoveDistancePoolSize), this, 55);
			AnimStartThreshold = new ConfigWrapper<float>(nameof(AnimStartThreshold), this, 0.04f);
			AnimMaxThreshold = new ConfigWrapper<float>(nameof(AnimMaxThreshold), this, 0.2f);
			MoveCoordinatePoolSize = new ConfigWrapper<int>(nameof(MoveCoordinatePoolSize), this, 8);
			StrongMotionThreshold = new ConfigWrapper<float>(nameof(StrongMotionThreshold), this, 0.03f);
			WeakMotionThreshold = new ConfigWrapper<float>(nameof(WeakMotionThreshold), this, 0.01f);
			StrongThresholdMultiplier = new ConfigWrapper<float>(nameof(StrongThresholdMultiplier), this, 1.3f);
			MaleYaw = new ConfigWrapper<bool>(nameof(MaleYaw), this, true);
			MaleHandsDisplay = new ConfigWrapper<HideHandMode>(nameof(MaleHandsDisplay), this, HideHandMode.Auto);
			HideParentCon = new ConfigWrapper<bool>(nameof(HideParentCon), this, true);
		}
		public enum ParentMode
		{
			PositionOnly,
			PositionAndAnimation,
			AnimationOnly
		}

		public enum ControllerAnimMode
		{
			SetParentController,
			OtherController,
			BothControllers
		}

		public enum BodyPart
		{
			Ass,
			Torso,
			Head
		}

		public enum HideHandMode
		{
			AlwaysHide,
			Auto,
			AlwaysShow
		}
	}
}
