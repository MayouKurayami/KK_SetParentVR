using UnityEngine;

namespace SetParentKK
{
	public class FixBodyParts : MonoBehaviour
	{
		public void Init(SetParent sp, FixBodyParts.bodyParts p)
		{
			setParent = sp;
			parts = p;
		}

		private void Start()
		{
			rg = base.gameObject.AddComponent<Rigidbody>();
			sc = base.gameObject.AddComponent<SphereCollider>();
			rg.isKinematic = true;
			sc.radius = 0.05f;
			sc.isTrigger = true;
		}

		private void OnTriggerEnter(Collider other)
		{
			if (!SetParentLoader.SetCollider.Value || !setParent.setFlag)
			{
				return;
			}
			if (other.gameObject.name != "SPCollider")
			{
				return;
			}
			switch (parts)
			{
				case FixBodyParts.bodyParts.hand_L:
					if (setParent.objLeftHand == null)
					{
						setParent.PushFixLeftHandButton();
						return;
					}
					break;
				case FixBodyParts.bodyParts.hand_R:
					if (setParent.objRightHand == null)
					{
						setParent.PushFixRightHandButton();
						return;
					}
					break;
				case FixBodyParts.bodyParts.leg_L:
					if (setParent.objLeftLeg == null)
					{
						setParent.PushFixLeftLegButton();
						return;
					}
					break;
				case FixBodyParts.bodyParts.leg_R:
					if (setParent.objRightLeg == null)
					{
						setParent.PushFixRightLegButton();
					}
					break;
				default:
					return;
			}
		}

		private SetParent setParent;

		private FixBodyParts.bodyParts parts;

		private Rigidbody rg;

		private SphereCollider sc;

		public enum bodyParts
		{
			hand_L,
			hand_R,
			leg_L,
			leg_R
		}
	}
}
