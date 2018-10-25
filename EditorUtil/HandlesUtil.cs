#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System;
using System.Collections;

namespace CommonsHelper
{

	/// Utility functions for handles. As this script is located in an Editor folder, it can only be used within Editor scripts (scripts inside an Editor
	/// folder), not even for normal scripts with #if UNITY_EDITOR conditional macros.
	public static class HandlesUtil {

		/// Return resolution of a 2D camera in pixels per world distance unit. The camera does not need to be in 2D mode,
		/// but has to be in orthogonal view, looking either forward or backward, otherwise return 0f.
		/// If no camera can be found, also return 0f.
		///
		/// Since in orthographic view, the distance between the camera and the target is not relevant,
		/// Use this function to determine if a draw target will likely be too small for details.
		/// (inspired by HandleUtility.GetHandleSize() implementation, adapted to 2D mode)
		static float Get2DPixelResolution() {
			Camera camera = Camera.current;
			// Non orthographic and forward/backward cameras are not supported by our calculation
			if (camera == null || !(camera.orthographic && Mathf.Abs(camera.transform.forward.z) == 1f))
				return 0f;

			Vector3 a = camera.WorldToScreenPoint(Vector3.zero);  // point in front of camera on zero plane (on screen)
			Vector3 b = camera.WorldToScreenPoint(Vector3.right * 1f);  // 1 unit on the right (on screen)
			return Vector3.Distance(a, b);  // size of 1 unit on screen, pixel per meter, resolution
		}

	    /// Return resolution of a 2D or 3D camera in pixels per world distance unit.
	    /// This is more generic as Get2DPixelResolution and supports 3D cameras with different view angles.
	    static float GetPixelResolution(Vector3 position) {
	        // GetHandleSize is basically doing the right computation, plus some tweaks, so we reverse them
	        // to get the distance between two points separated by 1m on the same Z plane as the passed position,
	        // seen from the current camera.
	        return 80f / HandleUtility.GetHandleSize(position);
	    }

	    public static void DrawLine (Vector3 p1, Vector3 p2, Color color) {
	        Color oldColor = Handles.color;
	        Handles.color = color;
	        Handles.DrawLine(p1, p2);
	        Handles.color = oldColor;
	    }

	    public static void DrawArrow2D (Vector2 p1, Vector2 p2, Color color) {
	        Color oldColor = Handles.color;
	        Handles.color = color;
	        Handles.DrawLine(p1, p2);
	        Vector2 forward = (p2 - p1).normalized;
	        Vector2 right = VectorUtil.Rotate90CW(forward);
	        Handles.DrawLine(p2, p2 - 0.2f * forward - 0.2f * right);
	        Handles.DrawLine(p2, p2 - 0.2f * forward + 0.2f * right);
	        Handles.color = oldColor;
	    }

		#region Handle

		const float handleSize = 0.1f;
		static readonly Handles.CapFunction defaultHandleCap = Handles.CubeHandleCap;  // Unity 5.6

		/// Minimum camera resolution required to show the rectangle at all
		const float minDrawRectCameraResolution = 35f;

		/// Minimum camera resolution required to show the handles at all (bigger than minDrawRectCameraResolution)
		const float minDrawRectHandlesCameraResolution = 60f;

