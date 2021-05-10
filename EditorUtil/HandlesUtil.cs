#if UNITY_EDITOR

using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEditor;

namespace CommonsHelper
{

	/// Utility functions for handles. This script is exceptionally outside an Editor folder and assembly (but still
	/// inside #if UNITY_EDITOR) because non-editor classes may want to specialize their Handles drawing in their own
	/// body. However, all drawing-related methods must be inside #if UNITY_EDITOR.
	public static class HandlesUtil {

		/// Return resolution of a 2D camera in pixels per world distance unit. The camera does not need to be in 2D mode,
		/// but has to be in orthogonal view, looking either forward or backward, otherwise return 0f.
		/// If no camera can be found, also return 0f.
		///
		/// Since in orthographic view, the distance between the camera and the target is not relevant,
		/// Use this function to determine if a draw target will likely be too small for details.
		/// (inspired by HandleUtility.GetHandleSize() implementation, adapted to 2D mode)
		public static float Get2DPixelResolution() {
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
	    public static float GetPixelResolution(Vector3 position) {
	        // GetHandleSize is basically doing the right computation, plus some tweaks, so we reverse them
	        // to get the distance between two points separated by 1m on the same Z plane as the passed position,
	        // seen from the current camera.
	        return 80f / HandleUtility.GetHandleSize(position);
	    }

		/// Return size of a pixel in world distance unit, when looking with a 2D camera.
		/// This is essentially the reverse of Get2DPixelResolution(), but 0 if camera is not 2D.
		/// Multiply your Handles label, etc. position offsets by this to get screen-size-constant offsets. 
		public static float Get2DPixelSize()
		{
			float pixelsPerUnit = Get2DPixelResolution();
			if (pixelsPerUnit > 0)
			{
				return 1f / pixelsPerUnit;
			}
			
			// Non orthographic and forward/backward cameras are not supported by our calculation
			return 0f;
		}

	    /// Return size of a pixel of a 2D or 3D camera in world distance unit.
	    /// This is really the reverse of GetPixelResolution().
	    public static float GetPixelSize(Vector3 position) {
	        return HandleUtility.GetHandleSize(position) / 80f;
	    }

	    public static void DrawLine (Vector3 p1, Vector3 p2, Color color) {
		    using (new Handles.DrawingScope(color)) {
			    Handles.DrawLine(p1, p2);
		    }
	    }
	    
	    /// <summary>
	    /// Draw an open polyline from an array of points, using the current gizmos parameter
	    /// </summary>
	    /// <param name="points">Array of points of the polyline.</param>
	    /// <param name="color">Optional draw color. Current gizmos color if not set.</param>
	    public static void DrawPolyLine (Vector3[] points, Color? color = null) {
		    using (new Handles.DrawingScope(color ?? Handles.color)) {
				for (int i = 0; i < points.Length - 1; ++i) {
					Handles.DrawLine(points[i], points[i + 1]);
				}
		    }
	    }
	    
	    /// <summary>
	    /// Draw an open polyline from an array of 2D points, using the current gizmos parameter
	    /// </summary>
	    /// <param name="points">Array of 2D points of the polyline.</param>
	    /// <param name="color">Optional draw color. Current gizmos color if not set.</param>
	    public static void DrawPolyLine2D (Vector2[] points2D, Color? color = null)
	    {
	        Vector3[] points = Array.ConvertAll(points2D, p2D => (Vector3) p2D);
	        DrawPolyLine(points, color);
	    }

	    /// <summary>
	    /// Draw an arrow
	    /// </summary>
	    /// <param name="p1">Arrow start position</param>
	    /// <param name="p2">Arrow end position</param>
	    /// <param name="color">Draw color</param>
	    public static void DrawArrow2D (Vector2 p1, Vector2 p2, Color color) {
		    using (new Handles.DrawingScope(color)) {
			    Handles.DrawLine(p1, p2);
			    Vector2 forward = (p2 - p1).normalized;
			    Vector2 right = VectorUtil.Rotate90CW(forward);
			    Handles.DrawLine(p2, p2 - 0.2f * forward - 0.2f * right);
			    Handles.DrawLine(p2, p2 - 0.2f * forward + 0.2f * right);
		    }
	    }

	    /// <summary>
	    /// Draw an arrow head (V-shaped polyline)
	    /// </summary>
	    /// <param name="position">Position of the extremity of the arrow head</param>
	    /// <param name="direction">Direction the arrow head is pointing to. Will be normalized.</param>
	    /// <param name="color">Draw color</param>
	    public static void DrawArrowHead2D (Vector2 position, Vector2 direction, Color color) {
	        using (new Handles.DrawingScope(color)) {
				Vector2 forward = direction.normalized;
				Vector2 right = VectorUtil.Rotate90CW(forward);
				Handles.DrawLine(position, position - 0.2f * forward - 0.2f * right);
				Handles.DrawLine(position, position - 0.2f * forward + 0.2f * right);
			}
	    }

		#region Handle

		const float defaultHandleScreenSize = 0.1f;
		static readonly Handles.CapFunction defaultHandleCap = Handles.CubeHandleCap;  // Unity 5.6

		/// Minimum camera resolution required to show the rectangle at all
		const float minDrawRectCameraResolution = 35f;

		/// Minimum camera resolution required to show the handles at all (bigger than minDrawRectCameraResolution)
		const float minDrawRectHandlesCameraResolution = 60f;
		
		/// Adapted from Handles.CircleHandleCap
		/// Draws a circle with a cross inside so we can drag a wide circle while seeing the target position precisely
		public static void CrossedCircleHandleCap(int controlID, Vector3 position, Quaternion rotation, float size, EventType eventType)
		{
			switch (eventType)
			{
				case EventType.MouseMove:
				case EventType.Layout:
					HandleUtility.AddControl(controlID, HandleUtility.DistanceToRectangle(position, rotation, size));
					break;
				case EventType.Repaint:
					// Reflection code for Handles.StartCapDraw(position, rotation, size);
					var handlesEntries = Type.GetType("UnityEditor.Handles,UnityEditor.dll");
					if (handlesEntries != null)
					{
						var startCapDrawMethod = handlesEntries.GetMethod("StartCapDraw", BindingFlags.Static | BindingFlags.NonPublic);
						if (startCapDrawMethod != null)
						{
							startCapDrawMethod.Invoke(null, new object[]{position, rotation, size});
                        
							// End of reflection, do the rest of what Handles.CircleHandleCap does normally
							Vector3 normal = rotation * new Vector3(0.0f, 0.0f, 1f);
							Handles.DrawWireDisc(position, normal, size);
                        
							// Add custom code here to draw the cross inside the circle
							Handles.DrawLine(position + size * Vector3.left, position + size * Vector3.right);
							Handles.DrawLine(position + size * Vector3.up, position + size * Vector3.down);
						}
					}
					break;
			}
		}

		public static void DrawRect (ref Rect rect, Transform owner, Color color) {

	        float pixelResolution = GetPixelResolution(owner.position);

			if (pixelResolution < minDrawRectCameraResolution) return;

			Color drawingColor;

			// if the rectangle is reversed, change the color to notify the user
			if (rect.width >= 0 && rect.height >= 0)
				drawingColor = color;
			else if (rect.width < 0 && rect.height >= 0)
				drawingColor = Color.Lerp(color, Color.yellow, 0.5f);
			else if (rect.width >= 0)
				drawingColor = Color.Lerp(color, Color.yellow, 0.5f);
			else
				drawingColor = Color.Lerp(color, Color.red, 0.5f);

			Matrix4x4 drawingMatrix;

			// only use the local matrix if scale is valid (no null coordinates)
			// else, only consider position and rotation to avoid producing NaN values
			if (owner.lossyScale.x != 0f && owner.lossyScale.y != 0f && owner.lossyScale.z != 0f) {
				drawingMatrix = owner.localToWorldMatrix;
			}
			else {
				// rotation is supported by not recommended, esp. because AABB operations cannot be applied, and the world rect based on the min and max corners is incorrect
				drawingMatrix = Matrix4x4.TRS(owner.position, owner.rotation, Vector3.one);
			}

			using (new Handles.DrawingScope(drawingColor, drawingMatrix))
			{
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
			}
		}

	    /// Proxy for FreeMoveHandle with 2D position
	    public static Vector2 DrawFreeMoveHandle (Vector2 pos, Vector2? optionalSnap = null, Handles.CapFunction capFunction = null, float screenSizeScale = 1f, int? controlID = null) {
	        float size = HandleUtility.GetHandleSize ((Vector3) pos) * defaultHandleScreenSize * screenSizeScale;
	        Vector3 snap = optionalSnap ?? Vector3.one;
	        capFunction = capFunction ?? defaultHandleCap;
	        
	        return controlID != null ?
	            (Vector2) Handles.FreeMoveHandle ((int)controlID, (Vector3) pos, Quaternion.identity, size, snap, capFunction) :
                (Vector2) Handles.FreeMoveHandle ((Vector3) pos, Quaternion.identity, size, snap, capFunction);
		}

	    /// Proxy for FreeMoveHandle with 3D position
		public static Vector3 DrawFreeMoveHandle (Vector3 pos, Vector3? optionalSnap = null, Handles.CapFunction capFunction = null, float screenSizeScale = 1f, int? controlID = null) {
	        float size = HandleUtility.GetHandleSize ((Vector3) pos) * defaultHandleScreenSize * screenSizeScale;
	        Vector3 snap = optionalSnap ?? Vector3.one;
	        capFunction = capFunction ?? defaultHandleCap;
	        
	        return controlID != null ?
	            (Vector2) Handles.FreeMoveHandle ((int)controlID, pos, Quaternion.identity, size, snap, capFunction) :
	            (Vector2) Handles.FreeMoveHandle (pos, Quaternion.identity, size, snap, capFunction);
		}

	    /// Variant of DrawFreeMoveHandle (without controlID) with 2D position by reference
	    public static void DrawFreeMoveHandle (ref Vector2 pos, Color color, Vector2? snap = null, Handles.CapFunction capFunction = null, float screenSizeScale = 1f, int? controlID = null) {
		    using (new Handles.DrawingScope(color)) {
			    pos = DrawFreeMoveHandle(pos, snap, capFunction, screenSizeScale, controlID);
		    }
	    }

	    /// Variant of DrawFreeMoveHandle (without controlID) with 3D position by reference
	    public static void DrawFreeMoveHandle (ref Vector3 pos, Color color, Vector3? snap = null, Handles.CapFunction capFunction = null, float screenSizeScale = 1f, int? controlID = null) {
		    using (new Handles.DrawingScope(color)) {
				pos = DrawFreeMoveHandle(pos, snap, capFunction, screenSizeScale, controlID);
		    }
	    }

	    /// Proxy for DrawWireDisc (without controlID) with 2D position by reference
	    public static void DrawCircleHandles (ref Vector2 center, ref float radius, Color color, Vector3 snap = default(Vector3), Handles.CapFunction centerCapFunction = null, float screenSizeScale = 1f) {
		    using (new Handles.DrawingScope(color)) {
				DrawFreeMoveHandle(ref center, color, snap, centerCapFunction, screenSizeScale);  // center
				Handles.DrawWireDisc((Vector3)center, Vector3.forward, radius);                   // circle
			    // RadiusHandle doesn't allow customizing cap function, it always uses DotHandleCap
				radius = Handles.RadiusHandle(Quaternion.identity, center, radius);               // radius
		    }
	    }

		/// Return DrawingScope with local to world matrix of passed transform, ignoring scale if it has null components
		/// Usage: using (HandlesUtil.GetMatrixDrawingScope()) { /* draw your local handles */ }
		public static Handles.DrawingScope GetMatrixDrawingScope(Transform tr) {
			Matrix4x4 drawingMatrix;

			// only use the local matrix if scale is valid (no null coordinates)
			// else, only consider position and rotation to avoid producing NaN values
			if (tr.lossyScale.x != 0 && tr.lossyScale.y != 0 && tr.lossyScale.z != 0)
				drawingMatrix = tr.localToWorldMatrix;
			else {
				drawingMatrix = Matrix4x4.TRS(tr.position, tr.rotation, Vector3.one);
			}

			return new Handles.DrawingScope(drawingMatrix);
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

		[Obsolete("Use Label2D.")]
		public static void DrawVectorText(Vector3 position, string text, float sizeFactor = 1f,
			bool fixedFontSize = false, Color? color = null)
		{
			Label2D(position, text, sizeFactor, fixedFontSize, color);
		}
		
		/// Draw vector text on a scene camera at a given position, size and color. If fixedFontSize is false, the size is constant in world space, else it is constant in screen space.
		public static void Label2D(Vector3 position, string text, float sizeFactor = 1f, bool fixedFontSize = false, Color? color = null)
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
			Handles.Label(position, text, textGuiStyle);
		}

		#endregion

	}
}

#endif  // UNITY_EDITOR
