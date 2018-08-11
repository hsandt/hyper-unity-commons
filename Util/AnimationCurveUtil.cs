using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Commons.Helper
{

	public static class AnimationCurveUtil {

	    public static AnimationCurve CreateConstant (params Keyframe[] keys) {
	        AnimationCurve curve = new AnimationCurve(keys);
	        SetAllTangentsConstant(curve);
	        return curve;
	    }

	    public static AnimationCurve CreateLinear (params Keyframe[] keys) {
	        AnimationCurve curve = new AnimationCurve(keys);
	        SetAllTangentsLinear(curve);
	        return curve;
	    }

	    public static void SetAllTangentsConstant (AnimationCurve curve) {
	#if UNITY_EDITOR
	        for (int i = 0; i < curve.length; ++i) {
	            AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Constant);
	            AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Constant);
	        }
	#else
	        // This is a fallback in case I need to generate a curve in Standalone, but this implementation
	        // does not set the tangent flags, so in the editor, as soon as you start modifying the curve, tangents would revert.
	        // (code inspired by AnimationUtility.Internal_UpdateTangents)
	        for (int i = 0; i < curve.length; ++i) {
	            Keyframe key = curve[i];
	            key.inTangent = Mathf.Infinity;
	            key.outTangent = Mathf.Infinity;
	            curve.MoveKey(i, key);
	        }
	#endif
	    }

	    public static void SetAllTangentsLinear (AnimationCurve curve) {
	#if UNITY_EDITOR
	        for (int i = 0; i < curve.length; ++i) {
	            AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Linear);
	            AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Linear);
	        }
	#else
	        // This is a fallback in case I need to generate a curve in Standalone, but this implementation
	        // does not set the tangent flags, so in the editor, as soon as you start modifying the curve, tangents would revert.
	        // (code inspired by AnimationUtility.Internal_UpdateTangents)
	        for (int i = 0; i < curve.Length; ++i) {
	            if (i >= 1) {
	                Keyframe key = curve[i];
	                Keyframe previousKey = curve[i - 1];
	                key.inTangent = CalculateLinearTangent(previousKey, key);
	                curve.MoveKey(i, key);
	            }
	        if (i + 1 < curve.Length) {
	                Keyframe key = curve[i];
	                Keyframe nextKey = curve[i + 1];
	                key.outTangent = CalculateLinearTangent(key, nextKey);
	                curve.MoveKey(i, key);
	            }
	        }
	#endif
	    }

	    static float CalculateLinearTangent (Keyframe key1, Keyframe key2) {
	        float timeDiff = key1.time - key2.time;
	        if (Mathf.Approximately(timeDiff, 0f))
	            return 0f;
	        return (key1.value - key2.value) / timeDiff;
	    }

	    // @MitchStan
	    // https://answers.unity.com/questions/1259647/calculate-surface-under-a-curve-from-an-animationc.html
	    // I removed parameters float w and h because we don't need to draw the curve in a stretched window, we just want the actual integral
	    // I also renamed areaUnderCurve -> integral since the formula is generic and also works with negative values
	    public static float Integral(AnimationCurve curve)
	    {
	        float integral = 0f;
	        var keys = curve.keys;

	        for (int i = 0; i < keys.Length - 1; i++)
	        {
	            // Store the extreme interval points
	            Keyframe K1 = keys[i];
	            Keyframe K2 = keys[i + 1];
	            Vector2 A = new Vector2(K1.time, K1.value);
	            Vector2 D = new Vector2(K2.time, K2.value);

	            float intervalIntegral;

	            // If this portion of the curve is constant (i.e. either this key has a right tangent constant,
	            // or the next key has a left tangent constant), compute the integral directly as the signed area of a rectangle
	            if (float.IsInfinity(K1.outTangent) || float.IsInfinity(K2.inTangent)) {
	                intervalIntegral = A.y * (D.x - A.x);
	            }
	            else {
	                // Calculate the remaining 2 cubic Bezier control points from Unity AnimationCurve (a hermite cubic spline)
	                float e = (D.x - A.x) / 3.0f;
	                Vector2 B = A + new Vector2(e, e * K1.outTangent);
	                Vector2 C = D + new Vector2(-e, -e * K2.inTangent);

	                /*
	                 * The cubic Bezier curve function looks like this:
	                 *
	                 * f(x) = A(1 - x)^3 + 3B(1 - x)^2 x + 3C(1 - x) x^2 + Dx^3
	                 *
	                 * Where A, B, C and D are the control points and,
	                 * for the purpose of evaluating an instance of the Bezier curve,
	                 * are constants.
	                 *
	                 * Multiplying everything out and collecting terms yields the expanded polynomial form:
	                 * f(x) = (-A + 3B -3C + D)x^3 + (3A - 6B + 3C)x^2 + (-3A + 3B)x + A
	                 *
	                 * If we say:
	                 * a = -A + 3B - 3C + D
	                 * b = 3A - 6B + 3C
	                 * c = -3A + 3B
	                 * d = A
	                 *
	                 * Then we have the expanded polynomal:
	                 * f(x) = ax^3 + bx^2 + cx + d
	                 *
	                 * Whos indefinite integral is:
	                 * a/4 x^4 + b/3 x^3 + c/2 x^2 + dx + E
	                 * Where E is a new constant introduced by integration.
	                 *
	                 * The indefinite integral of the quadratic Bezier curve is:
	                 * (-A + 3B - 3C + D)/4 x^4 + (A - 2B + C) x^3 + 3/2 (B - A) x^2 + Ax + E
	                 */

	                float a, b, c, d;
	                a = -A.y + 3.0f * B.y - 3.0f * C.y + D.y;
	                b = 3.0f * A.y - 6.0f * B.y + 3.0f * C.y;
	                c = -3.0f * A.y + 3.0f * B.y;
	                d = A.y;

	                /*
	                 * a, b, c, d, now contain the y component from the Bezier control points.
	                 * In other words - the AnimationCurve Keyframe value * h data!
	                 *
	                 * What about the x component for the Bezier control points - the AnimationCurve
	                 * time data?  We will need to evaluate the x component when time = 1.
	                 *
	                 * x^4, x^3, X^2, X all equal 1, so we can conveniently drop this coefficient.
	                 *
	                 * Lastly, for each segment on the AnimationCurve we get the time difference of the
	                 * Keyframes and multiply by w.
	                 *
	                 * Iterate through the segments and add up all the areas for
	                 * the integral of the AnimationCurve!
	                 */

	                float t = K2.time - K1.time;

	                intervalIntegral = ((a / 4.0f) + (b / 3.0f) + (c / 2.0f) + d) * t;
	            }

	            integral += intervalIntegral;
	        }
	        return integral;
	    }

	}

}
