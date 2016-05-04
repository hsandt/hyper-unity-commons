using UnityEngine;
using System.Collections;

public static class GizmosUtil {

	/// <summary>
	/// Draw a line with local coordinates, with current gizmos parameters
	/// </summary>
	/// <param name="p1">Local 1st coordinates of the line.</param>
	/// <param name="p2">Local 2nt coordinates of the line.</param>
	public static void DrawLocalLine (Transform tr, Vector3 p1, Vector3 p2, Color? color = null) {
		Color oldColor = Gizmos.color;
		if (color != null)
			Gizmos.color = (Color) color;

		Gizmos.DrawLine(tr.TransformPoint(p1), tr.TransformPoint(p2));
		
		if (color != null)
			Gizmos.color = oldColor;
	}

	/// <summary>
	/// Draw the 2D part of 3D bounds at its center Z, with given color.
	/// Useful to draw 2D collider bounds at the object's Z.
	/// </summary>
	/// <param name="bounds">3D bounds to project on XY plane.</param>
	/// <param name="color">Draw color.</param>
	public static void DrawBounds2D(Bounds bounds, Color color)
	{
		Vector3 center = bounds.center;

		float x = bounds.extents.x;
		float y = bounds.extents.y;

		Vector3 bottomLeft = center + new Vector3(-x, -y);
		Vector3 bottomRight = center + new Vector3(x, -y);
		Vector3 topRight = center + new Vector3(x, y);
		Vector3 topLeft = center + new Vector3(-x, y);

		Color oldColor = Gizmos.color;
		Gizmos.color = color;

		Gizmos.DrawLine(bottomLeft, bottomRight);
		Gizmos.DrawLine(bottomRight, topRight);
		Gizmos.DrawLine(topRight, topLeft);
		Gizmos.DrawLine(topLeft, bottomLeft);

		Gizmos.color = oldColor;
	}

}
