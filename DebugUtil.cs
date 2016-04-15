using UnityEngine;
using System.Collections;

public static class DebugUtil {

	/// <summary>
	/// Debug the 2D part of 3D bounds at its center Z, with given color, for given duration.
	/// Useful to debug 2D collider bounds at the object's Z.
	/// </summary>
	/// <param name="bounds">3D bounds to project on XY plane.</param>
	/// <param name="color">Draw color.</param>
	/// <param name="duration">Debug duration.</param>
	/// <param name="depthTest">Depth test before drawing?</param>
	public static void DebugBounds2D(Bounds bounds, Color color, float duration = 0, bool depthTest = true)
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
	/// Debug a Rect as if its coordinates were world coordinates (origin at bottom-left)
	/// Here, Rect is used as a utility class, without UI / screen semantics
	/// </summary>
	/// <param name="rect">Rect in world coordinates.</param>
	/// <param name="color">Draw color.</param>
	/// <param name="duration">Debug duration.</param>
	/// <param name="depthTest">Depth test before drawing?</param>
	public static void DebugRect(Rect rect, Color color, float duration = 0, bool depthTest = true)
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
