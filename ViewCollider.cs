// http://adamboro.com/blog/unity/always-show-collider-in-unity/
// improved by hsandt

using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class ViewCollider : MonoBehaviour {

	// The Collider itself
	private EdgeCollider2D thisCollider;
	// array of collider points
	private Vector2[] points;
	// the transform of the collider
	private Transform _tr;

	void Start () {
		thisCollider = GetComponent ("EdgeCollider2D") as EdgeCollider2D;
		points = thisCollider.points;
		_tr = thisCollider.transform;
	}

	void OnDrawGizmos() {
		Gizmos.color = Color.blue;
		// for every point (except for the last one), draw line to the next point
		for(int i = 0; i < points.Length-1; i++)
		{
			// we use local scale, assuming parents are not scaled; lossy scale is fine to handle parent scale if no rotations
			// if some rotations, need rotation matrix too; not sure how to handle rotation + scale, it depends how Unity handles skew for 2D colliders
			Gizmos.DrawLine(new Vector3(_tr.position.x + points[i].x * _tr.localScale.x, _tr.position.y + points[i].y * _tr.localScale.y),
				new Vector3(_tr.position.x + points[i+1].x * _tr.localScale.x, _tr.position.y + points[i+1].y * _tr.localScale.y));
		}
	}

}
