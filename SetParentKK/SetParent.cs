﻿using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VRTK;
using static SetParentKK.SetParentLoader;
using Harmony;
using RootMotion.FinalIK;

namespace SetParentKK
{
	
	public class SetParent : MonoBehaviour
	{
		public void Start()
		{
			if (hSprite == null)
			{
				if (!(hSprite = GameObject.Find("VRTK/[VRTK_SDKManager]/SDKSetups/SteamVR/VRCameraBase/[CameraRig]/Controller (left)/Model/p_handL/HSceneMainCanvas/MainCanvas").GetComponent<HSprite>()))
				{
					BepInEx.Logger.Log(BepInEx.Logging.LogLevel.Error, "HSprite not found. SetParent will exit");
					Destroy(this);
				}
			}
			femaleExists = false;
			positionMenuPressed = false;
			hideCanvas = MenuHideDefault.Value;

			hFlag = hSprite.flags;
			f_device = typeof(VRViveController).GetField("device", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			cameraEye = hSprite.managerVR.objCamera;
			leftController = hSprite.managerVR.objMove.transform.Find("Controller (left)").gameObject;
			rightController = hSprite.managerVR.objMove.transform.Find("Controller (right)").gameObject;

			male = (ChaControl)Traverse.Create(hSprite).Field("male").GetValue();
			female = ((List<ChaControl>)Traverse.Create(hSprite).Field("females").GetValue())[0];
			obj_chaF_001 = female.objRoot;
			male_p_cf_bodybone = male.objAnim;
			female_p_cf_bodybone = female.objAnim;
			maleFBBIK = male_p_cf_bodybone.GetComponent<FullBodyBipedIK>();
			femaleFBBIK = female_p_cf_bodybone.GetComponent<FullBodyBipedIK>();

			femaleAim = femaleFBBIK.references.head.Find("cf_s_head/aim").gameObject;
			female_cf_t_hand_R = femaleFBBIK.solver.rightHandEffector.target.gameObject;
			female_cf_t_hand_L = femaleFBBIK.solver.leftHandEffector.target.gameObject;
			female_cf_t_leg_R = femaleFBBIK.solver.rightFootEffector.target.gameObject;
			female_cf_t_leg_L = femaleFBBIK.solver.leftFootEffector.target.gameObject;

			female_cf_j_root = femaleFBBIK.references.root.gameObject;
			female_cf_j_hips = femaleFBBIK.references.pelvis.gameObject;
			female_cf_n_height = femaleFBBIK.references.pelvis.parent.gameObject;
			female_cf_j_spine01 = femaleFBBIK.references.spine[0].gameObject;
			female_cf_j_spine02 = femaleFBBIK.references.spine[1].gameObject;
			female_cf_j_neck = femaleFBBIK.references.spine[2].gameObject;
			female_cf_j_spine03 = femaleFBBIK.references.spine[2].parent.gameObject;


			female_cf_pv_hand_R = female_cf_n_height.transform.Find("cf_pv_root/cf_pv_hand_R").gameObject;
			female_cf_pv_hand_L = female_cf_n_height.transform.Find("cf_pv_root/cf_pv_hand_L").gameObject;
			female_cf_pv_leg_R = female_cf_n_height.transform.Find("cf_pv_root/cf_pv_leg_R").gameObject;
			female_cf_pv_leg_L = female_cf_n_height.transform.Find("cf_pv_root/cf_pv_leg_L").gameObject;

			Transform male_cf_n_height = maleFBBIK.references.pelvis.parent;
			male_cf_pv_hand_R = male_cf_n_height.Find("cf_pv_root/cf_pv_hand_R").gameObject;
			male_cf_pv_hand_L = male_cf_n_height.Find("cf_pv_root/cf_pv_hand_L").gameObject;
		}

		private void InitCanvas()
		{
			GameObject rightHandCollider = new GameObject("RightHandCollider");
			rightHandCollider.AddComponent<FixBodyParts>().Init(this, FixBodyParts.bodyParts.hand_R);
			rightHandCollider.transform.parent = femaleFBBIK.solver.rightHandEffector.bone;
			rightHandCollider.transform.localPosition = Vector3.zero;
			
			GameObject leftHandCollider = new GameObject("LeftHandCollider");
			leftHandCollider.AddComponent<FixBodyParts>().Init(this, FixBodyParts.bodyParts.hand_L);
			leftHandCollider.transform.parent = femaleFBBIK.solver.leftHandEffector.bone;
			leftHandCollider.transform.localPosition = Vector3.zero;
		
			GameObject rightLegCollider = new GameObject("RightLegCollider");
			rightLegCollider.AddComponent<FixBodyParts>().Init(this, FixBodyParts.bodyParts.leg_R);
			rightLegCollider.transform.parent = femaleFBBIK.solver.rightFootEffector.bone;
			rightLegCollider.transform.localPosition = Vector3.zero;
			
			GameObject leftLegCollider = new GameObject("LeftLegCollider");
			leftLegCollider.AddComponent<FixBodyParts>().Init(this, FixBodyParts.bodyParts.leg_L);
			leftLegCollider.transform.parent = femaleFBBIK.solver.leftFootEffector.bone;
			leftLegCollider.transform.localPosition = Vector3.zero;

			objCanvasSetParent = new GameObject("CanvasSetParent", new Type[]
			{
				typeof(Canvas)
			});
			canvasSetParent = objCanvasSetParent.GetComponent<Canvas>();
			objCanvasSetParent.AddComponent<GraphicRaycaster>();
			objCanvasSetParent.AddComponent<VRTK_UICanvas>();
			objCanvasSetParent.AddComponent<VRTK_UIGraphicRaycaster>();
			canvasScalerSetParent = objCanvasSetParent.AddComponent<CanvasScaler>();
			canvasScalerSetParent.dynamicPixelsPerUnit = 20000f;
			canvasScalerSetParent.referencePixelsPerUnit = 80000f;
			canvasSetParent.renderMode = RenderMode.WorldSpace;
			objCanvasSetParent.transform.localScale = new Vector3(0.005f, 0.005f, 0.005f);
			if (GazeControl.Value)
			{
				VRTK_UIPointer vrtk_UIPointer = cameraEye.AddComponent<VRTK_UIPointer>();
				vrtk_UIPointer.activationButton = VRTK_ControllerEvents.ButtonAlias.Undefined;
				vrtk_UIPointer.activationMode = VRTK_UIPointer.ActivationMethods.AlwaysOn;
				vrtk_UIPointer.selectionButton = VRTK_ControllerEvents.ButtonAlias.Undefined;
				vrtk_UIPointer.clickMethod = VRTK_UIPointer.ClickMethods.ClickOnButtonUp;
				vrtk_UIPointer.clickAfterHoverDuration = 1f;
				vrtk_UIPointer.controller = cameraEye.AddComponent<VRTK_ControllerEvents>();
			}
			eventSystemSetParent = new GameObject("CanvasSetParentEventSystem", new Type[]
			{
				typeof(EventSystem)
			});
			eventSystemSetParent.AddComponent<StandaloneInputModule>();
			eventSystemSetParent.transform.SetParent(objCanvasSetParent.transform);
			GameObject gameObject5 = new GameObject("button");
			GameObject gameObject6 = new GameObject("text", new Type[]
			{
				typeof(Text)
			});
			Text text = gameObject6.GetComponent<Text>();
			Image image = gameObject5.AddComponent<Image>();
			image.rectTransform.sizeDelta = new Vector2(0.24f, 0.08f);
			image.color = new Color(0.8f, 0.8f, 0.8f);
			Button button = gameObject5.AddComponent<Button>();
			ColorBlock colors = button.colors;
			colors.highlightedColor = Color.red;
			colors.pressedColor = Color.cyan;
			button.colors = colors;
			text.rectTransform.sizeDelta = new Vector2(0.24f, 0.08f);
			text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
			text.fontSize = 13;
			text.alignment = TextAnchor.MiddleCenter;
			text.text = "右手固定";
			gameObject5.transform.SetParent(objCanvasSetParent.transform);
			gameObject5.transform.localPosition = new Vector3(-28f, -48f, 0f);
			gameObject6.transform.SetParent(gameObject5.transform);
			gameObject6.transform.localPosition = Vector3.zero;
			gameObject5.GetComponent<Button>().onClick.AddListener(delegate ()
			{
				PushFixRightHandButton();
			});
			gameObject5 = new GameObject("button");
			gameObject6 = new GameObject("text", new Type[]
			{
				typeof(Text)
			});
			text = gameObject6.GetComponent<Text>();
			Image image2 = gameObject5.AddComponent<Image>();
			image2.rectTransform.sizeDelta = new Vector2(0.24f, 0.08f);
			image2.color = new Color(0.8f, 0.8f, 0.8f);
			Button button2 = gameObject5.AddComponent<Button>();
			colors = button2.colors;
			colors.highlightedColor = Color.red;
			colors.pressedColor = Color.cyan;
			button2.colors = colors;
			text.rectTransform.sizeDelta = new Vector2(0.24f, 0.08f);
			text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
			text.fontSize = 13;
			text.alignment = TextAnchor.MiddleCenter;
			text.text = "左手固定";
			gameObject5.transform.SetParent(objCanvasSetParent.transform);
			gameObject5.transform.localPosition = new Vector3(28f, -48f, 0f);
			gameObject6.transform.SetParent(gameObject5.transform);
			gameObject6.transform.localPosition = Vector3.zero;
			gameObject5.GetComponent<Button>().onClick.AddListener(delegate ()
			{
				PushFixLeftHandButton();
			});
			gameObject5 = new GameObject("button");
			gameObject6 = new GameObject("text", new Type[]
			{
				typeof(Text)
			});
			text = gameObject6.GetComponent<Text>();
			Image image3 = gameObject5.AddComponent<Image>();
			image3.rectTransform.sizeDelta = new Vector2(0.24f, 0.08f);
			image3.color = new Color(0.8f, 0.8f, 0.8f);
			Button button3 = gameObject5.AddComponent<Button>();
			colors = button3.colors;
			colors.highlightedColor = Color.red;
			colors.pressedColor = Color.cyan;
			button3.colors = colors;
			text.rectTransform.sizeDelta = new Vector2(0.24f, 0.08f);
			text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
			text.fontSize = 13;
			text.alignment = TextAnchor.MiddleCenter;
			text.text = "右足固定";
			gameObject5.transform.SetParent(objCanvasSetParent.transform);
			gameObject5.transform.localPosition = new Vector3(-28f, -28f, 0f);
			gameObject6.transform.SetParent(gameObject5.transform);
			gameObject6.transform.localPosition = Vector3.zero;
			gameObject5.GetComponent<Button>().onClick.AddListener(delegate ()
			{
				PushFixRightLegButton();
			});
			gameObject5 = new GameObject("button");
			gameObject6 = new GameObject("text", new Type[]
			{
				typeof(Text)
			});
			text = gameObject6.GetComponent<Text>();
			Image image4 = gameObject5.AddComponent<Image>();
			image4.rectTransform.sizeDelta = new Vector2(0.24f, 0.08f);
			image4.color = new Color(0.8f, 0.8f, 0.8f);
			Button button4 = gameObject5.AddComponent<Button>();
			colors = button4.colors;
			colors.highlightedColor = Color.red;
			colors.pressedColor = Color.cyan;
			button4.colors = colors;
			text.rectTransform.sizeDelta = new Vector2(0.24f, 0.08f);
			text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
			text.fontSize = 13;
			text.alignment = TextAnchor.MiddleCenter;
			text.text = "左足固定";
			gameObject5.transform.SetParent(objCanvasSetParent.transform);
			gameObject5.transform.localPosition = new Vector3(28f, -28f, 0f);
			gameObject6.transform.SetParent(gameObject5.transform);
			gameObject6.transform.localPosition = Vector3.zero;
			gameObject5.GetComponent<Button>().onClick.AddListener(delegate ()
			{
				PushFixLeftLegButton();
			});
			gameObject5 = new GameObject("button");
			gameObject6 = new GameObject("text", new Type[]
			{
				typeof(Text)
			});
			text = (txtFixBody = gameObject6.GetComponent<Text>());
			Image image5 = gameObject5.AddComponent<Image>();
			image5.rectTransform.sizeDelta = new Vector2(0.24f, 0.08f);
			image5.color = new Color(0.8f, 0.8f, 0.8f);
			Button button5 = gameObject5.AddComponent<Button>();
			colors = button5.colors;
			colors.highlightedColor = Color.red;
			colors.pressedColor = Color.cyan;
			button5.colors = colors;
			text.rectTransform.sizeDelta = new Vector2(0.24f, 0.08f);
			text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
			text.fontSize = 13;
			text.alignment = TextAnchor.MiddleCenter;
			text.text = "手足固定IS OFF";
			gameObject5.transform.SetParent(objCanvasSetParent.transform);
			gameObject5.transform.localPosition = new Vector3(-28f, -8f, 0f);
			gameObject6.transform.SetParent(gameObject5.transform);
			gameObject6.transform.localPosition = Vector3.zero;
			gameObject5.GetComponent<Button>().onClick.AddListener(delegate ()
			{
				PushFixBodyButton();
			});
			gameObject5 = new GameObject("button");
			gameObject6 = new GameObject("text", new Type[]
			{
				typeof(Text)
			});
			text = (txtSetParentL = gameObject6.GetComponent<Text>());
			Image image6 = gameObject5.AddComponent<Image>();
			image6.rectTransform.sizeDelta = new Vector2(0.24f, 0.08f);
			image6.color = new Color(0.8f, 0.8f, 0.8f);
			Button button6 = gameObject5.AddComponent<Button>();
			colors = button6.colors;
			colors.highlightedColor = Color.red;
			colors.pressedColor = Color.cyan;
			button6.colors = colors;
			text.rectTransform.sizeDelta = new Vector2(0.24f, 0.08f);
			text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
			text.fontSize = 13;
			text.alignment = TextAnchor.MiddleCenter;
			text.text = "左 親子付け On";
			gameObject5.transform.SetParent(objCanvasSetParent.transform);
			gameObject5.transform.localPosition = new Vector3(-28f, 16f, 0f);
			gameObject6.transform.SetParent(gameObject5.transform);
			gameObject6.transform.localPosition = Vector3.zero;
			gameObject5.GetComponent<Button>().onClick.AddListener(delegate ()
			{
				PushPLButton();
			});
			gameObject5 = new GameObject("button");
			gameObject6 = new GameObject("text", new Type[]
			{
				typeof(Text)
			});
			text = (txtSetParentR = gameObject6.GetComponent<Text>());
			Image image7 = gameObject5.AddComponent<Image>();
			image7.rectTransform.sizeDelta = new Vector2(0.24f, 0.08f);
			image7.color = new Color(0.8f, 0.8f, 0.8f);
			Button button7 = gameObject5.AddComponent<Button>();
			colors = button7.colors;
			colors.highlightedColor = Color.red;
			colors.pressedColor = Color.cyan;
			button7.colors = colors;
			text.rectTransform.sizeDelta = new Vector2(0.24f, 0.08f);
			text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
			text.fontSize = 13;
			text.alignment = TextAnchor.MiddleCenter;
			text.text = "右 親子付け On";
			gameObject5.transform.SetParent(objCanvasSetParent.transform);
			gameObject5.transform.localPosition = new Vector3(28f, 16f, 0f);
			gameObject6.transform.SetParent(gameObject5.transform);
			gameObject6.transform.localPosition = Vector3.zero;
			gameObject5.GetComponent<Button>().onClick.AddListener(delegate ()
			{
				PushPRButton();
			});
			gameObject5 = new GameObject("button");
			gameObject6 = new GameObject("text", new Type[]
			{
				typeof(Text)
			});
			text = gameObject6.GetComponent<Text>();
			Image image8 = gameObject5.AddComponent<Image>();
			image8.rectTransform.sizeDelta = new Vector2(0.24f, 0.08f);
			image8.color = new Color(0.8f, 0.8f, 0.8f);
			Button button8 = gameObject5.AddComponent<Button>();
			colors = button8.colors;
			colors.highlightedColor = Color.red;
			colors.pressedColor = Color.cyan;
			button8.colors = colors;
			text.rectTransform.sizeDelta = new Vector2(0.24f, 0.08f);
			text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
			text.fontSize = 13;
			text.alignment = TextAnchor.MiddleCenter;
			text.text = "モーション 強弱";
			gameObject5.transform.SetParent(objCanvasSetParent.transform);
			gameObject5.transform.localPosition = new Vector3(-28f, 40f, 0f);
			gameObject6.transform.SetParent(gameObject5.transform);
			gameObject6.transform.localPosition = Vector3.zero;
			gameObject5.GetComponent<Button>().onClick.AddListener(delegate ()
			{
				PushMotionChangeButton();
			});
			gameObject5 = new GameObject("button");
			gameObject6 = new GameObject("text", new Type[]
			{
				typeof(Text)
			});
			text = gameObject6.GetComponent<Text>();
			Image image9 = gameObject5.AddComponent<Image>();
			image9.rectTransform.sizeDelta = new Vector2(0.24f, 0.08f);
			image9.color = new Color(0.8f, 0.8f, 0.8f);
			Button button9 = gameObject5.AddComponent<Button>();
			colors = button9.colors;
			colors.highlightedColor = Color.red;
			colors.pressedColor = Color.cyan;
			button9.colors = colors;
			text.rectTransform.sizeDelta = new Vector2(0.24f, 0.08f);
			text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
			text.fontSize = 13;
			text.alignment = TextAnchor.MiddleCenter;
			text.text = "モーション 開始/停止";
			gameObject5.transform.SetParent(objCanvasSetParent.transform);
			gameObject5.transform.localPosition = new Vector3(28f, 40f, 0f);
			gameObject6.transform.SetParent(gameObject5.transform);
			gameObject6.transform.localPosition = Vector3.zero;
			gameObject5.GetComponent<Button>().onClick.AddListener(delegate ()
			{
				PushModeChangeButton();
			});
			gameObject5 = new GameObject("button");
			gameObject6 = new GameObject("text", new Type[]
			{
				typeof(Text)
			});
			text = gameObject6.GetComponent<Text>();
			Image image10 = gameObject5.AddComponent<Image>();
			image10.rectTransform.sizeDelta = new Vector2(0.24f, 0.08f);
			image10.color = new Color(0.8f, 0.8f, 0.8f);
			Button button10 = gameObject5.AddComponent<Button>();
			colors = button10.colors;
			colors.highlightedColor = Color.red;
			colors.pressedColor = Color.cyan;
			button10.colors = colors;
			text.rectTransform.sizeDelta = new Vector2(0.24f, 0.08f);
			text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
			text.fontSize = 13;
			text.alignment = TextAnchor.MiddleCenter;
			text.text = "中に出すよ";
			gameObject5.transform.SetParent(objCanvasSetParent.transform);
			gameObject5.transform.localPosition = new Vector3(-28f, 60f, 0f);
			gameObject6.transform.SetParent(gameObject5.transform);
			gameObject6.transform.localPosition = Vector3.zero;
			gameObject5.GetComponent<Button>().onClick.AddListener(delegate ()
			{
				PushFIButton();
			});
			gameObject5 = new GameObject("button");
			gameObject6 = new GameObject("text", new Type[]
			{
				typeof(Text)
			});
			text = gameObject6.GetComponent<Text>();
			Image image11 = gameObject5.AddComponent<Image>();
			image11.rectTransform.sizeDelta = new Vector2(0.24f, 0.08f);
			image11.color = new Color(0.8f, 0.8f, 0.8f);
			Button button11 = gameObject5.AddComponent<Button>();
			colors = button11.colors;
			colors.highlightedColor = Color.red;
			colors.pressedColor = Color.cyan;
			button11.colors = colors;
			text.rectTransform.sizeDelta = new Vector2(0.24f, 0.08f);
			text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
			text.fontSize = 13;
			text.alignment = TextAnchor.MiddleCenter;
			text.text = "外に出すよ";
			gameObject5.transform.SetParent(objCanvasSetParent.transform);
			gameObject5.transform.localPosition = new Vector3(28f, 60f, 0f);
			gameObject6.transform.SetParent(gameObject5.transform);
			gameObject6.transform.localPosition = Vector3.zero;
			gameObject5.GetComponent<Button>().onClick.AddListener(delegate ()
			{
				PushFOButton();
			});
			gameObject5 = new GameObject("button");
			gameObject6 = new GameObject("text", new Type[]
			{
				typeof(Text)
			});
			text = gameObject6.GetComponent<Text>();
			Image image12 = gameObject5.AddComponent<Image>();
			image12.rectTransform.sizeDelta = new Vector2(0.24f, 0.08f);
			image12.color = new Color(0.8f, 0.8f, 0.8f);
			Button button12 = gameObject5.AddComponent<Button>();
			colors = button12.colors;
			colors.highlightedColor = Color.red;
			colors.pressedColor = Color.cyan;
			button12.colors = colors;
			text.rectTransform.sizeDelta = new Vector2(0.24f, 0.08f);
			text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
			text.fontSize = 13;
			text.alignment = TextAnchor.MiddleCenter;
			text.text = "入れるよ";
			gameObject5.transform.SetParent(objCanvasSetParent.transform);
			gameObject5.transform.localPosition = new Vector3(-28f, 80f, 0f);
			gameObject6.transform.SetParent(gameObject5.transform);
			gameObject6.transform.localPosition = Vector3.zero;
			gameObject5.GetComponent<Button>().onClick.AddListener(delegate ()
			{
				PushIButton();
			});
			gameObject5 = new GameObject("button");
			gameObject6 = new GameObject("text", new Type[]
			{
				typeof(Text)
			});
			text = gameObject6.GetComponent<Text>();
			Image image13 = gameObject5.AddComponent<Image>();
			image13.rectTransform.sizeDelta = new Vector2(0.24f, 0.08f);
			image13.color = new Color(0.8f, 0.8f, 0.8f);
			Button button13 = gameObject5.AddComponent<Button>();
			colors = button13.colors;
			colors.highlightedColor = Color.red;
			colors.pressedColor = Color.cyan;
			button13.colors = colors;
			text.rectTransform.sizeDelta = new Vector2(0.24f, 0.08f);
			text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
			text.fontSize = 13;
			text.alignment = TextAnchor.MiddleCenter;
			text.text = "イレル";
			gameObject5.transform.SetParent(objCanvasSetParent.transform);
			gameObject5.transform.localPosition = new Vector3(28f, 80f, 0f);
			gameObject6.transform.SetParent(gameObject5.transform);
			gameObject6.transform.localPosition = Vector3.zero;
			gameObject5.GetComponent<Button>().onClick.AddListener(delegate ()
			{
				PushINButton();
			});
			gameObject5 = new GameObject("button");
			gameObject6 = new GameObject("text", new Type[]
			{
				typeof(Text)
			});
			text = gameObject6.GetComponent<Text>();
			Image image14 = gameObject5.AddComponent<Image>();
			image14.rectTransform.sizeDelta = new Vector2(0.24f, 0.08f);
			image14.color = new Color(0.8f, 0.8f, 0.8f);
			Button button14 = gameObject5.AddComponent<Button>();
			colors = button14.colors;
			colors.highlightedColor = Color.red;
			colors.pressedColor = Color.cyan;
			button14.colors = colors;
			text.rectTransform.sizeDelta = new Vector2(0.24f, 0.08f);
			text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
			text.fontSize = 13;
			text.alignment = TextAnchor.MiddleCenter;
			text.text = "アナル入れるよ";
			gameObject5.transform.SetParent(objCanvasSetParent.transform);
			gameObject5.transform.localPosition = new Vector3(-28f, 100f, 0f);
			gameObject6.transform.SetParent(gameObject5.transform);
			gameObject6.transform.localPosition = Vector3.zero;
			gameObject5.GetComponent<Button>().onClick.AddListener(delegate ()
			{
				PushIAButton();
			});
			gameObject5 = new GameObject("button");
			gameObject6 = new GameObject("text", new Type[]
			{
				typeof(Text)
			});
			text = gameObject6.GetComponent<Text>();
			Image image15 = gameObject5.AddComponent<Image>();
			image15.rectTransform.sizeDelta = new Vector2(0.24f, 0.08f);
			image15.color = new Color(0.8f, 0.8f, 0.8f);
			Button button15 = gameObject5.AddComponent<Button>();
			colors = button15.colors;
			colors.highlightedColor = Color.red;
			colors.pressedColor = Color.cyan;
			button15.colors = colors;
			text.rectTransform.sizeDelta = new Vector2(0.24f, 0.08f);
			text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
			text.fontSize = 13;
			text.alignment = TextAnchor.MiddleCenter;
			text.text = "アナルイレル";
			gameObject5.transform.SetParent(objCanvasSetParent.transform);
			gameObject5.transform.localPosition = new Vector3(28f, 100f, 0f);
			gameObject6.transform.SetParent(gameObject5.transform);
			gameObject6.transform.localPosition = Vector3.zero;
			gameObject5.GetComponent<Button>().onClick.AddListener(delegate ()
			{
				PushIANButton();
			});
			gameObject5 = new GameObject("button");
			gameObject6 = new GameObject("text", new Type[]
			{
				typeof(Text)
			});
			text = gameObject6.GetComponent<Text>();
			Image image16 = gameObject5.AddComponent<Image>();
			image16.rectTransform.sizeDelta = new Vector2(0.24f, 0.08f);
			image16.color = new Color(0.8f, 0.8f, 0.8f);
			Button button16 = gameObject5.AddComponent<Button>();
			colors = button16.colors;
			colors.highlightedColor = Color.red;
			colors.pressedColor = Color.cyan;
			button16.colors = colors;
			text.rectTransform.sizeDelta = new Vector2(0.24f, 0.08f);
			text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
			text.fontSize = 13;
			text.alignment = TextAnchor.MiddleCenter;
			text.text = "ヌク";
			gameObject5.transform.SetParent(objCanvasSetParent.transform);
			gameObject5.transform.localPosition = new Vector3(-28f, 120f, 0f);
			gameObject6.transform.SetParent(gameObject5.transform);
			gameObject6.transform.localPosition = Vector3.zero;
			gameObject5.GetComponent<Button>().onClick.AddListener(delegate ()
			{
				PushPullButton();
			});
			Vector3 point = femaleAim.transform.position - cameraEye.transform.position;
			point.y = 0f;
			point.Normalize();
			canvasSetParent.transform.position = new Vector3(femaleAim.transform.position.x, cameraEye.transform.position.y - 0.4f, femaleAim.transform.position.z) + Quaternion.Euler(0f, 90f, 0f) * point * 1.5f;
			canvasSetParent.transform.forward = (canvasSetParent.transform.position - cameraEye.transform.position).normalized;
			objCanvasMotion = new GameObject("CanvasMotion", new Type[]
			{
				typeof(Canvas)
			});
			canvasMotion = objCanvasMotion.GetComponent<Canvas>();
			objCanvasMotion.AddComponent<GraphicRaycaster>();
			objCanvasMotion.AddComponent<VRTK_UICanvas>();
			objCanvasMotion.AddComponent<VRTK_UIGraphicRaycaster>();
			canvasScalerMotion = objCanvasMotion.AddComponent<CanvasScaler>();
			canvasScalerMotion.dynamicPixelsPerUnit = 20000f;
			canvasScalerMotion.referencePixelsPerUnit = 80000f;
			canvasMotion.renderMode = RenderMode.WorldSpace;
			objCanvasMotion.transform.localScale = new Vector3(0.005f, 0.005f, 0.005f);
			eventSystemMotion = new GameObject("CanvasEventSystemMotion", new Type[]
			{
				typeof(EventSystem)
			});
			eventSystemMotion.AddComponent<StandaloneInputModule>();
			eventSystemMotion.transform.SetParent(objCanvasMotion.transform);
			gameObject5 = new GameObject("button");
			gameObject6 = new GameObject("text", new Type[]
			{
				typeof(Text)
			});
			text = gameObject6.GetComponent<Text>();
			Image image17 = gameObject5.AddComponent<Image>();
			image17.rectTransform.sizeDelta = new Vector2(0.24f, 0.08f);
			image17.color = new Color(0.8f, 0.8f, 0.8f);
			Button button17 = gameObject5.AddComponent<Button>();
			colors = button17.colors;
			colors.highlightedColor = Color.red;
			colors.pressedColor = Color.cyan;
			button17.colors = colors;
			text.rectTransform.sizeDelta = new Vector2(0.24f, 0.08f);
			text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
			text.fontSize = 13;
			text.alignment = TextAnchor.MiddleCenter;
			text.text = "正常位";
			gameObject5.transform.SetParent(objCanvasMotion.transform);
			gameObject5.transform.localPosition = new Vector3(-28f, -28f, 0f);
			gameObject6.transform.SetParent(gameObject5.transform);
			gameObject6.transform.localPosition = Vector3.zero;
			gameObject5.GetComponent<Button>().onClick.AddListener(delegate ()
			{
				PushSeijyouiButton();
			});
			gameObject5 = new GameObject("button");
			gameObject6 = new GameObject("text", new Type[]
			{
				typeof(Text)
			});
			text = gameObject6.GetComponent<Text>();
			Image image18 = gameObject5.AddComponent<Image>();
			image18.rectTransform.sizeDelta = new Vector2(0.24f, 0.08f);
			image18.color = new Color(0.8f, 0.8f, 0.8f);
			Button button18 = gameObject5.AddComponent<Button>();
			colors = button18.colors;
			colors.highlightedColor = Color.red;
			colors.pressedColor = Color.cyan;
			button18.colors = colors;
			text.rectTransform.sizeDelta = new Vector2(0.24f, 0.08f);
			text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
			text.fontSize = 13;
			text.alignment = TextAnchor.MiddleCenter;
			text.text = "開脚正常位";
			gameObject5.transform.SetParent(objCanvasMotion.transform);
			gameObject5.transform.localPosition = new Vector3(28f, -28f, 0f);
			gameObject6.transform.SetParent(gameObject5.transform);
			gameObject6.transform.localPosition = Vector3.zero;
			gameObject5.GetComponent<Button>().onClick.AddListener(delegate ()
			{
				PushKSeijyouiButton();
			});
			gameObject5 = new GameObject("button");
			gameObject6 = new GameObject("text", new Type[]
			{
				typeof(Text)
			});
			text = gameObject6.GetComponent<Text>();
			Image image19 = gameObject5.AddComponent<Image>();
			image19.rectTransform.sizeDelta = new Vector2(0.24f, 0.08f);
			image19.color = new Color(0.8f, 0.8f, 0.8f);
			Button button19 = gameObject5.AddComponent<Button>();
			colors = button19.colors;
			colors.highlightedColor = Color.red;
			colors.pressedColor = Color.cyan;
			button19.colors = colors;
			text.rectTransform.sizeDelta = new Vector2(0.24f, 0.08f);
			text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
			text.fontSize = 13;
			text.alignment = TextAnchor.MiddleCenter;
			text.text = "後背位";
			gameObject5.transform.SetParent(objCanvasMotion.transform);
			gameObject5.transform.localPosition = new Vector3(-28f, -12f, 0f);
			gameObject6.transform.SetParent(gameObject5.transform);
			gameObject6.transform.localPosition = Vector3.zero;
			gameObject5.GetComponent<Button>().onClick.AddListener(delegate ()
			{
				PushKohaiiButton();
			});
			gameObject5 = new GameObject("button");
			gameObject6 = new GameObject("text", new Type[]
			{
				typeof(Text)
			});
			text = gameObject6.GetComponent<Text>();
			Image image20 = gameObject5.AddComponent<Image>();
			image20.rectTransform.sizeDelta = new Vector2(0.24f, 0.08f);
			image20.color = new Color(0.8f, 0.8f, 0.8f);
			Button button20 = gameObject5.AddComponent<Button>();
			colors = button20.colors;
			colors.highlightedColor = Color.red;
			colors.pressedColor = Color.cyan;
			button20.colors = colors;
			text.rectTransform.sizeDelta = new Vector2(0.24f, 0.08f);
			text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
			text.fontSize = 13;
			text.alignment = TextAnchor.MiddleCenter;
			text.text = "腕引っ張り後背位";
			gameObject5.transform.SetParent(objCanvasMotion.transform);
			gameObject5.transform.localPosition = new Vector3(28f, -12f, 0f);
			gameObject6.transform.SetParent(gameObject5.transform);
			gameObject6.transform.localPosition = Vector3.zero;
			gameObject5.GetComponent<Button>().onClick.AddListener(delegate ()
			{
				PushUdeHippariKohaiiButton();
			});
			gameObject5 = new GameObject("button");
			gameObject6 = new GameObject("text", new Type[]
			{
				typeof(Text)
			});
			text = gameObject6.GetComponent<Text>();
			Image image21 = gameObject5.AddComponent<Image>();
			image21.rectTransform.sizeDelta = new Vector2(0.24f, 0.08f);
			image21.color = new Color(0.8f, 0.8f, 0.8f);
			Button button21 = gameObject5.AddComponent<Button>();
			colors = button21.colors;
			colors.highlightedColor = Color.red;
			colors.pressedColor = Color.cyan;
			button21.colors = colors;
			text.rectTransform.sizeDelta = new Vector2(0.24f, 0.08f);
			text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
			text.fontSize = 13;
			text.alignment = TextAnchor.MiddleCenter;
			text.text = "騎乗位(二体位)";
			gameObject5.transform.SetParent(objCanvasMotion.transform);
			gameObject5.transform.localPosition = new Vector3(-28f, 4f, 0f);
			gameObject6.transform.SetParent(gameObject5.transform);
			gameObject6.transform.localPosition = Vector3.zero;
			gameObject5.GetComponent<Button>().onClick.AddListener(delegate ()
			{
				PushKijyouiButton();
			});
			gameObject5 = new GameObject("button");
			gameObject6 = new GameObject("text", new Type[]
			{
				typeof(Text)
			});
			text = gameObject6.GetComponent<Text>();
			Image image22 = gameObject5.AddComponent<Image>();
			image22.rectTransform.sizeDelta = new Vector2(0.24f, 0.08f);
			image22.color = new Color(0.8f, 0.8f, 0.8f);
			Button button22 = gameObject5.AddComponent<Button>();
			colors = button22.colors;
			colors.highlightedColor = Color.red;
			colors.pressedColor = Color.cyan;
			button22.colors = colors;
			text.rectTransform.sizeDelta = new Vector2(0.24f, 0.08f);
			text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
			text.fontSize = 13;
			text.alignment = TextAnchor.MiddleCenter;
			text.text = "片足上げ側位";
			gameObject5.transform.SetParent(objCanvasMotion.transform);
			gameObject5.transform.localPosition = new Vector3(28f, 4f, 0f);
			gameObject6.transform.SetParent(gameObject5.transform);
			gameObject6.transform.localPosition = Vector3.zero;
			gameObject5.GetComponent<Button>().onClick.AddListener(delegate ()
			{
				PushSokuiButton();
			});
			gameObject5 = new GameObject("button");
			gameObject6 = new GameObject("text", new Type[]
			{
				typeof(Text)
			});
			text = gameObject6.GetComponent<Text>();
			Image image23 = gameObject5.AddComponent<Image>();
			image23.rectTransform.sizeDelta = new Vector2(0.24f, 0.08f);
			image23.color = new Color(0.8f, 0.8f, 0.8f);
			Button button23 = gameObject5.AddComponent<Button>();
			colors = button23.colors;
			colors.highlightedColor = Color.red;
			colors.pressedColor = Color.cyan;
			button23.colors = colors;
			text.rectTransform.sizeDelta = new Vector2(0.24f, 0.08f);
			text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
			text.fontSize = 13;
			text.alignment = TextAnchor.MiddleCenter;
			text.text = "立位";
			gameObject5.transform.SetParent(objCanvasMotion.transform);
			gameObject5.transform.localPosition = new Vector3(-28f, 20f, 0f);
			gameObject6.transform.SetParent(gameObject5.transform);
			gameObject6.transform.localPosition = Vector3.zero;
			gameObject5.GetComponent<Button>().onClick.AddListener(delegate ()
			{
				PushRituiButton();
			});
			gameObject5 = new GameObject("button");
			gameObject6 = new GameObject("text", new Type[]
			{
				typeof(Text)
			});
			text = gameObject6.GetComponent<Text>();
			Image image24 = gameObject5.AddComponent<Image>();
			image24.rectTransform.sizeDelta = new Vector2(0.24f, 0.08f);
			image24.color = new Color(0.8f, 0.8f, 0.8f);
			Button button24 = gameObject5.AddComponent<Button>();
			colors = button24.colors;
			colors.highlightedColor = Color.red;
			colors.pressedColor = Color.cyan;
			button24.colors = colors;
			text.rectTransform.sizeDelta = new Vector2(0.24f, 0.08f);
			text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
			text.fontSize = 13;
			text.alignment = TextAnchor.MiddleCenter;
			text.text = "駅弁";
			gameObject5.transform.SetParent(objCanvasMotion.transform);
			gameObject5.transform.localPosition = new Vector3(28f, 20f, 0f);
			gameObject6.transform.SetParent(gameObject5.transform);
			gameObject6.transform.localPosition = Vector3.zero;
			gameObject5.GetComponent<Button>().onClick.AddListener(delegate ()
			{
				PushEkibenButton();
			});
			gameObject5 = new GameObject("button");
			gameObject6 = new GameObject("text", new Type[]
			{
				typeof(Text)
			});
			text = gameObject6.GetComponent<Text>();
			Image image25 = gameObject5.AddComponent<Image>();
			image25.rectTransform.sizeDelta = new Vector2(0.24f, 0.08f);
			image25.color = new Color(0.8f, 0.8f, 0.8f);
			Button button25 = gameObject5.AddComponent<Button>();
			colors = button25.colors;
			colors.highlightedColor = Color.red;
			colors.pressedColor = Color.cyan;
			button25.colors = colors;
			text.rectTransform.sizeDelta = new Vector2(0.24f, 0.08f);
			text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
			text.fontSize = 13;
			text.alignment = TextAnchor.MiddleCenter;
			text.text = "椅子対面";
			gameObject5.transform.SetParent(objCanvasMotion.transform);
			gameObject5.transform.localPosition = new Vector3(-28f, 36f, 0f);
			gameObject6.transform.SetParent(gameObject5.transform);
			gameObject6.transform.localPosition = Vector3.zero;
			gameObject5.GetComponent<Button>().onClick.AddListener(delegate ()
			{
				PushIsuTaimenButton();
			});
			gameObject5 = new GameObject("button");
			gameObject6 = new GameObject("text", new Type[]
			{
				typeof(Text)
			});
			text = gameObject6.GetComponent<Text>();
			Image image26 = gameObject5.AddComponent<Image>();
			image26.rectTransform.sizeDelta = new Vector2(0.24f, 0.08f);
			image26.color = new Color(0.8f, 0.8f, 0.8f);
			Button button26 = gameObject5.AddComponent<Button>();
			colors = button26.colors;
			colors.highlightedColor = Color.red;
			colors.pressedColor = Color.cyan;
			button26.colors = colors;
			text.rectTransform.sizeDelta = new Vector2(0.24f, 0.08f);
			text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
			text.fontSize = 13;
			text.alignment = TextAnchor.MiddleCenter;
			text.text = "椅子背面";
			gameObject5.transform.SetParent(objCanvasMotion.transform);
			gameObject5.transform.localPosition = new Vector3(28f, 36f, 0f);
			gameObject6.transform.SetParent(gameObject5.transform);
			gameObject6.transform.localPosition = Vector3.zero;
			gameObject5.GetComponent<Button>().onClick.AddListener(delegate ()
			{
				PushIsuHaimenButton();
			});
			gameObject5 = new GameObject("button");
			gameObject6 = new GameObject("text", new Type[]
			{
				typeof(Text)
			});
			text = gameObject6.GetComponent<Text>();
			Image image27 = gameObject5.AddComponent<Image>();
			image27.rectTransform.sizeDelta = new Vector2(0.24f, 0.08f);
			image27.color = new Color(0.8f, 0.8f, 0.8f);
			Button button27 = gameObject5.AddComponent<Button>();
			colors = button27.colors;
			colors.highlightedColor = Color.red;
			colors.pressedColor = Color.cyan;
			button27.colors = colors;
			text.rectTransform.sizeDelta = new Vector2(0.24f, 0.08f);
			text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
			text.fontSize = 13;
			text.alignment = TextAnchor.MiddleCenter;
			text.text = "足閉じ立バック";
			gameObject5.transform.SetParent(objCanvasMotion.transform);
			gameObject5.transform.localPosition = new Vector3(-28f, 52f, 0f);
			gameObject6.transform.SetParent(gameObject5.transform);
			gameObject6.transform.localPosition = Vector3.zero;
			gameObject5.GetComponent<Button>().onClick.AddListener(delegate ()
			{
				PushIsuBackButton();
			});
			gameObject5 = new GameObject("button");
			gameObject6 = new GameObject("text", new Type[]
			{
				typeof(Text)
			});
			text = gameObject6.GetComponent<Text>();
			Image image28 = gameObject5.AddComponent<Image>();
			image28.rectTransform.sizeDelta = new Vector2(0.24f, 0.08f);
			image28.color = new Color(0.8f, 0.8f, 0.8f);
			Button button28 = gameObject5.AddComponent<Button>();
			colors = button28.colors;
			colors.highlightedColor = Color.red;
			colors.pressedColor = Color.cyan;
			button28.colors = colors;
			text.rectTransform.sizeDelta = new Vector2(0.24f, 0.08f);
			text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
			text.fontSize = 13;
			text.alignment = TextAnchor.MiddleCenter;
			text.text = "腕引っ張り立バック";
			gameObject5.transform.SetParent(objCanvasMotion.transform);
			gameObject5.transform.localPosition = new Vector3(28f, 52f, 0f);
			gameObject6.transform.SetParent(gameObject5.transform);
			gameObject6.transform.localPosition = Vector3.zero;
			gameObject5.GetComponent<Button>().onClick.AddListener(delegate ()
			{
				PushUdeHippariIsuBackButton();
			});
			gameObject5 = new GameObject("button");
			gameObject6 = new GameObject("text", new Type[]
			{
				typeof(Text)
			});
			text = gameObject6.GetComponent<Text>();
			Image image29 = gameObject5.AddComponent<Image>();
			image29.rectTransform.sizeDelta = new Vector2(0.24f, 0.08f);
			image29.color = new Color(0.8f, 0.8f, 0.8f);
			Button button29 = gameObject5.AddComponent<Button>();
			colors = button29.colors;
			colors.highlightedColor = Color.red;
			colors.pressedColor = Color.cyan;
			button29.colors = colors;
			text.rectTransform.sizeDelta = new Vector2(0.24f, 0.08f);
			text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
			text.fontSize = 13;
			text.alignment = TextAnchor.MiddleCenter;
			text.text = "騎乗位(単一体位)";
			gameObject5.transform.SetParent(objCanvasMotion.transform);
			gameObject5.transform.localPosition = new Vector3(-28f, 68f, 0f);
			gameObject6.transform.SetParent(gameObject5.transform);
			gameObject6.transform.localPosition = Vector3.zero;
			gameObject5.GetComponent<Button>().onClick.AddListener(delegate ()
			{
				PushTukueNeiButton();
			});
			gameObject5 = new GameObject("button");
			gameObject6 = new GameObject("text", new Type[]
			{
				typeof(Text)
			});
			text = gameObject6.GetComponent<Text>();
			Image image30 = gameObject5.AddComponent<Image>();
			image30.rectTransform.sizeDelta = new Vector2(0.24f, 0.08f);
			image30.color = new Color(0.8f, 0.8f, 0.8f);
			Button button30 = gameObject5.AddComponent<Button>();
			colors = button30.colors;
			colors.highlightedColor = Color.red;
			colors.pressedColor = Color.cyan;
			button30.colors = colors;
			text.rectTransform.sizeDelta = new Vector2(0.24f, 0.08f);
			text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
			text.fontSize = 13;
			text.alignment = TextAnchor.MiddleCenter;
			text.text = "机側位";
			gameObject5.transform.SetParent(objCanvasMotion.transform);
			gameObject5.transform.localPosition = new Vector3(28f, 68f, 0f);
			gameObject6.transform.SetParent(gameObject5.transform);
			gameObject6.transform.localPosition = Vector3.zero;
			gameObject5.GetComponent<Button>().onClick.AddListener(delegate ()
			{
				PushTukueSokuiButton();
			});
			gameObject5 = new GameObject("button");
			gameObject6 = new GameObject("text", new Type[]
			{
				typeof(Text)
			});
			text = gameObject6.GetComponent<Text>();
			Image image31 = gameObject5.AddComponent<Image>();
			image31.rectTransform.sizeDelta = new Vector2(0.24f, 0.08f);
			image31.color = new Color(0.8f, 0.8f, 0.8f);
			Button button31 = gameObject5.AddComponent<Button>();
			colors = button31.colors;
			colors.highlightedColor = Color.red;
			colors.pressedColor = Color.cyan;
			button31.colors = colors;
			text.rectTransform.sizeDelta = new Vector2(0.24f, 0.08f);
			text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
			text.fontSize = 13;
			text.alignment = TextAnchor.MiddleCenter;
			text.text = "跳び箱バック";
			gameObject5.transform.SetParent(objCanvasMotion.transform);
			gameObject5.transform.localPosition = new Vector3(-28f, 84f, 0f);
			gameObject6.transform.SetParent(gameObject5.transform);
			gameObject6.transform.localPosition = Vector3.zero;
			gameObject5.GetComponent<Button>().onClick.AddListener(delegate ()
			{
				PushTukueBackButton();
			});
			gameObject5 = new GameObject("button");
			gameObject6 = new GameObject("text", new Type[]
			{
				typeof(Text)
			});
			text = gameObject6.GetComponent<Text>();
			Image image32 = gameObject5.AddComponent<Image>();
			image32.rectTransform.sizeDelta = new Vector2(0.24f, 0.08f);
			image32.color = new Color(0.8f, 0.8f, 0.8f);
			Button button32 = gameObject5.AddComponent<Button>();
			colors = button32.colors;
			colors.highlightedColor = Color.red;
			colors.pressedColor = Color.cyan;
			button32.colors = colors;
			text.rectTransform.sizeDelta = new Vector2(0.24f, 0.08f);
			text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
			text.fontSize = 13;
			text.alignment = TextAnchor.MiddleCenter;
			text.text = "脚持つ正常位";
			gameObject5.transform.SetParent(objCanvasMotion.transform);
			gameObject5.transform.localPosition = new Vector3(28f, 84f, 0f);
			gameObject6.transform.SetParent(gameObject5.transform);
			gameObject6.transform.localPosition = Vector3.zero;
			gameObject5.GetComponent<Button>().onClick.AddListener(delegate ()
			{
				PushUdeHippariTukueBackButton();
			});
			gameObject5 = new GameObject("button");
			gameObject6 = new GameObject("text", new Type[]
			{
				typeof(Text)
			});
			text = gameObject6.GetComponent<Text>();
			Image image33 = gameObject5.AddComponent<Image>();
			image33.rectTransform.sizeDelta = new Vector2(0.24f, 0.08f);
			image33.color = new Color(0.8f, 0.8f, 0.8f);
			Button button33 = gameObject5.AddComponent<Button>();
			colors = button33.colors;
			colors.highlightedColor = Color.red;
			colors.pressedColor = Color.cyan;
			button33.colors = colors;
			text.rectTransform.sizeDelta = new Vector2(0.24f, 0.08f);
			text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
			text.fontSize = 13;
			text.alignment = TextAnchor.MiddleCenter;
			text.text = "壁対面片足上げ";
			gameObject5.transform.SetParent(objCanvasMotion.transform);
			gameObject5.transform.localPosition = new Vector3(-28f, 100f, 0f);
			gameObject6.transform.SetParent(gameObject5.transform);
			gameObject6.transform.localPosition = Vector3.zero;
			gameObject5.GetComponent<Button>().onClick.AddListener(delegate ()
			{
				PushKabeTaimenKataasiageButton();
			});
			gameObject5 = new GameObject("button");
			gameObject6 = new GameObject("text", new Type[]
			{
				typeof(Text)
			});
			text = gameObject6.GetComponent<Text>();
			Image image34 = gameObject5.AddComponent<Image>();
			image34.rectTransform.sizeDelta = new Vector2(0.24f, 0.08f);
			image34.color = new Color(0.8f, 0.8f, 0.8f);
			Button button34 = gameObject5.AddComponent<Button>();
			colors = button34.colors;
			colors.highlightedColor = Color.red;
			colors.pressedColor = Color.cyan;
			button34.colors = colors;
			text.rectTransform.sizeDelta = new Vector2(0.24f, 0.08f);
			text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
			text.fontSize = 13;
			text.alignment = TextAnchor.MiddleCenter;
			text.text = "立バック";
			gameObject5.transform.SetParent(objCanvasMotion.transform);
			gameObject5.transform.localPosition = new Vector3(28f, 100f, 0f);
			gameObject6.transform.SetParent(gameObject5.transform);
			gameObject6.transform.localPosition = Vector3.zero;
			gameObject5.GetComponent<Button>().onClick.AddListener(delegate ()
			{
				PushKabeBackButton();
			});
			gameObject5 = new GameObject("button");
			gameObject6 = new GameObject("text", new Type[]
			{
				typeof(Text)
			});
			text = gameObject6.GetComponent<Text>();
			Image image35 = gameObject5.AddComponent<Image>();
			image35.rectTransform.sizeDelta = new Vector2(0.24f, 0.08f);
			image35.color = new Color(0.8f, 0.8f, 0.8f);
			Button button35 = gameObject5.AddComponent<Button>();
			colors = button35.colors;
			colors.highlightedColor = Color.red;
			colors.pressedColor = Color.cyan;
			button35.colors = colors;
			text.rectTransform.sizeDelta = new Vector2(0.24f, 0.08f);
			text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
			text.fontSize = 13;
			text.alignment = TextAnchor.MiddleCenter;
			text.text = "片足上げ立バック";
			gameObject5.transform.SetParent(objCanvasMotion.transform);
			gameObject5.transform.localPosition = new Vector3(-28f, 116f, 0f);
			gameObject6.transform.SetParent(gameObject5.transform);
			gameObject6.transform.localPosition = Vector3.zero;
			gameObject5.GetComponent<Button>().onClick.AddListener(delegate ()
			{
				PushKataasiageKabeBackButton();
			});
			gameObject5 = new GameObject("button");
			gameObject6 = new GameObject("text", new Type[]
			{
				typeof(Text)
			});
			text = gameObject6.GetComponent<Text>();
			Image image36 = gameObject5.AddComponent<Image>();
			image36.rectTransform.sizeDelta = new Vector2(0.24f, 0.08f);
			image36.color = new Color(0.8f, 0.8f, 0.8f);
			Button button36 = gameObject5.AddComponent<Button>();
			colors = button36.colors;
			colors.highlightedColor = Color.red;
			colors.pressedColor = Color.cyan;
			button36.colors = colors;
			text.rectTransform.sizeDelta = new Vector2(0.24f, 0.08f);
			text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
			text.fontSize = 13;
			text.alignment = TextAnchor.MiddleCenter;
			text.text = "水中バック";
			gameObject5.transform.SetParent(objCanvasMotion.transform);
			gameObject5.transform.localPosition = new Vector3(28f, 116f, 0f);
			gameObject6.transform.SetParent(gameObject5.transform);
			gameObject6.transform.localPosition = Vector3.zero;
			gameObject5.GetComponent<Button>().onClick.AddListener(delegate ()
			{
				PushPoolBackButton();
			});
			gameObject5 = new GameObject("button");
			gameObject6 = new GameObject("text", new Type[]
			{
				typeof(Text)
			});
			text = gameObject6.GetComponent<Text>();
			Image image37 = gameObject5.AddComponent<Image>();
			image37.rectTransform.sizeDelta = new Vector2(0.24f, 0.08f);
			image37.color = new Color(0.8f, 0.8f, 0.8f);
			Button button37 = gameObject5.AddComponent<Button>();
			colors = button37.colors;
			colors.highlightedColor = Color.red;
			colors.pressedColor = Color.cyan;
			button37.colors = colors;
			text.rectTransform.sizeDelta = new Vector2(0.24f, 0.08f);
			text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
			text.fontSize = 13;
			text.alignment = TextAnchor.MiddleCenter;
			text.text = "うっ伏せ後背位";
			gameObject5.transform.SetParent(objCanvasMotion.transform);
			gameObject5.transform.localPosition = new Vector3(-28f, 132f, 0f);
			gameObject6.transform.SetParent(gameObject5.transform);
			gameObject6.transform.localPosition = Vector3.zero;
			gameObject5.GetComponent<Button>().onClick.AddListener(delegate ()
			{
				PushFenceKouhaiiButton();
			});
			gameObject5 = new GameObject("button");
			gameObject6 = new GameObject("text", new Type[]
			{
				typeof(Text)
			});
			text = gameObject6.GetComponent<Text>();
			Image image38 = gameObject5.AddComponent<Image>();
			image38.rectTransform.sizeDelta = new Vector2(0.24f, 0.08f);
			image38.color = new Color(0.8f, 0.8f, 0.8f);
			Button button38 = gameObject5.AddComponent<Button>();
			colors = button38.colors;
			colors.highlightedColor = Color.red;
			colors.pressedColor = Color.cyan;
			button38.colors = colors;
			text.rectTransform.sizeDelta = new Vector2(0.24f, 0.08f);
			text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
			text.fontSize = 13;
			text.alignment = TextAnchor.MiddleCenter;
			text.text = "駅弁(単一体位)";
			gameObject5.transform.SetParent(objCanvasMotion.transform);
			gameObject5.transform.localPosition = new Vector3(28f, 132f, 0f);
			gameObject6.transform.SetParent(gameObject5.transform);
			gameObject6.transform.localPosition = Vector3.zero;
			gameObject5.GetComponent<Button>().onClick.AddListener(delegate ()
			{
				PushFenceTukamariEkibenButton();
			});
			point = femaleAim.transform.position - cameraEye.transform.position;
			point.y = 0f;
			point.Normalize();
			canvasMotion.transform.position = new Vector3(femaleAim.transform.position.x, cameraEye.transform.position.y - 0.4f, femaleAim.transform.position.z) + Quaternion.Euler(0f, -90f, 0f) * point * 1.5f;
			canvasMotion.transform.forward = (canvasMotion.transform.position - cameraEye.transform.position).normalized;
			if (SetCollider.Value)
			{
				foreach (Transform transform in GameObject.Find("Map").GetComponentsInChildren<Transform>())
				{
					MeshFilter component = transform.GetComponent<MeshFilter>();
					if (!(component == null))
					{
						GameObject gameObject7 = new GameObject("SPCollider");
						gameObject7.transform.parent = transform.transform;
						gameObject7.transform.localPosition = Vector3.zero;
						gameObject7.transform.localRotation = Quaternion.identity;
						gameObject7.AddComponent<Rigidbody>().isKinematic = true;
						if (component.mesh.bounds.size.x < 0.03f || component.mesh.bounds.size.y < 0.03f || component.mesh.bounds.size.z < 0.03f)
						{
							BoxCollider boxCollider = gameObject7.AddComponent<BoxCollider>();
							boxCollider.isTrigger = true;
							boxCollider.center = component.mesh.bounds.center;
							boxCollider.size = component.mesh.bounds.size;
							if (boxCollider.size.x < 0.03f)
							{
								boxCollider.size += new Vector3(0.04f, 0f, 0f);
							}
							if (boxCollider.size.y < 0.03f)
							{
								boxCollider.size += new Vector3(0f, 0.04f, 0f);
							}
							if (boxCollider.size.z < 0.03f)
							{
								boxCollider.size += new Vector3(0f, 0f, 0.04f);
							}
						}
						else
						{
							MeshCollider meshCollider = gameObject7.AddComponent<MeshCollider>();
							meshCollider.isTrigger = true;
							meshCollider.convex = false;
							meshCollider.sharedMesh = component.mesh;
						}
					}
				}
				GameObject gameObject8 = new GameObject("SPCollider");
				gameObject8.transform.parent = cameraEye.transform;
				gameObject8.transform.localPosition = new Vector3(0f, -0.25f, -0.15f);
				gameObject8.transform.localRotation = Quaternion.identity;
				BoxCollider boxCollider2 = gameObject8.AddComponent<BoxCollider>();
				boxCollider2.isTrigger = true;
				boxCollider2.center = Vector3.zero;
				boxCollider2.size = new Vector3(0.4f, 0.2f, 0.25f);
				gameObject8.AddComponent<Rigidbody>().isKinematic = true;
			}
		}

		private void PushFixBodyButton()
		{
			if (objRightHand == bFixBody)
				PushFixRightHandButton();
			if (objLeftHand == bFixBody)
				PushFixLeftHandButton();
			if (objRightLeg == bFixBody)
				PushFixRightLegButton();
			if (objLeftLeg == bFixBody)
				PushFixLeftLegButton();

			bFixBody = !bFixBody;
			if (bFixBody)
				txtFixBody.text = "手足固定IS ON";
			else
				txtFixBody.text = "手足固定IS OFF";
		}

		public void PushFixRightHandButton()
		{
			if (objRightHand == null)
			{
				objRightHand = new GameObject("objRightHand");
				objRightHand.transform.position = female_cf_pv_hand_R.transform.position;
				objRightHand.transform.rotation = female_cf_pv_hand_R.transform.rotation;
				femaleFBBIK.solver.rightHandEffector.target = objRightHand.transform;
				return;
			}
			UnityEngine.Object.DestroyImmediate(objRightHand);
			objRightHand = null;
			femaleFBBIK.solver.rightHandEffector.target = female_cf_t_hand_R.transform;
		}

		public void PushFixLeftHandButton()
		{
			if (objLeftHand == null)
			{
				objLeftHand = new GameObject("objLeftHand");
				objLeftHand.transform.position = female_cf_pv_hand_L.transform.position;
				objLeftHand.transform.rotation = female_cf_pv_hand_L.transform.rotation;
				femaleFBBIK.solver.leftHandEffector.target = objLeftHand.transform;
				return;
			}
			UnityEngine.Object.DestroyImmediate(objLeftHand);
			objLeftHand = null;
			femaleFBBIK.solver.leftHandEffector.target = female_cf_t_hand_L.transform;
		}

		public void PushFixRightLegButton()
		{
			if (objRightLeg == null)
			{
				objRightLeg = new GameObject("objRightLeg");
				objRightLeg.transform.position = female_cf_pv_leg_R.transform.position;
				objRightLeg.transform.rotation = female_cf_pv_leg_R.transform.rotation;
				femaleFBBIK.solver.rightFootEffector.target = objRightLeg.transform;
				return;
			}
			UnityEngine.Object.DestroyImmediate(objRightLeg);
			objRightLeg = null;
			femaleFBBIK.solver.rightFootEffector.target = female_cf_t_leg_R.transform;
		}

		public void PushFixLeftLegButton()
		{
			if (objLeftLeg == null)
			{
				objLeftLeg = new GameObject("objLeftLeg");
				objLeftLeg.transform.position = female_cf_pv_leg_L.transform.position;
				objLeftLeg.transform.rotation = female_cf_pv_leg_L.transform.rotation;
				femaleFBBIK.solver.leftFootEffector.target = objLeftLeg.transform;
				return;
			}
			UnityEngine.Object.DestroyImmediate(objLeftLeg);
			objLeftLeg = null;
			femaleFBBIK.solver.leftFootEffector.target = female_cf_t_leg_L.transform;
		}

		private void ChangeMotion(string path, string name)
		{
			Animator component = female_p_cf_bodybone.GetComponent<Animator>();
			RuntimeAnimatorController runtimeAnimatorController = CommonLib.LoadAsset<RuntimeAnimatorController>(path, name, false, string.Empty);
			AnimatorOverrideController animatorOverrideController = new AnimatorOverrideController(component.runtimeAnimatorController);
			foreach (AnimationClip animationClip in new AnimatorOverrideController(runtimeAnimatorController).animationClips)
			{
				animatorOverrideController[animationClip.name] = animationClip;
			}
			animatorOverrideController.name = runtimeAnimatorController.name;
			component.runtimeAnimatorController = animatorOverrideController;
			AssetBundleManager.UnloadAssetBundle(path, true, null, false);

			positionMenuPressed = true;
		}

		private void PushSeijyouiButton()
		{
			ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_00");
		}

		private void PushKSeijyouiButton()
		{
			ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_n00");
		}

		private void PushKohaiiButton()
		{
			ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_02");
		}

		private void PushUdeHippariKohaiiButton()
		{
			ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_n02");
		}

		private void PushKijyouiButton()
		{
			ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_n04");
		}

		private void PushSokuiButton()
		{
			ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_n06");
		}

		private void PushRituiButton()
		{
			ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_n07");
		}

		private void PushEkibenButton()
		{
			ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_n08");
		}

		private void PushIsuTaimenButton()
		{
			ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_n09");
		}

		private void PushIsuHaimenButton()
		{
			ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_n10");
		}

		private void PushIsuBackButton()
		{
			ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_11");
		}

		private void PushUdeHippariIsuBackButton()
		{
			ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_n11");
		}

		private void PushTukueNeiButton()
		{
			ChangeMotion("h/anim/female/02_13_00.unity3d", "khs_f_n27");
		}

		private void PushTukueBackButton()
		{
			ChangeMotion("h/anim/female/02_12_00.unity3d", "khs_f_n25");
		}

		private void PushUdeHippariTukueBackButton()
		{
			ChangeMotion("h/anim/female/02_12_00.unity3d", "khs_f_n24");
		}

		private void PushTukueSokuiButton()
		{
			ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_n16");
		}

		private void PushKabeTaimenKataasiageButton()
		{
			ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_n17");
		}

		private void PushKabeBackButton()
		{
			ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_18");
		}

		private void PushKataasiageKabeBackButton()
		{
			ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_n18");
		}

		private void PushPoolBackButton()
		{
			ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_n20");
		}

		private void PushFenceKouhaiiButton()
		{
			ChangeMotion("h/anim/female/02_13_00.unity3d", "khs_f_n26");
		}

		private void PushFenceTukamariEkibenButton()
		{
			ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_n22");
		}

		private void PushPullButton()
		{
			hSprite.OnPullClick();
		}

		private void PushIANButton()
		{
			hSprite.OnInsertAnalNoVoiceClick();
		}

		private void PushIAButton()
		{
			hSprite.OnInsertAnalClick();
		}

		private void PushINButton()
		{
			hSprite.OnInsertNoVoiceClick();
		}

		private void PushIButton()
		{
			hSprite.OnInsertClick();
		}

		private void PushPLButton()
		{
			if (!setFlag)
			{
				SetP(true);
			}
			else
			{
				UnsetP();
			}
			setFlag = !setFlag;
		}

		private void PushPRButton()
		{
			if (!setFlag)
			{
				SetP(false);
			}
			else
			{
				UnsetP();
			}
			setFlag = !setFlag;
		}

		private void PushModeChangeButton()
		{
			hFlag.click = HFlag.ClickKind.modeChange;
		}

		private void PushMotionChangeButton()
		{
			hFlag.click = HFlag.ClickKind.motionchange;
			AnimSpeedController component = obj_chaF_001.GetComponent<AnimSpeedController>();
			if (component != null)
			{
				component.weakMotion = !component.weakMotion;
			}
		}

		private void PushFIButton()
		{
			hSprite.OnInsideClick();
			AnimSpeedController component = obj_chaF_001.GetComponent<AnimSpeedController>();
			if (component != null)
			{
				component.fcount = 0f;
				component.moveFlag = false;
			}
		}

		private void PushFOButton()
		{
			hSprite.OnOutsideClick();
			AnimSpeedController component = obj_chaF_001.GetComponent<AnimSpeedController>();
			if (component != null)
			{
				component.fcount = 0f;
				component.moveFlag = false;
			}
		}

		public void LateUpdate()
		{
			//Checks if female object exists. If not, exits the function
			if (!femaleExists)
			{
				if (obj_chaF_001 == null)
				{
					femaleExists = false;
					return;
				}
				femaleExists = true;
			}

			//Find and assign left and right controllers variables if they are null
			if (leftDevice == null)
			{
				if (leftController == null)
					leftController = hSprite.managerVR.objMove.transform.Find("Controller (left)").gameObject;

				leftVVC = leftController.GetComponent<VRViveController>();
				leftDevice = (f_device.GetValue(leftVVC) as SteamVR_Controller.Device);
			}
			if (rightDevice == null)
			{
				if (rightController == null)
					rightController = hSprite.managerVR.objMove.transform.Find("Controller (right)").gameObject;

				rightVVC = rightController.GetComponent<VRViveController>();
				rightDevice = (f_device.GetValue(rightVVC) as SteamVR_Controller.Device);
			}


			if (objCanvasSetParent == null)
			{
				InitCanvas();
				if (objCanvasSetParent != null && hideCanvas)
				{
					objCanvasSetParent.SetActive(false);
					objCanvasMotion.SetActive(false);
				}
			}
			else
			{
				if (RightMenuPressing() || LeftMenuPressing())
				{
					hideCount += Time.deltaTime;
					if (hideCount >= 1f)
					{
						objCanvasSetParent.SetActive(!objCanvasSetParent.activeSelf);
						objCanvasMotion.SetActive(!objCanvasMotion.activeSelf);
						hideCount = 0f;
						if (objCanvasSetParent.activeSelf)
						{
							hideCanvas = false;
						}
						else
						{
							hideCanvas = true;
						}
					}
				}
				else
				{
					hideCount = 0f;
				}
				Vector3 point = femaleAim.transform.position - cameraEye.transform.position;
				point.y = 0f;
				point.Normalize();
				objCanvasSetParent.transform.position = new Vector3(femaleAim.transform.position.x, cameraEye.transform.position.y, femaleAim.transform.position.z) + Quaternion.Euler(0f, 90f, 0f) * point * 0.4f;
				objCanvasSetParent.transform.forward = (objCanvasSetParent.transform.position - cameraEye.transform.position).normalized;
				objCanvasMotion.transform.position = new Vector3(femaleAim.transform.position.x, cameraEye.transform.position.y, femaleAim.transform.position.z) + Quaternion.Euler(0f, -90f, 0f) * point * 0.4f;
				objCanvasMotion.transform.forward = (objCanvasMotion.transform.position - cameraEye.transform.position).normalized;
			}

			////////////////////////////////////////////////////////////
			//Enforcing and auto releasing male and female IK's based on how the limbs are stretched
			////////////////////////////////////////////////////////////
			MaleIKs();
			FemaleIKs();		
			
			//////////////
			//Activate/deactivate SetParent functionality by
			//* Pressing backslash key or 
			//* Pressing menu button and trigger at the same time
			//////////////
			bool setParentToggle = Input.GetKeyDown(KeyCode.Backslash) || (RightMenuPressing() && RightTriggerPressDown()) || (LeftMenuPressing() && LeftTriggerPressDown());
			if (setParentToggle)
			{
				//Toggle parenting based on current parenting status
				//* Set parent to the opposite controller of the one pressing the buttons, since the controller that's parenting the female would become invisible and unable to receive input
				//* Set parent to the left controller if activated by keypress
				if (!setFlag)
				{
					if (RightMenuPressing() && RightTriggerPressDown())
					{
						SetP(true);
					}
					else if (LeftMenuPressing() && LeftTriggerPressDown())
					{
						SetP(false);
					}
					else
					{
						SetP(true);
					}
				}
				else
				{
					UnsetP();
				}
				setFlag = !setFlag;
			}
			
			if (setFlag)
			{
				if (nameAnimation != hFlag.nowAnimationInfo.nameAnimation)
				{
					UnsetP();
					setFlag = !setFlag;
					nameAnimation = hFlag.nowAnimationInfo.nameAnimation;
				}

				ControllerActions();

				quatSpineRot[indexSpineRot] = femaleSpinePos.transform.rotation;
				if (indexSpineRot >= 19)
				{
					indexSpineRot = 0;
				}
				else
				{
					indexSpineRot++;
				}
				Quaternion quaternion = quatSpineRot[0];
				for (int i = 1; i < 20; i++)
				{
					quaternion = Quaternion.Lerp(quaternion, quatSpineRot[i], 1f / (i + 1));
				}
				if (TrackingMode.Value)
				{
					switch (ParentPart.Value)
					{
						case BodyPart.Ass:
							female_p_cf_bodybone.transform.rotation = quaternion * Quaternion.Inverse(female_cf_j_hips.transform.localRotation) * Quaternion.Inverse(female_cf_n_height.transform.localRotation) * Quaternion.Inverse(female_cf_j_root.transform.localRotation);
							break;
						case BodyPart.Belly:
							female_p_cf_bodybone.transform.rotation = quaternion * Quaternion.Inverse(female_cf_j_spine02.transform.localRotation) * Quaternion.Inverse(female_cf_j_spine01.transform.localRotation) * Quaternion.Inverse(female_cf_j_hips.transform.localRotation) * Quaternion.Inverse(female_cf_n_height.transform.localRotation) * Quaternion.Inverse(female_cf_j_root.transform.localRotation);
							break;
						case BodyPart.Head:
							female_p_cf_bodybone.transform.rotation = quaternion * Quaternion.Inverse(female_cf_j_neck.transform.localRotation) * Quaternion.Inverse(female_cf_j_spine03.transform.localRotation) * Quaternion.Inverse(female_cf_j_spine02.transform.localRotation) * Quaternion.Inverse(female_cf_j_spine01.transform.localRotation) * Quaternion.Inverse(female_cf_j_hips.transform.localRotation) * Quaternion.Inverse(female_cf_n_height.transform.localRotation) * Quaternion.Inverse(female_cf_j_root.transform.localRotation);
							break;
						default:
							female_p_cf_bodybone.transform.rotation = quaternion * Quaternion.Inverse(female_cf_j_spine02.transform.localRotation) * Quaternion.Inverse(female_cf_j_spine01.transform.localRotation) * Quaternion.Inverse(female_cf_j_hips.transform.localRotation) * Quaternion.Inverse(female_cf_n_height.transform.localRotation) * Quaternion.Inverse(female_cf_j_root.transform.localRotation);
							break;
					}
				}
				else
				{
					switch (ParentPart.Value)
					{
						case BodyPart.Ass:
							female_p_cf_bodybone.transform.rotation = femaleSpinePos.transform.rotation * Quaternion.Inverse(female_cf_j_hips.transform.localRotation) * Quaternion.Inverse(female_cf_n_height.transform.localRotation) * Quaternion.Inverse(female_cf_j_root.transform.localRotation);
							break;
						case BodyPart.Belly:
							female_p_cf_bodybone.transform.rotation = femaleSpinePos.transform.rotation * Quaternion.Inverse(female_cf_j_spine02.transform.localRotation) * Quaternion.Inverse(female_cf_j_spine01.transform.localRotation) * Quaternion.Inverse(female_cf_j_hips.transform.localRotation) * Quaternion.Inverse(female_cf_n_height.transform.localRotation) * Quaternion.Inverse(female_cf_j_root.transform.localRotation);
							break;
						case BodyPart.Head:
							female_p_cf_bodybone.transform.rotation = femaleSpinePos.transform.rotation * Quaternion.Inverse(female_cf_j_neck.transform.localRotation) * Quaternion.Inverse(female_cf_j_spine03.transform.localRotation) * Quaternion.Inverse(female_cf_j_spine02.transform.localRotation) * Quaternion.Inverse(female_cf_j_spine01.transform.localRotation) * Quaternion.Inverse(female_cf_j_hips.transform.localRotation) * Quaternion.Inverse(female_cf_n_height.transform.localRotation) * Quaternion.Inverse(female_cf_j_root.transform.localRotation);
							break;
						default:
							female_p_cf_bodybone.transform.rotation = femaleSpinePos.transform.rotation * Quaternion.Inverse(female_cf_j_spine02.transform.localRotation) * Quaternion.Inverse(female_cf_j_spine01.transform.localRotation) * Quaternion.Inverse(female_cf_j_hips.transform.localRotation) * Quaternion.Inverse(female_cf_n_height.transform.localRotation) * Quaternion.Inverse(female_cf_j_root.transform.localRotation);
							break;
					}
				}
				vecSpinePos[indexSpinePos] = femaleSpinePos.transform.position;
				if (indexSpinePos >= 19)
				{
					indexSpinePos = 0;
				}
				else
				{
					indexSpinePos++;
				}
				Vector3 a = Vector3.zero;
				foreach (Vector3 b in vecSpinePos)
				{
					a += b;
				}
				a /= 20f;
				if (TrackingMode.Value)
				{
					female_p_cf_bodybone.transform.position += a - femaleBase.transform.position;
				}
				else
				{
					female_p_cf_bodybone.transform.position += femaleSpinePos.transform.position - femaleBase.transform.position;
				}	


				if (male_p_cf_bodybone != null && SetParentMale.Value && currentCtrlstate != CtrlState.Following)
				{
					/////////////////////////
					// Make the male body rotate around the crotch to keep its head align with the HMD without moving the crotch by
					// -Create vector from male crotch to HMD position, then another vector from male crotch to male head to represent the spine
					// -Calculate the rotation from spine vector to HMD vector, then apply the rotation to the male body
					/////////////////////////
					Vector3 cameraVec = cameraEye.transform.position - maleCrotchPos.transform.position;
					Vector3 maleSpineVec = maleHeadPos.transform.position - maleCrotchPos.transform.position;
					Quaternion.FromToRotation(maleSpineVec, cameraVec).ToAngleAxis(out float lookRotAngle, out Vector3 lookRotAxis);
					male_p_cf_bodybone.transform.RotateAround(maleCrotchPos.transform.position, lookRotAxis, lookRotAngle);

					/////////////////////////
					// Update position of the spine vector, and using it as an axis to rotate the male body to "look" left or right by following the HMD's rotation
					//
					// - Since we're only interested in HMD's rotation along the spine axis, we take the HMD's right vector which will give us the rotation we need 
					//   and align it with the direction of the penis by rotating it to the left by 90 degress,  
					//   then calculate the rotation between the two vectors projected on the plane normal to the spine before applying it the male body
					/////////////////////////
					maleSpineVec = maleHeadPos.transform.position - maleCrotchPos.transform.position;
					Vector3 malePenisProjected = Vector3.ProjectOnPlane(maleCrotchPos.transform.forward, maleSpineVec);
					Vector3 cameraForwardProjected = Quaternion.AngleAxis(-90, maleSpineVec) * Vector3.ProjectOnPlane(cameraEye.transform.right, maleSpineVec);
					Quaternion.FromToRotation(malePenisProjected, cameraForwardProjected).ToAngleAxis(out lookRotAngle, out lookRotAxis);
					male_p_cf_bodybone.transform.RotateAround(maleCrotchPos.transform.position, lookRotAxis, lookRotAngle);		
				}
				if (hideCanvas)
				{
					Vector3 vector;
					if (parentIsLeft)
					{
						vector = cameraEye.transform.position - rightController.transform.position;
					}
					else
					{
						vector = cameraEye.transform.position - leftController.transform.position;
					}
					if (vector.magnitude <= 0.3f)
					{
						objCanvasSetParent.SetActive(true);
						objCanvasMotion.SetActive(true);
					}
					else
					{
						objCanvasSetParent.SetActive(false);
						objCanvasMotion.SetActive(false);
					}
				}
				txtSetParentL.text = "親子付け Off";
				txtSetParentR.text = "親子付け Off";
			}
			else
			{
				txtSetParentL.text = "左 親子付け On";
				txtSetParentR.text = "右 親子付け On";
			}
		}

		private void SetP(bool _parentIsLeft)
		{
			if (obj_chaF_001 == null)
			{
				return;
			}
			if (male_p_cf_bodybone == null)
			{
				GameObject.Find("chaM_001/BodyTop/p_cf_body_bone");
			}
			parentIsLeft = _parentIsLeft;
			nameAnimation = hFlag.nowAnimationInfo.nameAnimation;		

			switch (ParentPart.Value)
			{
				case BodyPart.Ass:
					femaleBase = female_cf_j_hips;
					break;
				case BodyPart.Belly:
					femaleBase = female_cf_j_spine02;
					break;
				case BodyPart.Head:
					femaleBase = female_cf_j_neck;
					break;
				default:
					femaleBase = female_cf_j_spine02;
					break;
			}
			if (femaleSpinePos == null)
			{
				femaleSpinePos = new GameObject("femaleSpinePos");
			}
			if (SetParentMode.Value == ParentMode.PositionOnly || SetParentMode.Value == ParentMode.PositionAndAnimation)
			{
				SetParentToController(_parentIsLeft, femaleSpinePos, femaleBase, true);
			}	
			else
			{
				femaleSpinePos.transform.position = femaleBase.transform.position;
				femaleSpinePos.transform.rotation = femaleBase.transform.rotation;
			}
			
			for (int i = 0; i < 20; i++)
			{
				vecSpinePos[i] = femaleSpinePos.transform.position;
			}
			indexSpinePos = 0;
			for (int j = 0; j < 20; j++)
			{
				quatSpineRot[j] = femaleSpinePos.transform.rotation;
			}
			indexSpineRot = 0;
			
			if (SetParentMale.Value && male_p_cf_bodybone != null && currentCtrlstate != CtrlState.Following)
			{
				GameObject maleNeck = maleFBBIK.references.spine[2].gameObject;	
				maleHeadPos = new GameObject("maleHeadPos");
				maleHeadPos.transform.position = maleNeck.transform.position;
				maleHeadPos.transform.rotation = maleNeck.transform.rotation;
				maleHeadPos.transform.parent = maleNeck.transform;
				maleHeadPos.transform.localPosition = new Vector3(0, 0, 0.08f);

				GameObject maleCrotch = maleFBBIK.references.leftThigh.parent.Find("cf_d_kokan/cm_J_dan_top").gameObject;
				maleCrotchPos = new GameObject("maleCrotchPos");
				maleCrotchPos.transform.position = maleCrotch.transform.position;
				maleCrotchPos.transform.rotation = maleCrotch.transform.rotation;
				maleCrotchPos.transform.parent = male_p_cf_bodybone.transform;
			}
			if (SetParentMode.Value == ParentMode.PositionAndAnimation || SetParentMode.Value == ParentMode.AnimationOnly)
			{
				AddAnimSpeedController(obj_chaF_001, _parentIsLeft, leftController, rightController);
			}
		}

		public void UnsetP()
		{
			if (obj_chaF_001 == null)
			{
				return;
			}
			obj_chaF_001.transform.parent = GameObject.Find("Component").transform;

			UnityEngine.Object.Destroy(maleHeadPos);
			UnityEngine.Object.Destroy(maleCrotchPos);
			UnityEngine.Object.Destroy(femaleSpinePos);
			GameObject obj_chaM_001 = male.objRoot;
			if (obj_chaM_001 != null)
			{
				obj_chaM_001.transform.parent = GameObject.Find("Component").transform;
			}
			leftController.transform.Find("Model").gameObject.SetActive(true);
			rightController.transform.Find("Model").gameObject.SetActive(true);
			if (obj_chaF_001.GetComponent<AnimSpeedController>() != null)
			{
				UnityEngine.Object.Destroy(obj_chaF_001.GetComponent<AnimSpeedController>());
			}
		}

		/// <summary>
		/// Release and attach male limbs based on the distance between the attaching target position and the default animation position
		/// </summary>
		private void MaleIKs()
		{
			if ((maleFBBIK.solver.rightHandEffector.target.position - male_cf_pv_hand_R.transform.position).magnitude > 0.2f)
			{
				maleFBBIK.solver.rightHandEffector.positionWeight = 0f;
				maleFBBIK.solver.rightHandEffector.rotationWeight = 0f;
			}
			else
			{
				maleFBBIK.solver.rightHandEffector.positionWeight = 1f;
				maleFBBIK.solver.rightHandEffector.rotationWeight = 1f;
			}

			if((maleFBBIK.solver.leftHandEffector.target.position - male_cf_pv_hand_L.transform.position).magnitude > 0.2f)
			{
				maleFBBIK.solver.leftHandEffector.positionWeight = 0f;
				maleFBBIK.solver.leftHandEffector.rotationWeight = 0f;
			}
			else
			{
				maleFBBIK.solver.leftHandEffector.positionWeight = 1f;
				maleFBBIK.solver.leftHandEffector.rotationWeight = 1f;
			}
		}

		/// <summary>
		/// Release and attach female limbs based on the distance between the attaching target position and the default animation position
		/// </summary>
		/// Attachment onto anchor points created by SetParent will enjoy a larger degree of freedom before being released
		private void FemaleIKs()
		{
			if (objRightHand != null && !bFixBody && (femaleFBBIK.solver.rightHandEffector.target.position - female_cf_pv_hand_R.transform.position).magnitude > 0.35f)
			{
				PushFixRightHandButton();
			}
			else if (objRightHand == null && (femaleFBBIK.solver.rightHandEffector.target.position - female_cf_pv_hand_R.transform.position).magnitude > 0.2f)
			{
				femaleFBBIK.solver.rightHandEffector.positionWeight = 0f;
				femaleFBBIK.solver.rightHandEffector.rotationWeight = 0f;

			}
			else
			{
				femaleFBBIK.solver.rightHandEffector.positionWeight = 1f;
				femaleFBBIK.solver.rightHandEffector.rotationWeight = 1f;
			}

			if (objLeftHand != null && !bFixBody && (femaleFBBIK.solver.leftHandEffector.target.position - female_cf_pv_hand_L.transform.position).magnitude > 0.35f)
			{
				PushFixLeftHandButton();
			}
			else if (objLeftHand == null && (femaleFBBIK.solver.leftHandEffector.target.position - female_cf_pv_hand_L.transform.position).magnitude > 0.2f)
			{
				femaleFBBIK.solver.leftHandEffector.positionWeight = 0f;
				femaleFBBIK.solver.leftHandEffector.rotationWeight = 0f;
			}
			else
			{
				femaleFBBIK.solver.leftHandEffector.positionWeight = 1f;
				femaleFBBIK.solver.leftHandEffector.rotationWeight = 1f;
			}

			if (objRightLeg != null && !bFixBody && (femaleFBBIK.solver.rightFootEffector.target.position - female_cf_pv_leg_R.transform.position).magnitude > 0.5f)
			{
				PushFixRightLegButton();
			}
			else
			{
				femaleFBBIK.solver.rightFootEffector.positionWeight = 1f;
				femaleFBBIK.solver.rightFootEffector.rotationWeight = 1f;
			}

			if (objLeftLeg != null && !bFixBody && (femaleFBBIK.solver.leftFootEffector.target.position - female_cf_pv_leg_L.transform.position).magnitude > 0.5f)
			{
				PushFixLeftLegButton();
			}
			else
			{
				femaleFBBIK.solver.leftFootEffector.positionWeight = 1f;
				femaleFBBIK.solver.leftFootEffector.rotationWeight = 1f;
			}
		}

		/// <summary>
		/// Change state of controller-to-characters relationship based on controller input
		/// </summary>
		private void ControllerActions()
		{
			///////////////////
			//Based on controller input, set characters into one of these 4 states based on controller input:
			//	1. Remain still relative to the scene if only the trigger is held
			//	2. Both male and female following parent controller (controller specified when activating set parent)
			//	3. Male body parented to non parent controller
			//	4. Female body parented to non parent controller 
			//	5. If no matching controller input is present, return to default state of parenting
			///////////////////
			if (parentIsLeft ? (RightTriggerPressing() && !RightTrackPadUp()) : (LeftTriggerPressing() && !LeftTrackPadUp()))
			{
				if (currentCtrlstate != CtrlState.Stationary)
					currentCtrlstate = ChangeControlState(currentCtrlstate, CtrlState.Stationary);
				return;
			}
			else if (parentIsLeft ? (RightTriggerPressing() && RightTrackPadUp()) : (LeftTriggerPressing() && LeftTrackPadUp()))
			{
				if (currentCtrlstate != CtrlState.Following)
					currentCtrlstate = ChangeControlState(currentCtrlstate, CtrlState.Following);
				return;
			}
			else if (parentIsLeft ? (RightGripPressing() && RightTrackPadDown()) : (LeftGripPressing() && LeftTrackPadDown()))
			{
				if (currentCtrlstate != CtrlState.MaleControl)
					currentCtrlstate = ChangeControlState(currentCtrlstate, CtrlState.MaleControl);
				return;
			}
			else if (parentIsLeft ? (RightGripPressing() && RightTrackPadUp()) : (LeftGripPressing() && LeftTrackPadUp()))
			{
				if (currentCtrlstate != CtrlState.FemaleControl)
					currentCtrlstate = ChangeControlState(currentCtrlstate, CtrlState.FemaleControl);
				return;
			}
			else
			{
				if (currentCtrlstate != CtrlState.None)
					currentCtrlstate = ChangeControlState(currentCtrlstate, CtrlState.None);	
				return;
			}
		}

		private void AddAnimSpeedController(GameObject character, bool _parentIsLeft, GameObject _leftController, GameObject _rightController)
		{
			if (character.GetComponent<AnimSpeedController>() != null)
			{
				return;
			}

			AnimSpeedController animSpeedController = character.AddComponent<AnimSpeedController>();
			if (_parentIsLeft)
			{
				animSpeedController.SetController(_leftController, _rightController, this);
				return;
			}
			else
				animSpeedController.SetController(_rightController, _leftController, this);
		}

		private void SetParentToController (bool _parentIsLeft, GameObject parentDummy, GameObject target, bool hideModel)
		{
			if (_parentIsLeft)
			{
				parentDummy.transform.parent = leftController.transform;
				if (hideModel)
					leftController.transform.Find("Model").gameObject.SetActive(false);
			}
			else
			{
				parentDummy.transform.parent = rightController.transform;
				if (hideModel)
					rightController.transform.Find("Model").gameObject.SetActive(false);
			}
			parentDummy.transform.position = target.transform.position;
			parentDummy.transform.rotation = target.transform.rotation;
		}
		/// <summary>
		/// Handles transition between controller-to-character parenting states
		/// </summary>
		/// <param name="fromState">initial state</param>
		/// <param name="toState">target state</param>
		/// <returns></returns>
		private CtrlState ChangeControlState (CtrlState fromState, CtrlState toState)
		{
			if (fromState == toState)
				return toState;
			
			// Undo effects of the current state
			switch (fromState)
			{
				case CtrlState.None:
					break;

				case CtrlState.MaleControl:
					male_p_cf_bodybone.transform.parent = male.objTop.transform;
					break;

				case CtrlState.FemaleControl:
					if (SetParentMode.Value == ParentMode.AnimationOnly)
						femaleSpinePos.transform.parent = null;
					else
						SetParentToController(parentIsLeft, femaleSpinePos, femaleBase, true);
					break;

				case CtrlState.Following:
					if (SetParentMode.Value != ParentMode.PositionOnly)
						AddAnimSpeedController(obj_chaF_001, parentIsLeft, leftController, rightController);
					if (SetParentMode.Value == ParentMode.AnimationOnly)
						femaleSpinePos.transform.parent = null;
					male_p_cf_bodybone.transform.parent = male.objTop.transform;
					break;

				case CtrlState.Stationary:
					if (SetParentMode.Value != ParentMode.AnimationOnly)
						SetParentToController(parentIsLeft, femaleSpinePos, femaleBase, true);
					if (SetParentMode.Value != ParentMode.PositionOnly)
						AddAnimSpeedController(obj_chaF_001, parentIsLeft, leftController, rightController);
					break;
			}
			
			//Apply effects of the target state and update current state to target state
			switch (toState)
			{
				case CtrlState.None:
					return CtrlState.None;

				case CtrlState.MaleControl:
					male_p_cf_bodybone.transform.parent = parentIsLeft ? rightController.transform : leftController.transform;
					return CtrlState.MaleControl;

				case CtrlState.FemaleControl:
					SetParentToController(!parentIsLeft, femaleSpinePos, femaleBase, false);
					return CtrlState.FemaleControl;

				case CtrlState.Following:
					if (SetParentMode.Value == ParentMode.AnimationOnly)
						SetParentToController(parentIsLeft, femaleSpinePos, femaleBase, false);
					if (obj_chaF_001.GetComponent<AnimSpeedController>() != null)
					{
						UnityEngine.Object.Destroy(obj_chaF_001.GetComponent<AnimSpeedController>());
					}
					male_p_cf_bodybone.transform.parent = female_p_cf_bodybone.transform;
					return CtrlState.Following;

				case CtrlState.Stationary:
					if (SetParentMode.Value != ParentMode.AnimationOnly)
						femaleSpinePos.transform.parent = null;
					if (obj_chaF_001.GetComponent<AnimSpeedController>() != null)
					{
						UnityEngine.Object.Destroy(obj_chaF_001.GetComponent<AnimSpeedController>());
					}
					return CtrlState.Stationary;

				default:
					return CtrlState.None;
			}
		}

		private bool RightTrackPadPressing()
		{
			return rightVVC.IsState(VRViveController.EViveButtonKind.Touchpad, -1) || rightDevice.GetPress(4294967296UL);
		}

		private bool LeftTrackPadPressing()
		{
			return leftVVC.IsState(VRViveController.EViveButtonKind.Touchpad, -1) || leftDevice.GetPress(4294967296UL);
		}

		private bool RightTrackPadUp()
		{
			return rightVVC.IsState(VRViveController.EViveButtonKind.Touchpad_Up, -1) || rightDevice.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0).y > 0.7f;
		}

