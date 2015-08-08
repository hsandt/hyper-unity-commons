using UnityEngine;
using System.Collections;

static public class MathUtil {

	public const float SQRT2 = 1.4142f; // or just type the truncated value yourself

	public static int Truncate(float x) {
		return (int) (Mathf.Sign(x) * Mathf.Floor(Mathf.Abs(x)));
	}

	/// Return the positive remainder of Euclidian division
	public static int PositiveRemainder(int dividend, int divisor) {
		return (dividend % divisor + divisor) % divisor;
		// int signedRemainder = dividend % divisor;
		// if (signedRemainder >= 0) return signedRemainder;
		// return signedRemainder + divisor;
	}

	/* complement x on total in place */
	public static float Complement(ref float x, float total) {
		return total - x;
	}

}
