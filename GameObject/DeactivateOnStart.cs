using UnityEngine;
using System.Collections;

namespace CommonsHelper
{

	public class DeactivateOnStart : MonoBehaviour {

		void Awake () {
			gameObject.SetActive(false);
		}

	}

}
