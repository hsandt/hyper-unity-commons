using UnityEngine;
using System.Collections;

public static class Physics2DUtil {

	/// Raycast and draw ray for debug
	public static RaycastHit2D RaycastDebug (Vector2 origin, Vector2 direction, float distance = Mathf.Infinity, int layerMask = Physics2D.DefaultRaycastLayers, float minDepth = -Mathf.Infinity, float maxDepth = Mathf.Infinity, Color? color = null) {
		Debug.DrawRay(origin, direction * distance, color ?? Color.red);
		return Physics2D.Raycast(origin, direction, distance, layerMask, minDepth, maxDepth);
	}

}
