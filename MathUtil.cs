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

	/// Complement x on total in place
	public static void Complement (ref float x, float total) {
		x = total - x;
	}

	/// Decrease value in place until 0
	public static void CountDown (ref float t, float delta) {
		if (t > 0f) {
			t -= delta;
			if (t < 0f) t = 0f;
		}
	}

}
