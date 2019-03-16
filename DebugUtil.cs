using UnityEngine;

using CommonsHelper;

namespace CommonsDebug
{

	public static class DebugUtil {

		/// <summary>
		/// Draw a 2D line at the given Z, with given color, for given duration.
		/// </summary>
		/// <param name="start">Point in world space where the line should start.</param>
		/// <param name="end">Point in world space where the line should end.</param>
		/// <param name="z">Z at which to draw.</param>
		/// <param name="color">Color of the line.</param>
		/// <param name="duration">How long the line should be visible (s).</param>
		/// <param name="depthTest">Should the line be obscured by objects closer to the camera?</param>
		public static void DrawLine2D(Vector2 start, Vector2 end, float z, Color color, float duration = 0f, bool depthTest = true)
		{
			Debug.DrawLine(start.ToVector3(z), end.ToVector3(z), color, duration, depthTest);
		}

	    /// <summary>
	    /// Draw a 2D ray at the given Z, with given color, for given duration.
	    /// Optionally draw a label to identify the ray.
	    /// </summary>
	    /// <param name="start">Point in world space where the line should start.</param>
	    /// <param name="dir">Direction and length of the ray.</param>
	    /// <param name="z">Z at which to draw.</param>
	    /// <param name="color">Color of the line.</param>
	    /// <param name="duration">How long the line should be visible (s).</param>
	    /// <param name="depthTest">Should the line be obscured by objects closer to the camera?</param>
	    /// <param name="text">Optional label text to identify the ray</param>
	    public static void DrawRay2D(Vector2 start, Vector2 dir, float z, Color color, float duration = 0f, bool depthTest = true, string text = null)
	    {
	        Debug.DrawRay(start.ToVector3(z), dir.ToVector3(z), color, duration, depthTest);
	        if (!string.IsNullOrEmpty(text))
	        {
	            // draw the label at a position less likely to intersect with the ray
	            // for rays with a deltaY > 0, the origin of the label should be near the middle of the ray, offset by a vector CW of the ray
	            // for rays with a deltaY < 0, the origin of the label should be near the middle of the ray, offset by a vector CCW of the ray
	            Vector2 offset = 0.2f * (dir.y > 0 ? VectorUtil.Rotate90CW(dir.normalized) : VectorUtil.Rotate90CCW(dir.normalized));
	            Vector2 textPosition = (start + dir / 2) + offset;
#if UNITY_EDITOR
	            DebugLabelManager.Print3D(textPosition.ToVector3(z), text, color, duration);
#endif
	        }
	    }

		/// <summary>
		/// Draw a 2D box at the given Z, with given color, for given duration.
		/// </summary>
		/// <param name="offset">Center of the box</param>
		/// <param name="size">Box size</param>
		/// <param name="angle">Box angle</param>
		/// <param name="z">Z at which to draw.</param>
		/// <param name="color">Color of the line.</param>
		/// <param name="duration">How long the line should be visible (s).</param>
		/// <param name="depthTest">Should the line be obscured by objects closer to the camera?</param>
		public static void DrawBox2D (Vector2 offset, Vector2 size, float angle, float z, Color color, float duration = 0f, bool depthTest = true) {
			Vector2[] corners = Draw2DUtil.GetCornersFromBox2DParams(offset, size, angle);

			// draw the 4 edges by cycling between pair of corners
			for (int i = 0; i < 4; ++i) {
				Debug.DrawLine(corners[i].ToVector3(z), corners[(i + 1) % 4].ToVector3(z), color, duration, depthTest);
			}
		}

	    /// <summary>
	    /// Draw a 2D boxcast with the start box, motion lines and optionally the end box, at the given Z, with given color, for given duration.
	    /// </summary>
	    /// <param name="offset">Center of the box</param>
	    /// <param name="size">Box size</param>
	    /// <param name="angle">Box angle</param>
	    /// <param name="z">Z at which to draw.</param>
	    /// <param name="color">Color of the line.</param>
	    /// <param name="duration">How long the line should be visible (s).</param>
	    /// <param name="depthTest">Should the line be obscured by objects closer to the camera?</param>
	    public static void DrawBoxWithRays2D (Vector2 offset, Vector2 size, float angle, Vector2 direction, float distance, float z, Color color, bool drawEndBox = true, float duration = 0f, bool depthTest = true) {
	        Vector2[] corners = Draw2DUtil.GetCornersFromBox2DParams(offset, size, angle);
	        Vector2 motion = direction.normalized * distance;

	        // draw the 4 edges by cycling between pair of corners and the 4 motion lines
	        for (int i = 0; i < 4; ++i) {
	            // edge i@start, i+1@start
	            Debug.DrawLine(corners[i].ToVector3(z), corners[(i + 1) % 4].ToVector3(z), color, duration, depthTest);
	            // motion line i@start, i@end
	            Debug.DrawLine(corners[i].ToVector3(z), (corners[i] + motion).ToVector3(z), color, duration, depthTest);
	        }

	        if (drawEndBox) {
	            for (int i = 0; i < 4; ++i) {
	                // edge i@end, i+1@end
	                Debug.DrawLine((corners[i] + motion).ToVector3(z), (corners[(i + 1) % 4] + motion).ToVector3(z), color, duration, depthTest);
	            }
	        }
	    }

		/// <summary>
		/// Draw the 2D part of 3D bounds at its center Z, with given color, for given duration.
		/// Useful to debug 2D collider bounds at the object's Z.
		/// </summary>
		/// <param name="bounds">3D bounds to project on XY plane.</param>
		/// <param name="color">Draw color.</param>
		/// <param name="duration">Debug duration.</param>
		/// <param name="depthTest">Depth test before drawing?</param>
		public static void DrawBounds2D(Bounds bounds, Color color, float duration = 0, bool depthTest = true)
		{
			Vector3 center = bounds.center;

			float x = bounds.extents.x;
			float y = bounds.extents.y;

			Vector3 bottomLeft = center + new Vector3(-x, -y);
			Vector3 bottomRight = center + new Vector3(x, -y);
			Vector3 topRight = center + new Vector3(x, y);
			Vector3 topLeft = center + new Vector3(-x, y);

			Debug.DrawLine(bottomLeft, bottomRight, color, duration, depthTest);
			Debug.DrawLine(bottomRight, topRight, color, duration, depthTest);
			Debug.DrawLine(topRight, topLeft, color, duration, depthTest);
			Debug.DrawLine(topLeft, bottomLeft, color, duration, depthTest);
		}

		/// <summary>
		/// Draw a debug Rect as if its coordinates were world coordinates (origin at bottom-left)
		/// Here, Rect is used as a utility class, without UI / screen semantics
		/// </summary>
		/// <param name="rect">Rect in world coordinates.</param>
		/// <param name="color">Draw color.</param>
		/// <param name="duration">Debug duration.</param>
		/// <param name="depthTest">Depth test before drawing?</param>
		public static void DrawRect(Rect rect, Color color, float duration = 0, bool depthTest = true)
		{
			Vector3 bottomLeft = new Vector3(rect.xMin, rect.yMin);
			Vector3 bottomRight = new Vector3(rect.xMax, rect.yMin);
			Vector3 topRight = new Vector3(rect.xMax, rect.yMax);
			Vector3 topLeft = new Vector3(rect.xMin, rect.yMax);

			Debug.DrawLine(bottomLeft, bottomRight, color, duration, depthTest);
			Debug.DrawLine(bottomRight, topRight, color, duration, depthTest);
			Debug.DrawLine(topRight, topLeft, color, duration, depthTest);
			Debug.DrawLine(topLeft, bottomLeft, color, duration, depthTest);
		}

	}

}