		public static void DrawRect (ref Rect rect, Transform owner, Color color) {

	        float pixelResolution = GetPixelResolution(owner.position);

			if (pixelResolution < minDrawRectCameraResolution) return;

			Color oldColor = Handles.color;

			// if the rectangle is reversed, change the color to notify the user
			if (rect.width >= 0 && rect.height >= 0)
				Handles.color = color;
			else if (rect.width < 0 && rect.height >= 0)
				Handles.color = Color.Lerp(color, Color.yellow, 0.5f);
			else if (rect.width >= 0)
				Handles.color = Color.Lerp(color, Color.yellow, 0.5f);
			else
				Handles.color = Color.Lerp(color, Color.red, 0.5f);

			Matrix4x4 oldMatrix = Handles.matrix;

			// only use the local matrix if scale is valid (no null coordinates)
			// else, only consider position and rotation to avoid producing NaN values
			if (owner.lossyScale.x != 0 && owner.lossyScale.y != 0 && owner.lossyScale.z != 0) {
				Handles.matrix = owner.localToWorldMatrix;
			}
			else {
				// rotation is supported by not recommended, esp. because AABB operations cannot be applied, and the world rect based on the min and max corners is incorrect
				Handles.matrix = Matrix4x4.TRS(owner.position, owner.rotation, Vector3.one);
			}

			// Draw rect edges
			var points = new Vector3[] {
				new Vector3(rect.xMin, rect.yMin),
				new Vector3(rect.xMax, rect.yMin),
				new Vector3(rect.xMax, rect.yMax),
				new Vector3(rect.xMin, rect.yMax),
				new Vector3(rect.xMin, rect.yMin)
			};
			Handles.DrawPolyLine(points);

			if (pixelResolution < minDrawRectHandlesCameraResolution) {
				Handles.matrix = oldMatrix;
				Handles.color = oldColor;
				return;
			}

			// Prepare temporary vector for the 9 handles
			Vector2 tempVec;

			// Draw center handle
			EditorGUI.BeginChangeCheck ();
			tempVec = DrawFreeMoveHandle(rect.center);
			if (EditorGUI.EndChangeCheck ()) {
				rect.center = tempVec;
			}

			// Draw left handle
			EditorGUI.BeginChangeCheck ();
			tempVec = DrawFreeMoveHandle(new Vector3(rect.xMin, rect.center.y));
			if (EditorGUI.EndChangeCheck ()) {
				rect.xMin = tempVec.x;
			}

			// Draw right handle
			EditorGUI.BeginChangeCheck ();
			tempVec = DrawFreeMoveHandle(new Vector3(rect.xMax, rect.center.y));
			if (EditorGUI.EndChangeCheck ()) {
				rect.xMax = tempVec.x;
			}

			// Draw bottom handle
			EditorGUI.BeginChangeCheck ();
			tempVec = DrawFreeMoveHandle(new Vector3(rect.center.x, rect.yMin));
			if (EditorGUI.EndChangeCheck ()) {
				rect.yMin = tempVec.y;
			}

			// Draw top handle
			EditorGUI.BeginChangeCheck ();
			tempVec = DrawFreeMoveHandle(new Vector3(rect.center.x, rect.yMax));
			if (EditorGUI.EndChangeCheck ()) {
				rect.yMax = tempVec.y;
			}

			// Draw bottom-left handle
			EditorGUI.BeginChangeCheck ();
			tempVec = DrawFreeMoveHandle(new Vector3(rect.xMin, rect.yMin));
			if (EditorGUI.EndChangeCheck ()) {
				rect.min = tempVec;
			}

			// Draw top-left handle
			EditorGUI.BeginChangeCheck ();
			tempVec = DrawFreeMoveHandle(new Vector3(rect.xMin, rect.yMax));
			if (EditorGUI.EndChangeCheck ()) {
				rect.xMin = tempVec.x;
				rect.yMax = tempVec.y;
			}

			// Draw bottom-right handle
			EditorGUI.BeginChangeCheck ();
			tempVec = DrawFreeMoveHandle(new Vector3(rect.xMax, rect.yMin));
			if (EditorGUI.EndChangeCheck ()) {
				rect.xMax = tempVec.x;
				rect.yMin = tempVec.y;
			}

			// Draw top-right handle
			EditorGUI.BeginChangeCheck ();
			tempVec = DrawFreeMoveHandle(new Vector3(rect.xMax, rect.yMax));
			if (EditorGUI.EndChangeCheck ()) {
				rect.xMax = tempVec.x;
				rect.yMax = tempVec.y;
			}

			Handles.matrix = oldMatrix;
			Handles.color = oldColor;
		}

	    public static Vector2 DrawFreeMoveHandle (Vector2 pos, Vector2? snap = null, Handles.CapFunction capFunction = null) {
			return (Vector2) Handles.FreeMoveHandle ((Vector3) pos, Quaternion.identity,
	            HandleUtility.GetHandleSize ((Vector3) pos) * handleSize, snap ?? Vector3.one, capFunction ?? defaultHandleCap);
		}

		public static Vector3 DrawFreeMoveHandle (Vector3 pos, Vector3? snap = null, Handles.CapFunction capFunction = null) {
			return Handles.FreeMoveHandle (pos, Quaternion.identity,
	           HandleUtility.GetHandleSize (pos) * handleSize, snap ?? Vector3.one, capFunction ?? defaultHandleCap);
		}

