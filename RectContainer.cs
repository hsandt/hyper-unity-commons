using UnityEngine;
using System.Collections;

public class RectContainer : MonoBehaviour {

	public Rect rect = new Rect(-0.5f, -0.5f, 1f, 1f);

	public Vector2 min { get { return transform.TransformPoint(rect.min); } }
	public Vector2 max { get { return transform.TransformPoint(rect.max); } }

}
