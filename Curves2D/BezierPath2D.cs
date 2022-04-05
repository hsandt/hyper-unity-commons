using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace CommonsHelper
{
    /// A Bezier path is a series of connected Bezier curves
    [Serializable]
    public class BezierPath2D : Path2D
    {
        [SerializeField, Tooltip("List of control points of each successive Bezier curve, concatenated. " +
             "The end of one Bezier curve is the start of the next one, so to reduce the size of the list, " +
             "we consider points linking two curves only once. " +
             "There is a key point every 3 control points (the path must go through each key point) " +
             "We use `indices` for flat iterations, and `key indices` for iteration on key points only. " +
             "Format: [keyPoint1, outTangentPoint1, inTangentPoint2, keyPoint2, outTangentPoint2, ... inTangentPointN, keyPointN] " +
             "A valid path must have a number of control points in the format 3*n + 1 with n >= 1.")]
        [FormerlySerializedAs("controlPoints")]
        private List<Vector2> m_ControlPoints;
        public override ReadOnlyCollection<Vector2> ControlPoints => m_ControlPoints.AsReadOnly();

        /// Compute point on 2D Bezier curve of control points p0, p1, p2, p3 at parameter t
        public static Vector2 InterpolateBezier(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            return (1-t) * (1-t) * (1-t) * p0 + 3 * (1-t) * (1-t) * t * p1 + 3 * (1-t) * t * t * p2 + t * t * t * p3;
        }

        /// Compute point on 2D Bezier curve (array of 4 points) at parameter t
        public static Vector2 InterpolateBezier(Vector2[] curve, float t)
        {
            return InterpolateBezier(curve[0], curve[1], curve[2], curve[3], t);
        }

        /// Compute point on 2D Bezier curve (array of 4 points) at parameter t (polymorphic version)
        public override Vector2 Interpolate(Vector2[] curve, float t)
        {
            return InterpolateBezier(curve, t);
        }

        public BezierPath2D()
        {
            Init();
        }

        public BezierPath2D(IReadOnlyCollection<Vector2> controlPoints)
        {
            if (IsListOfControlPointsValid(controlPoints))
            {
                m_ControlPoints = controlPoints.ToList();
            }
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            else
            {
                Debug.LogErrorFormat("[BezierPath2D] Constructor taking controlPoints is used, " +
                    "but m_ControlPoints.Count is {0}, which is not in the form 3*n + 1 with n >= 1. " +
                    "Falling back to default initialization.",
                    controlPoints.Count);
                Init();
            }
            #endif
        }

        private void Init()
        {
            // default to a kind of wave to demonstrate
            m_ControlPoints = new List<Vector2>
            {
                new Vector2(0f, 0f),
                new Vector2(1f, 1f),
                new Vector2(2f, -1f),
                new Vector2(3f, 0f)
            };
        }

        protected override bool IsListOfControlPointsValid(IReadOnlyCollection<Vector2> controlPoints)
        {
            // We need at least one curve, and then each curve adds 3 points,
            // so control points count should be in the form 3*n + 1 with n >= 1
            return controlPoints.Count >= 4 && (controlPoints.Count - 1) % 3 == 0;
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
                return;
            }

            int countModulo = (m_ControlPoints.Count - 1) % 3;
            if (countModulo > 0)
            {
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogWarningFormat("Path is invalid: m_ControlPoints.Count is {0}," +
                    "which is not in the form 3*n + 1 with n >= 1. Cutting {1} extra points.",
                    m_ControlPoints.Count, countModulo);
                #endif

                m_ControlPoints.RemoveRange(m_ControlPoints.Count - countModulo, countModulo);
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
        // This is to prevent adding and removing points manually, as it would break the required format of
        // control points.
        // Of course, you can still edit points in the inspector, but if you add/remove a non-multiple of 3,
        // it will auto-SanitizePath.

        /// Return point at start of path
        /// Bezier: this is the first key point, and also the first control point
        public override Vector2 GetPathStartPoint()
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.AssertFormat(m_ControlPoints.Count > 0,
                "[CatmullRomPath2D] GetPathStartPoint: m_ControlPoints.Count is {0}, expected at least 1 (even 4).",
                m_ControlPoints.Count);
            #endif

            return m_ControlPoints[0];
        }

        /// Return point at end of path
        /// Bezier: this is the last key point, and also the last control point
        public override Vector2 GetPathEndPoint()
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.AssertFormat(m_ControlPoints.Count > 0,
                "[CatmullRomPath2D] GetPathStartPoint: m_ControlPoints.Count is {0}, expected at least 1 (even 4).",
                m_ControlPoints.Count);
            #endif

            return m_ControlPoints[^1];
        }

        /// Return the number of control points in this path (not the number of key points).
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

        /// Return the number of key points (equal to the number of curves + 1)
        public override int GetKeyPointsCount()
        {
            return GetCurvesCount() + 1;
        }

        /// Return key point at given key index
        public override Vector2 GetKeyPoint(int keyIndex)
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.AssertFormat(keyIndex >= 0 && keyIndex < GetKeyPointsCount(), "Invalid keyIndex: {0}. Expected keyIndex between 0 and Key Points Count - 1 ({1})", keyIndex, GetKeyPointsCount() - 1);
            #endif

            return m_ControlPoints[3 * keyIndex];
        }

        /// Move key point at given key index to given position
        public void SetKeyPoint(int keyIndex, Vector2 controlPosition)
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.AssertFormat(keyIndex >= 0 && keyIndex < GetKeyPointsCount(), "Invalid keyIndex: {0}. Expected keyIndex between 0 and Key Points Count - 1 ({1})", keyIndex, GetKeyPointsCount() - 1);
            #endif

            m_ControlPoints[3 * keyIndex] = controlPosition;
        }

        /// Yield all key points
        public IEnumerable<Vector2> GetKeyPoints()
        {
            int keyPointsCount = GetKeyPointsCount();
            for (int keyIndex = 0; keyIndex < keyPointsCount; keyIndex++)
            {
                yield return GetKeyPoint(keyIndex);
            }
        }

        /// Return the index of the key point the nearest to passed position
        public int GetNearestKeyPointIndex(Vector2 position)
        {
            int nearestKeyPointIndex = -1;
            float nearestKeyPointDistance = float.MaxValue;

            int keyPointsCount = GetKeyPointsCount();
            for (int keyIndex = 0; keyIndex < keyPointsCount; keyIndex++)
            {
                float distance = Vector2.SqrMagnitude(GetKeyPoint(keyIndex) - position);
                if (distance < nearestKeyPointDistance)
                {
                    nearestKeyPointIndex = keyIndex;
                    nearestKeyPointDistance = distance;
                }
            }

            return nearestKeyPointIndex;
        }

        /// Add a key point at the end of the path, automatically choosing smooth control points between the added
        /// key point and the previous one (the former end).
        public override void AddKeyPoint(Vector2 newKeyPoint)
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.AssertFormat(IsValid(), "[BezierPath2D] AddKeyPoint: Path is invalid: m_ControlPoints.Count is {0}, " +
                "which is not in the form 3*n + 1 with n >= 1.", m_ControlPoints.Count);
            #endif

            Vector2 previousControlPointB = m_ControlPoints[^2];
            Vector2 previousKeyPoint = m_ControlPoints[^1];
            CalculateSmoothTangentPointsBetween(newKeyPoint, previousKeyPoint, previousControlPointB,
                out Vector2 newInTangentPoint, out Vector2 newOutTangentPoint);
            m_ControlPoints.AddRange(new[] {newOutTangentPoint, newInTangentPoint, newKeyPoint});
        }

        /// Insert a key point at the start of the path, automatically choosing smooth control points between the insert
        /// key point and the next one (the former start).
        public override void InsertKeyPointAtStart(Vector2 newKeyPoint)
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.AssertFormat(IsValid(), "[BezierPath2D] InsertKeyPointAtStart: Path is invalid: m_ControlPoints.Count is {0}, " +
                "which is not in the form 3*n + 1 with n >= 1.", m_ControlPoints.Count);
            #endif

            Vector2 firstKeyPoint = m_ControlPoints[0];
            Vector2 firstOutTangentPoint = m_ControlPoints[1];
            CalculateSmoothTangentPointsBetween(newKeyPoint, firstKeyPoint, firstOutTangentPoint,
                out Vector2 newOutTangentPoint, out Vector2 newInTangentPoint);
            m_ControlPoints.InsertRange(0, new[] {newKeyPoint, newOutTangentPoint, newInTangentPoint});
        }

        /// <summary>
        /// From a new key point to (A) insert at the start or (B) add at the end, an existing key point on that
        /// extremity, and its unique tangent point so far, deduce the second tangent point to associate to this
        /// existing key point (as it won't be an extremity anymore) and the unique tangent point to associate to the
        /// new key point (which is the new extremity) so that the first two tangents are symmetrical, and the previous
        /// extreme key point has symmetrical tangent (continuous derivative at this point).
        /// </summary>
        /// <param name="newExtremeKeyPoint">The new key point to insert at an extremity</param>
        /// <param name="previousExtremeKeyPoint">The existing key point at the extremity</param>
        /// <param name="previousExtremeTangentPoint">The unique tangent point of the existing key point so far</param>
        /// <param name="outTangentPointOfNewExtremeKeyPoint">Calculated unique tangent point of the new key point</param>
        /// <param name="outSecondTangentPointOfPreviousExtremeKeyPoint">Calculated second tangent point of the previous extreme key point, in the direction of the new key point</param>
        private static void CalculateSmoothTangentPointsBetween(Vector2 newExtremeKeyPoint, Vector2 previousExtremeKeyPoint,
            Vector2 previousExtremeTangentPoint, out Vector2 outTangentPointOfNewExtremeKeyPoint, out Vector2 outSecondTangentPointOfPreviousExtremeKeyPoint)
        {
            Vector2 existingTangent = previousExtremeTangentPoint - previousExtremeKeyPoint;
            if (existingTangent.sqrMagnitude < Mathf.Epsilon)
            {
                // fallback to keep control point visible
                existingTangent = Vector2.up;
            }

            // mirror the position of the tangent point before the first point for a symmetrical tangent (continuous
            // parametric derivative)
            outSecondTangentPointOfPreviousExtremeKeyPoint = previousExtremeKeyPoint - existingTangent;

            // mirror the first new control point to get a second one that makes the curve come back smoothly to the added key point
            // (note that if the new key point is on the opposite direction of the last tangent, it may produce a spiral pattern)
            // reflecting off a normal is the same as reflecting off a tangent and opposing

            Vector2 previousToNewVector = newExtremeKeyPoint - previousExtremeKeyPoint;
            if (previousToNewVector.sqrMagnitude < Mathf.Epsilon)
            {
                // fallback to allow mirroring
                previousToNewVector = Vector2.right;
            }

            Vector2 tangentOfNewExtremePoint = VectorUtil.Mirror(existingTangent, previousToNewVector);
            outTangentPointOfNewExtremeKeyPoint = newExtremeKeyPoint + tangentOfNewExtremePoint;
        }

        /// Insert a key point in the middle of the path, at the given key point index
        /// keyIndex must be between 1 and Key Points Count - 1
        /// keyIndex: 1 will insert a key point just after the first one
        /// keyIndex: Key Points Count - 1 will insert a key point just before the last one
        /// To add a key point at the end of the path, use AddKeyPoint.
        public void InsertKeyPoint(int keyIndex, Vector2 newKeyPoint, Vector2 inTangentPoint, Vector2 outTangentPoint)
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.AssertFormat(IsValid(), "[BezierPath2D] InsertKeyPoint: Path is invalid: m_ControlPoints.Count is {0}," +
                "which is not in the form 3*n + 1 with n >= 1.", m_ControlPoints.Count);

            int keyPointsCount = GetKeyPointsCount();
            Debug.AssertFormat(keyIndex >= 1 && keyIndex < keyPointsCount,
                "Invalid key index: {0}. Expected index between 1 and key points count - 1 ({1}).", keyIndex, keyPointsCount - 1);
            #endif

            // We must insert 3 control points at once: the new key point, and its two surrounding tangent points.
            // The first control point to add is the in tangent point, so we must insert at the same index as GetInTangentPoint: 3 * i - 1
            m_ControlPoints.InsertRange(3 * keyIndex - 1, new[] {inTangentPoint, newKeyPoint, outTangentPoint});
        }

        /// Split curve in two parts by inserting a key at [parameterRatio] along the curve at [curveIndex]
        /// with existing tangents adjusted, and new tangents calculated so that we preserve the shape of the Bezier path.
        /// Note that this does not preserve the path velocity since we need to adjust tangent magnitudes (reducing speed)
        /// and, in fact, when parameterRatio is not 0.5, the tangents of the inserted point are not even symmetrical,
        /// so the path speed won't be continuous.
        /// If moving an entity along this path, it is recommended to define a motion velocity separately, then call
        /// InterpolatePathBy(Normalized)Distance to retrace the path at the wanted velocity, ignoring tangent magnitude.
        public override void SplitCurveAtParameterRatio(int curveIndex, float parameterRatio)
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.AssertFormat(parameterRatio >= 0f && parameterRatio <= 1f,
                "[BezierPath2D] SplitCurveAtParameterRatio: parameterRatio is {0}, expected value between 0 and 1",
                parameterRatio);
            #endif

            // We use De CastleJau subdivision at given parameter ratio
            // https://en.wikipedia.org/wiki/De_Casteljau%27s_algorithm
            // Illustration for parameter ratio = 0.5:
            // https://stackoverflow.com/questions/18655135/divide-bezier-curve-into-two-equal-halves

            // First, get the 4 control points (curve start, tangent out point, tangent in point, curve end)
            Vector2[] curveControlPoints = GetCurve(curveIndex);

            // 1st generation of Lerp, where the 1st and 3rd points are also the adjusted tangent points
            Vector2 p10 = Vector2.Lerp(curveControlPoints[0], curveControlPoints[1], parameterRatio);
            Vector2 p11 = Vector2.Lerp(curveControlPoints[1], curveControlPoints[2], parameterRatio);
            Vector2 p12 = Vector2.Lerp(curveControlPoints[2], curveControlPoints[3], parameterRatio);

            // 2nd generation of Lerp, which is also the inserted key point's in and out tangent points
            Vector2 newInTangentPoint = Vector2.Lerp(p10, p11, parameterRatio);
            Vector2 newOutTangentPoint = Vector2.Lerp(p11, p12, parameterRatio);

            // Final Lerp gives the split point. It is the curve point at [parameterRatio],
            // just computed via De Casteljau's algorithm instead of InterpolateBezier
            Vector2 splitPoint = Vector2.Lerp(newInTangentPoint, newOutTangentPoint, parameterRatio);

            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Vector2 splitPointViaInterpolateBezier = InterpolateBezier(curveControlPoints, parameterRatio);
            Debug.AssertFormat(splitPoint == splitPointViaInterpolateBezier,
                "[BezierPath2D] SplitCurveAtParameterRatio: splitPoint {0} doesn't match " +
                "InterpolateBezier(curveControlPoints, parameterRatio) {1} (if ratio is outside [0; 1], " +
                "this is expected, fix the first assertion first)",
                splitPoint, splitPointViaInterpolateBezier);
            #endif

            // Adjust existing tangents by applying a factor of [parameterRatio] and [1-parameterRatio]
            // to out and in tangents respectively. We have already calculated the result of such a shrink
            // during De Casteljau's algorithm, in first generation
            SetOutTangentPoint(curveIndex, p10);
            SetInTangentPoint(curveIndex + 1, p12);

            // When inserting between key point indices i and i + 1, the new key point is at index i + 1
            InsertKeyPoint(curveIndex + 1, splitPoint, newInTangentPoint, newOutTangentPoint);
        }

        /// Remove key point at key index, also removing the surrounding tangent points (1 for the start and end point,
        /// 2 for a middle point).
        /// UB unless there are at least 3 key points, and the keyIndex is a valid key point index.
        public override void RemoveKeyPoint(int keyIndex)
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.AssertFormat(IsValid(), "[BezierPath2D] RemoveKeyPoint: Path is invalid: m_ControlPoints.Count is {0}," +
                "which is not in the form 3*n + 1 with n >= 1.", m_ControlPoints.Count);
            #endif

            int keyPointsCount = GetKeyPointsCount();

            if (keyPointsCount <= 2)
            {
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogAssertionFormat("There are only {0} key points, cannot remove one more key point.", keyPointsCount);
                #endif

                return;
            }

            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.AssertFormat(keyIndex >= 0 && keyIndex < keyPointsCount, "Invalid key index: {0}. Expected index between 0 and {1}.", keyIndex, keyPointsCount - 1);
            #endif

            if (keyIndex == 0)
            {
                // remove start point, its out tangent point and the 2nd point's in tangent point
                m_ControlPoints.RemoveRange(0, 3);
            }
            else if (keyIndex == keyPointsCount - 1)
            {
                // remove end point, its in tangent point and the penultimate point's out tangent point
                m_ControlPoints.RemoveRange(3 * keyIndex - 2, 3);
            }
            else
            {
                m_ControlPoints.RemoveRange(3 * keyIndex - 1, 3);
            }
        }

        /// Return in tangent control point at given key index
        /// Key index must be between 1 and key points count - 1
        public Vector2 GetInTangentPoint(int keyIndex)
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.AssertFormat(keyIndex >= 1 && keyIndex < GetKeyPointsCount(), "Invalid keyIndex: {0}. Expected keyIndex between 1 and Key Points Count - 1 ({1})", keyIndex, GetKeyPointsCount() - 1);
            #endif

            return m_ControlPoints[3 * keyIndex - 1];
        }

        /// Move in tangent control point at given key index to given position
        /// Key index must be between 1 and key points count - 1
        public void SetInTangentPoint(int keyIndex, Vector2 controlPosition)
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.AssertFormat(keyIndex >= 1 && keyIndex < GetKeyPointsCount(), "Invalid keyIndex: {0}. Expected keyIndex between 1 and Key Points Count - 1 ({1})", keyIndex, GetKeyPointsCount() - 1);
            #endif

            m_ControlPoints[3 * keyIndex - 1] = controlPosition;
        }

        /// Return out tangent control point at given key index
        /// Key index must be between 0 and key points count - 2
        public Vector2 GetOutTangentPoint(int keyIndex)
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.AssertFormat(keyIndex >= 0 && keyIndex < GetKeyPointsCount() - 1, "Invalid keyIndex: {0}. Expected keyIndex between 0 and Key Points Count - 2 ({1})", keyIndex, GetKeyPointsCount() - 2);
            #endif

            return m_ControlPoints[3 * keyIndex + 1];
        }

        /// Move out tangent control point at given key index to given position
        /// Key index must be between 1 and key points count - 1
        public void SetOutTangentPoint(int keyIndex, Vector2 controlPosition)
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.AssertFormat(keyIndex >= 0 && keyIndex < GetKeyPointsCount() - 1, "Invalid keyIndex: {0}. Expected keyIndex between 0 and Key Points Count - 2 ({1})", keyIndex, GetKeyPointsCount() - 2);
            #endif

            m_ControlPoints[3 * keyIndex + 1] = controlPosition;
        }

        /// Return the number of curves compounding this path (equal to the number of key points - 1)
        public override int GetCurvesCount()
        {
            return (m_ControlPoints.Count - 1 ) / 3;
        }

        /// Return curve at given key index. A Bezier curve is a part of a Bezier path, compounded of 4 control points.
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
                m_ControlPoints[3 * curveIndex],
                m_ControlPoints[3 * curveIndex + 1],
                m_ControlPoints[3 * curveIndex + 2],
                m_ControlPoints[3 * curveIndex + 3]
            };
        }

        #endregion
    }
}
