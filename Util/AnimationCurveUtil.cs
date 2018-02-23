using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public static class AnimationCurveUtil {

    public static AnimationCurve CreateLinear (params Keyframe[] keys) {
        AnimationCurve curve = new AnimationCurve(keys);
#if UNITY_EDITOR
        for (int i = 0; i < keys.Length; ++i) {
            AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Linear);
            AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Linear);
        }
#else
        // code inspired by AnimationUtility.Internal_UpdateTangents
        for (int i = 0; i < keys.Length; ++i) {
            if (i >= 1) {
                Keyframe key = curve[i];
                Keyframe previousKey = curve[i - 1];
                key.inTangent = CalculateLinearTangent(previousKey, key);
                curve.MoveKey(i, key);
            }
            if (i + 1 < keys.Length) {
                Keyframe key = curve[i];
                Keyframe nextKey = curve[i + 1];
                key.outTangent = CalculateLinearTangent(key, nextKey);
                curve.MoveKey(i, key);
            }
        }
#endif
        return curve;
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
        int nbKeys = curve.keys.Length;
        for (int i = 0; i < nbKeys; ++i) {
            if (i >= 1) {
                Keyframe key = curve[i];
                Keyframe previousKey = curve[i - 1];
                key.inTangent = CalculateLinearTangent(previousKey, key);
                curve.MoveKey(i, key);
            }
            if (i + 1 < nbKeys) {
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

}
