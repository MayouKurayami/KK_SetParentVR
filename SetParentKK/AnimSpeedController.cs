using System;
using System.Collections;
using IllusionPlugin;
using UnityEngine;

namespace SetParent
{
	public class AnimSpeedController : MonoBehaviour
	{
		public void SetController(GameObject leftcon, GameObject rightcon)
		{
			this.leftController = leftcon;
			this.rightController = rightcon;
		}

		private void Start()
		{
			this.animObject = GameObject.Find("chaF_001/BodyTop/p_cf_body_bone");
			if (this.animObject == null)
			{
				return;
			}
			this.anim = this.animObject.GetComponent<Animator>();
			this.orgSpeed = this.anim.speed;
			this.hFlag = GameObject.Find("VRHScene").GetComponent<HFlag>();
			Transform transform = GameObject.Find("VRTK/[VRTK_SDKManager]/SDKSetups/SteamVR/VRCameraBase/[CameraRig]/Controller (left)/Model/p_handL").transform.Find("HSceneMainCanvas");
			if (transform == null)
			{
				transform = GameObject.Find("VRTK/[VRTK_SDKManager]/SDKSetups/SteamVR/VRCameraBase").transform.Find("[CameraRig]").Find("HSceneMainCanvas");
			}
			this.hSprite = transform.Find("MainCanvas").GetComponent<HSprite>();
			base.StartCoroutine("GuageControll");
			this.LoadFromModPref();
			this.female = GameObject.Find("chaF_001");
			this.spine01 = GameObject.Find("chaF_001/BodyTop/p_cf_body_bone/cf_j_root/cf_n_height/cf_j_hips/cf_j_spine01");
			this.nameAnimation = this.hFlag.nowAnimationInfo.nameAnimation;
			this.leftConVecBefore = (this.leftConVecNow = this.leftController.transform.position);
			this.rightConVecBefore = (this.rightConVecNow = this.rightController.transform.position);
			this.guagef = this.hFlag.lockGugeFemale;
			this.guagem = this.hFlag.lockGugeMale;
			this.hFlag.lockGugeFemale = true;
			this.hFlag.lockGugeMale = true;
			this.weakMotion = true;
		}

		private void LoadFromModPref()
		{
			this.threshold1 = ModPrefs.GetFloat("SetParent", "threshold1", 0.05f, true);
			this.threshold2 = ModPrefs.GetFloat("SetParent", "threshold2", 0.3f, true);
			this.moveDistancePoolSize = ModPrefs.GetInt("SetParent", "moveDistancePoolSize", 100, true);
			this.moveDistance = new float[this.moveDistancePoolSize];
			this.calcPattern = ModPrefs.GetInt("SetParent", "calcPattern", 0, true);
			this.finishCount = ModPrefs.GetFloat("SetParent", "finishcount", 5f, true);
			this.moveCoordinatePoolSize = ModPrefs.GetInt("SetParent", "moveCoordinatePoolSize", 100, true);
			this.moveCoordinate = new Vector3[this.moveCoordinatePoolSize];
			this.strongMotionThreshold = ModPrefs.GetFloat("SetParent", "strongMotionThreshold", 0.06f, true);
			this.weakMotionThreshold = ModPrefs.GetFloat("SetParent", "weakMotionThreshold", 0.01f, true);
			this.sMThreshold2Ratio = ModPrefs.GetFloat("SetParent", "sMThreshold2Ratio", 1.3f, true);
		}

