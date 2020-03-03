using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
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

			male_cf_t_leg_L = maleFBBIK.solver.leftFootEffector.target.gameObject;
			male_cf_t_leg_R = maleFBBIK.solver.rightFootEffector.target.gameObject;

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
			male_cf_pv_leg_R = male_cf_n_height.Find("cf_pv_root/cf_pv_leg_R").gameObject;
			male_cf_pv_leg_L = male_cf_n_height.Find("cf_pv_root/cf_pv_leg_L").gameObject;

			if (SetFemaleCollider.Value)
			{
				SetFemaleColliders();
			}
			if (SetMaleCollider.Value)
			{
				SetMaleFeetColliders();
			}
			if (SetFemaleCollider.Value || SetMaleCollider.Value)
			{
				SetMapObjectsColliders();
			}
		}

		private void InitCanvas()
		{
			objRightMenuCanvas = new GameObject("CanvasSetParent", new Type[]
			{
				typeof(Canvas)
			});
			canvasSetParent = objRightMenuCanvas.GetComponent<Canvas>();
			objRightMenuCanvas.AddComponent<GraphicRaycaster>();
			objRightMenuCanvas.AddComponent<VRTK_UICanvas>();
			objRightMenuCanvas.AddComponent<VRTK_UIGraphicRaycaster>();
			canvasScalerSetParent = objRightMenuCanvas.AddComponent<CanvasScaler>();
			canvasScalerSetParent.dynamicPixelsPerUnit = 20000f;
			canvasScalerSetParent.referencePixelsPerUnit = 80000f;
			canvasSetParent.renderMode = RenderMode.WorldSpace;
			objRightMenuCanvas.transform.localScale = new Vector3(0.005f, 0.005f, 0.005f);
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
			eventSystemSetParent.transform.SetParent(objRightMenuCanvas.transform);

			////////////////
			//Populate right side floating menu with buttons
			////////////////
			CreateButton("右足固定", new Vector3(28f, -48f, 0f), () => PushFixRightLegButton(true), objRightMenuCanvas);
			CreateButton("左足固定", new Vector3(-28f, -48f, 0f), () => PushFixLeftLegButton(true), objRightMenuCanvas);
			CreateButton("右手固定", new Vector3(28f, -28f, 0f), () => PushFixRightHandButton(true), objRightMenuCanvas);
			CreateButton("左手固定", new Vector3(-28f, -28f, 0f), () => PushFixLeftHandButton(true), objRightMenuCanvas);
			CreateButton("男の右足固定/解除", new Vector3(28f, -8f, 0f), () => MaleFixRightLegToggle(), objRightMenuCanvas);
			CreateButton("男の左足固定/解除", new Vector3(-28f, -8f, 0f), () => MaleFixLeftLegToggle(), objRightMenuCanvas);
			txtSetParentL = CreateButton("左 親子付け On", new Vector3(-28f, 16f, 0f), () => PushPLButton(), objRightMenuCanvas);
			txtSetParentR = CreateButton("右 親子付け On", new Vector3(28f, 16f, 0f), () => PushPRButton(), objRightMenuCanvas);
			CreateButton("モーション 強弱", new Vector3(-28f, 40f, 0f), () => PushMotionChangeButton(), objRightMenuCanvas);
			CreateButton("モーション 開始/停止", new Vector3(28f, 40f, 0f), () => PushModeChangeButton(), objRightMenuCanvas);
			CreateButton("中に出すよ", new Vector3(-28f, 60f, 0f), () => PushFIButton(), objRightMenuCanvas);
			CreateButton("外に出すよ", new Vector3(28f, 60f, 0f), () => PushFOButton(), objRightMenuCanvas);
			CreateButton("入れるよ", new Vector3(-28f, 80f, 0f), () => hSprite.OnInsertClick(), objRightMenuCanvas);
			CreateButton("イレル", new Vector3(28f, 80f, 0f), () => hSprite.OnInsertNoVoiceClick(), objRightMenuCanvas);
			CreateButton("アナル入れるよ", new Vector3(-28f, 100f, 0f), () => hSprite.OnInsertAnalClick(), objRightMenuCanvas);
			CreateButton("アナルイレル", new Vector3(28f, 100f, 0f), () => hSprite.OnInsertAnalNoVoiceClick(), objRightMenuCanvas);
			CreateButton("ヌク", new Vector3(-28f, 120f, 0f), () => hSprite.OnPullClick(), objRightMenuCanvas);


			Vector3 point = femaleAim.transform.position - cameraEye.transform.position;
			point.y = 0f;
			point.Normalize();
			canvasSetParent.transform.position = new Vector3(femaleAim.transform.position.x, cameraEye.transform.position.y - 0.4f, femaleAim.transform.position.z) + Quaternion.Euler(0f, 90f, 0f) * point * 1.5f;
			canvasSetParent.transform.forward = (canvasSetParent.transform.position - cameraEye.transform.position).normalized;
			objLeftMenuCanvas = new GameObject("CanvasMotion", new Type[]
			{
				typeof(Canvas)
			});
			canvasMotion = objLeftMenuCanvas.GetComponent<Canvas>();
			objLeftMenuCanvas.AddComponent<GraphicRaycaster>();
			objLeftMenuCanvas.AddComponent<VRTK_UICanvas>();
			objLeftMenuCanvas.AddComponent<VRTK_UIGraphicRaycaster>();
			canvasScalerMotion = objLeftMenuCanvas.AddComponent<CanvasScaler>();
			canvasScalerMotion.dynamicPixelsPerUnit = 20000f;
			canvasScalerMotion.referencePixelsPerUnit = 80000f;
			canvasMotion.renderMode = RenderMode.WorldSpace;
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
			CreateButton("正常位", new Vector3(-28f, -64f, 0f), () => ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_00"), objLeftMenuCanvas);
			CreateButton("開脚正常位", new Vector3(28f, -64f, 0f), () => ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_n00"), objLeftMenuCanvas);
			CreateButton("脚持つ正常位", new Vector3(-28f, -48f, 0f), () => ChangeMotion("h/anim/female/02_12_00.unity3d", "khs_f_n24"), objLeftMenuCanvas);
			CreateButton("脚持つ(強弱差分)", new Vector3(28f, -48f, 0f), () => ChangeMotion("h/anim/female/02_06_00.unity3d", "khs_f_n23"), objLeftMenuCanvas);

			CreateButton("側位(片足上げ)", new Vector3(-28f, -32f, 0f), () => ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_n06"), objLeftMenuCanvas);
			CreateButton("机側位", new Vector3(28f, -32f, 0f), () => ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_n16"), objLeftMenuCanvas);

			CreateButton("駅弁", new Vector3(-28f, -16f, 0f), () => ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_n22"), objLeftMenuCanvas);
			CreateButton("駅弁(強弱差分)", new Vector3(28f, -16f, 0f), () => ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_n08"), objLeftMenuCanvas);

			CreateButton("立位", new Vector3(-28f, 0f, 0f), () => ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_n07"), objLeftMenuCanvas);
			CreateButton("プール", new Vector3(28f, 0f, 0f), () => ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_n20"), objLeftMenuCanvas);
			
			CreateButton("跪くバック", new Vector3(-28f, 16f, 0f), () => ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_02"), objLeftMenuCanvas);
			CreateButton("腕引っ張りバック", new Vector3(28f, 16f, 0f), () => ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_n02"), objLeftMenuCanvas);
			CreateButton("椅子にバック", new Vector3(-28f, 32f, 0f), () => ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_11"), objLeftMenuCanvas);
			CreateButton("椅子腕引っ張りバック", new Vector3(28f, 32f, 0f), () => ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_n11"), objLeftMenuCanvas);
			CreateButton("壁にバック", new Vector3(-28f, 48f, 0f), () => ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_18"), objLeftMenuCanvas);
			CreateButton("壁バック片足上げ", new Vector3(28f, 48f, 0f), () => ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_n18"), objLeftMenuCanvas);

			CreateButton("フェンス後背位", new Vector3(-28f, 64f, 0f), () => ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_n21"), objLeftMenuCanvas);
			CreateButton("押し付け壁バック", new Vector3(28f, 64f, 0f), () => ChangeMotion("h/anim/female/02_20_00.unity3d", "khs_f_n28"), objLeftMenuCanvas);

			CreateButton("寝バック", new Vector3(-28f, 80f, 0f), () => ChangeMotion("h/anim/female/02_13_00.unity3d", "khs_f_n26"), objLeftMenuCanvas);
			CreateButton("跳び箱バック", new Vector3(28f, 80f, 0f), () => ChangeMotion("h/anim/female/02_12_00.unity3d", "khs_f_n25"), objLeftMenuCanvas);

			CreateButton("騎乗位", new Vector3(-28f, 96f, 0f), () => ChangeMotion("h/anim/female/02_13_00.unity3d", "khs_f_n27"), objLeftMenuCanvas);
			CreateButton("騎乗位(強弱差分)", new Vector3(28f, 96f, 0f), () => ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_n04"), objLeftMenuCanvas);	

			CreateButton("座位対面", new Vector3(-28f, 112f, 0f), () => ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_n09"), objLeftMenuCanvas);
			CreateButton("座位背面", new Vector3(28f, 112f, 0f), () => ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_n10"), objLeftMenuCanvas);
			

			point = femaleAim.transform.position - cameraEye.transform.position;
			point.y = 0f;
			point.Normalize();
			canvasMotion.transform.position = new Vector3(femaleAim.transform.position.x, cameraEye.transform.position.y - 0.4f, femaleAim.transform.position.z) + Quaternion.Euler(0f, -90f, 0f) * point * 1.5f;
			canvasMotion.transform.forward = (canvasMotion.transform.position - cameraEye.transform.position).normalized;
		}

		private void SetFemaleColliders()
		{
			GameObject rightHandCollider = new GameObject("RightHandCollider");
			rightHandCollider.AddComponent<FixBodyParts>().Init(this, FixBodyParts.BodyParts.hand_R);
			rightHandCollider.transform.parent = femaleFBBIK.solver.rightHandEffector.bone;
			rightHandCollider.transform.localPosition = Vector3.zero;

			GameObject leftHandCollider = new GameObject("LeftHandCollider");
			leftHandCollider.AddComponent<FixBodyParts>().Init(this, FixBodyParts.BodyParts.hand_L);
			leftHandCollider.transform.parent = femaleFBBIK.solver.leftHandEffector.bone;
			leftHandCollider.transform.localPosition = Vector3.zero;

			GameObject rightLegCollider = new GameObject("RightLegCollider");
			rightLegCollider.AddComponent<FixBodyParts>().Init(this, FixBodyParts.BodyParts.leg_R);
			rightLegCollider.transform.parent = femaleFBBIK.solver.rightFootEffector.bone;
			rightLegCollider.transform.localPosition = Vector3.zero;

			GameObject leftLegCollider = new GameObject("LeftLegCollider");
			leftLegCollider.AddComponent<FixBodyParts>().Init(this, FixBodyParts.BodyParts.leg_L);
			leftLegCollider.transform.parent = femaleFBBIK.solver.leftFootEffector.bone;
			leftLegCollider.transform.localPosition = Vector3.zero;


			shoulderCollider = new GameObject("SPCollider");
			shoulderCollider.transform.parent = cameraEye.transform;
			shoulderCollider.transform.localPosition = new Vector3(0f, -0.25f, -0.15f);
			shoulderCollider.transform.localRotation = Quaternion.identity;
			BoxCollider boxCollider2 = shoulderCollider.AddComponent<BoxCollider>();
			boxCollider2.isTrigger = true;
			boxCollider2.center = Vector3.zero;
			boxCollider2.size = new Vector3(0.4f, 0.2f, 0.25f);
			shoulderCollider.AddComponent<Rigidbody>().isKinematic = true;
		}

		private void SetMaleFeetColliders()
		{
			GameObject rightMaleFootCollider = new GameObject("RightFootCollider");
			rightMaleFootCollider.AddComponent<FixBodyParts>().Init(this, FixBodyParts.BodyParts.male_ft_R);
			rightMaleFootCollider.transform.parent = maleFBBIK.solver.rightFootEffector.bone;
			rightMaleFootCollider.transform.localPosition = Vector3.zero;

			GameObject leftMaleFootCollider = new GameObject("LeftFootCollider");
			leftMaleFootCollider.AddComponent<FixBodyParts>().Init(this, FixBodyParts.BodyParts.male_ft_L);
			leftMaleFootCollider.transform.parent = maleFBBIK.solver.leftFootEffector.bone;
			leftMaleFootCollider.transform.localPosition = Vector3.zero;
		}

		private void SetMapObjectsColliders()
		{
			foreach (Transform transform in GameObject.Find("Map").GetComponentsInChildren<Transform>())
			{
				MeshFilter meshFilter = transform.GetComponent<MeshFilter>();
				if (!(meshFilter == null))
				{
					GameObject mapObjCollider = new GameObject("SPCollider");
					mapObjCollider.transform.parent = transform.transform;
					mapObjCollider.transform.localPosition = Vector3.zero;
					mapObjCollider.transform.localRotation = Quaternion.identity;
					mapObjCollider.AddComponent<Rigidbody>().isKinematic = true;
					if (meshFilter.mesh.bounds.size.x < 0.03f || meshFilter.mesh.bounds.size.y < 0.03f || meshFilter.mesh.bounds.size.z < 0.03f)
					{
						BoxCollider boxCollider = mapObjCollider.AddComponent<BoxCollider>();
						boxCollider.isTrigger = true;
						boxCollider.center = meshFilter.mesh.bounds.center;
						boxCollider.size = meshFilter.mesh.bounds.size;
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
						MeshCollider meshCollider = mapObjCollider.AddComponent<MeshCollider>();
						meshCollider.isTrigger = true;
						meshCollider.convex = false;
						meshCollider.sharedMesh = meshFilter.mesh;
					}
				}
			}
		}

		public void PushFixRightHandButton(bool force = false)
		{
			if (objRightHand == null)
			{
				objRightHand = new GameObject("objRightHand");
				objRightHand.transform.position = female_cf_pv_hand_R.transform.position;
				objRightHand.transform.rotation = female_cf_pv_hand_R.transform.rotation;
				femaleFBBIK.solver.rightHandEffector.target = objRightHand.transform;
				fixRightHand = force;
				return;
			}
			UnityEngine.Object.DestroyImmediate(objRightHand);
			objRightHand = null;
			femaleFBBIK.solver.rightHandEffector.target = female_cf_t_hand_R.transform;
		}

		public void PushFixLeftHandButton(bool force = false)
		{
			if (objLeftHand == null)
			{
				objLeftHand = new GameObject("objLeftHand");
				objLeftHand.transform.position = female_cf_pv_hand_L.transform.position;
				objLeftHand.transform.rotation = female_cf_pv_hand_L.transform.rotation;
				femaleFBBIK.solver.leftHandEffector.target = objLeftHand.transform;
				fixLefttHand = force;
				return;
			}
			UnityEngine.Object.DestroyImmediate(objLeftHand);
			objLeftHand = null;
			femaleFBBIK.solver.leftHandEffector.target = female_cf_t_hand_L.transform;
		}

		public void PushFixRightLegButton(bool force = false)
		{
			if (objRightLeg == null)
			{
				objRightLeg = new GameObject("objRightLeg");
				objRightLeg.transform.position = female_cf_pv_leg_R.transform.position;
				objRightLeg.transform.rotation = female_cf_pv_leg_R.transform.rotation;
				femaleFBBIK.solver.rightFootEffector.target = objRightLeg.transform;
				fixRightLeg = force;
				return;
			}
			UnityEngine.Object.DestroyImmediate(objRightLeg);
			objRightLeg = null;
			femaleFBBIK.solver.rightFootEffector.target = female_cf_t_leg_R.transform;
		}

		public void PushFixLeftLegButton(bool force = false)
		{
			if (objLeftLeg == null)
			{
				objLeftLeg = new GameObject("objLeftLeg");
				objLeftLeg.transform.position = female_cf_pv_leg_L.transform.position;
				objLeftLeg.transform.rotation = female_cf_pv_leg_L.transform.rotation;
				femaleFBBIK.solver.leftFootEffector.target = objLeftLeg.transform;
				fixLeftLeg = force;
				return;
			}
			UnityEngine.Object.DestroyImmediate(objLeftLeg);
			objLeftLeg = null;
			femaleFBBIK.solver.leftFootEffector.target = female_cf_t_leg_L.transform;
		}

		public void MaleFixLeftLegToggle()
		{
			if (objLeftMaleFoot == null)
			{
				objLeftMaleFoot = new GameObject("objLeftMaleFoot");
				objLeftMaleFoot.transform.position = male_cf_pv_leg_L.transform.position;
				objLeftMaleFoot.transform.rotation = male_cf_pv_leg_L.transform.rotation;
				maleFBBIK.solver.leftFootEffector.target = objLeftMaleFoot.transform;
				return;
			}
			UnityEngine.Object.DestroyImmediate(objLeftMaleFoot);
			objLeftMaleFoot = null;
			maleFBBIK.solver.leftFootEffector.target = male_cf_t_leg_L.transform;
		}

		public void MaleFixRightLegToggle()
		{
			if (objRightMaleFoot == null)
			{
				objRightMaleFoot = new GameObject("objRightMaleFoot");
				objRightMaleFoot.transform.position = male_cf_pv_leg_R.transform.position;
				objRightMaleFoot.transform.rotation = male_cf_pv_leg_R.transform.rotation;
				maleFBBIK.solver.rightFootEffector.target = objRightMaleFoot.transform;
				return;
			}
			UnityEngine.Object.DestroyImmediate(objRightMaleFoot);
			objRightMaleFoot = null;
			maleFBBIK.solver.rightFootEffector.target = male_cf_t_leg_R.transform;
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


			if (objRightMenuCanvas == null)
				InitCanvas();
			else
			{
				if (RightMenuPressing() || LeftMenuPressing())
				{
					hideCount += Time.deltaTime;
					if (hideCount >= 1f)
					{
						hideCanvas = !hideCanvas;
						hideCount = 0f;
					}
				}
				else
				{
					hideCount = 0f;
				}
				Vector3 point = femaleAim.transform.position - cameraEye.transform.position;
				point.y = 0f;
				point.Normalize();
				objRightMenuCanvas.transform.position = new Vector3(femaleAim.transform.position.x, cameraEye.transform.position.y, femaleAim.transform.position.z) + Quaternion.Euler(0f, 90f, 0f) * point * 0.4f;
				objRightMenuCanvas.transform.forward = (objRightMenuCanvas.transform.position - cameraEye.transform.position).normalized;
				objLeftMenuCanvas.transform.position = new Vector3(femaleAim.transform.position.x, cameraEye.transform.position.y, femaleAim.transform.position.z) + Quaternion.Euler(0f, -90f, 0f) * point * 0.4f;
				objLeftMenuCanvas.transform.forward = (objLeftMenuCanvas.transform.position - cameraEye.transform.position).normalized;

			
				if (setFlag)
				{
					Vector3 vector;
					if (parentIsLeft)
						vector = cameraEye.transform.position - rightController.transform.position;
					else
						vector = cameraEye.transform.position - leftController.transform.position;

					if (vector.magnitude <= 0.3f)
					{
						objRightMenuCanvas.SetActive(true);
						objLeftMenuCanvas.SetActive(true);
					}
					else
					{
						objRightMenuCanvas.SetActive(!hideCanvas);
						objLeftMenuCanvas.SetActive(!hideCanvas);
					}
				}
				else 
				{
					objRightMenuCanvas.SetActive(!hideCanvas);
					objLeftMenuCanvas.SetActive(!hideCanvas);
				}
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
			}
			
			if (setFlag)
			{
				if (nowAnimState != hFlag.nowAnimStateName)
				{
					if (SetParentMale.Value)
						InitMaleFollow();
					nowAnimState = hFlag.nowAnimStateName;
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
					if (MaleYaw.Value)
					{
						maleSpineVec = maleHeadPos.transform.position - maleCrotchPos.transform.position;
						Vector3 malePenisProjected = Vector3.ProjectOnPlane(maleCrotchPos.transform.forward, maleSpineVec);
						Vector3 cameraForwardProjected = Quaternion.AngleAxis(-90, maleSpineVec) * Vector3.ProjectOnPlane(cameraEye.transform.right, maleSpineVec);
						Quaternion.FromToRotation(malePenisProjected, cameraForwardProjected).ToAngleAxis(out lookRotAngle, out lookRotAxis);
						male_p_cf_bodybone.transform.RotateAround(maleCrotchPos.transform.position, lookRotAxis, lookRotAngle);
					}
				}

				//Update player's shoulder collider's rotation
				if (SetFemaleCollider.Value)
					shoulderCollider.transform.LookAt(femaleBase.transform, cameraEye.transform.up);

				
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
			nowAnimState = hFlag.nowAnimStateName;		

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
				InitMaleFollow();
			}
			if (SetParentMode.Value == ParentMode.PositionAndAnimation || SetParentMode.Value == ParentMode.AnimationOnly)
			{
				AddAnimSpeedController(obj_chaF_001, _parentIsLeft, leftController, rightController);
			}

			setFlag = true;
		}

		public void UnsetP()
		{
			UnityEngine.Object.Destroy(maleHeadPos);
			UnityEngine.Object.Destroy(maleCrotchPos);
			UnityEngine.Object.Destroy(femaleSpinePos);

			if (objLeftHand != null)
				PushFixLeftHandButton();
			if (objRightHand != null)
				PushFixRightHandButton();
			if (objLeftLeg != null)
				PushFixLeftLegButton();
			if (objRightLeg != null)
				PushFixRightLegButton();
			if (objLeftMaleFoot != null)
				MaleFixLeftLegToggle();
			if (objRightMaleFoot != null)
				MaleFixRightLegToggle();

			leftController.transform.Find("Model").gameObject.SetActive(true);
			rightController.transform.Find("Model").gameObject.SetActive(true);
			if (obj_chaF_001.GetComponent<AnimSpeedController>() != null)
			{
				UnityEngine.Object.Destroy(obj_chaF_001.GetComponent<AnimSpeedController>());
			}

			setFlag = false;
		}

		public void InitMaleFollow()
		{
			GameObject maleNeck = maleFBBIK.references.spine[2].gameObject;
			if (maleHeadPos == null)
				maleHeadPos = new GameObject("maleHeadPos");
			maleHeadPos.transform.position = maleNeck.transform.position;
			maleHeadPos.transform.rotation = maleNeck.transform.rotation;
			maleHeadPos.transform.parent = maleNeck.transform;
			maleHeadPos.transform.localPosition = new Vector3(0, 0, 0.08f);
			
			GameObject maleCrotch = maleFBBIK.references.leftThigh.parent.Find("cf_d_kokan/cm_J_dan_top").gameObject;
			if (maleCrotchPos == null)
				maleCrotchPos = new GameObject("maleCrotchPos");
			maleCrotchPos.transform.position = maleCrotch.transform.position;
			maleCrotchPos.transform.rotation = maleCrotch.transform.rotation;
			maleCrotchPos.transform.parent = male_p_cf_bodybone.transform;
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
			image.rectTransform.sizeDelta = new Vector2(0.24f, 0.08f);
			image.color = new Color(0.8f, 0.8f, 0.8f);
			Button button = buttonObject.AddComponent<Button>();
			ColorBlock colors = button.colors;
			colors.highlightedColor = Color.red;
			colors.pressedColor = Color.cyan;
			button.colors = colors;
			text.rectTransform.sizeDelta = new Vector2(0.24f, 0.08f);
			text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
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
		/// Release and attach male limbs based on the distance between the attaching target position and the default animation position
		/// </summary>
		private void MaleIKs()
		{
			float rightHandDistance = (maleFBBIK.solver.rightHandEffector.target.position - male_cf_pv_hand_R.transform.position).magnitude;
			float rightHandTwist = Quaternion.Angle(maleFBBIK.solver.rightHandEffector.target.rotation, male_cf_pv_hand_R.transform.rotation);
			if (rightHandDistance > 0.2f || rightHandTwist > 45f)
			{
				maleFBBIK.solver.rightHandEffector.positionWeight = 0f;
				maleFBBIK.solver.rightHandEffector.rotationWeight = 0f;
			}
			else
			{
				maleFBBIK.solver.rightHandEffector.positionWeight = 1f;
				maleFBBIK.solver.rightHandEffector.rotationWeight = 1f;
			}

			float leftHandDistance = (maleFBBIK.solver.leftHandEffector.target.position - male_cf_pv_hand_L.transform.position).magnitude;
			float leftHandTwist = Quaternion.Angle(maleFBBIK.solver.leftHandEffector.target.rotation, male_cf_pv_hand_L.transform.rotation);
			if (leftHandDistance > 0.2f || leftHandTwist > 45f)
			{
				maleFBBIK.solver.leftHandEffector.positionWeight = 0f;
				maleFBBIK.solver.leftHandEffector.rotationWeight = 0f;
			}
			else
			{
				maleFBBIK.solver.leftHandEffector.positionWeight = 1f;
				maleFBBIK.solver.leftHandEffector.rotationWeight = 1f;
			}


			if (objRightMaleFoot != null && (maleFBBIK.solver.rightFootEffector.target.position - male_cf_pv_leg_R.transform.position).magnitude > 0.2f)
			{
				MaleFixRightLegToggle();
			}
			else
			{
				maleFBBIK.solver.rightFootEffector.positionWeight = 1f;
			}
			if (objLeftMaleFoot != null && (maleFBBIK.solver.leftFootEffector.target.position - male_cf_pv_leg_L.transform.position).magnitude > 0.2f)
			{
				MaleFixLeftLegToggle();
			}
			else
			{
				maleFBBIK.solver.leftFootEffector.positionWeight = 1f;
			}
		}

		/// <summary>
		/// Release and attach female limbs based on the distance between the attaching target position and the default animation position
		/// </summary>
		/// Attachment onto anchor points created by SetParent will enjoy a larger degree of freedom before being released
		private void FemaleIKs()
		{
			if (objRightHand != null && !fixRightHand && (femaleFBBIK.solver.rightHandEffector.target.position - female_cf_pv_hand_R.transform.position).magnitude > 0.35f)
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

			if (objLeftHand != null && !fixLefttHand && (femaleFBBIK.solver.leftHandEffector.target.position - female_cf_pv_hand_L.transform.position).magnitude > 0.35f)
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

			if (objRightLeg != null && !fixRightLeg && (femaleFBBIK.solver.rightFootEffector.target.position - female_cf_pv_leg_R.transform.position).magnitude > 0.5f)
			{
				PushFixRightLegButton();
			}
			else
			{
				femaleFBBIK.solver.rightFootEffector.positionWeight = 1f;
				femaleFBBIK.solver.rightFootEffector.rotationWeight = 1f;
			}

			if (objLeftLeg != null && !fixLeftLeg && (femaleFBBIK.solver.leftFootEffector.target.position - female_cf_pv_leg_L.transform.position).magnitude > 0.5f)
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
			if (hFlag.timeNoClickItem == 0 && (parentIsLeft ? (RightTriggerPressing() && !RightTrackPadUp()) : (LeftTriggerPressing() && !LeftTrackPadUp())))
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

		private bool femaleExists;

		private HFlag hFlag;

		private ChaControl male;

		private ChaControl female;

		private FullBodyBipedIK maleFBBIK;

		private FullBodyBipedIK femaleFBBIK;

		private string nowAnimState = "";

		private GameObject leftController;

		private GameObject rightController;

		private GameObject cameraEye;

		private GameObject shoulderCollider;

		private GameObject femaleAim;

		private FieldInfo f_device;

		private VRViveController leftVVC;

		private VRViveController rightVVC;

		private SteamVR_Controller.Device leftDevice;

		private SteamVR_Controller.Device rightDevice;

		private Canvas canvasSetParent;

		private GameObject objRightMenuCanvas;

		private CanvasScaler canvasScalerSetParent;

		private GameObject eventSystemSetParent;

		private Canvas canvasMotion;

		private GameObject objLeftMenuCanvas;

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

		internal GameObject male_p_cf_bodybone;

		private GameObject maleHeadPos;

		private GameObject maleCrotchPos;

		private GameObject male_cf_pv_hand_R;

		private GameObject male_cf_pv_hand_L;

		private GameObject male_cf_pv_leg_L;

		private GameObject male_cf_pv_leg_R;

		private GameObject male_cf_t_leg_L;

		private GameObject male_cf_t_leg_R;

		internal GameObject objLeftMaleFoot;

		internal GameObject objRightMaleFoot;

		private Text txtSetParentL;

		private Text txtSetParentR;

		private bool fixRightHand;

		private bool fixLefttHand;

		private bool fixRightLeg;

		private bool fixLeftLeg;

		private Vector3[] vecSpinePos = new Vector3[20];

		private int indexSpinePos;

		private Quaternion[] quatSpineRot = new Quaternion[20];

		private int indexSpineRot;

		private bool hideCanvas;

		private bool parentIsLeft;
	}
}
