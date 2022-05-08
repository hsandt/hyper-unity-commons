using UnityEngine;

namespace CommonsHelper
{
    public static class MathUtil
    {
        /// Square root of 2 with precision of 4 decimals
        public const float SQRT2 = 1.4142f;

        /// Return ternary value corresponding to x
        /// Equivalent to returning sign of x as an integer.
        /// Unlike Mathf.Sign(), it returns 0 for 0 and returns an int.
        public static int ToTernary(float x)
        {
            if (x < 0) return -1;
            else if (x > 0) return 1;
            else return 0;
        }

        /// Return x without its fractional part, i.e. first integer from x when going toward 0
        public static int Truncate(float x)
        {
            return (int) (Mathf.Sign(x) * Mathf.Floor(Mathf.Abs(x)));
        }

        /// Return input rounded to nearest multiple of snapValue
        /// This uses Mathf.Round and therefore follows Banker's round,
        /// choosing the nearest even of input / snapValue if fraction is +/-0.5
        /// (so it rounds up or down every other time)
        public static float Round(float input, float snapValue)
        {
            return snapValue * Mathf.Round(input / snapValue);
        }

        /// Return the positive remainder of Euclidian division
        public static int PositiveRemainder(int dividend, int divisor)
        {
            return (dividend % divisor + divisor) % divisor;
        }

        /// Complement x on total in place
        public static void Complement(ref float x, float total)
        {
            x = total - x;
        }

        /// Decrease value in place until 0
        public static void CountDown(ref float t, float delta)
        {
            if (t > 0f)
            {
                t -= delta;
                if (t < 0f) t = 0f;
            }
        }

        /// Return the signed smallest angle from angle from to angle to, in degrees, in both angles are between 0 and 360 or -180 and 180
        public static float GetSignedSmallestAngle(float from, float to)
        {
            float delta = to - from;
            if (delta < -180f) delta += 360f;
            if (delta > 180f) delta -= 360f;
            return delta;
        }

        /// Remap a value with an affine that maps xA => yA, xB => yB, and clamp the result to [vA, vB]
        public static float Remap(float xA, float xB, float yA, float yB, float x)
        {
            return Mathf.Lerp(yA, yB, (x - xA) / (xB - xA));
        }
    }
}