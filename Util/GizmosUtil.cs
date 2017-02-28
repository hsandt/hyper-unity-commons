using UnityEngine;
using System.Collections;

public static class GizmosUtil {

	/// <summary>
	/// Draw a line with local coordinates
	/// </summary>
	/// <param name="p1">Local 1st coordinates of the line.</param>
	/// <param name="p2">Local 2nd coordinates of the line.</param>
	/// <param name="tr">Transform used for local coordinates.</param>
	/// <param name="color">Optional draw color. Current gizmos color if not set.</param>
	public static void DrawLocalLine (Vector3 p1, Vector3 p2, Transform tr, Color? color = null) {
		Color oldColor = Gizmos.color;
		if (color != null)
			Gizmos.color = (Color) color;

		Matrix4x4 oldMatrix;
		SetGizmosMatrix(tr, out oldMatrix);

		Gizmos.DrawLine(p1, p2);

		Gizmos.matrix = oldMatrix;

		if (color != null)
			Gizmos.color = oldColor;
	}

	/// <summary>
	/// Draw a ray with local coordinates
	/// </summary>
	/// <param name="p1">Local origin of the ray.</param>
	/// <param name="p2">Direction (and distance) of the ray.</param>
	/// <param name="tr">Transform used for local coordinates.</param>
	/// <param name="preserveRayScale">Should direction length be preserved? If false, direction is scaled with transform tr.</param>
	/// <param name="color">Optional draw color. Current gizmos color if not set.</param>
	public static void DrawLocalRay (Vector3 localOrigin, Vector3 localDirection, Transform tr, bool preserveRayScale = true, Color? color = null) {
		Color oldColor = Gizmos.color;
		if (color != null)
			Gizmos.color = (Color) color;

		Vector3 origin = tr.TransformPoint(localOrigin);
		Vector3 direction = preserveRayScale ? tr.TransformDirection(localDirection) : tr.TransformVector(localDirection);
		Gizmos.DrawRay(origin, direction);

		if (color != null)
			Gizmos.color = oldColor;
	}

	/// <summary>
	/// Draw a polyline from an array of points, using the current gizmos parameter
	/// </summary>
	/// <param name="points">Array of points of the polyline.</param>
	public static void DrawPolyLine (Vector3[] points) {
		for (int i = 0; i < points.Length - 1; ++i) {
			Gizmos.DrawLine(points[i], points[i + 1]);
		}
	}

	/// <summary>
	/// Draw a closed polyline from an array of points, using the current gizmos parameter
	/// </summary>
	/// <param name="points">Array of points of the polyline not duplicating the 1st point as the last.</param>
	public static void DrawClosedPolyLine (Vector3[] points) {
		for (int i = 0; i < points.Length; ++i) {
			Gizmos.DrawLine(points[i], points[(i + 1) % points.Length]);
		}
	}

	public static void DrawLocalBox2D (float left, float right, float bottom, float top, Transform tr, Color? color = null) {
		Color oldColor = Gizmos.color;
		if (color != null)
			Gizmos.color = (Color) color;

		Matrix4x4 oldMatrix;
		SetGizmosMatrix(tr, out oldMatrix);

		Vector2[] corners = GetCornersFromLimits(left, right, bottom, top);

		// draw the 4 edges by cycling between pair of corners
		for (int i = 0; i < 4; ++i) {
			Gizmos.DrawLine((Vector3) corners[i], (Vector3) corners[(i + 1) % 4]);
		}

		Gizmos.matrix = oldMatrix;

		if (color != null)
			Gizmos.color = oldColor;
	}

	public static void DrawLocalBox2D (Vector2 offset, Vector2 size, Transform tr, Color? color = null) {
		Color oldColor = Gizmos.color;
		if (color != null)
			Gizmos.color = (Color) color;

		Matrix4x4 oldMatrix;
		SetGizmosMatrix(tr, out oldMatrix);

		Vector2[] corners = GetCornersFromBox2DParams(offset, size);

		// draw the 4 edges by cycling between pair of corners
		for (int i = 0; i < 4; ++i) {
			Gizmos.DrawLine((Vector3) corners[i], (Vector3) corners[(i + 1) % 4]);
		}

		Gizmos.matrix = oldMatrix;

		if (color != null)
			Gizmos.color = oldColor;
	}

	/// Draw a filled rectangle from offset and size in transform local frame
	public static void DrawLocalSquare (Vector2 offset, Vector2 size, Transform tr, Color? color = null) {
		Color oldColor = Gizmos.color;
		if (color != null) {
			Color baseColor = (Color) color;
			Gizmos.color = new Color(baseColor.r * 2f, baseColor.g * 2f, baseColor.b * 2f, baseColor.a);  // 3D gizmos are drawn with half color intensity, balance that
		}

		Matrix4x4 oldMatrix;
		SetGizmosMatrix(tr, out oldMatrix);

		// draw a flat cube to simulate a filled square
		Gizmos.DrawCube((Vector3) offset, (Vector3) size);

		Gizmos.matrix = oldMatrix;

		if (color != null)
			Gizmos.color = oldColor;
	}

