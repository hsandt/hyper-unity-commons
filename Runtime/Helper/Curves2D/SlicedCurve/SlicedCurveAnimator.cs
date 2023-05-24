using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HyperUnityCommons
{
    /// A Sliced Curve Animator handles a Sliced Curve
    /// It can be started, tracks its own slice phase and time, and must be
    /// updated before being evaluated.
    /// To start outro, user must manually call PlayOutro if condition is true or some event is sent.
    /// Note that there if sliced curve is still in Intro phase, PlayOutro will still interrupt the intro and play the outro
    /// immediately for safety, possibly suddenly changing the evaluated value. If this is an issue, you can manually check
    /// the slice phase first.
    ///
    /// Usage:
    ///
    /// 1. Init
    /// var slicedCurveAnimator = new SlicedCurveAnimator();
    /// slicedCurveAnimator.StartCurve(MyData.slicedCurve);
    /// // From here, slicedCurveAnimator.IsRunning() returns true until next Setup()
    ///
    /// 2. Update & Evaluate (can be done in different methods as long as Update is done first)
    /// slicedCurveAnimator.Update(Time.deltaTime)
    /// float value = slicedCurveAnimator.Evaluate();
    /// // use value...
    ///
    /// 3. Start outro
    /// if (custom outro condition / event received)
    /// {
    ///     slicedCurveAnimator.PlayOutro();
    /// }
    ///
    /// 4. End
    /// if (slicedCurveAnimator.HasReachedEnd())
    /// {
    ///     // End process using the sliced curve so we don't update it anymore
    ///     // You can also call slicedCurveAnimator.Setup() to clear everything.
    /// }
    public class SlicedCurveAnimator
    {
        /* Injected parameters */

        /// Current sliced curve used for evaluation
        private SlicedCurve m_SlicedCurve;


        /* State vars */

        /// Current slice in the sliced curve
        private SlicedCurvePhase m_CurrentPhase;
        public SlicedCurvePhase CurrentPhase => m_CurrentPhase;

        /// Derived: Current slice curve (derived from m_CurrentSlicedCurvePhase)
        /// If no curve is associated to the current phase, this is null
        private AnimationCurve m_CurrentSliceCurve;

        /// Current time inside the current slice
        private float m_CurrentSliceTime;
        #if UNITY_EDITOR
        public float CurrentSliceTime => m_CurrentSliceTime;
        #endif

        public SlicedCurveAnimator()
        {
            Setup();
        }

        /// Setup all members, including injected parameters, to default values
        /// so that this animator is assigned to no sliced curve
        /// Can be called to clear animator parameters and state completely
        public void Setup()
        {
            m_SlicedCurve = null;

            // This will set phase to None, m_CurrentSlice to null and slice time to 0f
            EnterPhase(SlicedCurvePhase.None);
        }

        /// Start the passed curve with Intro phase
        public void StartCurve(SlicedCurve slicedCurve)
        {
            #if UNITY_EDITOR
            Debug.AssertFormat(slicedCurve != null,
                "[SlicedCurveAnimator] StartCurve: passed slicedCurve is null");
            #endif

            m_SlicedCurve = slicedCurve;

            // Enter Intro phase so we can start updating and evaluating the animator
            EnterPhase(SlicedCurvePhase.Intro);
        }

        /// Start Outro phase, interrupting current phase
        /// UB unless current phase is Intro or Sustain
        public void PlayOutro()
        {
            if (CanPlayOutro())
            {
                EnterPhase(SlicedCurvePhase.Outro);
            }
            #if UNITY_EDITOR
            else
            {
                Debug.LogErrorFormat("[SlicedCurveAnimator] PlayOutro: phase is {0}, expected to play outro from " +
                    "Intro or Sustain",
                    m_CurrentPhase);
            }
            #endif
        }

        /// Enter phase, set derivated slice curve appropriately and reset slice time to 0
        private void EnterPhase(SlicedCurvePhase phase)
        {
            m_CurrentPhase = phase;

            // Get curve corresponding to current phase, if any
            // If there is no sliced curve at all (when phase is None), there is no curve either
            m_CurrentSliceCurve = m_SlicedCurve?.GetCurveForPhase(phase);

            // Note that we don't try to preserve any remainder (time overshoot from last phase)
            // by subtracting the last phase's duration; since Sustain phase doesn't track time, and
            // Outro starts at an arbitrary time when PlayOutro is called, there is no time leftover
            // from the previous phase, so just reset it.
            m_CurrentSliceTime = 0f;
        }

        public void Update(float deltaTime)
        {
            if (m_CurrentSliceCurve != null)
            {
                // We are indeed using a curve for this phase (should be Intro or Outro),
                // so we must update time and
                m_CurrentSliceTime += deltaTime;

                // Check if the current curve is finished
                if (m_CurrentSliceTime > m_CurrentSliceCurve.GetDuration())
                {
                    // Advance to next phase
                    // Note that the last phase, End, has no associated slice,
                    // so if we enter this, we cannot be in End phase, and therefore
                    // increment the phase enum index is always valid
                    EnterPhase(m_CurrentPhase + 1);
                }
            }
        }

        /// Evaluate the sliced curve at the current slice and slice time
        public float Evaluate()
        {
            if (m_SlicedCurve != null)
            {
                return m_SlicedCurve.Evaluate(m_CurrentPhase, m_CurrentSliceTime);
            }

            Debug.LogErrorFormat("[SlicedCurveAnimator] Evaluate: sliced curve is null, " +
                "cannot Evaluate. Make sure you called StartCurve on the SlicedCurveAnimator since last Setup().");
            return 0f;
        }

        /// Is the sliced curve animator running?
        /// Note that it returns true even in phase End, which has a valid value for Evaluate (Outro's last value).
        /// To check for phase End, use HasReachedEnd.
        /// If true, we expect m_SlicedCurve to be set.
        public bool IsRunning()
        {
            return m_CurrentPhase != SlicedCurvePhase.None;
        }

        /// Can we play outro now?
        /// True iff current phase is Intro of Sustain.
        public bool CanPlayOutro()
        {
            return m_CurrentPhase is SlicedCurvePhase.Intro or SlicedCurvePhase.Sustain;
        }

        /// Has the animator reached the end of the sliced curve?
        public bool HasReachedEnd()
        {
            return m_CurrentPhase == SlicedCurvePhase.End;
        }
    }
}
