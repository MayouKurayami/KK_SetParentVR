using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Illusion.Component.Correct;
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
		const int SmoothBuffer = 20;
		public void Init(HSprite _hsprite, List<MotionIK> _lstMotionIK)
		{
			hSprite = _hsprite;
			lstMotionIK = _lstMotionIK;
			//If keys are not set in configuration manager, it ruturns "Not set" instead of "None", causing it fail to parse to KeyCode
			try
			{
				shortcuts[(int)Key.SetParent] = (KeyCode)Enum.Parse(typeof(KeyCode), SetParentToggle.Value.ToString());
			}
			catch (ArgumentException e)
			{ 
				shortcuts[(int)Key.SetParent] = KeyCode.None;
			}
			try
			{
				shortcuts[(int)Key.LimbRelease] = (KeyCode)Enum.Parse(typeof(KeyCode), LimbReleaseKey.Value.ToString());
			}
			catch (ArgumentException e)
			{ 
				shortcuts[(int)Key.LimbRelease] = KeyCode.None; 
			}
			
		}
		
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

			female_cf_j_root = femaleFBBIK.references.root.gameObject;
			female_cf_j_hips = femaleFBBIK.references.pelvis.gameObject;
			female_cf_n_height = femaleFBBIK.references.pelvis.parent.gameObject;
			female_cf_j_spine01 = femaleFBBIK.references.spine[0].gameObject;
			female_cf_j_spine02 = femaleFBBIK.references.spine[1].gameObject;
			female_cf_j_neck = femaleFBBIK.references.spine[2].gameObject;
			female_cf_j_spine03 = femaleFBBIK.references.spine[2].parent.gameObject;

			switch (ParentPart.Value)
			{
				case BodyPart.Ass:
					femaleBase = female_cf_j_hips;
					break;
				case BodyPart.Torso:
					femaleBase = female_cf_j_spine02;
					break;
				case BodyPart.Head:
					femaleBase = female_cf_j_neck;
					break;
				default:
					femaleBase = female_cf_j_spine02;
					break;
			}
			femaleSpinePos = new GameObject("femaleSpinePos");

			Transform female_cf_pv_hand_R = female_cf_n_height.transform.Find("cf_pv_root/cf_pv_hand_R");
			Transform female_cf_pv_hand_L = female_cf_n_height.transform.Find("cf_pv_root/cf_pv_hand_L");
			Transform female_cf_pv_leg_R = female_cf_n_height.transform.Find("cf_pv_root/cf_pv_leg_R");
			Transform female_cf_pv_leg_L = female_cf_n_height.transform.Find("cf_pv_root/cf_pv_leg_L");
			BaseData female_hand_L_bd = femaleFBBIK.solver.leftHandEffector.target.GetComponent<BaseData>();
			BaseData female_hand_R_bd = femaleFBBIK.solver.rightHandEffector.target.GetComponent<BaseData>();
			BaseData female_leg_L_bd = femaleFBBIK.solver.leftFootEffector.target.GetComponent<BaseData>();
			BaseData female_leg_R_bd = femaleFBBIK.solver.rightFootEffector.target.GetComponent<BaseData>();

			Transform male_cf_n_height = maleFBBIK.references.pelvis.parent;
			Transform male_cf_pv_hand_R = male_cf_n_height.Find("cf_pv_root/cf_pv_hand_R");
			Transform male_cf_pv_hand_L = male_cf_n_height.Find("cf_pv_root/cf_pv_hand_L");
			Transform male_cf_pv_leg_R = male_cf_n_height.Find("cf_pv_root/cf_pv_leg_R");
			Transform male_cf_pv_leg_L = male_cf_n_height.Find("cf_pv_root/cf_pv_leg_L");
			BaseData male_hand_L_bd = maleFBBIK.solver.leftHandEffector.target.GetComponent<BaseData>();
			BaseData male_hand_R_bd = maleFBBIK.solver.rightHandEffector.target.GetComponent<BaseData>();
			BaseData male_leg_L_bd = maleFBBIK.solver.leftFootEffector.target.GetComponent<BaseData>();
			BaseData male_leg_R_bd = maleFBBIK.solver.rightFootEffector.target.GetComponent<BaseData>();

			limbs[(int)LimbName.FemaleLeftHand] = new Limb(
				LimbName.FemaleLeftHand, 
				null, 
				female_cf_pv_hand_L, 
				femaleFBBIK.solver.leftHandEffector, 
				femaleFBBIK.solver.leftHandEffector.target, 
				female_hand_L_bd, 
				femaleFBBIK.solver.leftArmChain);

			limbs[(int)LimbName.FemaleRightHand] = new Limb(
				LimbName.FemaleRightHand, 
				null, 
				female_cf_pv_hand_R, 
				femaleFBBIK.solver.rightHandEffector, 
				femaleFBBIK.solver.rightHandEffector.target, 
				female_hand_R_bd, 
				femaleFBBIK.solver.rightArmChain);

			limbs[(int)LimbName.FemaleLeftFoot] = new Limb(
				LimbName.FemaleLeftFoot, 
				null, 
				female_cf_pv_leg_L, 
				femaleFBBIK.solver.leftFootEffector, 
				femaleFBBIK.solver.leftFootEffector.target, 
				female_leg_L_bd, 
				femaleFBBIK.solver.leftLegChain);

			limbs[(int)LimbName.FemaleRightFoot] = new Limb(
				LimbName.FemaleRightFoot, 
				null, 
				female_cf_pv_leg_R, 
				femaleFBBIK.solver.rightFootEffector, 
				femaleFBBIK.solver.rightFootEffector.target, 
				female_leg_R_bd, 
				femaleFBBIK.solver.rightLegChain);

			limbs[(int)LimbName.MaleLeftHand] = new Limb(
				LimbName.MaleLeftHand, 
				null, 
				male_cf_pv_hand_L, 
				maleFBBIK.solver.leftHandEffector, 
				maleFBBIK.solver.leftHandEffector.target, 
				male_hand_L_bd);

			limbs[(int)LimbName.MaleRightHand] = new Limb(
				LimbName.MaleRightHand, 
				null, 
				male_cf_pv_hand_R, 
				maleFBBIK.solver.rightHandEffector, 
				maleFBBIK.solver.rightHandEffector.target, 
				male_hand_R_bd);

			limbs[(int)LimbName.MaleLeftFoot] = new Limb(
				LimbName.MaleLeftFoot, 
				null, 
				male_cf_pv_leg_L, 
				maleFBBIK.solver.leftFootEffector, 
				maleFBBIK.solver.leftFootEffector.target, 
				male_leg_L_bd);

			limbs[(int)LimbName.MaleRightFoot] = new Limb(
				LimbName.MaleRightFoot, 
				null, 
				male_cf_pv_leg_R, 
				maleFBBIK.solver.rightFootEffector, 
				maleFBBIK.solver.rightFootEffector.target, 
				male_leg_R_bd);


			SetBodyColliders();
			SetMapObjectsColliders();

			if (SetControllerCollider.Value)
			{
				SetControllerColliders(leftController);
				SetControllerColliders(rightController);
			}
			if (SetMaleCollider.Value)
			{
				SetMaleFeetColliders();
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
			CreateButton("右足固定/解除", new Vector3(28f, -68f, 0f), () => FixLimbToggle(limbs[(int)LimbName.FemaleRightFoot], true), objRightMenuCanvas) ;
			CreateButton("左足固定/解除", new Vector3(-28f, -68f, 0f), () => FixLimbToggle(limbs[(int)LimbName.FemaleLeftFoot], true), objRightMenuCanvas);
			CreateButton("右手固定/解除", new Vector3(28f, -48f, 0f), () => FixLimbToggle(limbs[(int)LimbName.FemaleRightHand], true), objRightMenuCanvas);
			CreateButton("左手固定/解除", new Vector3(-28f, -48f, 0f), () => FixLimbToggle(limbs[(int)LimbName.FemaleLeftHand], true), objRightMenuCanvas);
			CreateButton("男の右足固定/解除", new Vector3(28f, -28f, 0f), () => FixLimbToggle(limbs[(int)LimbName.MaleRightFoot], true), objRightMenuCanvas);
			CreateButton("男の左足固定/解除", new Vector3(-28f, -28f, 0f), () => FixLimbToggle(limbs[(int)LimbName.MaleLeftFoot], true), objRightMenuCanvas);
			txtSetParentL = CreateButton("左 親子付け Turn On", new Vector3(-28f, -4f, 0f), () => PushPLButton(), objRightMenuCanvas);
			txtSetParentR = CreateButton("右 親子付け Turn On", new Vector3(28f, -4f, 0f), () => PushPRButton(), objRightMenuCanvas);
			CreateButton("ヌク", new Vector3(-28f, 20f, 0f), () => hSprite.OnPullClick(), objRightMenuCanvas);
			txtSetParentMode = CreateButton(SetParentMode.Value.ToString(), new Vector3(28f, 20f, 0f), () => ParentModeChangeButton(), objRightMenuCanvas);
			CreateButton("モーション 強弱", new Vector3(-28f, 40f, 0f), () => PushMotionChangeButton(), objRightMenuCanvas);
			CreateButton("モーション 開始/停止", new Vector3(28f, 40f, 0f), () => PushModeChangeButton(), objRightMenuCanvas);	
			CreateButton("中に出すよ", new Vector3(-28f, 60f, 0f), () => PushFIButton(), objRightMenuCanvas);
			CreateButton("外に出すよ", new Vector3(28f, 60f, 0f), () => PushFOButton(), objRightMenuCanvas);
			CreateButton("入れるよ", new Vector3(-28f, 80f, 0f), () => hSprite.OnInsertClick(), objRightMenuCanvas);
			CreateButton("イレル", new Vector3(28f, 80f, 0f), () => hSprite.OnInsertNoVoiceClick(), objRightMenuCanvas);
			CreateButton("アナル入れるよ", new Vector3(-28f, 100f, 0f), () => hSprite.OnInsertAnalClick(), objRightMenuCanvas);
			CreateButton("アナルイレル", new Vector3(28f, 100f, 0f), () => hSprite.OnInsertAnalNoVoiceClick(), objRightMenuCanvas);
			


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
			CreateButton("正常位", new Vector3(-28f, -64f, 0f), () => StartCoroutine(ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_00")), objLeftMenuCanvas);
			CreateButton("開脚正常位", new Vector3(28f, -64f, 0f), () => StartCoroutine(ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_n00")), objLeftMenuCanvas);
			CreateButton("脚持つ正常位", new Vector3(-28f, -48f, 0f), () => StartCoroutine(ChangeMotion("h/anim/female/02_12_00.unity3d", "khs_f_n24")), objLeftMenuCanvas);
			CreateButton("脚持つ(強弱差分)", new Vector3(28f, -48f, 0f), () => StartCoroutine(ChangeMotion("h/anim/female/02_06_00.unity3d", "khs_f_n23")), objLeftMenuCanvas);

			CreateButton("側位(片足上げ)", new Vector3(-28f, -32f, 0f), () => StartCoroutine(ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_n06")), objLeftMenuCanvas);
			CreateButton("机側位", new Vector3(28f, -32f, 0f), () => StartCoroutine(ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_n16")), objLeftMenuCanvas);

			CreateButton("駅弁", new Vector3(-28f, -16f, 0f), () => StartCoroutine(ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_n22")), objLeftMenuCanvas);
			CreateButton("駅弁(強弱差分)", new Vector3(28f, -16f, 0f), () => StartCoroutine(ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_n08")), objLeftMenuCanvas);

			CreateButton("立位", new Vector3(-28f, 0f, 0f), () => StartCoroutine(ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_n07")), objLeftMenuCanvas);
			CreateButton("プール", new Vector3(28f, 0f, 0f), () => StartCoroutine(ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_n20")), objLeftMenuCanvas);
			
			CreateButton("跪くバック", new Vector3(-28f, 16f, 0f), () => StartCoroutine(ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_02")), objLeftMenuCanvas);
			CreateButton("腕引っ張りバック", new Vector3(28f, 16f, 0f), () => StartCoroutine(ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_n02")), objLeftMenuCanvas);
			CreateButton("椅子にバック", new Vector3(-28f, 32f, 0f), () => StartCoroutine(ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_11")), objLeftMenuCanvas);
			CreateButton("椅子腕引っ張りバック", new Vector3(28f, 32f, 0f), () => StartCoroutine(ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_n11")), objLeftMenuCanvas);
			CreateButton("壁にバック", new Vector3(-28f, 48f, 0f), () => StartCoroutine(ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_18")), objLeftMenuCanvas);
			CreateButton("壁バック片足上げ", new Vector3(28f, 48f, 0f), () => StartCoroutine(ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_n18")), objLeftMenuCanvas);

			CreateButton("フェンス後背位", new Vector3(-28f, 64f, 0f), () => StartCoroutine(ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_n21")), objLeftMenuCanvas);
			CreateButton("押し付け壁バック", new Vector3(28f, 64f, 0f), () => StartCoroutine(ChangeMotion("h/anim/female/02_20_00.unity3d", "khs_f_n28")), objLeftMenuCanvas);

			CreateButton("寝バック", new Vector3(-28f, 80f, 0f), () => StartCoroutine(ChangeMotion("h/anim/female/02_13_00.unity3d", "khs_f_n26")), objLeftMenuCanvas);
			CreateButton("跳び箱バック", new Vector3(28f, 80f, 0f), () => StartCoroutine(ChangeMotion("h/anim/female/02_12_00.unity3d", "khs_f_n25")), objLeftMenuCanvas);

			CreateButton("騎乗位", new Vector3(-28f, 96f, 0f), () => StartCoroutine(ChangeMotion("h/anim/female/02_13_00.unity3d", "khs_f_n27")), objLeftMenuCanvas);
			CreateButton("騎乗位(強弱差分)", new Vector3(28f, 96f, 0f), () => StartCoroutine(ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_n04")), objLeftMenuCanvas);	

			CreateButton("座位対面", new Vector3(-28f, 112f, 0f), () => StartCoroutine(ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_n09")), objLeftMenuCanvas);
			CreateButton("座位背面", new Vector3(28f, 112f, 0f), () => StartCoroutine(ChangeMotion("h/anim/female/02_00_00.unity3d", "khs_f_n10")), objLeftMenuCanvas);
			

			point = femaleAim.transform.position - cameraEye.transform.position;
			point.y = 0f;
			point.Normalize();
			canvasMotion.transform.position = new Vector3(femaleAim.transform.position.x, cameraEye.transform.position.y - 0.4f, femaleAim.transform.position.z) + Quaternion.Euler(0f, -90f, 0f) * point * 1.5f;
			canvasMotion.transform.forward = (canvasMotion.transform.position - cameraEye.transform.position).normalized;
		}

		private void SetBodyColliders()
		{
			for (int i = (int)LimbName.FemaleLeftHand; i <= (int)LimbName.FemaleRightFoot; i++)
			{
				GameObject collider = new GameObject(limbs[i].LimbPart.ToString() + "Collider");
				collider.AddComponent<FixBodyParts>().Init(this, limbs[i].LimbPart);
				collider.transform.parent = limbs[i].Effector.bone;
				collider.transform.localPosition = Vector3.zero;
			}

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

		private void SetControllerColliders(GameObject controller)
		{
			GameObject CtrlCollider = new GameObject("ControllerCollider");
			CtrlCollider.transform.parent = controller.transform;
			CtrlCollider.transform.localPosition = Vector3.zero;
			CtrlCollider.transform.localRotation = Quaternion.identity;
			SphereCollider sphereCollider = CtrlCollider.AddComponent<SphereCollider>();
			sphereCollider.isTrigger = true;
			sphereCollider.center = Vector3.zero;
			sphereCollider.radius = 0.05f;
			CtrlCollider.AddComponent<Rigidbody>().isKinematic = true;
		}

		private void SetMaleFeetColliders()
		{
			for (int i = (int)LimbName.MaleLeftFoot; i <= (int)LimbName.MaleRightFoot; i++)
			{
				GameObject collider = new GameObject(limbs[i].LimbPart.ToString() + "Collider");
				collider.AddComponent<FixBodyParts>().Init(this, limbs[i].LimbPart);
				collider.transform.parent = limbs[i].Effector.bone;
				collider.transform.localPosition = Vector3.zero;
			}
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
						meshCollider.convex = false;
						meshCollider.sharedMesh = meshFilter.mesh;
					}
				}
			}
		}

		/// <summary>
		/// Toggle for creating/destroying anchor objects for attaching the limbs onto
		/// </summary>
		/// <param name="limb">the limb to attach/detach</param>
		/// <param name="fix">flag to indicate whether limb should be automatically detached</param>
		internal void FixLimbToggle(Limb limb, bool fix = false)
		{
			if (!limb.AnchorObj)
			{
				limb.AnchorObj = new GameObject(limb.LimbPart.ToString() + "Anchor");
				limb.AnchorObj.transform.position = limb.Effector.bone.position;
				limb.AnchorObj.transform.rotation = limb.Effector.bone.rotation;
				limb.Effector.target = limb.AnchorObj.transform;
				limb.Fixed = fix;
				return;
			}
			UnityEngine.Object.Destroy(limb.AnchorObj);
			limb.Effector.target = limb.OrigTarget;
			limb.Fixed = false;

			//When the IK target is set to the anchor object and a motion change undergoes, the Basedata bone of the original target would be set to null due to some unknown reason,
			//causing the effector bone to not approach the target correctly.
			//This resets motionIK using the current animation to prevent bone target Basedata being set to null
			if (limb.TargetBone.bone == null)
				lstMotionIK.ForEach((MotionIK motionIK) => motionIK.Calc(hFlag.nowAnimStateName));
		}

		/// <summary>
		/// Change female animation/position
		/// </summary>
		/// <param name="path"></param>
		/// <param name="name"></param>
		private IEnumerator ChangeMotion(string path, string name)
		{
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
		private void ParentModeChangeButton()
		{
			int index = (int)SetParentMode.Value + 1;
			SetParentMode.Value =  (ParentMode)(index % Enum.GetNames(typeof(ParentMode)).Length);
			txtSetParentMode.text = SetParentMode.Value.ToString();
		}

		public void LateUpdate()
		{
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
				{
					leftController = hSprite.managerVR.objMove.transform.Find("Controller (left)").gameObject;
					if (SetControllerCollider.Value)
						SetControllerColliders(leftController);
				}
				leftVVC = leftController.GetComponent<VRViveController>();
				leftDevice = (f_device.GetValue(leftVVC) as SteamVR_Controller.Device);
			}
			if (rightDevice == null)
			{
				if (rightController == null)
				{
					rightController = hSprite.managerVR.objMove.transform.Find("Controller (right)").gameObject;
					if (SetControllerCollider.Value)
						SetControllerColliders(rightController);
				}	
				rightVVC = rightController.GetComponent<VRViveController>();
				rightDevice = (f_device.GetValue(rightVVC) as SteamVR_Controller.Device);
			}

			//Initiate canvas if it's null
			if (objRightMenuCanvas == null)
				InitCanvas();
			else
			{
				//Hold button for 1 second to hide/unhide floating menu
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

				//Make floating menu follow and rotate around female
				Vector3 point = femaleAim.transform.position - cameraEye.transform.position;
				point.y = 0f;
				point.Normalize();
				objRightMenuCanvas.transform.position = new Vector3(femaleAim.transform.position.x, cameraEye.transform.position.y, femaleAim.transform.position.z) + Quaternion.Euler(0f, 90f, 0f) * point * 0.4f;
				objRightMenuCanvas.transform.forward = (objRightMenuCanvas.transform.position - cameraEye.transform.position).normalized;
				objLeftMenuCanvas.transform.position = new Vector3(femaleAim.transform.position.x, cameraEye.transform.position.y, femaleAim.transform.position.z) + Quaternion.Euler(0f, -90f, 0f) * point * 0.4f;
				objLeftMenuCanvas.transform.forward = (objLeftMenuCanvas.transform.position - cameraEye.transform.position).normalized;
			
				//When SetParent is active, display the menu regardless of being hidden when user brings controller within set distance to the headset
				if (setFlag)
				{
					Vector3 vector;
					if (parentIsLeft)
						vector = cameraEye.transform.position - rightController.transform.position;
					else
						vector = cameraEye.transform.position - leftController.transform.position;

					if (vector.magnitude <= MenuUpProximity.Value)
					{
						objRightMenuCanvas.SetActive(true);
						objLeftMenuCanvas.SetActive(hFlag.mode != HFlag.EMode.aibu);
					}
					else
					{
						objRightMenuCanvas.SetActive(!hideCanvas);
						objLeftMenuCanvas.SetActive(!hideCanvas && hFlag.mode != HFlag.EMode.aibu);
					}
				}
				else 
				{
					objRightMenuCanvas.SetActive(!hideCanvas);
					objLeftMenuCanvas.SetActive(!hideCanvas && hFlag.mode != HFlag.EMode.aibu);
				}
			}			
			
			//////////////
			//Activate/deactivate SetParent functionality by
			//* Pressing keyboard shortcut or 
			//* Pressing menu button and trigger at the same time
			//////////////
			bool setParentToggle = Input.GetKeyDown(shortcuts[(int)Key.SetParent]) || (RightMenuPressing() && RightTriggerPressDown()) || (LeftMenuPressing() && LeftTriggerPressDown());
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

			//If trigger is pressed, call function to interact with limbs. Otherwise increase timer since last trigger press
			if (LeftTriggerRelease())
				ControllerLimbActions(leftController, ref lastTriggerRelease[0]);
			else
				lastTriggerRelease[0] += Time.deltaTime;

			if (RightTriggerRelease())
				ControllerLimbActions(rightController, ref lastTriggerRelease[1]);
			else
				lastTriggerRelease[1] += Time.deltaTime;

			//If keyboard shortcut for limb release is pressed, call function to interact with limbs with paramemters that will ensure the release of all limbs
			if (Input.GetKeyDown(shortcuts[(int)Key.LimbRelease]))
			{
				float _ = 0;
				ControllerLimbActions(leftController, ref _, true);
			}

			MaleIKs();

			FemaleIKs();


			if (setFlag)
			{
				//Reposition male rotation axis if motion changed
				if (nowAnimState != hFlag.nowAnimStateName)
				{
					if (SetParentMale.Value)
						InitMaleFollow();
					nowAnimState = hFlag.nowAnimStateName;
				}			
			
				ControllerCharacterAdjustment();


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

				//Update player's shoulder collider's rotation to always be facing the girl
				shoulderCollider.transform.LookAt(femaleBase.transform, cameraEye.transform.up);

				
				txtSetParentL.text = "親子付け Turn Off";
				txtSetParentR.text = "親子付け Turn Off";
			}
			else
			{
				txtSetParentL.text = "左 親子付け Turn On";
				txtSetParentR.text = "右 親子付け Turn On";
			}

			/////////////////////////////////////////////////////////
			///Use arrays to store the position and rotation of the female pivot object during the last constant number of frames.
			///Fill the arrays with the current position and rotation if we want the female to strictly follow
			if (currentCtrlstate == CtrlState.Following)
			{
				for (int j = 0; j < SmoothBuffer; j++)
					quatSpineRot[j] = femaleSpinePos.transform.rotation;
				for (int i = 0; i < SmoothBuffer; i++)
					vecSpinePos[i] = femaleSpinePos.transform.position;
			}
			else
			{
				quatSpineRot[indexSpineRot] = femaleSpinePos.transform.rotation;
				vecSpinePos[indexSpinePos] = femaleSpinePos.transform.position;
			}
				
			if (indexSpineRot >= (SmoothBuffer -1))
				indexSpineRot = 0;
			else
				indexSpineRot++;
			if (indexSpinePos >= (SmoothBuffer - 1))
				indexSpinePos = 0;
			else
				indexSpinePos++;


			if ((setFlag && SetParentMode.Value < ParentMode.AnimationOnly) || currentCtrlstate == CtrlState.Following || currentCtrlstate == CtrlState.FemaleControl)
				FemalePositionUpdate(femaleSpinePos);
		}

		/// <summary>
		/// Enable SetParent functionality
		/// </summary>
		/// <param name="_parentIsLeft">Whether the left controller is the parenting controller</param>
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
			parentController = _parentIsLeft ? leftController : rightController;

			if (femaleSpinePos == null)
			{
				femaleSpinePos = new GameObject("femaleSpinePos");
			}
			if (SetParentMode.Value == ParentMode.PositionOnly || SetParentMode.Value == ParentMode.PositionAndAnimation)
			{
				SetParentToController(parentController, femaleSpinePos, femaleBase, true);
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

		/// <summary>
		/// Diable SetParent functionality
		/// </summary>
		public void UnsetP()
		{
			UnityEngine.Object.Destroy(maleHeadPos);
			UnityEngine.Object.Destroy(maleCrotchPos);
			femaleSpinePos.transform.parent = null;

			foreach (Limb limb in limbs)
			{
				if (limb.AnchorObj)
					FixLimbToggle(limb);
			}

			leftController.transform.Find("Model").gameObject.SetActive(true);
			rightController.transform.Find("Model").gameObject.SetActive(true);
			if (SetControllerCollider.Value)
			{
				leftController.transform.Find("ControllerCollider").GetComponent<SphereCollider>().enabled = true;
				rightController.transform.Find("ControllerCollider").GetComponent<SphereCollider>().enabled = true;
			}
			
			if (obj_chaF_001.GetComponent<AnimSpeedController>() != null)
			{
				UnityEngine.Object.Destroy(obj_chaF_001.GetComponent<AnimSpeedController>());
			}

			setFlag = false;
		}

		/// <summary>
		/// Initialize and position objects representing male's neck and crotch to be used for rotation calculation
		/// </summary>
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
		/// Update female position and rotation
		/// </summary>
		/// <param name="target">The object to synchronize female body to</param>
		private void FemalePositionUpdate(GameObject target)
		{
			if (TrackingMode.Value && currentCtrlstate != CtrlState.Following)
			{
				Quaternion quaternion = quatSpineRot[0];
				for (int i = 1; i < 20; i++)
				{
					quaternion = Quaternion.Lerp(quaternion, quatSpineRot[i], 1f / (i + 1));
				}
				switch (ParentPart.Value)
				{
					case BodyPart.Ass:
						female_p_cf_bodybone.transform.rotation = quaternion * Quaternion.Inverse(female_cf_j_hips.transform.localRotation) * Quaternion.Inverse(female_cf_n_height.transform.localRotation) * Quaternion.Inverse(female_cf_j_root.transform.localRotation);
						break;
					case BodyPart.Torso:
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
						female_p_cf_bodybone.transform.rotation = target.transform.rotation * Quaternion.Inverse(female_cf_j_hips.transform.localRotation) * Quaternion.Inverse(female_cf_n_height.transform.localRotation) * Quaternion.Inverse(female_cf_j_root.transform.localRotation);
						break;
					case BodyPart.Torso:
						female_p_cf_bodybone.transform.rotation = target.transform.rotation * Quaternion.Inverse(female_cf_j_spine02.transform.localRotation) * Quaternion.Inverse(female_cf_j_spine01.transform.localRotation) * Quaternion.Inverse(female_cf_j_hips.transform.localRotation) * Quaternion.Inverse(female_cf_n_height.transform.localRotation) * Quaternion.Inverse(female_cf_j_root.transform.localRotation);
						break;
					case BodyPart.Head:
						female_p_cf_bodybone.transform.rotation = target.transform.rotation * Quaternion.Inverse(female_cf_j_neck.transform.localRotation) * Quaternion.Inverse(female_cf_j_spine03.transform.localRotation) * Quaternion.Inverse(female_cf_j_spine02.transform.localRotation) * Quaternion.Inverse(female_cf_j_spine01.transform.localRotation) * Quaternion.Inverse(female_cf_j_hips.transform.localRotation) * Quaternion.Inverse(female_cf_n_height.transform.localRotation) * Quaternion.Inverse(female_cf_j_root.transform.localRotation);
						break;
					default:
						female_p_cf_bodybone.transform.rotation = target.transform.rotation * Quaternion.Inverse(female_cf_j_spine02.transform.localRotation) * Quaternion.Inverse(female_cf_j_spine01.transform.localRotation) * Quaternion.Inverse(female_cf_j_hips.transform.localRotation) * Quaternion.Inverse(female_cf_n_height.transform.localRotation) * Quaternion.Inverse(female_cf_j_root.transform.localRotation);
						break;
				}
			}


			if (TrackingMode.Value && currentCtrlstate != CtrlState.Following)
			{
				Vector3 a = Vector3.zero;
				foreach (Vector3 b in vecSpinePos)
				{
					a += b;
				}
				a /= 20f;
				female_p_cf_bodybone.transform.position += a - femaleBase.transform.position;
			}
			else
			{
				female_p_cf_bodybone.transform.position += target.transform.position - femaleBase.transform.position;
			}
		}

		/// <summary>
		/// Release and attach male limbs based on the distance between the attaching target position and the default animation position
		/// </summary>
		private void MaleIKs()
		{
			//Algorithm for the male hands
			for (int i = (int)LimbName.MaleLeftHand; i <= (int)LimbName.MaleRightHand; i++)
			{
				//Since the male hands don't have colliders and won't attach to objects, we only need to consider the default IKs (e.g., grabing female body parts)
				//To prevent excessive stretching or the hands being at a weird angle, 
				//if position and rotation difference between the IK effector and original animation is beyond threshold, set IK weights to 0. 
				//Set IK weights to 1 if otherwise.
				float distance = (limbs[i].Effector.target.position - limbs[i].AnimPos.position).magnitude;
				float twist = Quaternion.Angle(limbs[i].Effector.target.rotation, limbs[i].AnimPos.rotation);
				if (distance > 0.2f || twist > 45f)
				{
					limbs[i].Effector.positionWeight = 0f;
					limbs[i].Effector.rotationWeight = 0f;
				}
				else
				{
					limbs[i].Effector.positionWeight = 1f;
					limbs[i].Effector.rotationWeight = 1f;
				}
			}

			//Algorithm for the male feet
			for (int i = (int)LimbName.MaleLeftFoot; i <= (int)LimbName.MaleRightFoot; i++)
			{
				//Release the male feet from attachment if streched beyond threshold
				if(limbs[i].AnchorObj && !limbs[i].Fixed && (limbs[i].Effector.target.position - limbs[i].AnimPos.position).magnitude > 0.2f)
				{
					FixLimbToggle(limbs[i]);
				}
				else
				{
					limbs[i].Effector.positionWeight = 1f;
				}
			}
		}

		/// <summary>
		/// Release and attach female limbs based on the distance between the attaching target position and the default animation position
		/// </summary>
		/// Attachment onto anchor points created by SetParent will enjoy a larger degree of freedom before being released
		private void FemaleIKs()
		{
			//Algorithm for female hands
			for (int i = (int)LimbName.FemaleLeftHand; i <= (int)LimbName.FemaleRightHand; i++)
			{
				//Calculate distance between effector target and original animation to determine stretching
				float distance = (limbs[i].Effector.target.position - limbs[i].AnimPos.position).magnitude;
				if (limbs[i].AnchorObj && distance > StretchLimitArms.Value)
				{
					//If stretched beyond set threshold and limbs do not have fixed flag enabled, release the limb
					if (!limbs[i].Fixed)
					{
						FixLimbToggle(limbs[i]);
						continue;
					}
					//If fixed flag is enabled but arms are stretched past set threshold, reduce bending weight to 0 
					//to avoid arms bending towards a bending goal that is no longer in a natural position
					else
						limbs[i].Chain.bendConstraint.weight = 0f;
				}
				//If arms are not attached to objects, we still need to take care of the default IK's (e.g., hands sticking to male)
				//If stretching is beyond a set threshold then reduce effector weights to 0
				else if (!(limbs[i].AnchorObj) && distance > 0.15f)
				{
					limbs[i].Effector.positionWeight = (0.3f - distance) / 0.15f;
					limbs[i].Effector.rotationWeight = (0.3f - distance) / 0.15f;
					continue;
				}

				//If arms are not overly stretched, set effector weights to max and set other IK parameters to improve animation naturalness
				limbs[i].Effector.positionWeight = 1f;
				limbs[i].Effector.rotationWeight = 1f;
				limbs[i].Effector.maintainRelativePositionWeight = 1f;
				limbs[i].Chain.push = 0.1f;
				limbs[i].Chain.pushParent = 0.5f;
			}

			//Algorithm for female feet
			for (int i = (int)LimbName.FemaleLeftFoot; i <= (int)LimbName.FemaleRightFoot; i++)
			{
				//Again use distance between effecotr target and animation position to determine stretching
				//Since feet don't have default IK's (they don't grab onto anything by default), 
				//we only need to release them from grabbing onto objects if stretched too far
				float distance = (limbs[i].Effector.target.position - limbs[i].AnimPos.position).magnitude;
				if (limbs[i].AnchorObj && !limbs[i].Fixed && distance > StretchLimitLegs.Value)
				{
					FixLimbToggle(limbs[i]);
				}
				//If feet are not overly stretched, set effector weights to max and set other IK parameters to improve animation naturalness
				else
				{
					limbs[i].Effector.positionWeight = 1f;
					limbs[i].Effector.rotationWeight = 1f;
					limbs[i].Chain.push = 0.1f;
					limbs[i].Chain.pushParent = 0.5f;
					limbs[i].Chain.bendConstraint.weight = 0f;	
				}
			}
		}

		/// <summary>
		/// Given the controller input, freeze or release limbs 
		/// </summary>
		/// <param name="controller">The controller being pressed</param>
		/// <param name="timeNoClick">Time since the last click, for double click registration</param>
		/// <param name="forceAll">Whether to force release all limbs</param>
		private void ControllerLimbActions(GameObject controller, ref float timeNoClick, bool forceAll = false)
		{
			//If time since last click is greater than 0.25 second, register as single click and freeze the limb that is currently following this controller
			if (timeNoClick > 0.25f)
			{
				for (int i = (int)LimbName.FemaleLeftHand; i <= (int)LimbName.FemaleRightFoot; i++)
				{
					if (limbs[i].AnchorObj && limbs[i].AnchorObj.transform.parent && limbs[i].AnchorObj.transform.parent.parent == controller.transform)
						limbs[i].AnchorObj.transform.parent = null;
				}
			}
			//double click registered
			else
			{
				bool singleLimbRelease = false;
				//If not forcing all limbs' release, release any limb that is within proximity of the controller
				if (!forceAll)
				{
					for (int i = (int)LimbName.FemaleLeftHand; i <= (int)LimbName.FemaleRightFoot; i++)
					{
						if (limbs[i].AnchorObj && (limbs[i].AnchorObj.transform.position - controller.transform.position).magnitude < 0.2f)
						{
							FixLimbToggle(limbs[i]);
							singleLimbRelease = true;
						}
					}
				}
				// If no a single limb is close to the controller, release all limbs
				if (singleLimbRelease == false)
				{
					for (int i = (int)LimbName.FemaleLeftHand; i <= (int)LimbName.FemaleRightFoot; i++)
					{
						if (limbs[i].AnchorObj)
							FixLimbToggle(limbs[i]);
					}
				}
			}

			timeNoClick = 0f;
		}



		/// <summary>
		/// Change state of controller-to-characters relationship based on controller input
		/// </summary>
		private void ControllerCharacterAdjustment()
		{
			///////////////////
			//Based on controller input, set characters into one of these 4 states based on controller input:
			//	1. Remain still relative to the scene if only the trigger is held
			//	2. Both male and female following parent controller (controller specified when activating set parent)
			//	3. Male body parented to non parent controller
			//	4. Female body parented to non parent controller 
			//	5. If no matching controller input is present, return to default state of parenting
			///////////////////
			if (hFlag.timeNoClickItem == 0 && (parentIsLeft ? (RightTriggerPressing() && !RightGripPressing()) : (LeftTriggerPressing() && !LeftGripPressing())))
			{
				if (currentCtrlstate != CtrlState.Stationary)
					currentCtrlstate = ChangeControlState(currentCtrlstate, CtrlState.Stationary);
				return;
			}
			else if (parentIsLeft ? (RightTriggerPressing() && RightGripPressing()) : (LeftTriggerPressing() && LeftGripPressing()))
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

		private void SetParentToController (GameObject controller, GameObject parentDummy, GameObject target, bool hideModel)
		{
			parentDummy.transform.parent = controller.transform;
			if (hideModel)
			{
				controller.transform.Find("Model").gameObject.SetActive(false);
				if (SetControllerCollider.Value)
					controller.transform.Find("ControllerCollider").GetComponent<SphereCollider>().enabled = false;
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
						SetParentToController(parentController, femaleSpinePos, femaleBase, true);
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
						SetParentToController(parentController, femaleSpinePos, femaleBase, true);
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
					SetParentToController(parentIsLeft ? rightController : leftController, femaleSpinePos, femaleBase, false);
					return CtrlState.FemaleControl;

				case CtrlState.Following:
					if (SetParentMode.Value == ParentMode.AnimationOnly)
						SetParentToController(parentController, femaleSpinePos, femaleBase, false);
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

		public enum LimbName
		{
			FemaleLeftHand,
			FemaleRightHand,
			FemaleLeftFoot,
			FemaleRightFoot,
			MaleLeftHand,
			MaleRightHand,
			MaleLeftFoot,
			MaleRightFoot
		}

		public enum Key
		{
			SetParent,
			LimbRelease
		}

		internal class Limb
		{
			internal GameObject AnchorObj;
			internal Transform AnimPos;
			internal IKEffector Effector;
			internal FBIKChain Chain;
			internal Transform OrigTarget;
			internal bool Fixed;
			internal LimbName LimbPart;
			internal BaseData TargetBone;

			internal Limb(LimbName limbpart, GameObject anchorObj, Transform animPos, IKEffector effector, Transform origTarget, BaseData targetBone, FBIKChain chain = null, bool fix = false)
			{
				LimbPart = limbpart;
				AnchorObj = anchorObj;
				AnimPos = animPos;
				Effector = effector;
				Chain = chain;
				OrigTarget = origTarget;
				TargetBone = targetBone;
				Fixed = fix;
			}
		}


		internal CtrlState currentCtrlstate;
		
		internal bool setFlag;

		private bool femaleExists;

		private HFlag hFlag;

		private ChaControl male;

		private ChaControl female;

		private FullBodyBipedIK maleFBBIK;

		private FullBodyBipedIK femaleFBBIK;

		private List<MotionIK> lstMotionIK;

		private string nowAnimState = "";

		private GameObject leftController;

		private GameObject rightController;

		private GameObject parentController;

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

		internal Limb[] limbs = new Limb[8];

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

		private Text txtSetParentL;

		private Text txtSetParentR;

		private Text txtSetParentMode;

		private Vector3[] vecSpinePos = new Vector3[SmoothBuffer];

		private int indexSpinePos;

		private Quaternion[] quatSpineRot = new Quaternion[SmoothBuffer];

		private int indexSpineRot;

		private bool hideCanvas;

		private bool parentIsLeft;

		private float[] lastTriggerRelease = new float[2] { 0, 0 };

		private KeyCode[] shortcuts = new KeyCode[2];
	}
}
