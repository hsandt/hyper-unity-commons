/* DEPENDENCIES
 *
 * DebugDrawingExtension Unity Asset Store package
 *
 */

using UnityEngine;
using Debug = UnityEngine.Debug;
using System.Collections;
using System.Diagnostics;

public static class Physics2DUtil {

	/// Max distance used to represent an infinite ray
	const float maxDrawLineDistance = 100f;

	static Color noHitColor = Color.green;
	static Color hitColor = Color.red;

	/// Draw a raycast, providing its result hit, with an optional not hit color, a draw duration and a Z depth where the ray should be drawn.
	[Conditional("DEBUG")]
	public static void DrawRaycast (Vector2 origin, Vector2 direction, float distance, RaycastHit2D hit, Color? color = null, float duration = 2f, float z = 0f)
	{
		if (distance == Mathf.Infinity)
			distance = maxDrawLineDistance;

		// By default, draw the no hit part of the ray in green, and the hit part in red
		DebugUtil.DrawLine2D(origin, hit.point, z, color ?? noHitColor, duration, depthTest: false);
		DebugUtil.DrawLine2D(hit.point, origin + direction.normalized * distance, z, hitColor, duration, depthTest: false);
	}

	/// Raycast and draw debug at the same time. Set a color to override the no hit color.
	public static RaycastHit2D RaycastDebug (Vector2 origin, Vector2 direction, float distance = Mathf.Infinity, int layerMask = Physics2D.DefaultRaycastLayers, float minDepth = -Mathf.Infinity, float maxDepth = Mathf.Infinity, Color? color = null, float duration = 2f, float z = 0f) {
		RaycastHit2D hit = Physics2D.Raycast(origin, direction, distance, layerMask, minDepth, maxDepth);
		DrawRaycast(origin, direction, distance, hit, color, duration, z);
		return hit;
	}

	/// Draw a raycast all or non-alloc, providing its result hits array
	[Conditional("DEBUG")]
	public static void DrawRaycastMulti (Vector2 origin, Vector2 direction, float distance, RaycastHit2D[] hits, Color? color = null, float duration = 2f, float z = 0f)
	{
		if (distance == Mathf.Infinity)
			distance = maxDrawLineDistance;

		Vector2 end = origin + direction.normalized * distance;

		if (hits.Length == 0)
		{
			DebugUtil.DrawLine2D(origin, end, z, color ?? noHitColor, duration, depthTest: false);
		}
		else
		{
			// For now, just start drawing red after first hit
			// IMPROVE: show each and every hit with a cross or a circle
			DebugUtil.DrawLine2D(origin, hits[0].point, z, color ?? noHitColor, duration, depthTest: false);
			DebugUtil.DrawLine2D(hits[0].point, end, z, hitColor, duration, depthTest: false);
		}
	}

	/// Raycast all colliders and draw debug at the same time. Set a color to override the no hit color.
	public static RaycastHit2D[] RaycastAllDebug (Vector2 origin, Vector2 direction, float distance = Mathf.Infinity, int layerMask = Physics2D.DefaultRaycastLayers, float minDepth = -Mathf.Infinity, float maxDepth = Mathf.Infinity, Color? color = null, float duration = 2f, float z = 0f)
	{
		RaycastHit2D[] hits = Physics2D.RaycastAll(origin, direction, distance, layerMask, minDepth, maxDepth);
		DrawRaycastMulti(origin, direction, distance, hits, color, duration, z);
		return hits;
	}

	/// Raycast all colliders with no allocation and draw debug at the same time. Set a color to override the no hit color.
	public static void RaycastNonAllocDebug(Vector2 origin, Vector2 direction, RaycastHit2D[] hits, float distance = Mathf.Infinity, int layerMask = Physics2D.DefaultRaycastLayers, float minDepth = -Mathf.Infinity, float maxDepth = Mathf.Infinity, Color? color = null, float duration = 2f, float z = 0f)
	{
		Physics2D.RaycastNonAlloc(origin, direction, hits, distance, layerMask, minDepth, maxDepth);
		DrawRaycastMulti(origin, direction, distance, hits, color, duration, z);
	}

	/// [Unity 5.6 new overload] Raycast all colliders from origin toward direction over distance and store the result in hits. Draw with optional custom color for duration seconds at depth z.
	public static void RaycastDebug(Vector2 origin, Vector2 direction, ContactFilter2D contactFilter, RaycastHit2D[] hits, float distance = Mathf.Infinity, Color? color = null, float duration = 2f, float z = 0f)
	{
		Physics2D.Raycast(origin, direction, contactFilter, hits, distance);
		DrawRaycastMulti(origin, direction, distance, hits, color, duration, z);
	}

	/// Check overlapping collider and draw area bounds for debug
	public static Collider2D OverlapAreaDebug(Vector2 pointA, Vector2 pointB, int layerMask = Physics2D.DefaultRaycastLayers, float minDepth = -Mathf.Infinity, float maxDepth = Mathf.Infinity, Color? color = null, float duration = 0f) {
	#if UNITY_EDITOR
		Bounds debugBounds = new Bounds();
		debugBounds.min = (Vector3) pointA;
		debugBounds.max = (Vector3) pointB;
		DebugUtil.DebugBounds2D(debugBounds, color ?? Color.red, duration, depthTest: false);
	#endif
		return Physics2D.OverlapArea(pointA, pointB, layerMask, minDepth, maxDepth);
	}

	/// Check overlapping colliders and draw area bounds for debug
	public static int OverlapAreaNonAllocDebug(Vector2 pointA, Vector2 pointB, Collider2D[] results, int layerMask = Physics2D.DefaultRaycastLayers, float minDepth = -Mathf.Infinity, float maxDepth = Mathf.Infinity, Color? color = null, float duration = 0f) {
	#if UNITY_EDITOR
		Bounds debugBounds = new Bounds();
		debugBounds.min = (Vector3) pointA;
		debugBounds.max = (Vector3) pointB;
		DebugUtil.DebugBounds2D(debugBounds, color ?? Color.red, duration, depthTest: false);
	#endif
		return Physics2D.OverlapAreaNonAlloc(pointA, pointB, results, layerMask, minDepth, maxDepth);
	}

}
