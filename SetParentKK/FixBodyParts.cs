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
			if (!setParentObj.limbAutoAttach)
				return;
			if (other.gameObject.name != "SPCollider" && other.gameObject.name != "ControllerCollider")
				return;
			if (setParentObj.limbs[(int)limbName].AnchorObj && setParentObj.limbs[(int)limbName].AnchorObj.transform.parent == other.transform)
				return;

			if (other.gameObject.name == "ControllerCollider" || setParentObj.limbs[(int)limbName].Fixed)
			{
				if (setParentObj.limbs[(int)limbName].AnchorObj)
				{
					UnityEngine.Object.Destroy(setParentObj.limbs[(int)limbName].AnchorObj);
					setParentObj.limbs[(int)limbName].AnchorObj = null;
				}
				setParentObj.FixLimbToggle(setParentObj.limbs[(int)limbName], true);
				setParentObj.limbs[(int)limbName].AnchorObj.transform.parent = other.transform;
			}
			else if (setParentObj.setFlag && !setParentObj.limbs[(int)limbName].AnchorObj)
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
