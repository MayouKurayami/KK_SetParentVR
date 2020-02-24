﻿using IllusionPlugin;
using System.Collections;
using UnityEngine;

namespace SetParent
{
	public class AnimSpeedController : MonoBehaviour
	{
		public void SetController(GameObject leftcon, GameObject rightcon)
		{
			parentController = leftcon;
			subController = rightcon;
		}

		private void Start()
		{
			animObject = GameObject.Find("chaF_001/BodyTop/p_cf_body_bone");
			if (animObject == null)
			{
				return;
			}
			anim = animObject.GetComponent<Animator>();
			orgSpeed = anim.speed;
			hFlag = GameObject.Find("VRHScene").GetComponent<HFlag>();
			Transform transform = GameObject.Find("VRTK/[VRTK_SDKManager]/SDKSetups/SteamVR/VRCameraBase/[CameraRig]/Controller (left)/Model/p_handL").transform.Find("HSceneMainCanvas");
			if (transform == null)
			{
				transform = GameObject.Find("VRTK/[VRTK_SDKManager]/SDKSetups/SteamVR/VRCameraBase").transform.Find("[CameraRig]").Find("HSceneMainCanvas");
			}
			hSprite = transform.Find("MainCanvas").GetComponent<HSprite>();
			base.StartCoroutine("GuageControll");
			LoadFromModPref();
			female = GameObject.Find("chaF_001");
			spine01 = GameObject.Find("chaF_001/BodyTop/p_cf_body_bone/cf_j_root/cf_n_height/cf_j_hips/cf_j_spine01");
			nameAnimation = hFlag.nowAnimationInfo.nameAnimation;
			parentConVecBefore = (parentConVecNow = parentController.transform.position);
			subConVecBefore = (subConVecNow = subController.transform.position);
			weakMotion = true;
			weakMotionCount = 1.5f;
		}

		private void LoadFromModPref()
		{
			threshold1 = ModPrefs.GetFloat("SetParent", "threshold1", 0.05f, true);
			threshold2 = ModPrefs.GetFloat("SetParent", "threshold2", 0.3f, true);
			moveDistancePoolSize = ModPrefs.GetInt("SetParent", "moveDistancePoolSize", 100, true);
			moveDistance = new float[moveDistancePoolSize];
			calcPattern = ModPrefs.GetInt("SetParent", "calcPattern", 0, true);
			finishCount = ModPrefs.GetFloat("SetParent", "finishcount", 5f, true);
			moveCoordinatePoolSize = ModPrefs.GetInt("SetParent", "moveCoordinatePoolSize", 100, true);
			moveCoordinate = new Vector3[moveCoordinatePoolSize];
			strongMotionThreshold = ModPrefs.GetFloat("SetParent", "strongMotionThreshold", 0.06f, true);
			weakMotionThreshold = ModPrefs.GetFloat("SetParent", "weakMotionThreshold", 0.01f, true);
			sMThreshold2Ratio = ModPrefs.GetFloat("SetParent", "sMThreshold2Ratio", 1.3f, true);
		}

		private void Update()
		{
			if (animObject == null)
			{
				return;
			}
			if (nameAnimation != hFlag.nowAnimationInfo.nameAnimation)
			{
				UnityEngine.Object.DestroyImmediate(this);
				return;
			}
			if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl))
			{
				LoadFromModPref();
			}
			parentConVecNow = parentController.transform.position;
			subConVecNow = subController.transform.position;
			switch (calcPattern)
			{
				case 0:
					SaveMoveDistance(Mathf.Clamp((parentConVecNow - parentConVecBefore).magnitude, 0f, 0.005f));
					break;
				case 1:
					SaveMoveDistance(Mathf.Clamp((subConVecNow - subConVecBefore).magnitude, 0f, 0.005f));
					break;
				case 2:
					SaveMoveDistance(Mathf.Clamp((parentConVecNow - parentConVecBefore).magnitude + (subConVecNow - subConVecBefore).magnitude, 0f, 0.005f));
					break;
				default:
					SaveMoveDistance(Mathf.Clamp((parentConVecNow - parentConVecBefore).magnitude, 0f, 0.005f));
					break;
			}
			diffSum = LoadMoveDistance();
			SaveMoveCoordinate(parentConVecNow);

