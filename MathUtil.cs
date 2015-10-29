using UnityEngine;
using System.Collections;

public static class MathUtil {

	public const float SQRT2 = 1.4142f; // or just type the truncated value yourself

	public static int Truncate (float x) {
		return (int) (Mathf.Sign(x) * Mathf.Floor(Mathf.Abs(x)));
	}

	/// Return the positive remainder of Euclidian division
	public static int PositiveRemainder (int dividend, int divisor) {
		return (dividend % divisor + divisor) % divisor;
		// int signedRemainder = dividend % divisor;
		// if (signedRemainder >= 0) return signedRemainder;
		// return signedRemainder + divisor;
	}

	/* complement x on total in place */
	public static float Complement (ref float x, float total) {
		return total - x;
	}

}


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

}
