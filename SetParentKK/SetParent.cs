using System;
using System.Reflection;
using Illusion.Component.Correct;
using IllusionPlugin;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VRTK;

namespace SetParent
{
	public class SetParent : IPlugin
	{
		public string Name
		{
			get
			{
				return "SetParent Plugin For Koikatu";
			}
		}

		public string Version
		{
			get
			{
				return "0.10a";
			}
		}

		public void OnApplicationStart()
		{
			this.setParentMode = ModPrefs.GetInt("SetParent", "setParentMode", 1, true);
			ModPrefs.GetFloat("SetParent", "threshold1", 0.05f, true);
			ModPrefs.GetFloat("SetParent", "threshold2", 0.3f, true);
			ModPrefs.GetInt("SetParent", "moveDistancePoolSize", 100, true);
			ModPrefs.GetInt("SetParent", "calcPattern", 0, true);
			ModPrefs.GetFloat("SetParent", "finishcount", 5f, true);
			this.setParentMale = ModPrefs.GetBool("SetParent", "setParentMale", false, true);
			ModPrefs.GetInt("SetParent", "moveCoordinatePoolSize", 100, true);
			ModPrefs.GetFloat("SetParent", "strongMotionThreshold", 0.06f, true);
			ModPrefs.GetFloat("SetParent", "weakMotionThreshold", 0.01f, true);
			ModPrefs.GetFloat("SetParent", "sMThreshold2Ratio", 1.3f, true);
			ModPrefs.GetBool("SetParent", "SetCollider", true, true);
			ModPrefs.GetInt("SetParent", "ParentPart", 1, true);
			ModPrefs.GetBool("SetParent", "TrackingMode", true, true);
			ModPrefs.GetFloat("SetParent", "sonyuGaugeMax", 72f, true);
			ModPrefs.GetFloat("SetParent", "houshiGaugeMax", 69f, true);
			this.f_device = typeof(VRViveController).GetField("device", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			SceneManager.sceneLoaded += this.OnSceneLoaded;
		}

		public void OnApplicationQuit()
		{
		}

		public void OnLevelWasLoaded(int level)
		{
		}

		private void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode)
		{
			this.femaleFlag = false;
			this.hideCanvas = false;
		}

