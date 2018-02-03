using UnityEngine;
using System.Collections;

public class DeactivateOnStart : MonoBehaviour {

	void Awake () {
		gameObject.SetActive(false);
	}

}
