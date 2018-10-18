using UnityEngine;
using Debug = UnityEngine.Debug;
using System.Collections;
using System.Diagnostics;

 /* DEPENDENCIES
 *
 * DebugDrawingExtension Unity Asset Store package
 *
 */

namespace Commons.Debug
{

    public static class PhysicsUtil {

		/// Max distance used to represent an infinite ray
		const float maxDrawLineDistance = 100f;

		static Color noHitColor = Color.green;
		static Color hitColor = Color.red;

		/// Draw a raycast, providing the raycast bool result, with an optional not hit color, a draw duration and a Z depth where the ray should be drawn.
		[Conditional("DEBUG")]
		public static void DrawRaycast (Vector3 origin, Vector3 direction, float distance, bool result, Color? color = null, float duration = 2f)
		{
			if (distance == Mathf.Infinity)
				distance = maxDrawLineDistance;

			// Since we only know whether there is a hit or not, just draw the line completely in one or the other color
			Debug.DrawRay(origin, direction.normalized * distance, result ? hitColor : (color ?? noHitColor), duration);
		}

		/// Raycast and draw debug at the same time. Set a color to override the no hit color.
		public static bool RaycastDebug (Vector3 origin, Vector3 direction, float distance = Mathf.Infinity, int layerMask = Physics2D.DefaultRaycastLayers, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal, Color? color = null, float duration = 2f) {
			bool result = Physics.Raycast(origin, direction, distance, layerMask);
			DrawRaycast(origin, direction, distance, result, color, duration);
			return result;
		}

		/// Draw a raycast that hit a collider, providing the hit info, with an optional not hit color, a draw duration and a Z depth where the ray should be drawn.
		/// Do not call this method if there was no hit, call the overload with bool result = false instead.
		[Conditional("DEBUG")]
		public static void DrawRaycast (Vector3 origin, Vector3 direction, float distance, RaycastHit hitInfo, Color? color = null, float duration = 2f)
		{
			if (hitInfo.collider == null)
			{
				Debug.LogWarning("Do not call Physics3DUtil.DrawRaycast with the 'RaycastHit hitInfo' parameter if there was no hit, pass 'bool result' instead.");
				return;
			}

			if (distance == Mathf.Infinity)
				distance = maxDrawLineDistance;

			// By default, draw the no hit part of the ray in green, and the hit part in red
			Debug.DrawLine(origin, hitInfo.point, color ?? noHitColor, duration, depthTest: false);
			Debug.DrawLine(hitInfo.point, origin + direction.normalized * distance, hitColor, duration, depthTest: false);
		}

		/// Raycast and draw debug at the same time. Set a color to override the no hit color.
		public static bool RaycastDebug (Vector3 origin, Vector3 direction, out RaycastHit hitInfo, float distance = Mathf.Infinity, int layerMask = Physics2D.DefaultRaycastLayers, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal, Color? color = null, float duration = 2f) {
			bool result = Physics.Raycast(origin, direction, out hitInfo, distance, layerMask);
			if (result)
				DrawRaycast(origin, direction, distance, hitInfo, color, duration);
			else
				DrawRaycast(origin, direction, distance, false, color, duration);
			return result;
		}

		/// Draw a raycast all or non-alloc, providing its result hits array
		[Conditional("DEBUG")]
		public static void DrawRaycastMulti (Vector3 origin, Vector3 direction, float distance, RaycastHit[] hits, Color? color = null, float duration = 2f)
		{
			if (distance == Mathf.Infinity)
				distance = maxDrawLineDistance;

			Vector3 end = origin + direction.normalized * distance;

			if (hits.Length == 0)
			{
				Debug.DrawLine(origin, end, color ?? noHitColor, duration, depthTest: false);
			}
			else
			{
				// For now, just start drawing red after first hit
				// IMPROVE: show each and every hit with a cross or a circle
				Debug.DrawLine(origin, hits[0].point, color ?? noHitColor, duration, depthTest: false);
				Debug.DrawLine(hits[0].point, end, hitColor, duration, depthTest: false);
			}
		}

		/// Raycast all colliders and draw debug at the same time. Set a color to override the no hit color.
		public static RaycastHit[] RaycastAllDebug (Vector3 origin, Vector3 direction, float distance = Mathf.Infinity, int layerMask = Physics2D.DefaultRaycastLayers, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal, Color? color = null, float duration = 2f)
		{
			RaycastHit[] hits = Physics.RaycastAll(origin, direction, distance, layerMask, queryTriggerInteraction);
			DrawRaycastMulti(origin, direction, distance, hits, color, duration);
			return hits;
		}

		/// Raycast all colliders with no allocation and draw debug at the same time. Set a color to override the no hit color.
		public static void RaycastNonAllocDebug(Vector3 origin, Vector3 direction, RaycastHit[] hits, float distance = Mathf.Infinity, int layerMask = Physics2D.DefaultRaycastLayers, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal, Color? color = null, float duration = 2f)
		{
			Physics.RaycastNonAlloc(origin, direction, hits, distance, layerMask, queryTriggerInteraction);
			DrawRaycastMulti(origin, direction, distance, hits, color, duration);
		}

	}

}
