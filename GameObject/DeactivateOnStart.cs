using UnityEngine;
using System.Collections;

namespace Commons.Helper
{

	public class DeactivateOnStart : MonoBehaviour {

		void Awake () {
			gameObject.SetActive(false);
		}

	}

}
