using System;
using UnityEngine;
using RootMotion.FinalIK;
using Illusion.Component.Correct;
using static SetParentKK.KK_SetParentVR;


namespace SetParentKK
{
	public partial class SetParent
	{
		internal Limb[] limbs = new Limb[8];

		private SkinnedMeshRenderer[] itemHands = new SkinnedMeshRenderer[2];

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

			internal BaseData ParentJointBone;
			internal IKEffector ParentJointEffector;
			internal Transform ParentJointAnimPos;

			internal Limb(LimbName limbpart, GameObject anchorObj, Transform animPos, IKEffector effector, Transform origTarget,
				BaseData targetBone, FBIKChain chain = null, BaseData parentJointBone = null, IKEffector parentJointEffector = null,
				Transform parentJointAnimPos = null, bool fix = false)
			{
				LimbPart = limbpart;
				AnchorObj = anchorObj;
				AnimPos = animPos;
				Effector = effector;
				Chain = chain;
				OrigTarget = origTarget;
				TargetBone = targetBone;
				Fixed = fix;

				ParentJointBone = parentJointBone;
				ParentJointEffector = parentJointEffector;
				ParentJointAnimPos = parentJointAnimPos;
			}
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
		/// Release and attach male limbs based on the distance between the attaching target position and the default animation position
		/// </summary>
		private void MaleIKs()
		{
			bool hideGropeHands = setFlag && hFlag.mode != HFlag.EMode.aibu && GropeHandsDisplay.Value < HideHandMode.AlwaysShow;

			//Algorithm for the male hands
			for (int i = (int)LimbName.MaleLeftHand; i <= (int)LimbName.MaleRightHand; i++)
			{
				//If anchors exist for the male hands, that means they are following the controllers
				//Lower bendConstraint weight to allow rotation of the arm
				//Set chain pull weight to zero to avoid arms from pulling the body (may not be effective however)
				if (limbs[i].AnchorObj)
				{
					limbs[i].Effector.positionWeight = 1f;
					limbs[i].Effector.rotationWeight = 1f;
					limbs[i].Chain.bendConstraint.weight = 0.2f;
					limbs[i].Chain.pull = 0f;

					//Hide/unhide the additional hands that show up when groping, depending on config setting
					//If settings is set to auto (neither AlwaysShow or AlwaysHide), hide them only when the controller gets close
					if (hideGropeHands)
					{
						if (GropeHandsDisplay.Value == HideHandMode.AlwaysHide)
							itemHands[i - 4].enabled = false;
						else if ((itemHands[i - 4].transform.position - limbs[i].AnchorObj.transform.position).magnitude > 0.2f)
							itemHands[i - 4].enabled = true;
						else
							itemHands[i - 4].enabled = false;
					}
					continue;
				}

				//Restore IK parameters to default if hands are not attached
				limbs[i].Chain.bendConstraint.weight = 1f;
				limbs[i].Chain.pull = 1f;

				//To prevent excessive stretching or the hands being at a weird angle with the default IKs (e.g., grabing female body parts),
				//if rotation difference between the IK effector and original animation is beyond threshold, set IK weights to 0. 
				//Set IK weights to 1 if otherwise.
				float twist = Quaternion.Angle(limbs[i].Effector.target.rotation, limbs[i].AnimPos.rotation);
				if (twist > 45f)
				{
					limbs[i].Effector.positionWeight = 0f;
					limbs[i].Effector.rotationWeight = 0f;
				}
				else
				{
					limbs[i].Effector.positionWeight = 1f;
					limbs[i].Effector.rotationWeight = 1f;
				}

				//Assign bone to male shoulder effectors and fix it in place to prevent hands from pulling the body
				//Does not run if male hands are in sync with controllers to allow further movement of the hands
				if (setFlag)
				{
					limbs[i].ParentJointBone.bone = limbs[i].ParentJointAnimPos;
					limbs[i].ParentJointEffector.positionWeight = 1f;
				}
			}

			//Algorithm for the male feet
			for (int i = (int)LimbName.MaleLeftFoot; i <= (int)LimbName.MaleRightFoot; i++)
			{
				//Release the male feet from attachment if streched beyond threshold
				if (limbs[i].AnchorObj && !limbs[i].Fixed && (limbs[i].Effector.target.position - limbs[i].AnimPos.position).magnitude > 0.2f)
				{
					FixLimbToggle(limbs[i]);
				}
				else
				{
					limbs[i].Effector.positionWeight = 1f;
				}
			}

			if (setFlag)
			{
				//Fix male hips to animation position to prevent male genital from drifting due to pulling from limb chains
				male_hips_bd.bone = male_cf_pv_hips;
				maleFBBIK.solver.bodyEffector.positionWeight = 1f;
				maleFBBIK.solver.bodyEffector.rotationWeight = 1f;
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
				//Reset parameters to default values		
				limbs[i].Effector.positionWeight = 1f;
				limbs[i].Effector.rotationWeight = 1f;

				//Calculate distance between effector target and original animation to determine stretching
				float distance = (limbs[i].Effector.target.position - limbs[i].AnimPos.position).magnitude;

				if (limbs[i].AnchorObj)
				{
					//If limb is manually held or placed, disable bending goal to avoid unnatural bending
					if (limbs[i].Fixed)
					{
						limbs[i].Chain.bendConstraint.weight = 0f;
					}
					//If stretched beyond set threshold and limbs do not have fixed flag enabled, release the limb
					else if (distance > StretchLimitArms.Value)
					{
						FixLimbToggle(limbs[i]);
						continue;
					}
					//Optimize IK behaviors if arms are attached to objects
					limbs[i].Effector.maintainRelativePositionWeight = 1f;
					limbs[i].Chain.push = 0.1f;
					limbs[i].Chain.pushParent = 0.5f;
				}
				else
				{
					//Arms are not attached to objects, so restore IK parameters to default values
					limbs[i].Chain.bendConstraint.weight = 1f;
					limbs[i].Effector.maintainRelativePositionWeight = 0f;
					limbs[i].Chain.push = 0f;
					limbs[i].Chain.pushParent = 0f;

					//If arms are not attached to objects, we still need to take care of the default IK's (e.g., hands sticking to male)
					//If stretching is beyond a set threshold then gradually reduce effector weights to 0
					if (distance > 0.15f)
					{
						limbs[i].Effector.positionWeight = (0.3f - distance) / 0.15f;
						limbs[i].Effector.rotationWeight = (0.3f - distance) / 0.15f;
					}
				}
			}

			//Algorithm for female feet
			for (int i = (int)LimbName.FemaleLeftFoot; i <= (int)LimbName.FemaleRightFoot; i++)
			{
				if (limbs[i].AnchorObj)
				{
					//Use distance between effecotr target and animation position to determine stretching
					//Since feet don't have default IK's (they don't grab onto anything by default), 
					//we only need to release them from grabbing onto objects if stretched too far
					float distance = (limbs[i].Effector.target.position - limbs[i].AnimPos.position).magnitude;

					//If limb is manually held or placed, disable bending goal to avoid unnatural bending
					if (limbs[i].Fixed)
					{
						limbs[i].Chain.bendConstraint.weight = 0f;
					}
					//If stretched beyond set threshold and limbs do not have fixed flag enabled, release the limb
					else if (distance > StretchLimitLegs.Value)
					{
						FixLimbToggle(limbs[i]);
						continue;
					}
					//Adjust IK parameters to optimize behavior
					//Set effector weights to 1 just in case
					limbs[i].Effector.positionWeight = 1f;
					limbs[i].Effector.rotationWeight = 1f;
					limbs[i].Chain.push = 0.1f;
					limbs[i].Chain.pushParent = 0.5f;
				}
				else
				{
					//Restore IK parameters to default. 
					//No need to adjust effector weights because the defaults are already 1
					limbs[i].Chain.push = 0f;
					limbs[i].Chain.pushParent = 0f;
					limbs[i].Chain.bendConstraint.weight = 1f;
				}
			}
		}

