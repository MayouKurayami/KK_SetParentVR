using System;
using UnityEngine;

namespace SetParent
{
	public class FixBodyParts : MonoBehaviour
	{
		public void Init(SetParent sp, FixBodyParts.bodyParts p)
		{
			this.setParent = sp;
			this.parts = p;
		}

		private void Start()
		{
			this.rg = base.gameObject.AddComponent<Rigidbody>();
			this.sc = base.gameObject.AddComponent<SphereCollider>();
			this.rg.isKinematic = true;
			this.sc.radius = 0.05f;
			this.sc.isTrigger = true;
		}

		private void OnTriggerEnter(Collider other)
		{
			if (!this.setParent.setCollider)
			{
				return;
			}
			if (other.gameObject.name != "SPCollider")
			{
				return;
			}
			switch (this.parts)
			{
			case FixBodyParts.bodyParts.hand_L:
				if (this.setParent.objLeftHand == null)
				{
					this.setParent.PushFixLeftHandButton();
					return;
				}
				break;
			case FixBodyParts.bodyParts.hand_R:
				if (this.setParent.objRightHand == null)
				{
					this.setParent.PushFixRightHandButton();
					return;
				}
				break;
			case FixBodyParts.bodyParts.leg_L:
				if (this.setParent.objLeftLeg == null)
				{
					this.setParent.PushFixLeftLegButton();
					return;
				}
				break;
			case FixBodyParts.bodyParts.leg_R:
				if (this.setParent.objRightLeg == null)
				{
					this.setParent.PushFixRightLegButton();
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