	    public static void DrawFreeMoveHandle (ref Vector2 pos, Color color, Vector2? snap = null, Handles.CapFunction capFunction = null) {
	        Color oldColor = Handles.color;
	        Handles.color = color;
	        pos = DrawFreeMoveHandle(pos, snap, capFunction);
	        Handles.color = oldColor;
	    }

	    public static void DrawFreeMoveHandle (ref Vector3 pos, Color color, Vector3? snap = null, Handles.CapFunction capFunction = null) {
	        Color oldColor = Handles.color;
	        Handles.color = color;
	        pos = DrawFreeMoveHandle(pos, snap, capFunction);
	        Handles.color = oldColor;
	    }

	    public static void DrawCircleHandles (ref Vector2 center, ref float radius, Color color, Vector3 snap = default(Vector3), Handles.CapFunction capFunction = null) {
	        Color oldColor = Handles.color;
	        Handles.color = color;
	        DrawFreeMoveHandle(ref center, color, snap, capFunction);           // center
	        Handles.DrawWireDisc((Vector3)center, Vector3.forward, radius);     // circle
	        radius = Handles.RadiusHandle(Quaternion.identity, center, radius); // radius
	        Handles.color = oldColor;
	    }

		/// Store the current Handles matrix to oldMatrix reference, and set the Handles matrix to the local matrix
		/// of the passed transform, ignoring scale if it has null components
		public static void SetHandlesMatrix(Transform tr, out Matrix4x4 oldMatrix) {
			oldMatrix = Handles.matrix;

			// only use the local matrix if scale is valid (no null coordinates)
			// else, only consider position and rotation to avoid producing NaN values
			if (tr.lossyScale.x != 0 && tr.lossyScale.y != 0 && tr.lossyScale.z != 0)
				Handles.matrix = tr.localToWorldMatrix;
			else {
				Handles.matrix = Matrix4x4.TRS(tr.position, tr.rotation, Vector3.one);
			}
		}

	    #endregion

		#region Text

		/// Font size used when font factor size is 1 and camera resolution is minDrawCameraPixelsPerUnit. Also the size under which text is not drawn on screen.
		const float BASE_FONT_SIZE = 11f;

		/// Minimum camera resolution required to show text (to avoid too small characters), and also the resolution at which the BASE_FONT_SIZE * sizeFactor is used.
		/// Also used to determine when to draw fixed size text.
		const float minDrawTextCameraResolution = 35f;

		/// Temporary text GUI style, modified on each DrawVectorText call
		static readonly GUIStyle textGuiStyle = new GUIStyle();

		/// Draw vector text on a scene camera at a given position, size and color. If fixedFontSize is false, the size is constant in world space, else it is constant in screen space.
		public static void DrawVectorText(Vector3 position, string text, float sizeFactor = 1f, bool fixedFontSize = false, Color? color = null)
		{
			float pixelsPerUnit = Get2DPixelResolution();

			// Do not draw text if not in orthographic view, facing world forward, nor if the camera has not been found.
			// you can also check scene.in2DMode if you have access to the scene this camera belongs to, and you want to exclude manual orthographic view in 3D mode
			// if you really need to, look at HandleUtility.GetHandleSize() implementation to get the general pixels per unit formula
			if (pixelsPerUnit == 0f) return;

			float resolutionFactor = fixedFontSize ? 1f : pixelsPerUnit / minDrawTextCameraResolution;
			float finalSizeFactor = sizeFactor * resolutionFactor;

			// For fixed font size, do not draw if resolution is too low
			// For variable font size, do not draw if text is too small
			// We can sum this up in one equivalent comparison
			if (finalSizeFactor < 1f)
				return;

			// set color to white, or an optional overriding color
			textGuiStyle.normal.textColor = color ?? Color.white;

			// set text size depending on resolution
			textGuiStyle.fontSize = (int) Mathf.Floor (BASE_FONT_SIZE * finalSizeFactor);

			// draw label
			Handles.Label (position, text, textGuiStyle);
		}

		#endregion

	}
}

#endif  // UNITY_EDITOR
