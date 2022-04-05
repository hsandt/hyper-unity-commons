using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace CommonsHelper
{
    /// Base class for path = series of connected curves
    /// Remember to make your child class [Serializable] to make it editable inside component
    public abstract class Path2D
    {
        /// Read-only access to list of control points
        public abstract ReadOnlyCollection<Vector2> ControlPoints { get; }

        /// Compute point on 2D curve (array of N points) at parameter t (polymorphic version)
        public abstract Vector2 Interpolate(Vector2[] curve, float t);

        public bool IsValid()
        {
            return IsListOfControlPointsValid(ControlPoints);
        }

        protected abstract bool IsListOfControlPointsValid(IReadOnlyCollection<Vector2> controlPoints);

        public abstract void SanitizePath();

        public void SubtractPathStartOffset()
        {
            Vector2 offset = GetPathStartPoint();
            MoveAllControlPointsByOffset(offset);
        }

        protected abstract void MoveAllControlPointsByOffset(Vector2 offset);

        #region PointAndCurveAccessors

        /// Return point at start of path
        /// Bezier: this is the first key point, and also the first control point
        /// Catmull-Rom: this is the second control point
        public abstract Vector2 GetPathStartPoint();

        /// Return point at end of path
        /// Bezier: this is the last key point, and also the last control point
        /// Catmull-Rom: this is the penultimate control point
        public abstract Vector2 GetPathEndPoint();

        /// Return count of key points
        /// Bezier: this exclude tangent points
        /// Catmull-Rom: this is the same as count of control points
        public abstract int GetKeyPointsCount();

        /// Return key point at given key index
        /// Bezier: this does a calculation to find the correct key point among ControlPoints
        /// Catmull-Rom: this is the same as ControlPoints[keyIndex]
        public abstract Vector2 GetKeyPoint(int keyIndex);

        /// Add key point at end of path
        public abstract void AddKeyPoint(Vector2 newKeyPoint);

        /// Insert key point at start of path
        public abstract void InsertKeyPointAtStart(Vector2 newKeyPoint);

        /// Split curve in two parts by inserting a key at [parameterRatio] along the curve at [curveIndex]
        public abstract void SplitCurveAtParameterRatio(int curveIndex, float parameterRatio);

        /// Remove key point at key index
        public abstract void RemoveKeyPoint(int keyIndex);

        /// Return the number of curves compounding this path
        public abstract int GetCurvesCount();

        /// Return curve at given key index
        public abstract Vector2[] GetCurve(int curveIndex);

        /// Yield each curve compounding the path, from start to end.
        public IEnumerable<Vector2[]> GetCurves()
        {
            int curvesCount = GetCurvesCount();
            for (int curveIndex = 0; curveIndex < curvesCount; curveIndex++)
            {
                yield return GetCurve(curveIndex);
            }
        }

        #endregion

        #region Interpolation

        /// Return the position on the whole path at given cumulated parameter
        /// (0 for start, 1 for end of 1st curve, N for end of N-th curve)
        public Vector2 InterpolatePathByParameter(float pathT)
        {
            int curvesCount = GetCurvesCount();

            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Assert(pathT >= 0f && pathT <= (float) curvesCount);
            #endif

            // handle edge cases (including cases that would normally assert)
            if (pathT <= 0)
            {
                return GetPathStartPoint();
            }
            if (pathT >= (float) curvesCount)
            {
                return GetPathEndPoint();
            }

            int curveIndex = Mathf.FloorToInt(pathT);

            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            // we have handled normalizedT == 1 case, so we cannot be right at the end
            // and the curveIndex should never reach curvesCount
            Debug.Assert(curveIndex < curvesCount);
            #endif

            float remainder = pathT - curveIndex;  // or pathT % 1f;
            return InterpolateCurve(curveIndex, remainder);
        }

        /// Return the position on the whole path at given normalized parameter
        /// (0 for start, 1 for end of path)
        /// Each curve is associated an equal range of values (1 / curvesCount),
        /// so if normalizedT is increased at constant rate, the interpolated point
        /// "spends" the same amount of time in every curve.
        /// However, its speed may still vary inside a curve (due to Bezier derivative
        /// not being constant) and between curves (longer curves will lead to faster
        /// progression in average).
        public Vector2 InterpolatePathByNormalizedParameter(float normalizedT)
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Assert(normalizedT >= 0f && normalizedT <= 1f);
            #endif

            int curvesCount = GetCurvesCount();
            float pathT = normalizedT * curvesCount;
            return InterpolatePathByParameter(pathT);
        }

        /// Return the position of an interpolated point on curve of index i at parameter t
        public Vector2 InterpolateCurve(int curveIndex, float t)
        {
            int curvesCount = GetCurvesCount();

            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.AssertFormat(curveIndex >= 0 && curveIndex < curvesCount, "Invalid curve index: {0}. Expected index between 0 and {1}.", curveIndex, curvesCount - 1);
            Debug.AssertFormat(0f <= t && t <= 1f, "Parameter t is {0}, expected number between 0 and 1.", t);
            #endif

            Vector2[] curve = GetCurve(curveIndex);
            return Interpolate(curve, t);
        }

        /// Return an evaluation of the length, as the sum of evaluated curve lengths,
        /// with [segmentsCountPerCurve] segments approximation per curve
        public float EvaluateLength(int segmentsCountPerCurve)
        {
            int curvesCount = GetCurvesCount();

            float length = 0f;

            for (int keyIndex = 0; keyIndex < curvesCount; ++keyIndex)
            {
                length += EvaluateCurveLength(keyIndex, segmentsCountPerCurve);
            }

            return length;
        }

        /// Return an evaluation of the length of curve of index i, as the sum of [segmentsCount] segment approximation
        /// lengths
        public float EvaluateCurveLength(int keyIndex, int segmentsCount)
        {
            Vector2[] curve = GetCurve(keyIndex);
            return EvaluateCurveLength(curve, segmentsCount);
        }

        /// Return an evaluation of the passed curve (array of points), as the sum of [segmentsCount] segment approximation
        /// lengths
        public float EvaluateCurveLength(Vector2[] curve, int segmentsCount)
        {
            // store end point of the previous iteration to reuse it as start point for the next one
            Vector2 previousPoint = curve[0];

            // initialize total length to return
            float length = 0f;

            for (int i = 0; i < segmentsCount; i++)
            {
                // compute segment end point parameter on curve
                // this is the next point after current i, hence i + 1
                float nextT = (float) (i + 1) / (float) segmentsCount;

                // locate segment end and add segment length to total length
                Vector2 nextPoint = Interpolate(curve, nextT);
                length += Vector2.Distance(previousPoint, nextPoint);

                // update previous point for next iteration
                previousPoint = nextPoint;
            }

            // return sum of all segment lengths
            return length;
        }

        #endregion
    }
}
