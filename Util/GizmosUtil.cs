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
	/// Draw an open polyline from an array of points, using the current gizmos parameter
	/// </summary>
	/// <param name="points">Array of points of the polyline.</param>
	/// <param name="tr">Transform used for local coordinates.</param>
	/// <param name="color">Optional draw color. Current gizmos color if not set.</param>
	public static void DrawLocalPolyLine (Vector3[] points, Transform tr, Color? color = null) {
		Matrix4x4 oldMatrix;
		SetGizmosMatrix(tr, out oldMatrix);

		DrawPolyLine(points, color);

		Gizmos.matrix = oldMatrix;
	}

	/// <summary>
	/// Draw an open polyline from an array of points, using the current gizmos parameter
	/// </summary>
	/// <param name="points">Array of points of the polyline.</param>
	/// <param name="color">Optional draw color. Current gizmos color if not set.</param>
	public static void DrawPolyLine (Vector3[] points, Color? color = null) {
		Color oldColor = Gizmos.color;
		if (color != null)
			Gizmos.color = (Color) color;
		
		for (int i = 0; i < points.Length - 1; ++i) {
			Gizmos.DrawLine(points[i], points[i + 1]);
		}

		if (color != null)
			Gizmos.color = oldColor;
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

		Vector2[] corners = Draw2DUtil.GetCornersFromLimits(left, right, bottom, top);

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

		Vector2[] corners = Draw2DUtil.GetCornersFromBox2DParams(offset, size);

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
            // 3D gizmos are drawn with half color intensity, balance by multiplying by 2
            Gizmos.color = new Color(baseColor.r * 2f, baseColor.g * 2f, baseColor.b * 2f, baseColor.a);
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

		Vector3[] corners = Draw2DUtil.GetCornersFromBounds(bounds);

		// draw the 4 edges by cycling between pair of corners
		for (int i = 0; i < 4; ++i) {
			Gizmos.DrawLine(corners[i], corners[(i + 1) % 4]);
		}

		if (color != null)
			Gizmos.color = oldColor;
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
	public static void SetGizmosMatrix(Transform tr, out Matrix4x4 oldMatrix) {
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
