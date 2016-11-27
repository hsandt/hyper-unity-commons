using UnityEngine;
using System.Collections;

public class SelfDestructOnStart : MonoBehaviour {

	void Start () {
		Destroy(gameObject);
	}

}
