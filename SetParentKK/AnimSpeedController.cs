using System.Collections;
using UnityEngine;

namespace SetParentKK
{
	public class AnimSpeedController : MonoBehaviour
	{
		public void SetController(GameObject parentCon, GameObject subCon, SetParent setParent)
		{
			parentController = parentCon;
			subController = subCon;
			setParentObj = setParent;
		}

		private void Start()
		{
			animObject = setParentObj.female_p_cf_bodybone;
			if (animObject == null)
			{
				return;
			}
			hFlag = setParentObj.hSprite.flags;
			hSprite = setParentObj.hSprite;
			LoadFromLoader();
			nameAnimation = hFlag.nowAnimationInfo.nameAnimation;
			parentConVecBefore = (parentConVecNow = parentController.transform.position);
			subConVecBefore = (subConVecNow = subController.transform.position);
			weakMotion = true;
			weakMotionCount = 1.5f;
		}

		private void LoadFromLoader()
		{
			animMinThreshold = KK_SetParentVR.AnimStartThreshold.Value;
			animMaxThreshold = KK_SetParentVR.AnimMaxThreshold.Value;
			moveDistancePoolSize = KK_SetParentVR.MoveDistancePoolSize.Value;
			moveDistance = new float[moveDistancePoolSize];
			calcPattern = (int)KK_SetParentVR.CalcController.Value;
			finishCount = KK_SetParentVR.Finishcount.Value;
			moveCoordinatePoolSize = KK_SetParentVR.MoveCoordinatePoolSize.Value;
			moveCoordinate = new Vector3[moveCoordinatePoolSize];
			strongMotionThreshold = KK_SetParentVR.StrongMotionThreshold.Value;
			weakMotionThreshold = KK_SetParentVR.WeakMotionThreshold.Value;
			strongMotionMultiplier = KK_SetParentVR.StrongThresholdMultiplier.Value;
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
				if (diffSum < animMinThreshold)
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
				else if (animMinThreshold <= diffSum)
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
						hFlag.speedCalc = Mathf.Clamp((diffSum - animMinThreshold) / (animMaxThreshold - animMinThreshold), 0.1f, 1f);
					}
					else
					{
						hFlag.speedCalc = Mathf.Clamp((diffSum - animMinThreshold) / (animMaxThreshold * strongMotionMultiplier - animMinThreshold), 0.1f, 1f);
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
				if (animMinThreshold <= diffSum)
				{
					hFlag.speedCalc = 0.1f;
					hFlag.click = HFlag.ClickKind.speedup;
					weakMotion = true;
				}
			}
			parentConVecBefore = parentConVecNow;
			subConVecBefore = subConVecNow;
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


		private SetParent setParentObj;
		
		private GameObject animObject;

		private GameObject parentController;

		private GameObject subController;

		private Vector3 parentConVecBefore;

		private Vector3 parentConVecNow;

		private Vector3 subConVecBefore;

		private Vector3 subConVecNow;

		private HFlag hFlag;

		private float diffSum;

		private HSprite hSprite;

		private float animMinThreshold;

		private float animMaxThreshold;

		private float[] moveDistance;

		private int moveDistanceIndex;

		private int moveDistancePoolSize;

		public bool moveFlag;

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

		private float strongMotionMultiplier;

		private float stopCount;
	}
}
