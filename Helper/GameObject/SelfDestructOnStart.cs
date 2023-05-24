using UnityEngine;
using System.Collections;

namespace CommonsHelper
{

	public class SelfDestructOnStart : MonoBehaviour {

		void Start () {
			Destroy(gameObject);
		}

	}

}
