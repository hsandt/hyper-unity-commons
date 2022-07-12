using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// A Sliced Curve Animator handles a Sliced Curve + a check stop loop callback.
/// It contains an internal slice-time state and can be evaluated at any time.
/// However, it must be updated every frame before being evaluated.
public class SlicedCurveAnimator {

    /* Animation parameters */

	/// Current sliced curve used for evaluation
	SlicedCurve slicedCurve;
	
    /// Function used to check if we should stop the loop and enter the outro slice. If null, loop is never stopped.
    Func<bool> checkStopLoopCallback;


    /* State vars */

    /// Current slice in the sliced curve
    Slice currentSlice;

	/// Method to call on update for the current slice (derived attribute of slicedCurve and currentSlice)
	Action currentUpdateCallback;
	
    /// Current time inside the current slice
    float currentSliceTime;


    public SlicedCurveAnimator () {}

    public void StartCurve (SlicedCurve _slicedCurve, Func<bool> _checkStopLoopCallback = null) {
		slicedCurve = _slicedCurve;
		checkStopLoopCallback = _checkStopLoopCallback;

        currentSlice = Slice.Intro;
        currentUpdateCallback = UpdateIntro;
        currentSliceTime = 0f;
    }

	public void Update (float deltaTime) {
        if (currentSlice == Slice.Intro || currentSlice == Slice.Outro)
            currentSliceTime += deltaTime;
        if (currentUpdateCallback != null)
            currentUpdateCallback();
	}

    /// Evaluate the sliced curve at the current slice and slice time
    public float Evaluate () {
        Debug.Assert (slicedCurve != null, "SlicedCurveAnimator.Evaluate: sliced curve is null, cannot Evaluate. Make sure you called StartCurve on the SlicedCurveAnimator previously.");
        return slicedCurve.Evaluate(currentSlice, currentSliceTime);
    }

    /// Has the animator reached the end of the sliced curve?
    public bool HasReachedEnd() {
        return currentSlice == Slice.End;
    }
	
    void UpdateIntro () {
        if (currentSliceTime > slicedCurve.GetIntroCurveDuration()) {
            currentSlice = Slice.Loop;
			currentSliceTime = 0f;
            currentUpdateCallback = UpdateLoop;
        }
	}

    void UpdateLoop () {
        if (checkStopLoopCallback != null && checkStopLoopCallback()) {
            currentSlice = Slice.Outro;
            currentSliceTime = 0f;
            currentUpdateCallback = UpdateOutro;
        }
    }

    void UpdateOutro () {
        if (currentSliceTime > slicedCurve.GetOutroCurveDuration()) {
            currentSlice = Slice.End;
            currentSliceTime = 0f;
            currentUpdateCallback = null;
        }
    }

}
