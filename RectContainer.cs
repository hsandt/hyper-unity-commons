using UnityEngine;
using System.Collections;

public class RectContainer : MonoBehaviour {

	/// Actual rect data in local coordinates
	/// You may initialize the coords with other values in your MonoBehaviour's Reset
	public Rect rect = new Rect(-0.5f, -0.5f, 1f, 1f);

	// property for world coordinates (if there is some rotation, the world rect may not be an AABB and the coordinates may be irrelevant; rotation by a multiple of 90 degrees is fine)
	public Vector2 worldMin { get { return transform.TransformPoint(rect.min); } set { rect.min = transform.InverseTransformPoint(value); } }
	public Vector2 worldMax { get { return transform.TransformPoint(rect.max); } set { rect.max = transform.InverseTransformPoint(value); } }
	public Rect worldRect {
		get {
			Vector2 wMin = worldMin;
			Vector2 wMax = worldMax;
			return Rect.MinMaxRect(wMin.x, wMin.y, wMax.x, wMax.y);
		}
	}
	

	#if UNITY_EDITOR

	/// Should the rect be visible even when the game object is not selected?
	public bool alwaysShowRect = false;

	/// Color used to draw the rect
	public Color drawColor = Color.white;

	void OnDrawGizmos () {
		if (alwaysShowRect) {
			GizmosUtil.DrawRect(rect, transform, drawColor);
		}
	}

	#endif
}
