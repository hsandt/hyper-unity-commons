using UnityEngine;
using System.Collections;

namespace Commons.Helper
{

	public class SelfDestructOnStart : MonoBehaviour {

		void Start () {
			Destroy(gameObject);
		}

	}

}