		private void Update()
		{
			if (this.animObject == null)
			{
				return;
			}
			if (this.nameAnimation != this.hFlag.nowAnimationInfo.nameAnimation)
			{
				UnityEngine.Object.DestroyImmediate(this);
				return;
			}
			if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl))
			{
				this.LoadFromModPref();
			}
			this.leftConVecNow = this.leftController.transform.position;
			this.rightConVecNow = this.rightController.transform.position;
			switch (this.calcPattern)
			{
			case 0:
				this.SaveMoveDistance(Mathf.Clamp((this.leftConVecNow - this.leftConVecBefore).magnitude, 0f, 0.005f));
				break;
			case 1:
				this.SaveMoveDistance(Mathf.Clamp((this.rightConVecNow - this.rightConVecBefore).magnitude, 0f, 0.005f));
				break;
			case 2:
				this.SaveMoveDistance(Mathf.Clamp((this.leftConVecNow - this.leftConVecBefore).magnitude + (this.rightConVecNow - this.rightConVecBefore).magnitude, 0f, 0.005f));
				break;
			default:
				this.SaveMoveDistance(Mathf.Clamp((this.leftConVecNow - this.leftConVecBefore).magnitude, 0f, 0.005f));
				break;
			}
			this.diffSum = this.LoadMoveDistance();
			this.SaveMoveCoordinate(this.leftConVecNow);
			if (this.hFlag.nowAnimStateName == "WLoop" || this.hFlag.nowAnimStateName == "SLoop" || this.hFlag.nowAnimStateName == "A_WLoop" || this.hFlag.nowAnimStateName == "A_SLoop")
			{
				if (this.diffSum < this.threshold1)
				{
					if (this.moveFlag)
					{
						this.stopCount += Time.deltaTime;
						if (this.stopCount >= 2f)
						{
							this.hFlag.speedCalc = 0f;
							this.stopCount = 0f;
							this.moveFlag = false;
						}
						else
						{
							this.hFlag.speedCalc = 0.1f;
						}
					}
				}
				else if (this.threshold1 <= this.diffSum)
				{
					this.moveFlag = true;
					this.stopCount = 0f;
					if (this.weakMotion)
					{
						if ((this.leftConVecNow - this.CalcAvgCoordinate()).magnitude >= this.strongMotionThreshold)
						{
							this.hFlag.click = HFlag.ClickKind.motionchange;
							this.weakMotion = false;
							this.weakMotionCount = 2f;
						}
					}
					else if ((this.leftConVecNow - this.CalcAvgCoordinate()).magnitude <= this.weakMotionThreshold)
					{
						this.weakMotionCount -= Time.deltaTime;
						if (this.weakMotionCount < 0f)
						{
							this.hFlag.click = HFlag.ClickKind.motionchange;
							this.weakMotion = true;
						}
					}
					else
					{
						this.weakMotionCount = 2f;
					}
					if (this.weakMotion)
					{
						this.hFlag.speedCalc = Mathf.Clamp((this.diffSum - this.threshold1) / (this.threshold2 - this.threshold1), 0.1f, 1f);
					}
					else
					{
						this.hFlag.speedCalc = Mathf.Clamp((this.diffSum - this.threshold1) / (this.threshold2 * this.sMThreshold2Ratio - this.threshold1), 0.1f, 1f);
					}
					if (this.hFlag.gaugeFemale > 70f)
					{
						if (this.hFlag.speedCalc >= 1f)
						{
							this.fcount += Time.deltaTime;
						}
						else
						{
							this.fcount = 0f;
						}
					}
					else
					{
						this.fcount = 0f;
					}
					if (this.fcount >= this.finishCount)
					{
						this.hSprite.OnInsideClick();
						this.fcount = 0f;
						this.moveFlag = false;
					}
				}
			}
			else if (this.hFlag.nowAnimStateName == "InsertIdle" || this.hFlag.nowAnimStateName == "A_InsertIdle" || this.hFlag.nowAnimStateName == "IN_A" || this.hFlag.nowAnimStateName == "A_IN_A")
			{
				this.moveFlag = false;
				if (this.threshold1 <= this.diffSum)
				{
					this.hFlag.click = HFlag.ClickKind.speedup;
					this.weakMotion = true;
				}
			}
			this.leftConVecBefore = this.leftConVecNow;
			this.rightConVecBefore = this.rightConVecNow;
		}

		private void OnDestroy()
		{
			this.hFlag.lockGugeFemale = this.guagef;
			this.hFlag.lockGugeMale = this.guagem;
			base.StopCoroutine("GuageControll");
		}

		private void SaveMoveDistance(float dis)
		{
			if (this.moveDistanceIndex >= this.moveDistancePoolSize - 1)
			{
				this.moveDistanceIndex = 0;
			}
			else
			{
				this.moveDistanceIndex++;
			}
			this.moveDistance[this.moveDistanceIndex] = dis;
		}

		private void SaveMoveCoordinate(Vector3 vec)
		{
			if (this.moveCoordinateIndex >= this.moveCoordinatePoolSize - 1)
			{
				this.moveCoordinateIndex = 0;
			}
			else
			{
				this.moveCoordinateIndex++;
			}
			this.moveCoordinate[this.moveCoordinateIndex] = vec;
		}

		private Vector3 CalcAvgCoordinate()
		{
			Vector3 a = Vector3.zero;
			for (int i = 0; i < this.moveCoordinatePoolSize; i++)
			{
				a += this.moveCoordinate[i];
			}
			return a / (float)this.moveCoordinatePoolSize;
		}

		private float LoadMoveDistance()
		{
			float num = 0f;
			foreach (float num2 in this.moveDistance)
			{
				num += num2;
			}
			return num;
		}

		private float LoadRecentMoveDistance(int num)
		{
			float num2 = 0f;
			int num3 = this.moveDistanceIndex;
			for (int i = 0; i < num; i++)
			{
				num2 += this.moveDistance[num3];
				if (num3 <= 0)
				{
					num3 = this.moveDistanceIndex - 1;
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
			for (;;)
			{
				yield return new WaitForSeconds(1f);
				if (this.moveFlag)
				{
					if (this.hFlag.gaugeFemale < 99f)
					{
						this.hFlag.gaugeFemale += 1f;
					}
					if (this.hFlag.gaugeMale < 99f)
					{
						this.hFlag.gaugeMale += 1f;
					}
				}
				else
				{
					if (this.hFlag.gaugeFemale > 5f)
					{
						this.hFlag.gaugeFemale -= 2f;
					}
					if (this.hFlag.gaugeMale > 5f)
					{
						this.hFlag.gaugeMale -= 2f;
					}
				}
			}
			yield break;
		}

		private GameObject animObject;

		private Animator anim;

		private float orgSpeed;

		private GameObject leftController;

		private GameObject rightController;

		private Vector3 leftConVecBefore;

		private Vector3 leftConVecNow;

		private Vector3 rightConVecBefore;

		private Vector3 rightConVecNow;

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

		private bool guagef;

		private bool guagem;
	}
}
