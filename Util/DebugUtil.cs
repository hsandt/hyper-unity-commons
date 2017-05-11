// DEPENDENCIES: Commons-Helper Extensions

using UnityEngine;
using System.Collections;

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
	/// Draw a local 2D box at the given Z, with given color, for given duration.
	/// </summary>
	/// <param name="offset">Center of the box</param>
	/// <param name="size">Box size</param>
	/// <param name="angle">Box angle</param>
	/// <param name="z">Z at which to draw.</param>
	/// <param name="color">Color of the line.</param>
	/// <param name="duration">How long the line should be visible (s).</param>
	/// <param name="depthTest">Should the line be obscured by objects closer to the camera?</param>
	public static void DrawLocalBox2D (Vector2 offset, Vector2 size, float angle, float z, Color color, float duration = 0f, bool depthTest = true) {
		Vector2[] corners = Draw2DUtil.GetCornersFromBox2DParams(offset, size, angle);

		// draw the 4 edges by cycling between pair of corners
		for (int i = 0; i < 4; ++i) {
			Debug.DrawLine(corners[i].ToVector3(z), corners[(i + 1) % 4].ToVector3(z), color, duration, depthTest);
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
