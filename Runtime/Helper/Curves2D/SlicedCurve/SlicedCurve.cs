using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace HyperUnityCommons
{
    /// A Sliced Curve is a curve made of 3 parts: Intro, Sustain, Outro.
    /// The concept is similar to music, where the intro and the outro is played once,
    /// and the loop is repeated, except our loop is just a constant value, so we called it "sustain" as in ADSR envelopes.
    /// It can be used e.g. for easing motion over time, with a Sustain phase of undefined duration.
    /// The Sliced Curve can be evaluated at a given slice and time.
    /// Use a Sliced Curve Animator to easily handle slice and time.
    /// Set the sustain value to 1f and normalize the intro and outro curves if you need a normalized sliced curve.
    [Serializable]
    public class SlicedCurve
    {
        /* Parameters */

        [Tooltip("Curve defining value evolution during Intro phase")]
        public AnimationCurve introCurve = new(new Keyframe(0f, 0f), new Keyframe(0.5f, 1f));

        [Tooltip("Value used during the Sustain phase")]
        [FormerlySerializedAs("loopValue")]
        public float sustainValue = 1f;

        [Tooltip("Curve defining value evolution during Outro phase")]
        public AnimationCurve outroCurve = new(new Keyframe(0f, 1f), new Keyframe(0.5f, 0f));


        /// Return curve corresponding to passed phase, or null if there is none for this phase
        public AnimationCurve GetCurveForPhase(SlicedCurvePhase phase)
        {
            return phase switch
            {
                SlicedCurvePhase.Intro => introCurve,
                SlicedCurvePhase.Outro => outroCurve,
                _ => null
            };
        }

        public float Evaluate(SlicedCurvePhase phase, float sliceTime)
        {
            return phase switch
            {
                SlicedCurvePhase.Intro => introCurve.Evaluate(sliceTime),
                SlicedCurvePhase.Sustain => sustainValue,
                SlicedCurvePhase.Outro => outroCurve.Evaluate(sliceTime),
                // Once sliced curve end has been reached, just stick to the last value of the Outro curve
                // (this should be a reasonable value for your purpose, e.g. 0 for a speed curve if you want to stop motion)
                SlicedCurvePhase.End => outroCurve.keys.Last().value,
                // We should never be in None phase yet has a sliced curve defined, so either there is a code error,
                // or we added a new, unhandled phase
                _ => throw new ArgumentOutOfRangeException(nameof(phase), phase, "[SlicedCurve] Evaluate: Invalid phase")
            };
        }
    }
}
