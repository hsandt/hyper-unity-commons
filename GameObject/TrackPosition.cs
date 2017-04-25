using UnityEngine;
using System.Collections;

/// Keep a transform at a target position
public class TrackPosition : MonoBehaviour {

	public Transform target;

	[SerializeField] Vector3 offset = Vector3.zero;

	void FixedUpdate () {
		transform.position = target.position + offset;
	}

}