		private bool LeftTrackPadUp()
		{
			return leftVVC.IsState(VRViveController.EViveButtonKind.Touchpad_Up, -1) || leftDevice.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0).y > 0.7f;
		}

		private bool RightTrackPadDown()
		{
			return rightVVC.IsState(VRViveController.EViveButtonKind.Touchpad_Down, -1) || rightDevice.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0).y < -0.7f;
		}

		private bool LeftTrackPadDown()
		{
			return leftVVC.IsState(VRViveController.EViveButtonKind.Touchpad_Down, -1) || leftDevice.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0).y < -0.7f;
		}

		private bool RightMenuPressing()
		{
			return rightVVC.IsState(VRViveController.EViveButtonKind.Menu, -1) || rightDevice.GetPress(2UL);
		}

		private bool LeftMenuPressing()
		{
			return leftVVC.IsState(VRViveController.EViveButtonKind.Menu, -1) || leftDevice.GetPress(2UL);
		}

		private bool RightTriggerPressDown()
		{
			return rightVVC.IsPressDown(VRViveController.EViveButtonKind.Trigger, -1) || rightDevice.GetPressDown(8589934592UL);
		}

		private bool LeftTriggerPressDown()
		{
			return leftVVC.IsPressDown(VRViveController.EViveButtonKind.Trigger, -1) || leftDevice.GetPressDown(8589934592UL);
		}

		private bool RightTriggerRelease()
		{
			return rightVVC.IsPressUp(VRViveController.EViveButtonKind.Trigger, -1) || rightDevice.GetPressUp(8589934592UL);
		}

		private bool LeftTriggerRelease()
		{
			return leftVVC.IsPressUp(VRViveController.EViveButtonKind.Trigger, -1) || leftDevice.GetPressUp(8589934592UL);
		}

		private bool RightTriggerPressing()
		{
			return rightVVC.IsState(VRViveController.EViveButtonKind.Trigger, -1) || rightDevice.GetPress(8589934592UL);
		}

		private bool LeftTriggerPressing()
		{
			return leftVVC.IsState(VRViveController.EViveButtonKind.Trigger, -1) || leftDevice.GetPress(8589934592UL);
		}

		private bool LeftGripPressing()
		{
			return leftVVC.IsState(VRViveController.EViveButtonKind.Grip, -1) || leftDevice.GetPress(4UL);
		}

		private bool LeftGripPressDown()
		{
			return leftVVC.IsPressDown(VRViveController.EViveButtonKind.Grip, -1) || leftDevice.GetPressDown(4UL);
		}

		private bool RightGripPressing()
		{
			return rightVVC.IsState(VRViveController.EViveButtonKind.Grip, -1) || rightDevice.GetPress(4UL);
		}

		private bool RightGripPressDown()
		{
			return rightVVC.IsPressDown(VRViveController.EViveButtonKind.Grip, -1) || rightDevice.GetPressDown(4UL);
		}

		
		/// <summary>
		/// Describes the parenting relationship between the controller and the female/male character
		/// </summary>
		internal enum CtrlState
		{
			None,
			Stationary,
			Following,
			MaleControl,
			FemaleControl
		}


		internal CtrlState currentCtrlstate;
		
		internal bool setFlag;

		internal bool femaleExists;

		private HFlag hFlag;

		private ChaControl male;

		private ChaControl female;

		private FullBodyBipedIK maleFBBIK;

		private FullBodyBipedIK femaleFBBIK;

		private string nameAnimation = "";

		private GameObject leftController;

		private GameObject rightController;

		private GameObject cameraEye;

		private GameObject femaleAim;

		private FieldInfo f_device;

		private VRViveController leftVVC;

		private VRViveController rightVVC;

		private SteamVR_Controller.Device leftDevice;

		private SteamVR_Controller.Device rightDevice;

		private Canvas canvasSetParent;

		private GameObject objCanvasSetParent;

		private CanvasScaler canvasScalerSetParent;

		private GameObject eventSystemSetParent;

		private Canvas canvasMotion;

		private GameObject objCanvasMotion;

		private CanvasScaler canvasScalerMotion;

		private GameObject eventSystemMotion;

		private float hideCount;

		internal HSprite hSprite;

		private GameObject female_cf_t_hand_R;

		private GameObject female_cf_pv_hand_R;

		public GameObject objRightHand;

		private GameObject female_cf_t_hand_L;

		private GameObject female_cf_pv_hand_L;

		public GameObject objLeftHand;

		private GameObject female_cf_t_leg_R;

		private GameObject female_cf_pv_leg_R;

		public GameObject objRightLeg;

		private GameObject female_cf_t_leg_L;

		private GameObject female_cf_pv_leg_L;

		public GameObject objLeftLeg;

		private GameObject obj_chaF_001;

		internal GameObject female_p_cf_bodybone;

		private GameObject female_cf_j_root;

		private GameObject female_cf_n_height;

		private GameObject female_cf_j_hips;

		private GameObject female_cf_j_spine01;

		private GameObject female_cf_j_spine02;

		private GameObject female_cf_j_spine03;

		private GameObject female_cf_j_neck;

		private GameObject femaleBase;

		private GameObject femaleSpinePos;

		private GameObject male_p_cf_bodybone;

		private GameObject maleHeadPos;

		private GameObject maleCrotchPos;

		private GameObject male_cf_pv_hand_R;

		private GameObject male_cf_pv_hand_L;

		private Text txtSetParentL;

		private Text txtSetParentR;

		private Text txtFixBody;

		private bool bFixBody;

		private Vector3[] vecSpinePos = new Vector3[20];

		private int indexSpinePos;

		private Quaternion[] quatSpineRot = new Quaternion[20];

		private int indexSpineRot;

		internal bool hideCanvas;

		private bool parentIsLeft;

		internal bool positionMenuPressed;
	}
}
