using UnityEngine;

namespace SetParentKK
{
	public class FixBodyParts : MonoBehaviour
	{
		public void Init(SetParent sp, BodyParts p)
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
			if ((!SetParentLoader.SetFemaleCollider.Value && !SetParentLoader.SetMaleCollider.Value) || !setParent.setFlag)
			{
				return;
			}
			if (other.gameObject.name != "SPCollider")
			{
				return;
			}
			switch (parts)
			{
				case BodyParts.hand_L:
					if (setParent.objLeftHand == null)
					{
						setParent.PushFixLeftHandButton();
						setParent.objLeftHand.transform.parent = other.transform;
						return;
					}
					break;
				case BodyParts.hand_R:
					if (setParent.objRightHand == null)
					{
						setParent.PushFixRightHandButton();
						setParent.objRightHand.transform.parent = other.transform;
						return;
					}
					break;
				case BodyParts.leg_L:
					if (setParent.objLeftLeg == null)
					{
						setParent.PushFixLeftLegButton();
						setParent.objLeftLeg.transform.parent = other.transform;
						return;
					}
					break;
				case BodyParts.leg_R:
					if (setParent.objRightLeg == null)
					{
						setParent.PushFixRightLegButton();
						setParent.objRightLeg.transform.parent = other.transform;
					}
					break;
				case BodyParts.male_ft_L:
					if (setParent.objLeftMaleFoot == null)
					{
						setParent.MaleFixLeftLegToggle();
						setParent.objRightMaleFoot.transform.parent = other.transform;
					}
					break;
				case BodyParts.male_ft_R:
					if (setParent.objRightMaleFoot == null)
					{
						setParent.MaleFixRightLegToggle();
						setParent.objRightMaleFoot.transform.parent = other.transform;
					}
					break;
				default:
					return;
			}
		}

		private SetParent setParent;

		private BodyParts parts;

		private Rigidbody rg;

		private SphereCollider sc;

		public enum BodyParts
		{
			hand_L,
			hand_R,
			leg_L,
			leg_R,
			male_ft_L,
			male_ft_R
		}
	}
}
