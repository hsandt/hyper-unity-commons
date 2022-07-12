using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// A Sliced Curve is a curve made of 3 parts: Intro, Loop, Outro.
/// The concept is similar to music, where the intro and the outro is played once,
/// and the loop is repeated, except our loop is just a constant value.
/// The Sliced Curve can be evaluated at a given slice and time.
/// Use a Sliced Curve Animator to easily handle slice and time.
/// Set the loop value to 1f and normalize the intro and outro curves if you need a normalized sliced curve.
[System.Serializable]
public class SlicedCurve {

    /* Parameters */

    public AnimationCurve introCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.5f, 1f));
    public float loopValue = 1f;
    public AnimationCurve outroCurve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(0.5f, 0f));


    public float GetIntroCurveDuration () {
        return introCurve.keys.Last().time;
    }

    public float GetOutroCurveDuration () {
        return outroCurve.keys.Last().time;
    }

    public float Evaluate (Slice slice, float sliceTime) {
        switch (slice) {
            case Slice.Intro:
                return introCurve.Evaluate(sliceTime);
            case Slice.Loop:
                return loopValue;
            case Slice.Outro:
                return outroCurve.Evaluate(sliceTime);
            default:  // Slice.End
                // 0f is a reasonable default beyond the end of the curve (e.g. to stop any motion when using a speed curve)
                return 0f;
        }
	}

}
