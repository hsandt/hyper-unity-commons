using UnityEngine;
using System.Collections;

public static class VectorUtil {

	/// Return vector projected on direction vector
	public static Vector2 ProjectParallel (Vector2 vector, Vector2 direction) {
		// p = (<v, e> / ||e||^2) * e
		if (direction == Vector2.zero) {
			throw ExceptionsUtil.CreateExceptionFormat("Cannot project on null direction");
		}
		return Vector2.Dot(vector, direction) / direction.sqrMagnitude * direction;
	}

	/// Return vector projected orthogonally to normal
	public static Vector2 ProjectOrthogonal (Vector2 vector, Vector2 normal) {
		// q = v - p
		return vector - ProjectParallel(vector, normal);
		// return (Vector2) Vector3.ProjectOnPlane((Vector3) vector, (Vector3) normal);
	}

	/// Return vector rotated by 90 clockwise
	public static Vector2 Rotate90CW (Vector2 vector) {
		return new Vector2(vector.y, -vector.x);
	}

}
