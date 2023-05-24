using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace CommonsHelper
{
    /// A Catmull-Rom path (spline) is a series of connected Catmull-Rom curves
    [Serializable]
    public class CatmullRomPath2D : Path2D
    {
        private static float GetKnotInterval(Vector2 a, Vector2 b, float alpha)
        {
            return Mathf.Pow(Vector2.SqrMagnitude(a - b), 0.5f * alpha);
        }

        /// Compute point on 2D CatmullRom curve of control points p0, p1, p2, p3 with distance exponent alpha
        /// at normalized parameter t
        /// When alpha = 0.5, this is Centripetal Catmull-Rom.
        /// Note that we map t in [0, 1] to the curve portion of interested between p1 and p2, ignoring extra parts
        /// linking p0 and p1, p2 and p3.
        public static Vector2 InterpolateCatmullRom(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float alpha, float t)
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.AssertFormat(0 <= t && t <= 1, "[CatmullRomPath2D] InterpolateCatmullRom: t is {0}, " +
                "expected value between 0 and 1", t);
            #endif

            // https://en.wikipedia.org/wiki/Centripetal_Catmull%E2%80%93Rom_spline
            // Unity code example
            float t0 = 0;
            float t1 = GetKnotInterval(p0, p1, alpha) + t0;
            float t2 = GetKnotInterval(p1, p2, alpha) + t1;
            float t3 = GetKnotInterval(p2, p3, alpha) + t2;

            // We are working with t in [0, 1] mapping to the central part of the curve, between p1 and p2,
            // and we are not interested in the extra parts linking p0 and p1, p2 and p3.
            // So remap t as u between t1 and t2, so t = 0 -> p1 amd t = 1 -> p2.
            // Lerp would work too, but since we've already asserted t in [0, 1],
            // LerpUnclamped is slightly less work.
            float u = Mathf.LerpUnclamped(t1, t2, t);

            // Here, u is in [t1, t2] only so we need an unclamped remap for a1, a3, b1 and b2
            // We may as well use an unclamped remap for a2 and return too since it's less work.
            Vector2 a1 = VectorUtil.RemapUnclamped(t0, t1, p0, p1, u);
            Vector2 a2 = VectorUtil.RemapUnclamped(t1, t2, p1, p2, u);
            Vector2 a3 = VectorUtil.RemapUnclamped(t2, t3, p2, p3, u);

            Vector2 b1 = VectorUtil.RemapUnclamped(t0, t2, a1, a2, u);
            Vector2 b2 = VectorUtil.RemapUnclamped(t1, t3, a2, a3, u);

            return VectorUtil.RemapUnclamped(t1, t2, b1, b2, u);
        }

        /// Compute point on 2D CatmullRom curve (array of 4 points) with tension alpha at parameter t
        public static Vector2 InterpolateCatmullRom(Vector2[] curve, float alpha, float t)
        {
            return InterpolateCatmullRom(curve[0], curve[1], curve[2], curve[3], alpha, t);
        }

        /// Compute point on 2D CatmullRom curve (array of 4 points) with current tension alpha at parameter t
        /// (polymorphic version)
        public override Vector2 Interpolate(Vector2[] curve, float t)
        {
            return InterpolateCatmullRom(curve, m_Alpha, t);
        }


        [SerializeField, Tooltip("List of control points of each successive CatmullRom curve, concatenated. " +
             "A curve is defined by 4 successive control points (P0, P1, P2, P3), but is only used to interpolate the" +
             "section between the two middle control points (P1 and P2). Therefore, the two extreme points of a curve" +
             "(the first and the last ones) are not part of the interpolated path. However, to avoid \"wasting\" points" +
             "and ensuring curve continuity (and derivative continuity), we reuse those extreme points as middle points" +
             "for the neighboring curves (if any)." +
             "This means that:" +
             "- curves are overlapping, with two neighbor curves sharing 3 points (but only one is a middle point on both)" +
             "- each control point, except the ones near the extremities, is shared by 4 curves, playing a different role" +
             "  for each" +
             "- to iterate on curves, we advance point by point, without skipping any" +
             "A valid path must have at least 4 points.")]
        private List<Vector2> m_ControlPoints;
        public override ReadOnlyCollection<Vector2> ControlPoints => m_ControlPoints.AsReadOnly();

        [SerializeField, Tooltip("Catmull-Rom tension, used as exponent of distance. Increase for tighter curves.")]
        [Range(0f, 1f)]
        private float m_Alpha = 0.5f;

        public CatmullRomPath2D()
        {
            Init();
        }

        private void Init()
        {
            // default to a kind of arc to demonstrate
            // note that we prefer having the second point at the origin (like SubtractPathStartOffset)
            // since it's the actual path start
            m_ControlPoints = new List<Vector2>
            {
                new Vector2(-1f, -1f),
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(2f, -1f)
            };
        }

        protected override bool IsListOfControlPointsValid(IReadOnlyCollection<Vector2> controlPoints)
        {
            // We need at least one curve, and then each curve adds 1 point,
            // so just check the count, no need for modulo like Bezier
            return controlPoints.Count >= 4;
        }

        public override void SanitizePath()
        {
            if (m_ControlPoints.Count < 4)
            {
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogWarningFormat("Path is invalid: m_ControlPoints.Count is {0}," +
                    "expected at least 4 points. Reinitializing points.",
                    m_ControlPoints.Count);
                #endif

                Init();
            }
        }

        protected override void MoveAllControlPointsByOffset(Vector2 offset)
        {
            for (int i = 0; i < m_ControlPoints.Count; i++)
            {
                m_ControlPoints[i] -= offset;
            }
        }

        #region PointAndCurveAccessors

        // Only allow access to the points via a conservative interface.
        // This is to prevent removing points manually, as we need at least 4 control points.
        // Unlike Bezier though, there is no modulo constraint, so generally speaking most changes will result in
        // a valid path.

        /// Return point at start of path
        /// Catmull-Rom: this is the second control point
        public override Vector2 GetPathStartPoint()
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.AssertFormat(m_ControlPoints.Count > 1,
                "[CatmullRomPath2D] GetPathStartPoint: m_ControlPoints.Count is {0}, expected at least 2 (even 4).",
                m_ControlPoints.Count);
            #endif

            return m_ControlPoints[1];
        }

        /// Return point at end of path
        /// Catmull-Rom: this is the penultimate control point
        public override Vector2 GetPathEndPoint()
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.AssertFormat(m_ControlPoints.Count > 1,
                "[CatmullRomPath2D] GetPathStartPoint: m_ControlPoints.Count is {0}, expected at least 2 (even 4).",
                m_ControlPoints.Count);
            #endif

            return m_ControlPoints[^2];
        }

        /// Return the number of key points (equal to the number of control points)
        /// Note that the first and last control points, despite not being part of the path, are considered key points
        public override int GetKeyPointsCount()
        {
            return GetControlPointsCount();
        }

        /// Return key point at given key index (same as control point)
        /// Note that the first and last control points, despite not being part of the path, are considered key points
        public override Vector2 GetKeyPoint(int keyIndex)
        {
            return GetControlPoint(keyIndex);
        }

        /// Add key (control) point at end of path
        public override void AddKeyPoint(Vector2 newKeyPoint)
        {
            AddControlPoint(newKeyPoint);
        }

        /// Remove key point at key index (same as control point)
        /// Note that the first and last control points, despite not being part of the path, are considered key points
        public override void RemoveKeyPoint(int keyIndex)
        {
            RemoveControlPoint(keyIndex);
        }

        /// Return the number of control points in this path
        public int GetControlPointsCount()
        {
            return m_ControlPoints.Count;
        }

        /// Return control point at given index
        public Vector2 GetControlPoint(int index)
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.AssertFormat(index >= 0 && index < m_ControlPoints.Count, "Invalid index: {0}. Expected index between 0 and {1}", index, m_ControlPoints.Count - 1);
            #endif
            return m_ControlPoints[index];
        }

        /// Move an existing control point at given index to given position
        public void SetControlPoint(int index, Vector2 controlPosition)
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.AssertFormat(index >= 0 && index < m_ControlPoints.Count, "Invalid index: {0}. Expected index between 0 and {1}", index, m_ControlPoints.Count - 1);
            #endif
            m_ControlPoints[index] = controlPosition;
        }

        /// Return the index of the control point the nearest to passed position
        public int GetNearestControlPointIndex(Vector2 position)
        {
            int nearestControlPointIndex = -1;
            float nearestControlPointDistance = float.MaxValue;

            int controlPointsCount = GetControlPointsCount();
            for (int controlPointIndex = 0; controlPointIndex < controlPointsCount; controlPointIndex++)
            {
                float distance = Vector2.SqrMagnitude(GetControlPoint(controlPointIndex) - position);
                if (distance < nearestControlPointDistance)
                {
                    nearestControlPointIndex = controlPointIndex;
                    nearestControlPointDistance = distance;
                }
            }

            return nearestControlPointIndex;
        }

        /// Add a control point at the end of the path
        public void AddControlPoint(Vector2 newControlPoint)
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.AssertFormat(IsValid(), "[CatmullRomPath2D] InsertControlPointAtStart: Path is invalid: m_ControlPoints.Count is {0}, " +
                "expected at least 4.", m_ControlPoints.Count);
            #endif

            m_ControlPoints.Add(newControlPoint);
        }

        /// Insert a key (control) point at the start of the path
        public override void InsertKeyPointAtStart(Vector2 newControlPoint)
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.AssertFormat(IsValid(), "[CatmullRomPath2D] InsertKeyPointAtStart: Path is invalid: m_ControlPoints.Count is {0}, " +
                "expected at least 4.", m_ControlPoints.Count);
            #endif

            m_ControlPoints.Insert(0, newControlPoint);
        }

        /// Insert a control point in the path, at the given control point index
        /// controlIndex must be between 0 and Control Points Count
        /// controlIndex: 0 will do the same as InsertControlPointAtStart
        /// controlIndex: 1 will insert a control point just after the first one
        /// controlIndex: Control Points Count - 1 will insert a control point just before the last one
        /// controlIndex: Control Points Count will do the same as AddControlPoint
        public void InsertControlPoint(int controlIndex, Vector2 newControlPoint)
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.AssertFormat(IsValid(), "[CatmullRomPath2D] InsertControlPointAtStart: Path is invalid: m_ControlPoints.Count is {0}, " +
                "expected at least 4.", m_ControlPoints.Count);

            int controlPointsCount = GetControlPointsCount();
            Debug.AssertFormat(controlIndex >= 0 && controlIndex <= controlPointsCount,
                "Invalid control index: {0}. Expected index between 0 and control points count ({1}).", controlIndex, controlPointsCount);
            #endif

            m_ControlPoints.Insert(controlIndex, newControlPoint);
        }

        /// Split curve in two parts by inserting a key at [parameterRatio] along the curve at [curveIndex]
        /// Note that this does not preserve the path velocity and generally slightly alters the path on the affected curves.
        /// If moving an entity along this path, it is recommended to define a motion velocity separately, then call
        /// InterpolatePathBy(Normalized)Distance to retrace the path at the wanted velocity.
        public override void SplitCurveAtParameterRatio(int curveIndex, float parameterRatio)
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.AssertFormat(parameterRatio >= 0f && parameterRatio <= 1f,
                "[CatmullRomPath2D] SplitCurveAtParameterRatio: parameterRatio is {0}, expected value between 0 and 1",
                parameterRatio);
            #endif

            // First, get the 4 control points for the curve at target index
            Vector2[] curveControlPoints = GetCurve(curveIndex);

            Vector2 splitPoint = Interpolate(curveControlPoints, parameterRatio);

            // When inserting point on curve i, we are inserting key point between key points of indices i + 1 and i + 2,
            // so the new key point is at index i + 2
            InsertControlPoint(curveIndex + 2, splitPoint);
        }

        /// Remove control point at control index
        /// UB unless there are at least 5 control points (i.e. 4 after removal), and the controlPointIndex is a valid control point index.
        public void RemoveControlPoint(int controlPointIndex)
        {
            int controlPointsCount = GetControlPointsCount();

            if (controlPointsCount <= 4)
            {
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogAssertionFormat("There are only {0} control points, cannot remove one more control point or we will have less than 4.",
                    controlPointsCount);
                #endif

                return;
            }

            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.AssertFormat(controlPointIndex >= 0 && controlPointIndex < controlPointsCount, "Invalid control index: {0}. Expected index between 0 and {1}.", controlPointIndex, controlPointsCount - 1);
            #endif

            m_ControlPoints.RemoveAt(controlPointIndex);
        }

        /// Return the number of curves compounding this path (equal to the number of control points - 3)
        public override int GetCurvesCount()
        {
            return m_ControlPoints.Count - 3;
        }

        /// Return curve at given curve index. A Catmull-Rom curve is a part of a Catmull-Rom path, compounded of 4 control points.
        /// Note that it is only used to interpolate the middle section (between P1 and P2).
        public override Vector2[] GetCurve(int curveIndex)
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            int curvesCount = GetCurvesCount();
            Debug.AssertFormat(curveIndex < curvesCount, "[BezierPath2D] GetCurve: invalid curveIndex {0}, " +
                "should be less than curves count {1}.",
                curveIndex, curvesCount);
            #endif

            return new[]
            {
                m_ControlPoints[curveIndex],
                m_ControlPoints[curveIndex + 1],
                m_ControlPoints[curveIndex + 2],
                m_ControlPoints[curveIndex + 3]
            };
        }

        #endregion
    }
}
