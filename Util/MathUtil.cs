using UnityEngine;

namespace CommonsHelper
{

	public static class MathUtil {

	    public const float SQRT2 = 1.4142f;

	    public static int ToTernary(float x) {
	        if (x < 0)
	            return -1;
	        else if (x > 0)
	            return 1;
	        else
	            return 0;
	    }

	    public static int Truncate (float x) {
	        return (int) (Mathf.Sign(x) * Mathf.Floor(Mathf.Abs(x)));
	    }

	    /// Return the positive remainder of Euclidian division
	    public static int PositiveRemainder (int dividend, int divisor) {
	        return (dividend % divisor + divisor) % divisor;
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

	    /// Return the signed smallest angle from angle from to angle to, in degrees, in both angles are between 0 and 360 or -180 and 180
	    public static float GetSignedSmallestAngle (float from, float to) {
	        float delta = to - from;
	        if (delta < -180f) delta += 360f;
	        if (delta > 180f) delta -= 360f;
	        return delta;
	    }

	}

}
