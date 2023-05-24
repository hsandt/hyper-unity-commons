using UnityEngine;
using System.Collections;

namespace HyperUnityCommons
{

	public class SelfDestructOnStart : MonoBehaviour {

		void Start () {
			Destroy(gameObject);
		}

	}

}
