using UnityEngine;

namespace HyperUnityCommons
{
    public static class RandomUtil
    {
        /// Return a random value in [0; exclusiveMax) (exclusive upper bound)
        /// Default exclusiveMax is 1f, so you can use FloatExclusive() directly instead Random.value
        /// to get a random value in [0; 1) (exclusive upper bound).
        public static float FloatExclusive(float exclusiveMax = 1f)
        {
            // First, use Unity's native random value to get a value in inclusive range
            float inclusiveRandomValue = Random.value;

            // Second, clamp the value to exclude upper bound 1. While 0.999... is not the actual greatest float below 1,
            // it's good enough for our purpose.
            // If you really want to get the float just below 1, or exclusiveMax if you clamp after scaling,
            // see https://stackoverflow.com/questions/14278248/find-the-float-just-below-a-value
            float exclusiveRandomValue = Mathf.Min(inclusiveRandomValue, 0.9999999f);

            // Finally, apply scaling (technically, it will also scale the small space between 0.999... and 1,
            // but again, it's good enough for our purpose).
            return exclusiveRandomValue * exclusiveMax;
        }
    }
}