			bool piston = false;
			if (hFlag.nowAnimStateName == "SLoop" || hFlag.nowAnimStateName == "A_SLoop")
			{
				weakMotion = false;
				piston = true;
			}
			else if (hFlag.nowAnimStateName == "WLoop" || hFlag.nowAnimStateName == "A_WLoop")
			{
				weakMotion = true;
				piston = true;
			}
			if (piston)
			{
				if (diffSum < threshold1)
				{
					if (moveFlag)
					{
						stopCount += Time.deltaTime;
						if (stopCount >= 2f)
						{
							hFlag.speedCalc = 0f;
							stopCount = 0f;
							moveFlag = false;
						}
						else
						{
							hFlag.speedCalc = 0.1f;
						}
					}
				}
				else if (threshold1 <= diffSum)
				{
					moveFlag = true;
					stopCount = 0f;
					if (weakMotion)
					{
						if ((parentConVecNow - CalcAvgCoordinate()).magnitude >= strongMotionThreshold)
						{
							hFlag.click = HFlag.ClickKind.motionchange;
							weakMotion = false;
							weakMotionCount = 1.5f;
						}
					}
					else if ((parentConVecNow - CalcAvgCoordinate()).magnitude <= weakMotionThreshold)
					{
						weakMotionCount -= Time.deltaTime;
						if (weakMotionCount < 0f)
						{
							hFlag.click = HFlag.ClickKind.motionchange;
							weakMotion = true;
						}
					}
					else
					{
						weakMotionCount = 1.5f;
					}
					if (weakMotion)
					{
						hFlag.speedCalc = Mathf.Clamp((diffSum - threshold1) / (threshold2 - threshold1), 0.1f, 1f);
					}
					else
					{
						hFlag.speedCalc = Mathf.Clamp((diffSum - threshold1) / (threshold2 * sMThreshold2Ratio - threshold1), 0.1f, 1f);
					}
					if (hFlag.gaugeFemale > 70f)
					{
						if (hFlag.speedCalc >= 1f)
						{
							fcount += Time.deltaTime;
						}
						else
						{
							fcount = 0f;
						}
					}
					else
					{
						fcount = 0f;
					}
					if (finishCount > 0 && fcount >= finishCount)
					{
						hSprite.OnInsideClick();
						fcount = 0f;
						moveFlag = false;
					}
				}
			}
			else if (hFlag.nowAnimStateName == "InsertIdle" || hFlag.nowAnimStateName == "A_InsertIdle" || hFlag.nowAnimStateName == "IN_A" || hFlag.nowAnimStateName == "A_IN_A")
			{
				moveFlag = false;
				if (threshold1 <= diffSum)
				{
					hFlag.speedCalc = 0.1f;
					hFlag.click = HFlag.ClickKind.speedup;
					weakMotion = true;
				}
			}
			parentConVecBefore = parentConVecNow;
			subConVecBefore = subConVecNow;
		}

		private void OnDestroy()
		{
			base.StopCoroutine("GuageControll");
		}

		private void SaveMoveDistance(float dis)
		{
			if (moveDistanceIndex >= moveDistancePoolSize - 1)
			{
				moveDistanceIndex = 0;
			}
			else
			{
				moveDistanceIndex++;
			}
			moveDistance[moveDistanceIndex] = dis;
		}

		private void SaveMoveCoordinate(Vector3 vec)
		{
			if (moveCoordinateIndex >= moveCoordinatePoolSize - 1)
			{
				moveCoordinateIndex = 0;
			}
			else
			{
				moveCoordinateIndex++;
			}
			moveCoordinate[moveCoordinateIndex] = vec;
		}

		private Vector3 CalcAvgCoordinate()
		{
			Vector3 a = Vector3.zero;
			for (int i = 0; i < moveCoordinatePoolSize; i++)
			{
				a += moveCoordinate[i];
			}
			return a / moveCoordinatePoolSize;
		}

		private float LoadMoveDistance()
		{
			float num = 0f;
			foreach (float num2 in moveDistance)
			{
				num += num2;
			}
			return num;
		}

		private float LoadRecentMoveDistance(int num)
		{
			float num2 = 0f;
			int num3 = moveDistanceIndex;
			for (int i = 0; i < num; i++)
			{
				num2 += moveDistance[num3];
				if (num3 <= 0)
				{
					num3 = moveDistanceIndex - 1;
				}
				else
				{
					num3--;
				}
			}
			return num2;
		}

		private IEnumerator GuageControll()
		{
			for (; ; )
			{
				yield return new WaitForSeconds(1f);
				if (moveFlag)
				{
					if (hFlag.gaugeFemale < 99f && !hFlag.lockGugeFemale)
					{
						hFlag.gaugeFemale += 1f;
					}
					if (hFlag.gaugeMale < 99f && !hFlag.lockGugeMale)
					{
						hFlag.gaugeMale += 1f;
					}
				}
				else
				{
					if (hFlag.gaugeFemale > 5f && !hFlag.lockGugeFemale)
					{
						hFlag.gaugeFemale -= 2f;
					}
					if (hFlag.gaugeMale > 5f && !hFlag.lockGugeMale)
					{
						hFlag.gaugeMale -= 2f;
					}
				}			
			}
		}

		private GameObject animObject;

		private Animator anim;

		private float orgSpeed;

		private GameObject parentController;

		private GameObject subController;

		private Vector3 parentConVecBefore;

		private Vector3 parentConVecNow;

		private Vector3 subConVecBefore;

		private Vector3 subConVecNow;

		private HFlag hFlag;

		private float diffSum;

		private HSprite hSprite;

		private float threshold1;

		private float threshold2;

		private float[] moveDistance;

		private int moveDistanceIndex;

		private int moveDistancePoolSize;

		public bool moveFlag;

		private Vector3 before = Vector3.zero;

		private Vector3 now = Vector3.zero;

		private GameObject female;

		private GameObject spine01;

		private string nameAnimation = "";

		private int calcPattern;

		private float finishCount;

		public float fcount;

		private Vector3[] moveCoordinate;

		private int moveCoordinateIndex;

		private int moveCoordinatePoolSize;

		private float strongMotionThreshold;

		private float weakMotionThreshold;

		public bool weakMotion;

		private float weakMotionCount;

		private float sMThreshold2Ratio;

		private float stopCount;
	}
}