		private void InitCanvas()
		{
			this.cameraEye = GameObject.Find("VRTK/[VRTK_SDKManager]/SDKSetups/SteamVR/VRCameraBase/[CameraRig]/Camera (eye)");
			this.femaleAim = GameObject.Find("chaF_001/BodyTop/p_cf_body_bone/cf_j_root/cf_n_height/cf_j_hips/cf_j_spine01/cf_j_spine02/cf_j_spine03/cf_j_neck/cf_j_head/cf_s_head/aim");
			this.hFlag = GameObject.Find("VRHScene").GetComponent<HFlag>();
			this.hSprite = GameObject.Find("VRTK/[VRTK_SDKManager]/SDKSetups/SteamVR/VRCameraBase/[CameraRig]/Controller (left)/Model/p_handL").transform.Find("HSceneMainCanvas").Find("MainCanvas").GetComponent<HSprite>();
			if (this.cameraEye == null)
			{
				return;
			}
			if (this.femaleAim == null)
			{
				return;
			}
			if (this.hSprite == null)
			{
				return;
			}
			this.obj_cf_t_hand_R = GameObject.Find("chaF_001/BodyTop/p_cf_body_bone/cf_t_root/cf_t_hand_R");
			this.bd_cf_t_hand_R = this.obj_cf_t_hand_R.GetComponent<BaseData>();
			this.obj_cf_pv_hand_R = GameObject.Find("chaF_001/BodyTop/p_cf_body_bone/cf_j_root/cf_n_height/cf_pv_root/cf_pv_hand_R");
			GameObject gameObject = new GameObject("RightHandCollider");
			gameObject.AddComponent<FixBodyParts>().Init(this, FixBodyParts.bodyParts.hand_R);
			gameObject.transform.parent = this.obj_cf_t_hand_R.transform;
			gameObject.transform.localPosition = Vector3.zero;
			this.obj_cf_t_hand_L = GameObject.Find("chaF_001/BodyTop/p_cf_body_bone/cf_t_root/cf_t_hand_L");
			this.bd_cf_t_hand_L = this.obj_cf_t_hand_L.GetComponent<BaseData>();
			this.obj_cf_pv_hand_L = GameObject.Find("chaF_001/BodyTop/p_cf_body_bone/cf_j_root/cf_n_height/cf_pv_root/cf_pv_hand_L");
			GameObject gameObject2 = new GameObject("LeftHandCollider");
			gameObject2.AddComponent<FixBodyParts>().Init(this, FixBodyParts.bodyParts.hand_L);
			gameObject2.transform.parent = this.obj_cf_t_hand_L.transform;
			gameObject2.transform.localPosition = Vector3.zero;
			this.obj_cf_t_leg_R = GameObject.Find("chaF_001/BodyTop/p_cf_body_bone/cf_t_root/cf_t_leg_R");
			this.bd_cf_t_leg_R = this.obj_cf_t_leg_R.GetComponent<BaseData>();
			this.obj_cf_pv_leg_R = GameObject.Find("chaF_001/BodyTop/p_cf_body_bone/cf_j_root/cf_n_height/cf_pv_root/cf_pv_leg_R");
			GameObject gameObject3 = new GameObject("RightLegCollider");
			gameObject3.AddComponent<FixBodyParts>().Init(this, FixBodyParts.bodyParts.leg_R);
			gameObject3.transform.parent = this.obj_cf_t_leg_R.transform;
			gameObject3.transform.localPosition = Vector3.zero;
			this.obj_cf_t_leg_L = GameObject.Find("chaF_001/BodyTop/p_cf_body_bone/cf_t_root/cf_t_leg_L");
			this.bd_cf_t_leg_L = this.obj_cf_t_leg_L.GetComponent<BaseData>();
			this.obj_cf_pv_leg_L = GameObject.Find("chaF_001/BodyTop/p_cf_body_bone/cf_j_root/cf_n_height/cf_pv_root/cf_pv_leg_L");
			GameObject gameObject4 = new GameObject("LeftLegCollider");
			gameObject4.AddComponent<FixBodyParts>().Init(this, FixBodyParts.bodyParts.leg_L);
			gameObject4.transform.parent = this.obj_cf_t_leg_L.transform;
			gameObject4.transform.localPosition = Vector3.zero;
			this.objCanvasSetParent = new GameObject("CanvasSetParent", new Type[]
			{
				typeof(Canvas)
			});
			this.canvasSetParent = this.objCanvasSetParent.GetComponent<Canvas>();
			this.objCanvasSetParent.AddComponent<GraphicRaycaster>();
			this.objCanvasSetParent.AddComponent<VRTK_UICanvas>();
			this.objCanvasSetParent.AddComponent<VRTK_UIGraphicRaycaster>();
			this.canvasScalerSetParent = this.objCanvasSetParent.AddComponent<CanvasScaler>();
			this.canvasScalerSetParent.dynamicPixelsPerUnit = 20000f;
			this.canvasScalerSetParent.referencePixelsPerUnit = 80000f;
			this.canvasSetParent.renderMode = RenderMode.WorldSpace;
			this.objCanvasSetParent.transform.localScale = new Vector3(0.005f, 0.005f, 0.005f);
			VRTK_UIPointer vrtk_UIPointer = this.cameraEye.AddComponent<VRTK_UIPointer>();
			vrtk_UIPointer.activationButton = VRTK_ControllerEvents.ButtonAlias.Undefined;
			vrtk_UIPointer.activationMode = VRTK_UIPointer.ActivationMethods.AlwaysOn;
			vrtk_UIPointer.selectionButton = VRTK_ControllerEvents.ButtonAlias.Undefined;
			vrtk_UIPointer.clickMethod = VRTK_UIPointer.ClickMethods.ClickOnButtonUp;
			vrtk_UIPointer.clickAfterHoverDuration = 1f;
			vrtk_UIPointer.controller = this.cameraEye.AddComponent<VRTK_ControllerEvents>();
			this.eventSystemSetParent = new GameObject("CanvasSetParentEventSystem", new Type[]
			{
				typeof(EventSystem)
			});
			this.eventSystemSetParent.AddComponent<StandaloneInputModule>();
			this.eventSystemSetParent.transform.SetParent(this.objCanvasSetParent.transform);
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
			gameObject5.transform.SetParent(this.objCanvasSetParent.transform);
			gameObject5.transform.localPosition = new Vector3(-28f, -48f, 0f);
			gameObject6.transform.SetParent(gameObject5.transform);
			gameObject6.transform.localPosition = Vector3.zero;
			gameObject5.GetComponent<Button>().onClick.AddListener(delegate()
			{
				this.PushFixRightHandButton();
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
			gameObject5.transform.SetParent(this.objCanvasSetParent.transform);
			gameObject5.transform.localPosition = new Vector3(28f, -48f, 0f);
			gameObject6.transform.SetParent(gameObject5.transform);
			gameObject6.transform.localPosition = Vector3.zero;
			gameObject5.GetComponent<Button>().onClick.AddListener(delegate()
			{
				this.PushFixLeftHandButton();
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
			gameObject5.transform.SetParent(this.objCanvasSetParent.transform);
			gameObject5.transform.localPosition = new Vector3(-28f, -28f, 0f);
			gameObject6.transform.SetParent(gameObject5.transform);
			gameObject6.transform.localPosition = Vector3.zero;
			gameObject5.GetComponent<Button>().onClick.AddListener(delegate()
			{
				this.PushFixRightLegButton();
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
			gameObject5.transform.SetParent(this.objCanvasSetParent.transform);
			gameObject5.transform.localPosition = new Vector3(28f, -28f, 0f);
			gameObject6.transform.SetParent(gameObject5.transform);
			gameObject6.transform.localPosition = Vector3.zero;
			gameObject5.GetComponent<Button>().onClick.AddListener(delegate()
			{
				this.PushFixLeftLegButton();
			});
			gameObject5 = new GameObject("button");
			gameObject6 = new GameObject("text", new Type[]
			{
				typeof(Text)
			});
			text = (this.txtFixBody = gameObject6.GetComponent<Text>());
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
			text.text = "手足固定 On";
			gameObject5.transform.SetParent(this.objCanvasSetParent.transform);
			gameObject5.transform.localPosition = new Vector3(-28f, -8f, 0f);
			gameObject6.transform.SetParent(gameObject5.transform);
			gameObject6.transform.localPosition = Vector3.zero;
			gameObject5.GetComponent<Button>().onClick.AddListener(delegate()
			{
				this.PushFixBodyButton();
			});
			gameObject5 = new GameObject("button");
			gameObject6 = new GameObject("text", new Type[]
			{
				typeof(Text)
			});
			text = (this.txtSetParentL = gameObject6.GetComponent<Text>());
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
			gameObject5.transform.SetParent(this.objCanvasSetParent.transform);
			gameObject5.transform.localPosition = new Vector3(-28f, 16f, 0f);
			gameObject6.transform.SetParent(gameObject5.transform);
			gameObject6.transform.localPosition = Vector3.zero;
			gameObject5.GetComponent<Button>().onClick.AddListener(delegate()
			{
				this.PushPLButton();
			});
			gameObject5 = new GameObject("button");
			gameObject6 = new GameObject("text", new Type[]
			{
				typeof(Text)
			});
			text = (this.txtSetParentR = gameObject6.GetComponent<Text>());
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
			gameObject5.transform.SetParent(this.objCanvasSetParent.transform);
			gameObject5.transform.localPosition = new Vector3(28f, 16f, 0f);
			gameObject6.transform.SetParent(gameObject5.transform);
			gameObject6.transform.localPosition = Vector3.zero;
			gameObject5.GetComponent<Button>().onClick.AddListener(delegate()
			{
				this.PushPRButton();
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
			gameObject5.transform.SetParent(this.objCanvasSetParent.transform);
			gameObject5.transform.localPosition = new Vector3(-28f, 40f, 0f);
			gameObject6.transform.SetParent(gameObject5.transform);
			gameObject6.transform.localPosition = Vector3.zero;
			gameObject5.GetComponent<Button>().onClick.AddListener(delegate()
			{
				this.PushMotionChangeButton();
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
			gameObject5.transform.SetParent(this.objCanvasSetParent.transform);
			gameObject5.transform.localPosition = new Vector3(28f, 40f, 0f);
			gameObject6.transform.SetParent(gameObject5.transform);
			gameObject6.transform.localPosition = Vector3.zero;
			gameObject5.GetComponent<Button>().onClick.AddListener(delegate()
			{
				this.PushModeChangeButton();
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
			gameObject5.transform.SetParent(this.objCanvasSetParent.transform);
			gameObject5.transform.localPosition = new Vector3(-28f, 60f, 0f);
			gameObject6.transform.SetParent(gameObject5.transform);
			gameObject6.transform.localPosition = Vector3.zero;
			gameObject5.GetComponent<Button>().onClick.AddListener(delegate()
			{
				this.PushFIButton();
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
			gameObject5.transform.SetParent(this.objCanvasSetParent.transform);
			gameObject5.transform.localPosition = new Vector3(28f, 60f, 0f);
			gameObject6.transform.SetParent(gameObject5.transform);
			gameObject6.transform.localPosition = Vector3.zero;
			gameObject5.GetComponent<Button>().onClick.AddListener(delegate()
			{
				this.PushFOButton();
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
			gameObject5.transform.SetParent(this.objCanvasSetParent.transform);
			gameObject5.transform.localPosition = new Vector3(-28f, 80f, 0f);
			gameObject6.transform.SetParent(gameObject5.transform);
			gameObject6.transform.localPosition = Vector3.zero;
			gameObject5.GetComponent<Button>().onClick.AddListener(delegate()
			{
				this.PushIButton();
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
			gameObject5.transform.SetParent(this.objCanvasSetParent.transform);
			gameObject5.transform.localPosition = new Vector3(28f, 80f, 0f);
			gameObject6.transform.SetParent(gameObject5.transform);
			gameObject6.transform.localPosition = Vector3.zero;
			gameObject5.GetComponent<Button>().onClick.AddListener(delegate()
			{
				this.PushINButton();
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
			gameObject5.transform.SetParent(this.objCanvasSetParent.transform);
			gameObject5.transform.localPosition = new Vector3(-28f, 100f, 0f);
			gameObject6.transform.SetParent(gameObject5.transform);
			gameObject6.transform.localPosition = Vector3.zero;
			gameObject5.GetComponent<Button>().onClick.AddListener(delegate()
			{
				this.PushIAButton();
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
			gameObject5.transform.SetParent(this.objCanvasSetParent.transform);
			gameObject5.transform.localPosition = new Vector3(28f, 100f, 0f);
			gameObject6.transform.SetParent(gameObject5.transform);
			gameObject6.transform.localPosition = Vector3.zero;
			gameObject5.GetComponent<Button>().onClick.AddListener(delegate()
			{
				this.PushIANButton();
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
			gameObject5.transform.SetParent(this.objCanvasSetParent.transform);
			gameObject5.transform.localPosition = new Vector3(-28f, 120f, 0f);
			gameObject6.transform.SetParent(gameObject5.transform);
			gameObject6.transform.localPosition = Vector3.zero;
			gameObject5.GetComponent<Button>().onClick.AddListener(delegate()
			{
				this.PushPullButton();
			});
			Vector3 point = this.femaleAim.transform.position - this.cameraEye.transform.position;
			point.y = 0f;
			point.Normalize();
			this.canvasSetParent.transform.position = new Vector3(this.femaleAim.transform.position.x, this.cameraEye.transform.position.y - 0.4f, this.femaleAim.transform.position.z) + Quaternion.Euler(0f, 90f, 0f) * point * 1.5f;
			this.canvasSetParent.transform.forward = (this.canvasSetParent.transform.position - this.cameraEye.transform.position).normalized;
			this.objCanvasMotion = new GameObject("CanvasMotion", new Type[]
			{
				typeof(Canvas)
			});
			this.canvasMotion = this.objCanvasMotion.GetComponent<Canvas>();
			this.objCanvasMotion.AddComponent<GraphicRaycaster>();
			this.objCanvasMotion.AddComponent<VRTK_UICanvas>();
			this.objCanvasMotion.AddComponent<VRTK_UIGraphicRaycaster>();
			this.canvasScalerMotion = this.objCanvasMotion.AddComponent<CanvasScaler>();
			this.canvasScalerMotion.dynamicPixelsPerUnit = 20000f;
			this.canvasScalerMotion.referencePixelsPerUnit = 80000f;
			this.canvasMotion.renderMode = RenderMode.WorldSpace;
			this.objCanvasMotion.transform.localScale = new Vector3(0.005f, 0.005f, 0.005f);
			this.eventSystemMotion = new GameObject("CanvasEventSystemMotion", new Type[]
			{
				typeof(EventSystem)
			});
			this.eventSystemMotion.AddComponent<StandaloneInputModule>();
			this.eventSystemMotion.transform.SetParent(this.objCanvasMotion.transform);
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
			gameObject5.transform.SetParent(this.objCanvasMotion.transform);
			gameObject5.transform.localPosition = new Vector3(-28f, -28f, 0f);
			gameObject6.transform.SetParent(gameObject5.transform);
			gameObject6.transform.localPosition = Vector3.zero;
			gameObject5.GetComponent<Button>().onClick.AddListener(delegate()
			{
				this.PushSeijyouiButton();
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
			gameObject5.transform.SetParent(this.objCanvasMotion.transform);
			gameObject5.transform.localPosition = new Vector3(28f, -28f, 0f);
			gameObject6.transform.SetParent(gameObject5.transform);
			gameObject6.transform.localPosition = Vector3.zero;
			gameObject5.GetComponent<Button>().onClick.AddListener(delegate()
			{
				this.PushKSeijyouiButton();
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
			gameObject5.transform.SetParent(this.objCanvasMotion.transform);
			gameObject5.transform.localPosition = new Vector3(-28f, -12f, 0f);
			gameObject6.transform.SetParent(gameObject5.transform);
			gameObject6.transform.localPosition = Vector3.zero;
			gameObject5.GetComponent<Button>().onClick.AddListener(delegate()
			{
				this.PushKohaiiButton();
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
			gameObject5.transform.SetParent(this.objCanvasMotion.transform);
			gameObject5.transform.localPosition = new Vector3(28f, -12f, 0f);
			gameObject6.transform.SetParent(gameObject5.transform);
			gameObject6.transform.localPosition = Vector3.zero;
			gameObject5.GetComponent<Button>().onClick.AddListener(delegate()
			{
				this.PushUdeHippariKohaiiButton();
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
			text.text = "騎乗位";
			gameObject5.transform.SetParent(this.objCanvasMotion.transform);
			gameObject5.transform.localPosition = new Vector3(-28f, 4f, 0f);
			gameObject6.transform.SetParent(gameObject5.transform);
			gameObject6.transform.localPosition = Vector3.zero;
			gameObject5.GetComponent<Button>().onClick.AddListener(delegate()
			{
				this.PushKijyouiButton();
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
			text.text = "側位";
			gameObject5.transform.SetParent(this.objCanvasMotion.transform);
			gameObject5.transform.localPosition = new Vector3(28f, 4f, 0f);
			gameObject6.transform.SetParent(gameObject5.transform);
			gameObject6.transform.localPosition = Vector3.zero;
			gameObject5.GetComponent<Button>().onClick.AddListener(delegate()
			{
				this.PushSokuiButton();
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
			gameObject5.transform.SetParent(this.objCanvasMotion.transform);
			gameObject5.transform.localPosition = new Vector3(-28f, 20f, 0f);
			gameObject6.transform.SetParent(gameObject5.transform);
			gameObject6.transform.localPosition = Vector3.zero;
			gameObject5.GetComponent<Button>().onClick.AddListener(delegate()
			{
				this.PushRituiButton();
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
			gameObject5.transform.SetParent(this.objCanvasMotion.transform);
			gameObject5.transform.localPosition = new Vector3(28f, 20f, 0f);
			gameObject6.transform.SetParent(gameObject5.transform);
			gameObject6.transform.localPosition = Vector3.zero;
			gameObject5.GetComponent<Button>().onClick.AddListener(delegate()
			{
				this.PushEkibenButton();
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
			gameObject5.transform.SetParent(this.objCanvasMotion.transform);
			gameObject5.transform.localPosition = new Vector3(-28f, 36f, 0f);
			gameObject6.transform.SetParent(gameObject5.transform);
			gameObject6.transform.localPosition = Vector3.zero;
			gameObject5.GetComponent<Button>().onClick.AddListener(delegate()
			{
				this.PushIsuTaimenButton();
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
			gameObject5.transform.SetParent(this.objCanvasMotion.transform);
			gameObject5.transform.localPosition = new Vector3(28f, 36f, 0f);
			gameObject6.transform.SetParent(gameObject5.transform);
			gameObject6.transform.localPosition = Vector3.zero;
			gameObject5.GetComponent<Button>().onClick.AddListener(delegate()
			{
				this.PushIsuHaimenButton();
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
			text.text = "椅子バック";
			gameObject5.transform.SetParent(this.objCanvasMotion.transform);
			gameObject5.transform.localPosition = new Vector3(-28f, 52f, 0f);
			gameObject6.transform.SetParent(gameObject5.transform);
			gameObject6.transform.localPosition = Vector3.zero;
			gameObject5.GetComponent<Button>().onClick.AddListener(delegate()
			{
				this.PushIsuBackButton();
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
			text.text = "腕引っ張り椅子バック";
			gameObject5.transform.SetParent(this.objCanvasMotion.transform);
			gameObject5.transform.localPosition = new Vector3(28f, 52f, 0f);
			gameObject6.transform.SetParent(gameObject5.transform);
			gameObject6.transform.localPosition = Vector3.zero;
			gameObject5.GetComponent<Button>().onClick.AddListener(delegate()
			{
				this.PushUdeHippariIsuBackButton();
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
			text.text = "机寝位";
			gameObject5.transform.SetParent(this.objCanvasMotion.transform);
			gameObject5.transform.localPosition = new Vector3(-28f, 68f, 0f);
			gameObject6.transform.SetParent(gameObject5.transform);
			gameObject6.transform.localPosition = Vector3.zero;
			gameObject5.GetComponent<Button>().onClick.AddListener(delegate()
			{
				this.PushTukueNeiButton();
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
			gameObject5.transform.SetParent(this.objCanvasMotion.transform);
			gameObject5.transform.localPosition = new Vector3(28f, 68f, 0f);
			gameObject6.transform.SetParent(gameObject5.transform);
			gameObject6.transform.localPosition = Vector3.zero;
			gameObject5.GetComponent<Button>().onClick.AddListener(delegate()
			{
				this.PushTukueSokuiButton();
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
			text.text = "机バック";
			gameObject5.transform.SetParent(this.objCanvasMotion.transform);
			gameObject5.transform.localPosition = new Vector3(-28f, 84f, 0f);
			gameObject6.transform.SetParent(gameObject5.transform);
			gameObject6.transform.localPosition = Vector3.zero;
			gameObject5.GetComponent<Button>().onClick.AddListener(delegate()
			{
				this.PushTukueBackButton();
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
			text.text = "腕引っ張り机バック";
			gameObject5.transform.SetParent(this.objCanvasMotion.transform);
			gameObject5.transform.localPosition = new Vector3(28f, 84f, 0f);
			gameObject6.transform.SetParent(gameObject5.transform);
			gameObject6.transform.localPosition = Vector3.zero;
			gameObject5.GetComponent<Button>().onClick.AddListener(delegate()
			{
				this.PushUdeHippariTukueBackButton();
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
			gameObject5.transform.SetParent(this.objCanvasMotion.transform);
			gameObject5.transform.localPosition = new Vector3(-28f, 100f, 0f);
			gameObject6.transform.SetParent(gameObject5.transform);
			gameObject6.transform.localPosition = Vector3.zero;
			gameObject5.GetComponent<Button>().onClick.AddListener(delegate()
			{
				this.PushKabeTaimenKataasiageButton();
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
			text.text = "壁バック";
			gameObject5.transform.SetParent(this.objCanvasMotion.transform);
			gameObject5.transform.localPosition = new Vector3(28f, 100f, 0f);
			gameObject6.transform.SetParent(gameObject5.transform);
			gameObject6.transform.localPosition = Vector3.zero;
			gameObject5.GetComponent<Button>().onClick.AddListener(delegate()
			{
				this.PushKabeBackButton();
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
			text.text = "片足上げ壁バック";
			gameObject5.transform.SetParent(this.objCanvasMotion.transform);
			gameObject5.transform.localPosition = new Vector3(-28f, 116f, 0f);
			gameObject6.transform.SetParent(gameObject5.transform);
			gameObject6.transform.localPosition = Vector3.zero;
			gameObject5.GetComponent<Button>().onClick.AddListener(delegate()
			{
				this.PushKataasiageKabeBackButton();
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
			text.text = "プールバック";
			gameObject5.transform.SetParent(this.objCanvasMotion.transform);
			gameObject5.transform.localPosition = new Vector3(28f, 116f, 0f);
			gameObject6.transform.SetParent(gameObject5.transform);
			gameObject6.transform.localPosition = Vector3.zero;
			gameObject5.GetComponent<Button>().onClick.AddListener(delegate()
			{
				this.PushPoolBackButton();
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
			text.text = "フェンス後背位";
			gameObject5.transform.SetParent(this.objCanvasMotion.transform);
			gameObject5.transform.localPosition = new Vector3(-28f, 132f, 0f);
			gameObject6.transform.SetParent(gameObject5.transform);
			gameObject6.transform.localPosition = Vector3.zero;
			gameObject5.GetComponent<Button>().onClick.AddListener(delegate()
			{
				this.PushFenceKouhaiiButton();
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
			text.text = "フェンス掴まり駅弁";
			gameObject5.transform.SetParent(this.objCanvasMotion.transform);
			gameObject5.transform.localPosition = new Vector3(28f, 132f, 0f);
			gameObject6.transform.SetParent(gameObject5.transform);
			gameObject6.transform.localPosition = Vector3.zero;
			gameObject5.GetComponent<Button>().onClick.AddListener(delegate()
			{
				this.PushFenceTukamariEkibenButton();
			});
			point = this.femaleAim.transform.position - this.cameraEye.transform.position;
			point.y = 0f;
			point.Normalize();
			this.canvasMotion.transform.position = new Vector3(this.femaleAim.transform.position.x, this.cameraEye.transform.position.y - 0.4f, this.femaleAim.transform.position.z) + Quaternion.Euler(0f, -90f, 0f) * point * 1.5f;
			this.canvasMotion.transform.forward = (this.canvasMotion.transform.position - this.cameraEye.transform.position).normalized;
			if (this.setCollider)
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
				gameObject8.transform.parent = this.cameraEye.transform;
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
			this.bFixBody = !this.bFixBody;
		}

		public void PushFixRightHandButton()
		{
			if (this.objRightHand == null)
			{
				this.objRightHand = new GameObject("objRightHand");
				this.objRightHand.transform.position = this.obj_cf_pv_hand_R.transform.position;
				this.objRightHand.transform.rotation = this.obj_cf_pv_hand_R.transform.rotation;
				this.bd_cf_t_hand_R.bone = this.objRightHand.transform;
				return;
			}
			UnityEngine.Object.DestroyImmediate(this.objRightHand);
			this.objRightHand = null;
			this.bd_cf_t_hand_R.bone = this.obj_cf_pv_hand_R.transform;
		}

		public void PushFixLeftHandButton()
		{
			if (this.objLeftHand == null)
			{
				this.objLeftHand = new GameObject("objLeftHand");
				this.objLeftHand.transform.position = this.obj_cf_pv_hand_L.transform.position;
				this.objLeftHand.transform.rotation = this.obj_cf_pv_hand_L.transform.rotation;
				this.bd_cf_t_hand_L.bone = this.objLeftHand.transform;
				return;
			}
			UnityEngine.Object.DestroyImmediate(this.objLeftHand);
			this.objLeftHand = null;
			this.bd_cf_t_hand_L.bone = this.obj_cf_pv_hand_L.transform;
		}

		public void PushFixRightLegButton()
		{
			if (this.objRightLeg == null)
			{
				this.objRightLeg = new GameObject("objRightLeg");
				this.objRightLeg.transform.position = this.obj_cf_pv_leg_R.transform.position;
				this.objRightLeg.transform.rotation = this.obj_cf_pv_leg_R.transform.rotation;
				this.bd_cf_t_leg_R.bone = this.objRightLeg.transform;
				return;
			}
			UnityEngine.Object.DestroyImmediate(this.objRightLeg);
			this.objRightLeg = null;
			this.bd_cf_t_leg_R.bone = this.obj_cf_pv_leg_R.transform;
		}

		public void PushFixLeftLegButton()
		{
			if (this.objLeftLeg == null)
			{
				this.objLeftLeg = new GameObject("objLeftLeg");
				this.objLeftLeg.transform.position = this.obj_cf_pv_leg_L.transform.position;
				this.objLeftLeg.transform.rotation = this.obj_cf_pv_leg_L.transform.rotation;
				this.bd_cf_t_leg_L.bone = this.objLeftLeg.transform;
				return;
			}
			UnityEngine.Object.DestroyImmediate(this.objLeftLeg);
			this.objLeftLeg = null;
			this.bd_cf_t_leg_L.bone = this.obj_cf_pv_leg_L.transform;
		}

		private void ChangeMotion(string path, string name)
		{
			Animator component = GameObject.Find("chaF_001/BodyTop/p_cf_body_bone").GetComponent<Animator>();
			RuntimeAnimatorController runtimeAnimatorController = CommonLib.LoadAsset<RuntimeAnimatorController>(path, name, false, string.Empty);
			AnimatorOverrideController animatorOverrideController = new AnimatorOverrideController(component.runtimeAnimatorController);
			foreach (AnimationClip animationClip in new AnimatorOverrideController(runtimeAnimatorController).animationClips)
			{
				animatorOverrideController[animationClip.name] = animationClip;
			}
			animatorOverrideController.name = runtimeAnimatorController.name;
			component.runtimeAnimatorController = animatorOverrideController;
			AssetBundleManager.UnloadAssetBundle(path, true, null, false);
		}

		private void PushSeijyouiButton()
		{
			this.ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_00");
		}

		private void PushKSeijyouiButton()
		{
			this.ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_n00");
		}

		private void PushKohaiiButton()
		{
			this.ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_02");
		}

		private void PushUdeHippariKohaiiButton()
		{
			this.ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_n02");
		}

		private void PushKijyouiButton()
		{
			this.ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_n04");
		}

		private void PushSokuiButton()
		{
			this.ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_n06");
		}

		private void PushRituiButton()
		{
			this.ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_n07");
		}

		private void PushEkibenButton()
		{
			this.ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_n08");
		}

		private void PushIsuTaimenButton()
		{
			this.ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_n09");
		}

		private void PushIsuHaimenButton()
		{
			this.ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_n10");
		}

		private void PushIsuBackButton()
		{
			this.ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_11");
		}

		private void PushUdeHippariIsuBackButton()
		{
			this.ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_n11");
		}

		private void PushTukueNeiButton()
		{
			this.ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_n13");
		}

		private void PushTukueBackButton()
		{
			this.ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_14");
		}

		private void PushUdeHippariTukueBackButton()
		{
			this.ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_n14");
		}

		private void PushTukueSokuiButton()
		{
			this.ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_n16");
		}

		private void PushKabeTaimenKataasiageButton()
		{
			this.ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_n17");
		}

		private void PushKabeBackButton()
		{
			this.ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_18");
		}

		private void PushKataasiageKabeBackButton()
		{
			this.ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_n18");
		}

		private void PushPoolBackButton()
		{
			this.ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_n20");
		}

		private void PushFenceKouhaiiButton()
		{
			this.ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_n21");
		}

		private void PushFenceTukamariEkibenButton()
		{
			this.ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_n22");
		}

		private void PushPullButton()
		{
			this.hSprite.OnPullClick();
		}

		private void PushIANButton()
		{
			this.hSprite.OnInsertAnalNoVoiceClick();
		}

		private void PushIAButton()
		{
			this.hSprite.OnInsertAnalClick();
		}

		private void PushINButton()
		{
			this.hSprite.OnInsertNoVoiceClick();
		}

		private void PushIButton()
		{
			this.hSprite.OnInsertClick();
		}

		private void PushPLButton()
		{
			if (!this.setFlag)
			{
				this.SetP(true);
			}
			else
			{
				this.UnsetP();
			}
			this.setFlag = !this.setFlag;
		}

		private void PushPRButton()
		{
			if (!this.setFlag)
			{
				this.SetP(false);
			}
			else
			{
				this.UnsetP();
			}
			this.setFlag = !this.setFlag;
		}

		private void PushModeChangeButton()
		{
			this.hFlag.click = HFlag.ClickKind.modeChange;
		}

		private void PushMotionChangeButton()
		{
			this.hFlag.click = HFlag.ClickKind.motionchange;
			AnimSpeedController component = GameObject.Find("chaF_001").GetComponent<AnimSpeedController>();
			if (component != null)
			{
				component.weakMotion = !component.weakMotion;
			}
		}

		private void PushFIButton()
		{
			this.hSprite.OnInsideClick();
			AnimSpeedController component = GameObject.Find("chaF_001").GetComponent<AnimSpeedController>();
			if (component != null)
			{
				component.fcount = 0f;
				component.moveFlag = false;
			}
		}

		private void PushFOButton()
		{
			this.hSprite.OnOutsideClick();
			AnimSpeedController component = GameObject.Find("chaF_001").GetComponent<AnimSpeedController>();
			if (component != null)
			{
				component.fcount = 0f;
				component.moveFlag = false;
			}
		}

		public void OnLevelWasInitialized(int level)
		{
		}

		public void OnFixedUpdate()
		{
		}

		public void OnUpdate()
		{
			if (!this.femaleFlag)
			{
				if (GameObject.Find("chaF_001") == null)
				{
					this.femaleFlag = false;
					return;
				}
				this.femaleFlag = true;
			}
			if (this.leftDevice == null)
			{
				this.leftController = GameObject.Find("VRTK/[VRTK_SDKManager]/SDKSetups/SteamVR/VRCameraBase/[CameraRig]/Controller (left)");
				this.leftVVC = this.leftController.GetComponent<VRViveController>();
				this.leftDevice = (this.f_device.GetValue(this.leftVVC) as SteamVR_Controller.Device);
			}
			if (this.rightDevice == null)
			{
				this.rightController = GameObject.Find("VRTK/[VRTK_SDKManager]/SDKSetups/SteamVR/VRCameraBase/[CameraRig]/Controller (right)");
				this.rightVVC = this.rightController.GetComponent<VRViveController>();
				this.rightDevice = (this.f_device.GetValue(this.rightVVC) as SteamVR_Controller.Device);
			}
			if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl))
			{
				this.LoadFromModPref();
			}
			if (this.objCanvasSetParent == null)
			{
				this.InitCanvas();
			}
			else
			{
				if (this.RightMenuPress() || this.LeftMenuPress())
				{
					this.hideCount += Time.deltaTime;
					if (this.hideCount >= 2f)
					{
						this.objCanvasSetParent.SetActive(!this.objCanvasSetParent.activeSelf);
						this.objCanvasMotion.SetActive(!this.objCanvasMotion.activeSelf);
						this.hideCount = 0f;
						if (this.objCanvasSetParent.activeSelf)
						{
							this.hideCanvas = false;
						}
						else
						{
							this.hideCanvas = true;
						}
					}
				}
				else
				{
					this.hideCount = 0f;
				}
				Vector3 point = this.femaleAim.transform.position - this.cameraEye.transform.position;
				point.y = 0f;
				point.Normalize();
				this.objCanvasSetParent.transform.position = new Vector3(this.femaleAim.transform.position.x, this.cameraEye.transform.position.y, this.femaleAim.transform.position.z) + Quaternion.Euler(0f, 90f, 0f) * point * 0.4f;
				this.objCanvasSetParent.transform.forward = (this.objCanvasSetParent.transform.position - this.cameraEye.transform.position).normalized;
				this.objCanvasMotion.transform.position = new Vector3(this.femaleAim.transform.position.x, this.cameraEye.transform.position.y, this.femaleAim.transform.position.z) + Quaternion.Euler(0f, -90f, 0f) * point * 0.4f;
				this.objCanvasMotion.transform.forward = (this.objCanvasMotion.transform.position - this.cameraEye.transform.position).normalized;
			}
			if (this.objRightHand != null && !this.bFixBody)
			{
				if ((this.obj_cf_t_hand_R.transform.position - this.obj_cf_pv_hand_R.transform.position).magnitude > 0.5f)
				{
					this.PushFixRightHandButton();
				}
				else
				{
					this.bd_cf_t_hand_R.bone = this.objRightHand.transform;
				}
			}
			if (this.objLeftHand != null && !this.bFixBody)
			{
				if ((this.obj_cf_t_hand_L.transform.position - this.obj_cf_pv_hand_L.transform.position).magnitude > 0.5f)
				{
					this.PushFixLeftHandButton();
				}
				else
				{
					this.bd_cf_t_hand_L.bone = this.objLeftHand.transform;
				}
			}
			if (this.objRightLeg != null && !this.bFixBody)
			{
				if ((this.obj_cf_t_leg_R.transform.position - this.obj_cf_pv_leg_R.transform.position).magnitude > 0.5f)
				{
					this.PushFixRightLegButton();
				}
				else
				{
					this.bd_cf_t_leg_R.bone = this.objRightLeg.transform;
				}
			}
			if (this.objLeftLeg != null && !this.bFixBody)
			{
				if ((this.obj_cf_t_leg_L.transform.position - this.obj_cf_pv_leg_L.transform.position).magnitude > 0.5f)
				{
					this.PushFixLeftLegButton();
				}
				else
				{
					this.bd_cf_t_leg_L.bone = this.objLeftLeg.transform;
				}
			}
			if (Input.GetKeyDown(KeyCode.Backslash) || (this.RightMenuPress() && this.RightTriggerPressDown()) || (this.LeftMenuPress() && this.LeftTriggerPressDown()))
			{
				if (!this.setFlag)
				{
					if (this.RightMenuPress() && this.RightTriggerPressDown())
					{
						this.SetP(true);
					}
					else if (this.LeftMenuPress() && this.LeftTriggerPressDown())
					{
						this.SetP(false);
					}
					else
					{
						this.SetP(true);
					}
				}
				else
				{
					this.UnsetP();
				}
				this.setFlag = !this.setFlag;
			}
			if (this.setFlag)
			{
				if (this.nameAnimation != this.hFlag.nowAnimationInfo.nameAnimation)
				{
					this.UnsetP();
					this.setFlag = !this.setFlag;
					GameObject.Find("chaF_001");
					this.nameAnimation = this.hFlag.nowAnimationInfo.nameAnimation;
				}
				if (this.setParentMode == 0 || this.setParentMode == 1)
				{
					if (this.leftController == null)
					{
						this.leftController = GameObject.Find("VRTK/[VRTK_SDKManager]/SDKSetups/SteamVR/VRCameraBase/[CameraRig]/Controller (left)");
					}
					this.quatSpineRot[this.indexSpineRot] = this.objSpinePos.transform.rotation;
					if (this.indexSpineRot >= 19)
					{
						this.indexSpineRot = 0;
					}
					else
					{
						this.indexSpineRot++;
					}
					Quaternion quaternion = this.quatSpineRot[0];
					for (int i = 1; i < 20; i++)
					{
						quaternion = Quaternion.Lerp(quaternion, this.quatSpineRot[i], 1f / (float)(i + 1));
					}
					if (this.trackingMode)
					{
						switch (this.parentPart)
						{
						case 0:
							this.obj_p_cf_body_bone.transform.rotation = quaternion * Quaternion.Inverse(this.obj_cf_j_hips.transform.localRotation) * Quaternion.Inverse(this.obj_cf_n_height.transform.localRotation) * Quaternion.Inverse(this.obj_cf_j_root.transform.localRotation);
							break;
						case 1:
							this.obj_p_cf_body_bone.transform.rotation = quaternion * Quaternion.Inverse(this.obj_cf_j_spine02.transform.localRotation) * Quaternion.Inverse(this.obj_cf_j_spine01.transform.localRotation) * Quaternion.Inverse(this.obj_cf_j_hips.transform.localRotation) * Quaternion.Inverse(this.obj_cf_n_height.transform.localRotation) * Quaternion.Inverse(this.obj_cf_j_root.transform.localRotation);
							break;
						case 2:
							this.obj_p_cf_body_bone.transform.rotation = quaternion * Quaternion.Inverse(this.obj_cf_j_neck.transform.localRotation) * Quaternion.Inverse(this.obj_cf_j_spine03.transform.localRotation) * Quaternion.Inverse(this.obj_cf_j_spine02.transform.localRotation) * Quaternion.Inverse(this.obj_cf_j_spine01.transform.localRotation) * Quaternion.Inverse(this.obj_cf_j_hips.transform.localRotation) * Quaternion.Inverse(this.obj_cf_n_height.transform.localRotation) * Quaternion.Inverse(this.obj_cf_j_root.transform.localRotation);
							break;
						default:
							this.obj_p_cf_body_bone.transform.rotation = quaternion * Quaternion.Inverse(this.obj_cf_j_spine02.transform.localRotation) * Quaternion.Inverse(this.obj_cf_j_spine01.transform.localRotation) * Quaternion.Inverse(this.obj_cf_j_hips.transform.localRotation) * Quaternion.Inverse(this.obj_cf_n_height.transform.localRotation) * Quaternion.Inverse(this.obj_cf_j_root.transform.localRotation);
							break;
						}
					}
					else
					{
						switch (this.parentPart)
						{
						case 0:
							this.obj_p_cf_body_bone.transform.rotation = this.objSpinePos.transform.rotation * Quaternion.Inverse(this.obj_cf_j_hips.transform.localRotation) * Quaternion.Inverse(this.obj_cf_n_height.transform.localRotation) * Quaternion.Inverse(this.obj_cf_j_root.transform.localRotation);
							break;
						case 1:
							this.obj_p_cf_body_bone.transform.rotation = this.objSpinePos.transform.rotation * Quaternion.Inverse(this.obj_cf_j_spine02.transform.localRotation) * Quaternion.Inverse(this.obj_cf_j_spine01.transform.localRotation) * Quaternion.Inverse(this.obj_cf_j_hips.transform.localRotation) * Quaternion.Inverse(this.obj_cf_n_height.transform.localRotation) * Quaternion.Inverse(this.obj_cf_j_root.transform.localRotation);
							break;
						case 2:
							this.obj_p_cf_body_bone.transform.rotation = this.objSpinePos.transform.rotation * Quaternion.Inverse(this.obj_cf_j_neck.transform.localRotation) * Quaternion.Inverse(this.obj_cf_j_spine03.transform.localRotation) * Quaternion.Inverse(this.obj_cf_j_spine02.transform.localRotation) * Quaternion.Inverse(this.obj_cf_j_spine01.transform.localRotation) * Quaternion.Inverse(this.obj_cf_j_hips.transform.localRotation) * Quaternion.Inverse(this.obj_cf_n_height.transform.localRotation) * Quaternion.Inverse(this.obj_cf_j_root.transform.localRotation);
							break;
						default:
							this.obj_p_cf_body_bone.transform.rotation = this.objSpinePos.transform.rotation * Quaternion.Inverse(this.obj_cf_j_spine02.transform.localRotation) * Quaternion.Inverse(this.obj_cf_j_spine01.transform.localRotation) * Quaternion.Inverse(this.obj_cf_j_hips.transform.localRotation) * Quaternion.Inverse(this.obj_cf_n_height.transform.localRotation) * Quaternion.Inverse(this.obj_cf_j_root.transform.localRotation);
							break;
						}
					}
					this.vecSpinePos[this.indexSpinePos] = this.objSpinePos.transform.position;
					if (this.indexSpinePos >= 19)
					{
						this.indexSpinePos = 0;
					}
					else
					{
						this.indexSpinePos++;
					}
					Vector3 a = Vector3.zero;
					foreach (Vector3 b in this.vecSpinePos)
					{
						a += b;
					}
					a /= 20f;
					if (this.trackingMode)
					{
						this.obj_p_cf_body_bone.transform.position += a - this.objBase.transform.position;
					}
					else
					{
						this.obj_p_cf_body_bone.transform.position += this.objSpinePos.transform.position - this.objBase.transform.position;
					}
				}
				if (this.hideCanvas)
				{
					Vector3 vector;
					if (this.parentIsLeft)
					{
						vector = this.cameraEye.transform.position - this.rightController.transform.position;
					}
					else
					{
						vector = this.cameraEye.transform.position - this.leftController.transform.position;
					}
					if (vector.magnitude <= 0.4f)
					{
						this.objCanvasSetParent.SetActive(true);
						this.objCanvasMotion.SetActive(true);
					}
					else
					{
						this.objCanvasSetParent.SetActive(false);
						this.objCanvasMotion.SetActive(false);
					}
				}
				this.txtSetParentL.text = "親子付け Off";
				this.txtSetParentR.text = "親子付け Off";
			}
			else
			{
				this.txtSetParentL.text = "左 親子付け On";
				this.txtSetParentR.text = "右 親子付け On";
			}
			if (this.bFixBody)
			{
				if (this.objRightHand != null)
				{
					this.bd_cf_t_hand_R.bone = this.objRightHand.transform;
				}
				if (this.objLeftHand != null)
				{
					this.bd_cf_t_hand_L.bone = this.objLeftHand.transform;
				}
				if (this.objRightLeg != null)
				{
					this.bd_cf_t_leg_R.bone = this.objRightLeg.transform;
				}
				if (this.objLeftLeg != null)
				{
					this.bd_cf_t_leg_L.bone = this.objLeftLeg.transform;
				}
				this.txtFixBody.text = "手足固定 On";
				return;
			}
			this.txtFixBody.text = "手足固定 Off";
		}

		private void SetP(bool _parentIsLeft)
		{
			GameObject gameObject = GameObject.Find("chaF_001");
			if (gameObject == null)
			{
				return;
			}
			GameObject gameObject2 = GameObject.Find("chaM_001");
			if (gameObject2 == null)
			{
				return;
			}
			this.parentIsLeft = _parentIsLeft;
			this.hFlag = GameObject.Find("VRHScene").GetComponent<HFlag>();
			this.nameAnimation = this.hFlag.nowAnimationInfo.nameAnimation;
			this.cameraEye = GameObject.Find("VRTK/[VRTK_SDKManager]/SDKSetups/SteamVR/VRCameraBase/[CameraRig]/Camera (eye)");
			this.maleAim = GameObject.Find("chaM_001/BodyTop/p_cf_body_bone/cf_j_root/cf_n_height/cf_j_hips/cf_j_spine01/cf_j_spine02/cf_j_spine03/cf_j_neck/cf_j_head/cf_s_head/aim");
			this.femaleAim = GameObject.Find("chaF_001/BodyTop/p_cf_body_bone/cf_j_root/cf_n_height/cf_j_hips/cf_j_spine01/cf_j_spine02/cf_j_spine03/cf_j_neck/cf_j_head/cf_s_head/aim");
			this.leftController = GameObject.Find("VRTK/[VRTK_SDKManager]/SDKSetups/SteamVR/VRCameraBase/[CameraRig]/Controller (left)");
			this.rightController = GameObject.Find("VRTK/[VRTK_SDKManager]/SDKSetups/SteamVR/VRCameraBase/[CameraRig]/Controller (right)");
			if (this.setParentMode == 0 || this.setParentMode == 1)
			{
				this.obj_chaF_001 = GameObject.Find("chaF_001");
				this.obj_BodyTop = GameObject.Find("chaF_001/BodyTop");
				this.obj_p_cf_body_bone = GameObject.Find("chaF_001/BodyTop/p_cf_body_bone");
				this.obj_cf_j_root = GameObject.Find("chaF_001/BodyTop/p_cf_body_bone/cf_j_root");
				this.obj_cf_n_height = GameObject.Find("chaF_001/BodyTop/p_cf_body_bone/cf_j_root/cf_n_height");
				this.obj_cf_j_hips = GameObject.Find("chaF_001/BodyTop/p_cf_body_bone/cf_j_root/cf_n_height/cf_j_hips");
				this.obj_cf_j_spine01 = GameObject.Find("chaF_001/BodyTop/p_cf_body_bone/cf_j_root/cf_n_height/cf_j_hips/cf_j_spine01");
				this.obj_cf_j_spine02 = GameObject.Find("chaF_001/BodyTop/p_cf_body_bone/cf_j_root/cf_n_height/cf_j_hips/cf_j_spine01/cf_j_spine02");
				this.obj_cf_j_spine03 = GameObject.Find("chaF_001/BodyTop/p_cf_body_bone/cf_j_root/cf_n_height/cf_j_hips/cf_j_spine01/cf_j_spine02/cf_j_spine03");
				this.obj_cf_j_neck = GameObject.Find("chaF_001/BodyTop/p_cf_body_bone/cf_j_root/cf_n_height/cf_j_hips/cf_j_spine01/cf_j_spine02/cf_j_spine03/cf_j_neck");
				switch (this.parentPart)
				{
				case 0:
					this.objBase = this.obj_cf_j_hips;
					break;
				case 1:
					this.objBase = this.obj_cf_j_spine02;
					break;
				case 2:
					this.objBase = this.obj_cf_j_neck;
					break;
				default:
					this.objBase = this.obj_cf_j_spine02;
					break;
				}
				if (this.objSpinePos == null)
				{
					this.objSpinePos = new GameObject("objSpinePos");
				}
				if (_parentIsLeft)
				{
					this.objSpinePos.transform.parent = this.leftController.transform;
					GameObject.Find("VRTK/[VRTK_SDKManager]/SDKSetups/SteamVR/VRCameraBase/[CameraRig]/Controller (left)/Model").SetActive(false);
				}
				else
				{
					this.objSpinePos.transform.parent = this.rightController.transform;
					GameObject.Find("VRTK/[VRTK_SDKManager]/SDKSetups/SteamVR/VRCameraBase/[CameraRig]/Controller (right)/Model").SetActive(false);
				}
				this.objSpinePos.transform.position = this.objBase.transform.position;
				this.objSpinePos.transform.rotation = this.objBase.transform.rotation;
				for (int i = 0; i < 20; i++)
				{
					this.vecSpinePos[i] = this.objSpinePos.transform.position;
				}
				this.indexSpinePos = 0;
				for (int j = 0; j < 20; j++)
				{
					this.quatSpineRot[j] = this.objSpinePos.transform.rotation;
				}
				this.indexSpineRot = 0;
			}
			if (this.setParentMale)
			{
				gameObject2.GetComponent<Transform>().parent = this.cameraEye.transform;
			}
			if (this.setParentMode == 1 || this.setParentMode == 2)
			{
				AnimSpeedController animSpeedController = gameObject.AddComponent<AnimSpeedController>();
				if (_parentIsLeft)
				{
					animSpeedController.SetController(this.leftController, this.rightController);
					return;
				}
				animSpeedController.SetController(this.rightController, this.leftController);
			}
		}

		public void UnsetP()
		{
			GameObject gameObject = GameObject.Find("chaF_001");
			if (gameObject == null)
			{
				return;
			}
			gameObject.transform.parent = GameObject.Find("Component").transform;
			GameObject gameObject2 = GameObject.Find("chaM_001");
			if (gameObject2 == null)
			{
				return;
			}
			gameObject2.transform.parent = GameObject.Find("Component").transform;
			GameObject.Find("VRTK/[VRTK_SDKManager]/SDKSetups/SteamVR/VRCameraBase/[CameraRig]/Controller (left)/Model").SetActive(true);
			GameObject.Find("VRTK/[VRTK_SDKManager]/SDKSetups/SteamVR/VRCameraBase/[CameraRig]/Controller (right)/Model").SetActive(true);
			if (gameObject.GetComponent<AnimSpeedController>() != null)
			{
				UnityEngine.Object.Destroy(gameObject.GetComponent<AnimSpeedController>());
			}
		}

		private bool RightTrackPadPressDown()
		{
			return this.rightVVC.IsPressDown(VRViveController.EViveButtonKind.Touchpad, -1) || this.rightDevice.GetPressDown(4294967296UL);
		}

		private bool LeftTrackPadPressDown()
		{
			return this.leftVVC.IsPressDown(VRViveController.EViveButtonKind.Touchpad, -1) || this.leftDevice.GetPressDown(4294967296UL);
		}

		private bool RightMenuPress()
		{
			return this.rightVVC.IsState(VRViveController.EViveButtonKind.Menu, -1) || this.rightDevice.GetPress(2UL);
		}

		private bool LeftMenuPress()
		{
			return this.leftVVC.IsState(VRViveController.EViveButtonKind.Menu, -1) || this.leftDevice.GetPress(2UL);
		}

		private bool RightTriggerPressDown()
		{
			return this.rightVVC.IsPressDown(VRViveController.EViveButtonKind.Trigger, -1) || this.rightDevice.GetPressDown(8589934592UL);
		}

		private bool LeftTriggerPressDown()
		{
			return this.leftVVC.IsPressDown(VRViveController.EViveButtonKind.Trigger, -1) || this.leftDevice.GetPressDown(8589934592UL);
		}

		private bool LeftGripPress()
		{
			return this.leftVVC.IsState(VRViveController.EViveButtonKind.Grip, -1) || this.leftDevice.GetPress(4UL);
		}

		private bool LeftGripPressDown()
		{
			return this.leftVVC.IsPressDown(VRViveController.EViveButtonKind.Grip, -1) || this.leftDevice.GetPressDown(4UL);
		}

		private bool RightGripPress()
		{
			return this.rightVVC.IsState(VRViveController.EViveButtonKind.Grip, -1) || this.rightDevice.GetPress(4UL);
		}

		private bool RightGripPressDown()
		{
			return this.rightVVC.IsPressDown(VRViveController.EViveButtonKind.Grip, -1) || this.rightDevice.GetPressDown(4UL);
		}

		private void LoadFromModPref()
		{
			this.setParentMode = ModPrefs.GetInt("SetParent", "setParentMode", 1, true);
			this.setParentMale = ModPrefs.GetBool("SetParent", "setParentMale", false, true);
			this.setCollider = ModPrefs.GetBool("SetParent", "SetCollider", true, true);
			this.parentPart = ModPrefs.GetInt("SetParent", "ParentPart", 1, true);
			this.trackingMode = ModPrefs.GetBool("SetParent", "TrackingMode", true, true);
		}

		private bool setFlag;

		private bool femaleFlag;

		private HFlag hFlag;

		private string nameAnimation = "";

		private GameObject leftController;

		private GameObject rightController;

		private GameObject cameraEye;

		private GameObject maleAim;

		private GameObject femaleAim;

		private FieldInfo f_device;

		private VRViveController leftVVC;

		private VRViveController rightVVC;

		private SteamVR_Controller.Device leftDevice;

		private SteamVR_Controller.Device rightDevice;

		private bool setParentMale;

		private int setParentMode;

		private Canvas canvasSetParent;

		private GameObject objCanvasSetParent;

		private CanvasScaler canvasScalerSetParent;

		private GameObject eventSystemSetParent;

		private Canvas canvasMotion;

		private GameObject objCanvasMotion;

		private CanvasScaler canvasScalerMotion;

		private GameObject eventSystemMotion;

		private float hideCount;

		private HSprite hSprite;

		private GameObject obj_cf_t_hand_R;

		private GameObject obj_cf_pv_hand_R;

		private BaseData bd_cf_t_hand_R;

		public GameObject objRightHand;

		private GameObject obj_cf_t_hand_L;

		private GameObject obj_cf_pv_hand_L;

		private BaseData bd_cf_t_hand_L;

		public GameObject objLeftHand;

		private GameObject obj_cf_t_leg_R;

		private GameObject obj_cf_pv_leg_R;

		private BaseData bd_cf_t_leg_R;

		public GameObject objRightLeg;

		private GameObject obj_cf_t_leg_L;

		private GameObject obj_cf_pv_leg_L;

		private BaseData bd_cf_t_leg_L;

		public GameObject objLeftLeg;

		private GameObject objParent;

		private GameObject obj_chaF_001;

		private GameObject obj_BodyTop;

		private GameObject obj_p_cf_body_bone;

		private GameObject obj_cf_j_root;

		private GameObject obj_cf_n_height;

		private GameObject obj_cf_j_hips;

		private GameObject obj_cf_j_spine01;

		private GameObject obj_cf_j_spine02;

		private GameObject obj_cf_j_spine03;

		private GameObject obj_cf_j_neck;

		private GameObject objBase;

		private GameObject objSpinePos;

		private Text txtSetParentL;

		private Text txtSetParentR;

		private Text txtFixBody;

		private bool bFixBody;

		public bool setCollider = true;

		private int parentPart;

		private Vector3[] vecSpinePos = new Vector3[20];

		private int indexSpinePos;

		private Quaternion[] quatSpineRot = new Quaternion[20];

		private int indexSpineRot;

		private bool trackingMode = true;

		private bool hideCanvas;

		private bool parentIsLeft;
	}
}
