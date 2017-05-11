﻿/* DEPENDENCIES
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
		// we can't draw an infinite ray so limit the draw distance
		if (distance == Mathf.Infinity)
			distance = maxDrawLineDistance;
		
		if (hit.collider == null) {
			// no hit, draw the full ray in no hit color
			DebugUtil.DrawLine2D(origin, origin + direction.normalized * distance, z, color ?? noHitColor, duration, depthTest: false);
		}
		else {
			// By default, draw the no hit part of the ray in green, and the hit part in red
			DebugUtil.DrawLine2D(origin, hit.point, z, color ?? noHitColor, duration, depthTest: false);
			DebugUtil.DrawLine2D(hit.point, origin + direction.normalized * distance, z, hitColor, duration, depthTest: false);
		}
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
			// we can't draw an infinite ray so limit the draw distance
		if (distance == Mathf.Infinity)
			distance = maxDrawLineDistance;

		Vector2 end = origin + direction.normalized * distance;

		if (hits.Length == 0)
		{
			// No hit, draw the full ray in no hit color
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

	/// Draw a boxcast, providing its result hit, with an optional not hit color, a draw duration and a Z depth where the boxcast should be drawn.
	[Conditional("DEBUG")]
	public static void DrawBoxCast (Vector2 origin, Vector2 size, float angle, Vector2 direction, float distance, RaycastHit2D hit, Color? color = null, float duration = 2f, float z = 0f)
	{
		// we can't draw an infinite boxcast so limit the draw distance
		if (distance == Mathf.Infinity)
			distance = maxDrawLineDistance;

		if (hit.collider == null) {
			// no hit, draw the full box in no hit color at the start and end point of the boxcast
			// to simplify, we only draw one line from the start center to the end center of the moving box,
			// but we could also draw the 4 segments connecting the 4 corners of the box (would require to define DebugUtil.DrawBoxCast)
			DebugUtil.DrawLocalBox2D(origin, size, angle, z, color ?? noHitColor, duration, depthTest: false);
			DebugUtil.DrawLine2D(origin, origin + direction.normalized * distance, z, color ?? noHitColor, duration, depthTest: false);
			DebugUtil.DrawLocalBox2D(origin + direction.normalized * distance, size, angle, z, color ?? noHitColor, duration, depthTest: false);
		}
		else {
			// By default, draw the no hit part of the boxcast in green, and the hit part in red
			DebugUtil.DrawLocalBox2D(origin, size, angle, z, color ?? noHitColor, duration, depthTest: false);
			DebugUtil.DrawLine2D(origin, hit.point, z, color ?? noHitColor, duration, depthTest: false);
			DebugUtil.DrawLocalBox2D(hit.point, size, angle, z, hitColor, duration, depthTest: false);
			DebugUtil.DrawLine2D(hit.point, origin + direction.normalized * distance, z, hitColor, duration, depthTest: false);
			DebugUtil.DrawLocalBox2D(origin + direction.normalized * distance, size, angle, z, hitColor, duration, depthTest: false);
		}
	}

	/// Boxcast and draw results for debug, with free area in green and collided area in red
	public static RaycastHit2D BoxCastDebug(Vector2 origin, Vector2 size, float angle, Vector2 direction, float distance, int layerMask = Physics2D.DefaultRaycastLayers, float minDepth = -Mathf.Infinity, float maxDepth = Mathf.Infinity, Color? color = null, float duration = 0f, float z = 0f) {
		RaycastHit2D hit = Physics2D.BoxCast(origin, size, angle, direction, distance, layerMask, minDepth, maxDepth);
		DrawBoxCast(origin, size, angle, direction, distance, hit, color, duration, z);
		return hit;
	}

	/// Check overlapping collider and draw area bounds for debug
	public static Collider2D OverlapAreaDebug(Vector2 pointA, Vector2 pointB, int layerMask = Physics2D.DefaultRaycastLayers, float minDepth = -Mathf.Infinity, float maxDepth = Mathf.Infinity, Color? color = null, float duration = 0f) {
	#if UNITY_EDITOR
		Bounds debugBounds = new Bounds();
		debugBounds.min = (Vector3) pointA;
		debugBounds.max = (Vector3) pointB;
		// IMPROVE: check results and draw in green if no collision
		DebugUtil.DrawBounds2D(debugBounds, color ?? Color.red, duration, depthTest: false);
	#endif
		return Physics2D.OverlapArea(pointA, pointB, layerMask, minDepth, maxDepth);
	}

	/// Check overlapping colliders and draw area bounds for debug
	public static int OverlapAreaNonAllocDebug(Vector2 pointA, Vector2 pointB, Collider2D[] results, int layerMask = Physics2D.DefaultRaycastLayers, float minDepth = -Mathf.Infinity, float maxDepth = Mathf.Infinity, Color? color = null, float duration = 0f) {
	#if UNITY_EDITOR
		Bounds debugBounds = new Bounds();
		debugBounds.min = (Vector3) pointA;
		debugBounds.max = (Vector3) pointB;
		// IMPROVE: check results and draw in green if no collision
		DebugUtil.DrawBounds2D(debugBounds, color ?? Color.red, duration, depthTest: false);
	#endif
		return Physics2D.OverlapAreaNonAlloc(pointA, pointB, results, layerMask, minDepth, maxDepth);
	}

}
