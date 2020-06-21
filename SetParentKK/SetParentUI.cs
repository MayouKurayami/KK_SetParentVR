using System;
using System.Collections;
using VRTK;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static SetParentKK.KK_SetParentVR;

namespace SetParentKK
{
	public partial class SetParent
	{
		private Text txtSetParentL;
		private Text txtSetParentR;
		private Text txtSetParentMode;
		private Text txtLimbAuto;
		internal Text txtMaleLeftFoot;
		internal Text txtMaleRightFoot;

		private Canvas canvasRight;
		private GameObject objRightMenuCanvas;
		private CanvasScaler canvasRightScaler;
		private GameObject eventSystemSetParent;
		private Canvas canvasLeft;
		private GameObject objLeftMenuCanvas;
		private CanvasScaler canvasLeftScaler;

		private GameObject eventSystemMotion;


		private void InitCanvas()
		{
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

			//Initialize right menu
			objRightMenuCanvas = new GameObject("CanvasSetParent", new Type[]
			{
				typeof(Canvas)
			});
			canvasRight = objRightMenuCanvas.GetComponent<Canvas>();
			objRightMenuCanvas.AddComponent<GraphicRaycaster>();
			objRightMenuCanvas.AddComponent<VRTK_UICanvas>();
			objRightMenuCanvas.AddComponent<VRTK_UIGraphicRaycaster>();
			canvasRightScaler = objRightMenuCanvas.AddComponent<CanvasScaler>();
			canvasRightScaler.dynamicPixelsPerUnit = 20000f;
			canvasRightScaler.referencePixelsPerUnit = 80000f;
			canvasRight.renderMode = RenderMode.WorldSpace;
			objRightMenuCanvas.transform.localScale = new Vector3(0.005f, 0.005f, 0.005f);
			eventSystemSetParent = new GameObject("CanvasSetParentEventSystem", new Type[]
			{
				typeof(EventSystem)
			});
			eventSystemSetParent.AddComponent<StandaloneInputModule>();
			eventSystemSetParent.transform.SetParent(objRightMenuCanvas.transform);

			////////////////
			//Populate right side floating menu with buttons
			////////////////
			txtMaleLeftFoot = CreateButton("男の左足固定", new Vector3(-26f, -26f, 0f), () => PushLimbButton(LimbName.MaleLeftFoot, txtMaleLeftFoot, "男の左足解除", "男の左足固定"), objRightMenuCanvas);
			txtMaleRightFoot = CreateButton("男の右足固定", new Vector3(26f, -26f, 0f), () => PushLimbButton(LimbName.MaleRightFoot, txtMaleRightFoot, "男の右足解除", "男の右足固定"), objRightMenuCanvas);
			CreateButton("女左足固定/解除", new Vector3(-26f, -13f, 0f), () => FixLimbToggle(limbs[(int)LimbName.FemaleLeftFoot], true), objRightMenuCanvas);
			CreateButton("女右足固定/解除", new Vector3(26f, -13f, 0f), () => FixLimbToggle(limbs[(int)LimbName.FemaleRightFoot], true), objRightMenuCanvas);
			CreateButton("女左手固定/解除", new Vector3(-26f, 0f, 0f), () => FixLimbToggle(limbs[(int)LimbName.FemaleLeftHand], true), objRightMenuCanvas);
			CreateButton("女右手固定/解除", new Vector3(26f, 0f, 0f), () => FixLimbToggle(limbs[(int)LimbName.FemaleRightHand], true), objRightMenuCanvas);
			CreateButton("男の左手親子付け ON/OFF", new Vector3(-26f, 13f, 0f), () => SyncMaleHandsToggle(!limbs[(int)LimbName.MaleLeftHand].AnchorObj, Side.Left), objRightMenuCanvas);
			CreateButton("男の右手親子付け ON/OFF", new Vector3(26f, 13f, 0f), () => SyncMaleHandsToggle(!limbs[(int)LimbName.MaleRightHand].AnchorObj, Side.Right), objRightMenuCanvas);
			txtLimbAuto = CreateButton("女手足固定 Turn Off", new Vector3(-26f, 26f, 0f), () => PushLimbAutoAttachButton(), objRightMenuCanvas);
			txtSetParentMode = CreateButton(SetParentMode.Value.ToString(), new Vector3(26f, 26f, 0f), () => PushParentModeChangeButton(), objRightMenuCanvas);
			txtSetParentL = CreateButton("左 親子付け Turn On", new Vector3(-26f, 39f, 0f), () => PushSetParentButton(isParentLeft: true), objRightMenuCanvas);
			txtSetParentR = CreateButton("右 親子付け Turn On", new Vector3(26f, 39f, 0f), () => PushSetParentButton(isParentLeft: false), objRightMenuCanvas);

			CreateButton("ヌク", new Vector3(-26f, 52f, 0f), () => hSprite.OnPullClick(), objRightMenuCanvas);

			CreateButton("モーション 強弱", new Vector3(-26f, 65f, 0f), () => PushMotionChangeButton(), objRightMenuCanvas);
			CreateButton("モーション 開始/停止", new Vector3(26f, 65f, 0f), () => PushModeChangeButton(), objRightMenuCanvas);

			CreateButton("中に出すよ", new Vector3(-26f, 78f, 0f), () => PushFIButton(), objRightMenuCanvas);
			CreateButton("外に出すよ", new Vector3(26f, 78f, 0f), () => PushFOButton(), objRightMenuCanvas);
			CreateButton("入れるよ", new Vector3(-26f, 91f, 0f), () => hSprite.OnInsertClick(), objRightMenuCanvas);
			CreateButton("イレル", new Vector3(26f, 91f, 0f), () => hSprite.OnInsertNoVoiceClick(), objRightMenuCanvas);
			CreateButton("アナル入れるよ", new Vector3(-26f, 104f, 0f), () => hSprite.OnInsertAnalClick(), objRightMenuCanvas);
			CreateButton("アナルイレル", new Vector3(26f, 104f, 0f), () => hSprite.OnInsertAnalNoVoiceClick(), objRightMenuCanvas);

			Vector3 point = femaleAim.transform.position - cameraEye.transform.position;
			point.y = 0f;
			point.Normalize();
			canvasRight.transform.position = new Vector3(femaleAim.transform.position.x, cameraEye.transform.position.y - 0.4f, femaleAim.transform.position.z) + Quaternion.Euler(0f, 90f, 0f) * point * 1.5f;
			canvasRight.transform.forward = (canvasRight.transform.position - cameraEye.transform.position).normalized;


			//Initialize left menu
			objLeftMenuCanvas = new GameObject("CanvasMotion", new Type[]
			{
				typeof(Canvas)
			});
			canvasLeft = objLeftMenuCanvas.GetComponent<Canvas>();
			objLeftMenuCanvas.AddComponent<GraphicRaycaster>();
			objLeftMenuCanvas.AddComponent<VRTK_UICanvas>();
			objLeftMenuCanvas.AddComponent<VRTK_UIGraphicRaycaster>();
			canvasLeftScaler = objLeftMenuCanvas.AddComponent<CanvasScaler>();
			canvasLeftScaler.dynamicPixelsPerUnit = 20000f;
			canvasLeftScaler.referencePixelsPerUnit = 80000f;
			canvasLeft.renderMode = RenderMode.WorldSpace;
			objLeftMenuCanvas.transform.localScale = new Vector3(0.005f, 0.005f, 0.005f);
			eventSystemMotion = new GameObject("CanvasEventSystemMotion", new Type[]
			{
				typeof(EventSystem)
			});
			eventSystemMotion.AddComponent<StandaloneInputModule>();
			eventSystemMotion.transform.SetParent(objLeftMenuCanvas.transform);

			////////////////
			//Populate left side floating menu with buttons
			////////////////
			CreateButton("正常位", new Vector3(-26f, -39f, 0f), () => StartCoroutine(ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_00")), objLeftMenuCanvas);
			CreateButton("開脚正常位", new Vector3(26f, -39f, 0f), () => StartCoroutine(ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_n00")), objLeftMenuCanvas);
			CreateButton("脚持つ正常位", new Vector3(-26f, -26f, 0f), () => StartCoroutine(ChangeMotion("h/anim/female/02_12_00.unity3d", "khs_f_n24")), objLeftMenuCanvas);
			CreateButton("脚持つ(強弱あり)", new Vector3(26f, -26f, 0f), () => StartCoroutine(ChangeMotion("h/anim/female/02_06_00.unity3d", "khs_f_n23")), objLeftMenuCanvas);

			CreateButton("側位(片足上げ)", new Vector3(-26f, -13f, 0f), () => StartCoroutine(ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_n06")), objLeftMenuCanvas);
			CreateButton("机側位", new Vector3(26f, -13f, 0f), () => StartCoroutine(ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_n16")), objLeftMenuCanvas);

			CreateButton("駅弁", new Vector3(-26f, 0f, 0f), () => StartCoroutine(ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_n22")), objLeftMenuCanvas);
			CreateButton("駅弁(強弱あり)", new Vector3(26f, 0f, 0f), () => StartCoroutine(ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_n08")), objLeftMenuCanvas);

			CreateButton("立位", new Vector3(-26f, 13f, 0f), () => StartCoroutine(ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_n07")), objLeftMenuCanvas);
			CreateButton("プール", new Vector3(26f, 13f, 0f), () => StartCoroutine(ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_n20")), objLeftMenuCanvas);

			CreateButton("跪く後背位", new Vector3(-26f, 26f, 0f), () => StartCoroutine(ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_02")), objLeftMenuCanvas);
			CreateButton("腕引っ張り後背位", new Vector3(26f, 26f, 0f), () => StartCoroutine(ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_n02")), objLeftMenuCanvas);
			CreateButton("椅子に後背位", new Vector3(-26f, 39f, 0f), () => StartCoroutine(ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_11")), objLeftMenuCanvas);
			CreateButton("椅子腕引っ張り後背位", new Vector3(26f, 39f, 0f), () => StartCoroutine(ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_n11")), objLeftMenuCanvas);
			CreateButton("壁に後背位", new Vector3(-26f, 52f, 0f), () => StartCoroutine(ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_18")), objLeftMenuCanvas);
			CreateButton("壁に片足上げ後背位", new Vector3(26f, 52f, 0f), () => StartCoroutine(ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_n18")), objLeftMenuCanvas);

			CreateButton("フェンス後背位", new Vector3(-26f, 65f, 0f), () => StartCoroutine(ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_n21")), objLeftMenuCanvas);
			CreateButton("壁に押し付け後背位", new Vector3(26f, 65f, 0f), () => StartCoroutine(ChangeMotion("h/anim/female/02_20_00.unity3d", "khs_f_n28")), objLeftMenuCanvas);

			CreateButton("寝後背位", new Vector3(-26f, 78f, 0f), () => StartCoroutine(ChangeMotion("h/anim/female/02_13_00.unity3d", "khs_f_n26")), objLeftMenuCanvas);
			CreateButton("跳び箱後背位", new Vector3(26f, 78f, 0f), () => StartCoroutine(ChangeMotion("h/anim/female/02_12_00.unity3d", "khs_f_n25")), objLeftMenuCanvas);

			CreateButton("騎乗位", new Vector3(-26f, 91f, 0f), () => StartCoroutine(ChangeMotion("h/anim/female/02_13_00.unity3d", "khs_f_n27")), objLeftMenuCanvas);
			CreateButton("騎乗位(強弱あり)", new Vector3(26f, 91f, 0f), () => StartCoroutine(ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_n04")), objLeftMenuCanvas);

			CreateButton("座位対面", new Vector3(-26f, 104f, 0f), () => StartCoroutine(ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_n09")), objLeftMenuCanvas);
			CreateButton("座位背面", new Vector3(26f, 104f, 0f), () => StartCoroutine(ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_n10")), objLeftMenuCanvas);


			point = femaleAim.transform.position - cameraEye.transform.position;
			point.y = 0f;
			point.Normalize();
			canvasLeft.transform.position = new Vector3(femaleAim.transform.position.x, cameraEye.transform.position.y - 0.4f, femaleAim.transform.position.z) + Quaternion.Euler(0f, -90f, 0f) * point * 1.5f;
			canvasLeft.transform.forward = (canvasLeft.transform.position - cameraEye.transform.position).normalized;
		}

		private Text CreateButton(string buttonText, Vector3 localPosition, UnityAction action, GameObject parentObject)
		{
			GameObject buttonObject = new GameObject("button");
			GameObject textObject = new GameObject("text", new Type[]
			{
				typeof(Text)
			});
			Text text = textObject.GetComponent<Text>();
			Image image = buttonObject.AddComponent<Image>();
			image.rectTransform.sizeDelta = new Vector2(0.25f, 0.06f);
			image.color = new Color(0.8f, 0.8f, 0.8f);
			Button button = buttonObject.AddComponent<Button>();
			ColorBlock colors = button.colors;
			colors.highlightedColor = Color.red;
			colors.pressedColor = Color.cyan;
			button.colors = colors;
			text.rectTransform.sizeDelta = new Vector2(0.25f, 0.06f);
			text.font = Font.CreateDynamicFontFromOSFont("Arial", 13);
			text.fontSize = 13;
			text.alignment = TextAnchor.MiddleCenter;
			text.text = buttonText;
			buttonObject.transform.SetParent(parentObject.transform);
			buttonObject.transform.localPosition = localPosition;
			textObject.transform.SetParent(buttonObject.transform);
			textObject.transform.localPosition = Vector3.zero;
			buttonObject.GetComponent<Button>().onClick.AddListener(action);

			return text;
		}

		/// <summary>
		/// Change female animation/position
		/// </summary>
		/// <param name="path"></param>
		/// <param name="name"></param>
		private IEnumerator ChangeMotion(string path, string name)
		{
			PushLimbAutoAttachButton(true);

			if (femaleSpinePos == null)
			{
				femaleSpinePos = new GameObject("femaleSpinePos");
			}
			femaleSpinePos.transform.position = femaleBase.transform.position;
			femaleSpinePos.transform.rotation = femaleBase.transform.rotation;

			CtrlState oldState = currentCtrlstate;
			currentCtrlstate = CtrlState.Following;

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

			for (int k = 0; k < 2; k++)
			{
				yield return null;
			}
			currentCtrlstate = oldState;
			yield break;
		}

		private void PushSetParentButton(bool isParentLeft)
		{
			if (!setFlag)
			{
				SetP(isParentLeft);
			}
			else
			{
				UnsetP();
			}
		}

		/// <summary>
		/// Start/stop piston movement
		/// </summary>
		private void PushModeChangeButton()
		{
			hFlag.click = HFlag.ClickKind.modeChange;
		}

		/// <summary>
		/// Toggle between strong/weak motion
		/// </summary>
		private void PushMotionChangeButton()
		{
			hFlag.click = HFlag.ClickKind.motionchange;
			AnimSpeedController component = obj_chaF_001.GetComponent<AnimSpeedController>();
			if (component != null)
			{
				component.weakMotion = !component.weakMotion;
			}
		}

		private void PushLimbAutoAttachButton(bool forceON = false)
		{
			limbAutoAttach = forceON ? true : !limbAutoAttach;
			txtLimbAuto.text = limbAutoAttach ? "女手足固定 Turn Off" : "女手足固定 Turn On";
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

		/// <summary>
		/// Iterate between the three different Parenting Modes: Animation Only, Position Only, and Both
		/// </summary>
		private void PushParentModeChangeButton()
		{
			int index = (int)SetParentMode.Value + 1;
			SetParentMode.Value = (ParentMode)(index % Enum.GetNames(typeof(ParentMode)).Length);
			txtSetParentMode.text = SetParentMode.Value.ToString();

			//Update parent controller hand visibility and collider if not in AnimationOnly mode, 
			//as they should be hidden and disabled when the parent controller is being used to control female position.
			//The actual parenting is taken care of by ControllerCharacterAdjustment() called from the trigger press
			if (setFlag)
			{
				bool parentHandShow = SetParentMode.Value == ParentMode.AnimationOnly && !HideParentConAlways.Value;

				parentController.transform.Find("Model").gameObject.SetActive(parentHandShow);

				if (SetControllerCollider.Value && !limbs[(int)ParentSideMaleHand()].AnchorObj)
					parentController.transform.Find("ControllerCollider").GetComponent<SphereCollider>().enabled = parentHandShow;
			}
		}

		/// <summary>
		/// Forcibly fix the limb in place. Release the limb if it's currently fixed.
		/// </summary>
		/// <param name="limb">The limb to toggle</param>
		/// <param name="buttonText">Text object of the button to change</param>
		/// <param name="turnOffText">Button text to display when limb is currently fixed</param>
		/// <param name="turnOnText">Button text to display when limb is currently not fixed</param>
		private void PushLimbButton(LimbName limb, Text buttonText = null, string turnOffText = null, string turnOnText = null)
		{		
			if (!limbs[(int)limb].AnchorObj)
			{
				FixLimbToggle(limbs[(int)limb], true);
				if (buttonText)
					buttonText.text = turnOffText;
			}
			else if (!limbs[(int)limb].Fixed)
			{
				limbs[(int)limb].Fixed = true;
				if (buttonText)
					buttonText.text = turnOffText;
			}
			else
			{
				FixLimbToggle(limbs[(int)limb]);
				if (buttonText)
					buttonText.text = turnOnText;
			}
		}
	}
}
