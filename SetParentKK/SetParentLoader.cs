using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using BepInEx;
using Harmony;

namespace SetParentKK
{
	[BepInPlugin(GUID, PluginName, Version)]
	public class SetParentLoader : BaseUnityPlugin
	{
		public const string GUID = "uppervolta.setparentVR";
		public const string Version = "2.0.0";
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
		public static ConfigWrapper<float> Finishcount { get; private set; }

		[DisplayName("Synchronize Male's Head with Headset")]
		[Description("If enabled, the male body will rotate to align his head with the headset")]
		public static ConfigWrapper<bool> SetParentMale { get; private set; }

		[DisplayName("Make Girl's Hands and Feet Grab onto Objects")]
		[Description("If enabled, girl's hands and feet will automatically grab onto objects when in contact")]
		public static ConfigWrapper<bool> SetFemaleCollider { get; private set; }

		[DisplayName("Make Man's Feet Grab onto Objects")]
		[Description("If enabled, man's feet will automatically grab onto objects when in contact. \nThis is useful to prevent man's feet from clipping into the ground")]
		public static ConfigWrapper<bool> SetMaleCollider { get; private set; }

		[DisplayName("Part of Girl's Body to Parent with The Controller")]
		[Description("Use this body to act as the center/origin that will correspond to the movement and rotation of the controller")]
		public static ConfigWrapper<BodyPart> ParentPart { get; private set; }

		[DisplayName("Smooth Tracking")]
		[Description("Enables smooth following of the girl's body to the controller. \nDisable to use strict and immediate following")]
		public static ConfigWrapper<bool> TrackingMode { get; private set; }

		[DisplayName("Gaze Control")]
		[Description("Enables selecting any menu item by looking at it for more than 1 second")]
		public static ConfigWrapper<bool> GazeControl { get; private set; }

		[DisplayName("Hide Floating Menu by Default")]
		[Description("Hides floating menu by default. \nBring it up by holding the menu/B button for more than 1 second, or bring the controller close to the headset when SetParent is active")]
		public static ConfigWrapper<bool> MenuHideDefault { get; private set; }

		///
		//////////////////// Advanced Settings /////////////////////////// 
		///
		[Category("Advanced Settings")]
		[DisplayName("Controller Movement Pool Size (frames)")]
		[Description("Movement amount of the controller will be calculated using the sum of distance moved in this number of frames")]
		public static ConfigWrapper<int> MoveDistancePoolSize { get; private set; }

		[Category("Advanced Settings")]
		[DisplayName("Animation Start Threshold")]
		[Description("Movement amount of the controller above this threshold will cause piston animation to start. Unit in meters")]
		public static ConfigWrapper<float> AnimStartThreshold { get; private set; }

		[Category("Advanced Settings")]
		[DisplayName("Animation Max Threshold")]
		[Description("When movement amount of the controller reaches this value, animation speed will be at max. Unit in meters")]
		public static ConfigWrapper<float> AnimMaxThreshold { get; private set; }

		[Category("Advanced Settings")]
		[DisplayName("Controller Average Position Pool Size (frames)")]
		[Description("Position of the controller will be calculated using the average position in this number of frames\n This value is used to calculate movement range of the controller")]
		public static ConfigWrapper<int> MoveCoordinatePoolSize { get; private set; }

		[Category("Advanced Settings")]
		[DisplayName("Strong Motion Threshold")]
		[Description("If the movement range of the controller exceeds this threshold, animation will switch to strong motion. Unit in meters")]
		public static ConfigWrapper<float> StrongMotionThreshold { get; private set; }

		[Category("Advanced Settings")]
		[DisplayName("Weak Motion Threshold")]
		[Description("If the movement range of the controller stays within this threshold for 1.5 seconds, animation will switch to weak motion. Unit in meters")]
		public static ConfigWrapper<float> WeakMotionThreshold { get; private set; }

		[Category("Advanced Settings")]
		[DisplayName("Strong Motion Speed Maximum Multiplier")]
		[Description("In strong motion, multiply the animation speed max threshold by this number to avoid reaching the maximum speed too easily due to the wide range of motion")]
		public static ConfigWrapper<float> StrongThresholdMultiplier { get; private set; }

		private void Start()
		{
			LoadFromModPref();

			if (!Application.dataPath.EndsWith("KoikatuVR_Data"))
				return;

			HarmonyInstance.Create(GUID).PatchAll(typeof(VRHScene_Load_Patch));
		}

		private void LoadFromModPref()
		{
			SetParentMode = new ConfigWrapper<ParentMode>("SetParentMode", this, ParentMode.PositionAndAnimation);
			CalcController = new ConfigWrapper<ControllerAnimMode>("CalcPattern", this, ControllerAnimMode.SetParentController);
			Finishcount = new ConfigWrapper<float>("Finishcount", this, 0f);
			SetParentMale = new ConfigWrapper<bool>("SetParentMale", this, true);
			SetFemaleCollider = new ConfigWrapper<bool>("SetFemaleCollider", this, true);
			SetMaleCollider = new ConfigWrapper<bool>("SetMaleCollider", this, true);
			ParentPart = new ConfigWrapper<BodyPart>("ParentPart", this, BodyPart.Belly);
			TrackingMode = new ConfigWrapper<bool>("TrackingMode", this, true);
			GazeControl = new ConfigWrapper<bool>("GazeControl", this, false);
			MenuHideDefault = new ConfigWrapper<bool>("MenuHideDefault", this, true);

			MoveDistancePoolSize = new ConfigWrapper<int>("MoveDistancePoolSize", this, 60);
			AnimStartThreshold = new ConfigWrapper<float>("AnimStartThreshold", this, 0.04f);
			AnimMaxThreshold = new ConfigWrapper<float>("AnimMaxThreshold", this, 0.2f);
			MoveCoordinatePoolSize = new ConfigWrapper<int>("MoveCoordinatePoolSize", this, 8);
			StrongMotionThreshold = new ConfigWrapper<float>("StrongMotionThreshold", this, 0.03f);
			WeakMotionThreshold = new ConfigWrapper<float>("WeakMotionThreshold", this, 0.01f);
			StrongThresholdMultiplier = new ConfigWrapper<float>("StrongThresholdMultiplier", this, 1.7f);
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
			Belly,
			Head
		}
	}
}