		/// <summary>
		/// Given the controller input, freeze or release limbs 
		/// </summary>
		/// <param name="controller">The controller being pressed</param>
		/// <param name="timeNoClick">Time since the last click, for double click registration</param>
		/// <param name="forceAll">Whether to force release all limbs</param>
		private void ControllerLimbActions(GameObject controller, bool doubleClick, bool forceAll = false)
		{
			//If time since last click is greater than 0.25 second, register as single click and freeze the limb that is currently following this controller
			if (!doubleClick)
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
		}

		/// <summary>
		/// Fix male feet in place, or release them if they're already fixed
		/// </summary>
		private void ControllerMaleFeetToggle(bool doubleClick = true)
		{
			if (!doubleClick)
				return;

			bool releaseAll = true;
			for (int i = (int)LimbName.MaleLeftFoot; i <= (int)LimbName.MaleRightFoot; i++)
			{
				if (!limbs[i].AnchorObj)
				{
					FixLimbToggle(limbs[i], fix: true);
					releaseAll = false;
				}
				else if (!limbs[i].Fixed)
				{
					limbs[i].Fixed = true;
					releaseAll = false;
				}
			}

			//Release both feet if and only if both feet are alrady fixed
			if (releaseAll)
			{
				for (int i = (int)LimbName.MaleLeftFoot; i <= (int)LimbName.MaleRightFoot; i++)
					FixLimbToggle(limbs[i]);

				txtMaleLeftFoot.text = "男の左足固定";
				txtMaleRightFoot.text = "男の右足固定";
			}
			else
			{
				txtMaleLeftFoot.text = "男の左足解除";
				txtMaleRightFoot.text = "男の右足解除";
			}
		}

