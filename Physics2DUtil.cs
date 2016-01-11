using UnityEngine;
using System.Collections;

public static class Physics2DUtil {

	/// Raycast and draw ray for debug
	public static RaycastHit2D RaycastDebug (Vector2 origin, Vector2 direction, float distance = Mathf.Infinity, int layerMask = Physics2D.DefaultRaycastLayers, float minDepth = -Mathf.Infinity, float maxDepth = Mathf.Infinity, Color? color = null) {
		Debug.DrawRay(origin, direction * distance, color ?? Color.red);
		return Physics2D.Raycast(origin, direction, distance, layerMask, minDepth, maxDepth);
	}

	/// Check overlapping collider and draw area bounds for debug
	public static Collider2D OverlapAreaDebug(Vector2 pointA, Vector2 pointB, int layerMask = Physics2D.DefaultRaycastLayers, float minDepth = -Mathf.Infinity, float maxDepth = Mathf.Infinity, Color? color = null, float duration = 0f) {
		Bounds debugBounds = new Bounds();
		debugBounds.min = (Vector3) pointA;
		debugBounds.max = (Vector3) pointB;
		DebugExtension.DebugBounds(debugBounds, color ?? Color.red, duration, depthTest: false);
		return Physics2D.OverlapArea(pointA, pointB, layerMask, minDepth, maxDepth);
	}

	/// Check overlapping colliders and draw area bounds for debug
	public static int OverlapAreaNonAllocDebug(Vector2 pointA, Vector2 pointB, Collider2D[] results, int layerMask = Physics2D.DefaultRaycastLayers, float minDepth = -Mathf.Infinity, float maxDepth = Mathf.Infinity, Color? color = null, float duration = 0f) {
		Bounds debugBounds = new Bounds();
		debugBounds.min = (Vector3) pointA;
		debugBounds.max = (Vector3) pointB;
		DebugExtension.DebugBounds(debugBounds, color ?? Color.red, duration, depthTest: false);
		return Physics2D.OverlapAreaNonAlloc(pointA, pointB, results, layerMask, minDepth, maxDepth);
	}

}
