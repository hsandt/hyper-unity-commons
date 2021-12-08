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
	    /// Draw an open polyline from an array of points, using the current handles parameter
	    /// </summary>
	    /// <param name="points">Array of points of the polyline.</param>
	    /// <param name="color">Optional draw color. Current handles color if not set.</param>
	    public static void DrawPolyLine (Vector3[] points, Color? color = null) {
		    using (new Handles.DrawingScope(color ?? Handles.color)) {
				for (int i = 0; i < points.Length - 1; ++i) {
					Handles.DrawLine(points[i], points[i + 1]);
				}
		    }
	    }
	    
	    /// <summary>
	    /// Draw an open polyline from an array of 2D points, using the current handles parameter
	    /// </summary>
	    /// <param name="points">Array of 2D points of the polyline.</param>
	    /// <param name="color">Optional draw color. Current handles color if not set.</param>
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
	    /// <param name="thickness">Draw thickness</param>
	    public static void DrawArrow2D (Vector2 p1, Vector2 p2, Color color, float thickness = 0f) {
		    using (new Handles.DrawingScope(color)) {
			    Handles.DrawLine(p1, p2, thickness);
			    Vector2 forward = (p2 - p1).normalized;
			    Vector2 right = VectorUtil.Rotate90CW(forward);
			    Handles.DrawLine(p2, p2 - 0.2f * forward - 0.2f * right, thickness);
			    Handles.DrawLine(p2, p2 - 0.2f * forward + 0.2f * right, thickness);
		    }
	    }

	    /// <summary>
	    /// Draw an arrow head (V-shaped polyline)
	    /// </summary>
	    /// <param name="position">Position of the extremity of the arrow head</param>
	    /// <param name="direction">Direction the arrow head is pointing to. Will be normalized.</param>
	    /// <param name="color">Draw color</param>
	    /// <param name="thickness">Draw thickness</param>
	    public static void DrawArrowHead2D (Vector2 position, Vector2 direction, Color color, float thickness = 0f) {
	        using (new Handles.DrawingScope(color)) {
				Vector2 forward = direction.normalized;
				Vector2 right = VectorUtil.Rotate90CW(forward);
				Handles.DrawLine(position, position - 0.2f * forward - 0.2f * right, thickness);
				Handles.DrawLine(position, position - 0.2f * forward + 0.2f * right, thickness);
			}
	    }

	    /// <summary>
	    /// Draw a circle in the XY plane
	    /// </summary>
	    /// <param name="center">Center of the circle</param>
	    /// <param name="radius">Radius of the circle</param>
	    /// <param name="color">Color of the circle</param>
	    /// <param name="thickness">Thickness of the circle</param>
	    public static void DrawCircle2D(Vector2 center, float radius, Color color, float thickness = 0f) {
		    using (new Handles.DrawingScope(color)) {
			    Handles.DrawWireDisc((Vector3) center, Vector3.forward, radius, thickness);
		    }
	    }

		#region Handle

		const float defaultHandleScreenSize = 0.1f;
		static readonly Handles.CapFunction defaultHandleCap = Handles.CubeHandleCap;  // Unity 5.6

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

		public static void DrawRect(Rect rect, Color color)
		{
			using (new Handles.DrawingScope(color))
			{
				// Draw rect edges
				var points = new Vector3[]
				{
					new Vector3(rect.xMin, rect.yMin),
					new Vector3(rect.xMax, rect.yMin),
					new Vector3(rect.xMax, rect.yMax),
					new Vector3(rect.xMin, rect.yMax),
					new Vector3(rect.xMin, rect.yMin)
				};
				Handles.DrawPolyLine(points);
			}
		}

		[Obsolete("Use DrawRectHandle")]
		public static void DrawRect(ref Rect rect, Transform owner, Color color)
		{
			DrawRectHandle(ref rect, owner, color);
		}

		/// <summary>
		/// Draw rectangle with 4 handles, one on each edge
		/// The rectangle follows the transform of its owner.
		/// </summary>
		/// <param name="rect">Rectangle to modify</param>
		/// <param name="owner">Owner transform</param>
	    /// <param name="color">Draw color</param>
		public static void DrawRectHandle (ref Rect rect, Transform owner, Color color) {
			
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
	
				// Prepare temporary vector for the 9 handles
				Vector2 tempVec;
	
				// Draw center handle
				using (var check = new EditorGUI.ChangeCheckScope()) {
					tempVec = DrawFreeMoveHandle(rect.center);
					if (check.changed) { rect.center = tempVec; }
				}
	
				// Draw left handle
				using (var check = new EditorGUI.ChangeCheckScope()) {
					tempVec = DrawFreeMoveHandle(new Vector3(rect.xMin, rect.center.y));
					if (check.changed) { rect.xMin = tempVec.x; }
				}

				// Draw right handle
				using (var check = new EditorGUI.ChangeCheckScope()) {
					tempVec = DrawFreeMoveHandle(new Vector3(rect.xMax, rect.center.y));
					if (check.changed) { rect.xMax = tempVec.x; }
				}

				// Draw bottom handle
				using (var check = new EditorGUI.ChangeCheckScope()) {
					tempVec = DrawFreeMoveHandle(new Vector3(rect.center.x, rect.yMin));
					if (check.changed) { rect.yMin = tempVec.y; }
				}

				// Draw top handle
				using (var check = new EditorGUI.ChangeCheckScope()) {
					tempVec = DrawFreeMoveHandle(new Vector3(rect.center.x, rect.yMax));
					if (check.changed) { rect.yMax = tempVec.y; }
				}

				// Draw bottom-left handle
				using (var check = new EditorGUI.ChangeCheckScope()) {
					tempVec = DrawFreeMoveHandle(new Vector3(rect.xMin, rect.yMin));
					if (check.changed) { rect.min = tempVec; }  // set xMin and yMin at once
				}

				// Draw top-left handle
				using (var check = new EditorGUI.ChangeCheckScope()) {
					tempVec = DrawFreeMoveHandle(new Vector3(rect.xMin, rect.yMax));
					if (check.changed) {
						rect.xMin = tempVec.x;
						rect.yMax = tempVec.y;
					}
				}

				// Draw bottom-right handle
				using (var check = new EditorGUI.ChangeCheckScope()) {
					tempVec = DrawFreeMoveHandle(new Vector3(rect.xMax, rect.yMin));
					if (check.changed) {
						rect.xMax = tempVec.x;
						rect.yMin = tempVec.y;
					}
				}

				// Draw top-right handle
				using (var check = new EditorGUI.ChangeCheckScope()) {
					tempVec = DrawFreeMoveHandle(new Vector3(rect.xMax, rect.yMax));
					if (check.changed) { rect.max = tempVec; }  // set xMax and yMax at once
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

		/// Draw handle to edit angle, by drawing a circle with a Free Move Handle that moves a point along the circle,
		/// and a solid arc from the reference direction.
		/// The resulting angle is the signed angle between the reference direction and the radial vector to this moved point.
	    public static void DrawAngleHandle (Vector2 center, float radius, Vector2 referenceDirection, ref float angle,
				Color circleColor, Color handleColor, Color solidArcColor, Handles.CapFunction centerCapFunction = null, float screenSizeScale = 1f, int? controlID = null) {
			Vector2 direction = VectorUtil.Rotate(Vector2.left, angle);
			Vector2 handlePosition = center + radius * direction;
			
			// Circle
			using (new Handles.DrawingScope(circleColor)) {
				Handles.DrawWireDisc(center, Vector3.forward, radius, 2f);
			}

	        using (var check = new EditorGUI.ChangeCheckScope())
	        {
				// Angle point handle
			    // Snapping is not relevant when you move a point along a circle
			    DrawFreeMoveHandle(ref handlePosition, handleColor, null, centerCapFunction, screenSizeScale, controlID);
			    
			    if (check.changed) {
			        angle = Vector2.SignedAngle(referenceDirection, handlePosition - center);
			    }
	        }
	        
	        // Draw semi-transparent Solid Arc from reference direction to selected angle direction to help visualization
	        using (new Handles.DrawingScope(solidArcColor))
	        {
				Handles.DrawSolidArc(center, Vector3.forward, (Vector3) referenceDirection, angle, radius);
	        }
		}

		/// Draw handle to edit angle range (pair of start and end angle), by drawing a circle with a Free Move Handle that moves a point along the circle,
		/// a line for the reference direction and a solid arc between the start and end angle.
		/// For solidArcColor, we recommend a semi-transparent color like ColorUtil.quarterInvisibleWhite.
	    public static void DrawAngleRangeHandle (Vector2 center, float radius, Vector2 referenceDirection, ref float startAngle, ref float endAngle,
				Color circleColor, Color referenceLineColor, Color startHandleColor, Color endHandleColor, Color solidArcColor, Handles.CapFunction centerCapFunction = null, float screenSizeScale = 1f, int? controlID = null) {
			Vector2 startDirection = VectorUtil.Rotate(Vector2.left, startAngle);
			Vector2 startHandlePosition = center + radius * startDirection;
			
			Vector2 endDirection = VectorUtil.Rotate(Vector2.left, endAngle);
			Vector2 endHandlePosition = center + radius * endDirection;
			
			// Circle
			using (new Handles.DrawingScope(circleColor)) {
				Handles.DrawWireDisc(center, Vector3.forward, radius, 2f);
			}
			
			// Reference direction
			// Unlike DrawAngleHandle, we must draw it as our solid arc is between start and end angle, so it doesn't
			// show the reference direction at all.
			using (new Handles.DrawingScope(referenceLineColor)) {
				Handles.DrawDottedLine(center, center + referenceDirection, 2f);
			}

	        using (var check = new EditorGUI.ChangeCheckScope())
	        {
				// Angle point handles
				// Snapping is not relevant when you move a point along a circle
			    DrawFreeMoveHandle(ref startHandlePosition, startHandleColor, null, centerCapFunction, screenSizeScale, controlID);
			    DrawFreeMoveHandle(ref endHandlePosition, endHandleColor, null, centerCapFunction, screenSizeScale, controlID);
			    
			    if (check.changed) {
			        startAngle = Vector2.SignedAngle(referenceDirection, startHandlePosition - center);
			        endAngle = Vector2.SignedAngle(referenceDirection, endHandlePosition - center);
			    }
	        }
	        
	        // Draw semi-transparent Solid Arc from reference direction to selected angle direction to help visualization
	        using (new Handles.DrawingScope(solidArcColor))
	        {
				Handles.DrawSolidArc(center, Vector3.forward, (Vector3) (startHandlePosition - center), endAngle - startAngle, radius);
	        }
		}

		/// Draw handles for a disc, allowing to move center and tune radius
	    /// Proxy for DrawFreeMoveHandle + DrawWireDisc + RadiusHandle (without controlID) with 2D position by reference
	    public static void DrawCircleHandles (ref Vector2 center, ref float radius, Color color, Vector3 snap = default(Vector3), Handles.CapFunction centerCapFunction = null, float screenSizeScale = 1f) {
		    using (new Handles.DrawingScope(color)) {
				DrawFreeMoveHandle(ref center, color, snap, centerCapFunction, screenSizeScale);  // center
				Handles.DrawWireDisc((Vector3)center, Vector3.forward, radius);            // circle
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

		/// Font size under which text is not drawn on screen.
		private const float MIN_FONT_SIZE = 10f;
		
		/// Font size used when font factor size is 1 with fixed world font size,
		/// or when font factor size is 1 and camera resolution is minDrawCameraPixelsPerUnit. 
		/// This is the default value of GUI.skin.label.fontSize.
		private const float BASE_FONT_SIZE = 12f;

		/// Camera resolution reference at which text is drawn at size BASE_FONT_SIZE with fixed screen font size.
		private const float BASE_DRAW_TEXT_CAMERA_RESOLUTION = 35f;

		/// Return font size to draw text at fixed screen size if fixedFontSize is true,
		/// fixed world size following BASE_DRAW_TEXT_CAMERA_RESOLUTION if fixedFontSize is false,
		/// with factor sizeFactor, and following BASE_FONT_SIZE.
		/// If computed size is less than MIN_FONT_SIZE, return 0 instead.
		private static int ComputeFontSize(float pixelsPerUnit, float sizeFactor = 1f, bool fixedFontSize = false)
		{
			// We cannot define a 2D font size if not in orthographic view nor facing the XY plane, in which case
			// pixelsPerUnit = 0, and we should just return 0.
			if (pixelsPerUnit == 0f)
			{
				return 0;
			}

			float resolutionFactor = fixedFontSize ? 1f : pixelsPerUnit / BASE_DRAW_TEXT_CAMERA_RESOLUTION;
			float finalSizeFactor = sizeFactor * resolutionFactor;
			int fontSize = Mathf.FloorToInt(BASE_FONT_SIZE * finalSizeFactor);
			
			// Reject font size under the minimum allowed, to avoid drawing text too small
			if (fontSize < MIN_FONT_SIZE)
			{
				return 0;
			}
			
			return fontSize;
		}
		
		/// Create and return Gui Style with computed font size and optional color.
		/// pixelsPerUnit: if not already computed, pass Get2DPixelResolution()
		/// Default sizeFactor: 1f
		/// Default fixedFontSize: false (fixed world size)
		/// Default color: white
		private static GUIStyle CreateGuiStyle(float pixelsPerUnit, float sizeFactor = 1f, bool fixedFontSize = false, Color? color = null)
		{
			int fontSize = ComputeFontSize(pixelsPerUnit, sizeFactor, fixedFontSize);

			if (fontSize == 0)
			{
				return null;
			}

			GUIStyle textGuiStyle = new GUIStyle(GUI.skin.label)
			{
				normal = {textColor = color ?? Color.white},
				fontSize = fontSize,
			};
			
			return textGuiStyle;
		}
		
		[Obsolete("Use Label2D.")]
		public static void DrawVectorText(Vector3 position, string text, float sizeFactor = 1f,
			bool fixedFontSize = false, Color? color = null)
		{
			Label2D(position, text, sizeFactor, fixedFontSize, color);
		}

		/// Draw vector text on a scene camera at a given position, size and color. If fixedFontSize is true, the size is constant in screen space, else it is constant in world space.
		public static void Label2D(Vector2 position, string text, float sizeFactor = 1f, bool fixedFontSize = false, Color? color = null)
		{
			GUIStyle textGuiStyle = CreateGuiStyle(Get2DPixelResolution(), sizeFactor, fixedFontSize, color);
			if (textGuiStyle != null)
			{
				Handles.Label(position, text, textGuiStyle);
			}
		}
		
		/// Draw vector text with rectangle background and auto-padding at given position (used for background rectangle topleft),
		/// size and color. If fixedFontSize is true, the size is constant in screen space, else it is constant in world space.
		public static void DrawLabelWithBackground(Vector2 position, string text, float sizeFactor = 1f, bool fixedFontSize = false, Color? textColor = null, Color? backgroundColor = null)
		{
			GUIContent textContent = new GUIContent(text);
			
			// Background label
			float pixelSize = Get2DPixelSize();
			GUIStyle guiStyle = CreateGuiStyle(pixelSize, sizeFactor, fixedFontSize, textColor);
			guiStyle.padding = new RectOffset(5, 5, 5, 5);
			guiStyle.alignment = TextAnchor.UpperLeft;  // we'll offset label manually, it's more reliable than MiddleCenter
			Vector2 textSize = guiStyle.CalcSize(textContent); // includes padding
			Vector2 textWorldSize = textSize * pixelSize;
			// Remember we work with y down, so height must be negative to work with top-left root position
			Rect textContainerRect = new Rect(position, new Vector2(textWorldSize.x, -textWorldSize.y));
			Handles.DrawSolidRectangleWithOutline(textContainerRect, backgroundColor ?? ColorUtil.halfInvisibleBlack, Color.clear);

			// Label
			// Magic number 1.5f was found by tuning to have the text touch the top then the bottom of the rectangle
			// (tested with 0 padding), then pick the number right in the middle.
			// It's not perfect, but aligns text vertically in most cases.
			Vector2 labelPosition = position + new Vector2(
				                        guiStyle.padding.left * pixelSize,
				                        (-guiStyle.padding.top + 1.5f * sizeFactor) * pixelSize);
			Handles.Label(labelPosition, textContent, guiStyle);
		}

		#endregion

	}
}

#endif  // UNITY_EDITOR