		/// <summary>
		/// Initialize or disable male hands from anchoring to the controllers, depends on the passed parameter
		/// </summary>
		/// <param name="enable">To enable or disable the functionality</param>
		internal void SyncMaleHandsToggle(bool enable, Side side)
		{
			if (hFlag.mode <= HFlag.EMode.aibu || (hFlag.mode >= HFlag.EMode.masturbation && hFlag.mode <= HFlag.EMode.lesbian))
				return;

			LimbName limb = side == Side.Left ? LimbName.MaleLeftHand : LimbName.MaleRightHand;

			if (enable)
			{
				if (!limbs[(int)limb].AnchorObj)
					FixLimbToggle(limbs[(int)limb]);

				limbs[(int)limb].AnchorObj.transform.parent = controllers[side].transform;

				//Reposition anchor to align the male hand model to the controller
				limbs[(int)limb].AnchorObj.transform.localPosition = new Vector3(0, 0, -0.1f);
				limbs[(int)limb].AnchorObj.transform.localRotation = Quaternion.Euler(-90f, side == Side.Left ? 90f : -90f, 0f) * Quaternion.Euler(0, side == Side.Left ? -30f : 30f, 0f);

				//Hide controller hand model
				foreach (SkinnedMeshRenderer mesh in controllers[side].transform.GetComponentsInChildren<SkinnedMeshRenderer>(true))
					mesh.enabled = false;

				//Restore male shoulder parameters to default as shoulder fixing will be disabled when hands are anchored to the controllers
				limbs[(int)limb].ParentJointBone.bone = null;
				limbs[(int)limb].ParentJointEffector.positionWeight = 0f;

				//Enable collider for the hand that is being synced, no reason to hide it as it is now visible
				if (SetControllerCollider.Value)
					controllers[side].transform.Find("ControllerCollider").GetComponent<SphereCollider>().enabled = true;
			}
			else
			{
				if (limbs[(int)limb].AnchorObj)
					FixLimbToggle(limbs[(int)limb]);

				if (GropeHandsDisplay.Value < HideHandMode.AlwaysShow)
					itemHands[(int)limb - 4].enabled = true;

				foreach (SkinnedMeshRenderer mesh in controllers[side].transform.GetComponentsInChildren<SkinnedMeshRenderer>(true))
					mesh.enabled = true;

				//Disable the collider if the controller model is currently hidden
				if (SetControllerCollider.Value && controllers[side].transform.Find("Model").gameObject.activeSelf == false)
					controllers[side].transform.Find("ControllerCollider").GetComponent<SphereCollider>().enabled = false;
			}
		}
	}
}
