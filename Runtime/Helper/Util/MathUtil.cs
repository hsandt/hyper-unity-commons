using System;
using UnityEngine;

namespace HyperUnityCommons
{
    public static class MathUtil
    {
        /// Square root of 2 with precision of 4 decimals
        public const float SQRT2 = 1.4142f;

        // Pseudo-constant used for volume decibel <-> factor conversion
        // It seems that Unity uses a 1/20 convention so multiply by 20 (instead of 10)
        // This means that we have half volume for around -6dB (instead of -3dB)
        public static readonly float dbLogFactor = 1 / 20f * Mathf.Log(10f);

        /// Minimum volume change allowed in Unity in the Audio Mixer
        public const float MIN_VOLUME_DB = -80f;

        /// Maximum volume change allowed in Unity in the Audio Mixer
        public const float MAX_VOLUME_DB = 20f;


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
        /// Use this instead of C# modulo % when dealing with negative numbers to guarantee a remainder
        /// between 0 and abs(divisor) - 1
        public static int PositiveRemainder(int dividend, int divisor)
        {
            return (dividend % divisor + Mathf.Abs(divisor)) % divisor;
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

        private static float RemapWithLerpCallback(float xA, float xB, float yA, float yB, float x,
            Func<float, float, float, float> lerpCallback)
        {
            float xDelta = xB - xA;

            if (xDelta == 0f)
            {
                #if UNITY_EDITOR
                if (yA != yB)
                {
                    Debug.LogErrorFormat("[MathUtil] RemapWithLerpCallback: xA and xB have same value {0}, yet yA ({1}) != yB ({2}). " +
                        "Since we cannot divide by 0, we will fall back to yA {1}, but that will be arbitrary " +
                        "(when yA == yB, this case is tolerated)", xA, yA, yB);
                }
                #endif

                return yA;
            }

            return lerpCallback(yA, yB, (x - xA) / xDelta);
        }

        /// Remap a value with an affine that maps xA => yA, xB => yB, and clamp the result to [yA, yB]
        public static float Remap(float xA, float xB, float yA, float yB, float x)
        {
            return RemapWithLerpCallback(xA, xB, yA, yB, x, Mathf.Lerp);
        }

        /// Remap a value with an affine that maps xA => yA, xB => yB, with no clamping
        public static float RemapUnclamped(float xA, float xB, float yA, float yB, float x)
        {
            return RemapWithLerpCallback(xA, xB, yA, yB, x, Mathf.LerpUnclamped);
        }

        /// Convert a volume in decibels to a volume factor
        public static float VolumeDbToFactor(float volumeDb)
        {
            if (volumeDb <= MIN_VOLUME_DB)
            {
                return 0f;
            }

            float clampedVolumeDb = Mathf.Min(volumeDb, MAX_VOLUME_DB);
            float clampedVolumeFactor = Mathf.Exp(dbLogFactor * clampedVolumeDb);
            return clampedVolumeFactor;
        }

        /// Convert a volume factor to a volume in decibels
        public static float VolumeFactorToDb(float volumeFactor)
        {
            if (volumeFactor <= 0f)
            {
                // Unfortunately, there is no way to really mute a sound in Unity without pausing / stopping the sound,
                // and without middleware. So, as a trick, we reduce the volume to the minimum allowed by the Audio Mixer,
                // -80 dB, which will effectively make it inaudible, but still playing in the background.
                return MIN_VOLUME_DB;
            }

            float unclampedVolumeDb = Mathf.Log(volumeFactor) / dbLogFactor;
            float clampedVolumeDb = Mathf.Min(unclampedVolumeDb, MAX_VOLUME_DB);
            return clampedVolumeDb;
        }
    }
}
