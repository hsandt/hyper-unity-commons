using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// Class containing helper methods for GizmosUtil and DebugUtil (for 2D)
public static class Draw2DUtil {

	public static Vector2[] GetCornersFromLimits(float left, float right, float bottom, float top) {
		Vector2 bottomLeft = new Vector2(left, bottom);
		Vector2 bottomRight = new Vector2(right, bottom);
		Vector2 topRight = new Vector2(right, top);
		Vector2 topLeft = new Vector2(left, top);
		return new Vector2[] {bottomLeft, bottomRight, topRight, topLeft};
	}

	public static Vector2[] GetCornersFromBox2DParams(Vector2 offset, Vector2 size, float angle = 0) {
		Vector2 extents = size / 2;
		Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);

		Vector2 centerToTopLeft = (Vector2) (rotation * new Vector3(- extents.x, extents.y));
		Vector2 centerToTopRight = (Vector2) (rotation * (Vector3) extents);

		Vector2 bottomLeft = offset - centerToTopRight;
		Vector2 bottomRight = offset - centerToTopLeft;
		Vector2 topRight = offset + centerToTopRight;
		Vector2 topLeft = offset + centerToTopLeft;

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

}
