using UnityEngine;
using System.Collections;

public class RectContainer : MonoBehaviour {

	#if UNITY_EDITOR
	/// Color used to draw the rect
	public Color drawColor = Color.white;
	#endif

	/// Actual rect data (initialize with other values in your MonoBehaviour's Reset)
	public Rect rect = new Rect(-0.5f, -0.5f, 1f, 1f);

	public Vector2 min { get { return transform.TransformPoint(rect.min); } set { rect.min = transform.InverseTransformPoint(value); } }
	public Vector2 max { get { return transform.TransformPoint(rect.max); } set { rect.max = transform.InverseTransformPoint(value); } }

}