	/// <summary>
	/// Draw the 2D part of 3D bounds at its center Z, with given color.
	/// Useful to draw 2D collider bounds at the object's Z.
	/// </summary>
	/// <param name="bounds">3D bounds to project on XY plane.</param>
	/// <param name="color">Draw color.</param>
	public static void DrawBounds2D(Bounds bounds, Color? color = null)
	{
		Color oldColor = Gizmos.color;
		if (color != null)
			Gizmos.color = (Color) color;

		Vector3[] corners = GetCornersFromBounds(bounds);

		// draw the 4 edges by cycling between pair of corners
		for (int i = 0; i < 4; ++i) {
			Gizmos.DrawLine(corners[i], corners[(i + 1) % 4]);
		}

		if (color != null)
			Gizmos.color = oldColor;
	}

	public static Vector2[] GetCornersFromLimits(float left, float right, float bottom, float top) {
		Vector2 bottomLeft = new Vector2(left, bottom);
		Vector2 bottomRight = new Vector2(right, bottom);
		Vector2 topRight = new Vector2(right, top);
		Vector2 topLeft = new Vector2(left, top);
		return new Vector2[] {bottomLeft, bottomRight, topRight, topLeft};
	}

	public static Vector2[] GetCornersFromBox2DParams(Vector2 offset, Vector2 size) {
		Vector2 extents = size / 2;
		Vector2 bottomLeft = offset - extents;
		Vector2 bottomRight = offset + new Vector2(extents.x, - extents.y);
		Vector2 topRight = offset + extents;
		Vector2 topLeft = offset + new Vector2(- extents.x, extents.y);
		return new Vector2[] {bottomLeft, bottomRight, topRight, topLeft};
	}

	/// Return 4 corners from bounds, in the order: bottom-left, bottom-right, top-right, top-left
	/// Note that it only preserves Z for the bounds center
	public static Vector3[] GetCornersFromBounds (Bounds bounds) {
		Vector3 center = bounds.center;
		Vector3 extents = bounds.extents;

		Vector3 bottomLeft = center + new Vector3(- extents.x, - extents.y);
		Vector3 bottomRight = center + new Vector3(extents.x, - extents.y);
		Vector3 topRight = center + new Vector3(extents.x, extents.y);
		Vector3 topLeft = center + new Vector3(- extents.x, extents.y);
		return new Vector3[] {bottomLeft, bottomRight, topRight, topLeft};
	}

	/// <summary>
	/// Draw a rect under a transform, ignoring its scale if one of the lossy scale coordinate is null
	/// </summary>
	/// <param name="rect">Rect to draw</param>
	/// <param name="owner">Rect owner transform</param>
	/// <param name="color">Draw color</param>
	public static void DrawRect (Rect rect, Transform owner, Color color) {
		Color oldColor = Gizmos.color;

		// if the rectangle is reversed, change the color to notify the user
		if (rect.width >= 0 && rect.height >= 0)
			Gizmos.color = color;
		else if (rect.width < 0 && rect.height >= 0)
			Gizmos.color = Color.Lerp(color, Color.yellow, 0.5f);
		else if (rect.width >= 0)
			Gizmos.color = Color.Lerp(color, Color.yellow, 0.5f);
		else
			Gizmos.color = Color.Lerp(color, Color.red, 0.5f);

		Matrix4x4 oldMatrix;
		SetGizmosMatrix(owner, out oldMatrix);

		// Draw rect edges
		var points = new Vector3[] {
			new Vector3(rect.xMin, rect.yMin),
			new Vector3(rect.xMax, rect.yMin),
			new Vector3(rect.xMax, rect.yMax),
			new Vector3(rect.xMin, rect.yMax)
		};

		DrawClosedPolyLine(points);

		Gizmos.matrix = oldMatrix;
		Gizmos.color = oldColor;
	}

	/// Store the current Gizmos matrix to oldMatrix reference, and set the Gizmos matrix to the local matrix
	// of the passed transform, ignoring scale if it has null components
	static void SetGizmosMatrix(Transform tr, out Matrix4x4 oldMatrix) {
		oldMatrix = Gizmos.matrix;

		// only use the local matrix if scale is valid (no null coordinates)
		// else, only consider position and rotation to avoid producing NaN values
		if (tr.lossyScale.x != 0 && tr.lossyScale.y != 0 && tr.lossyScale.z != 0)
			Gizmos.matrix = tr.localToWorldMatrix;
		else {
			Gizmos.matrix = Matrix4x4.TRS(tr.position, tr.rotation, Vector3.one);
		}
	}

}
