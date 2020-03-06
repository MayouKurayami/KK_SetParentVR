using UnityEngine;
using static SetParentKK.SetParent;

namespace SetParentKK
{
	public class FixBodyParts : MonoBehaviour
	{
		public void Init(SetParent sp, LimbName p)
		{
			setParentObj = sp;
			limbName = p;
		}

		private void Start()
		{
			rigibody = base.gameObject.AddComponent<Rigidbody>();
			sphereCollider = base.gameObject.AddComponent<SphereCollider>();
			rigibody.isKinematic = true;
			sphereCollider.radius = 0.05f;
			sphereCollider.isTrigger = true;
		}

		private void OnTriggerEnter(Collider other)
		{
			if ((!SetParentLoader.SetFemaleCollider.Value && !SetParentLoader.SetMaleCollider.Value) || !setParentObj.setFlag)
			{
				return;
			}
			if (other.gameObject.name != "SPCollider")
			{
				return;
			}

			if (!setParentObj.limbs[(int)limbName].AnchorObj)
			{
				setParentObj.FixLimbToggle(setParentObj.limbs[(int)limbName]);
				setParentObj.limbs[(int)limbName].AnchorObj.transform.parent = other.transform;
			}
		}

		private SetParent setParentObj;

		private LimbName limbName;

		private Rigidbody rigibody;

		private SphereCollider sphereCollider;
	}
}
