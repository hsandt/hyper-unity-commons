using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace HyperUnityCommons.Tests
{
    public class BezierPath2DStaticTests
    {
        private static readonly Vector2 o = Vector2.zero;
        private static readonly Vector2 u = Vector2.right;
        private static readonly Vector2 v = Vector2.up;
        private static readonly Vector2 w = u + v;

        [Test]

        public void InterpolateBezier_Start()
        {
            Assert.AreEqual(o, BezierPath2D.InterpolateBezier(o, v, w, u, 0f));
        }

        [Test]
        public void InterpolateBezier_End()
        {
            Assert.AreEqual(u, BezierPath2D.InterpolateBezier(o, v, w, u, 1f));
        }

        [Test]
        public void InterpolateBezier_Quarter()
        {
            Assert.AreEqual(new Vector2(0.15625f, 0.5625f), BezierPath2D.InterpolateBezier(o, v, w, u, 0.25f));
        }
        [Test]

        public void InterpolateBezier_Half()
        {
            Assert.AreEqual(new Vector2(0.5f, 0.75f), BezierPath2D.InterpolateBezier(o, v, w, u, 0.5f));
        }

        [Test]
        public void InterpolateBezier_ThreeQuarter()
        {
            Assert.AreEqual(new Vector2(1f-0.15625f, 0.5625f), BezierPath2D.InterpolateBezier(o, v, w, u, 0.75f));
        }
    }

    [TestFixture]
    public class BezierPath2DTests {

        BezierPath2D path;

        [SetUp]
        public void Init () {
            // This will initialize a path with 4 control points, including 2 key points at (0, 0) and (3, 0).
            // This is important as tests rely on the initial state.
            path = new BezierPath2D();
        }

        [Test]
        public void GetControlPointsCount_Initial()
        {
            Assert.AreEqual(4, path.GetControlPointsCount());
        }

        [Test]
        public void GetControlPointsCount_3KeyPoints()
        {
            path.AddKeyPoint(new Vector2(5f, 0));

            Assert.AreEqual(7, path.GetControlPointsCount());
        }

        [Test]
        public void SetControlPoint_KeyPoint1()
        {
            path.SetControlPoint(0, new Vector2(5f, 0));

            Assert.AreEqual(new Vector2(5f, 0), path.GetControlPoint(0));
        }

        [Test]
        public void SetControlPoint_ControlPoint1A()
        {
            path.SetControlPoint(1, new Vector2(0f, 10f));

            Assert.AreEqual(new Vector2(0f, 10), path.GetControlPoint(1));
        }

        [Test]
        public void GetKeyPointsCount_3KeyPoints()
        {
            path.AddKeyPoint(new Vector2(5f, 0f));

            Assert.AreEqual(3, path.GetKeyPointsCount());
        }

        [Test]
        public void GetKeyPoints_3KeyPoints()
        {
            path.AddKeyPoint(new Vector2(3f, 3f));

            Assert.AreEqual(
                new Vector2[] {new Vector2(0f, 0f), new Vector2(3f, 0f), new Vector2(3f, 3f)},
                path.GetKeyPoints().ToArray());
        }

        [Test]
        public void GetNearestKeyPointIndex_CloseTo1st()
        {
            path.AddKeyPoint(new Vector2(3f, 3f));

            Assert.AreEqual( 0, path.GetNearestKeyPointIndex(new Vector2(-0.5f, 0f)));
        }

        [Test]
        public void GetNearestKeyPointIndex_CloseTo2nd()
        {
            path.AddKeyPoint(new Vector2(3f, 3f));

            Assert.AreEqual( 1, path.GetNearestKeyPointIndex(new Vector2(4f, -1f)));
        }

        [Test]
        public void GetNearestKeyPointIndex_CloseTo2ndAnd3rd()
        {
            path.AddKeyPoint(new Vector2(3f, 3f));

            // It's a draw, so we return the lowest index
            Assert.AreEqual( 1, path.GetNearestKeyPointIndex(new Vector2(3f, 1.5f)));
        }

        [Test]
        public void GetNearestKeyPointIndex_CloseTo3rd()
        {
            path.AddKeyPoint(new Vector2(3f, 3f));

            Assert.AreEqual( 2, path.GetNearestKeyPointIndex(new Vector2(3f, 1.51f)));
        }

        [Test]
        public void GetCurvesCount_Initial()
        {
            Assert.AreEqual(1, path.GetCurvesCount());
        }

        [Test]
        public void GetCurvesCount_3KeyPoints()
        {
            path.AddKeyPoint(new Vector2(5f, 0f));

            Assert.AreEqual(2, path.GetCurvesCount());
        }

        [Test]
        public void GetCurve_0()
        {
            // arrange control points
            path.SetControlPoint(0, new Vector2(-2f, -1f));
            path.SetControlPoint(1, new Vector2(0f, -1f));
            path.SetControlPoint(2, new Vector2(0f, 1f));
            path.SetControlPoint(3, new Vector2(2f, 1f));

            Assert.AreEqual(new[] {
                new Vector2(-2f, -1f),
                new Vector2(0f, -1f),
                new Vector2(0f, 1f),
                new Vector2(2f, 1f)
            }, path.GetCurve(0));
        }

        [Test]
        public void GetCurve_1()
        {
            // arrange: prepare last control points and add new key point
            path.SetControlPoint(2, new Vector2(2f, -1f));
            path.SetControlPoint(3, new Vector2(2f, 0f));
            path.AddKeyPoint(new Vector2(5f, 0));

            // rearrange new control points in order to reduce the effect of AddKeyPoint on point positions in this test
            path.SetControlPoint(4, new Vector2(2f, 1f));
            path.SetControlPoint(5, new Vector2(5f, 1f));

            // verify last curve
            Assert.AreEqual(new[] {
                new Vector2(2f, 0f),
                new Vector2(2f, 1f),
                new Vector2(5f, 1f),
                new Vector2(5f, 0f)
            }, path.GetCurve(1));
        }

        [Test]
        public void AddKeyPoint_Add1KeyPointOnTheRight()
        {
            // add new key point
            path.AddKeyPoint(new Vector2(6f, 0));

            // verify added control points
            Assert.AreEqual(new[] {
                new Vector2(4f, 1f),
                new Vector2(5f, 1f),
                new Vector2(6f, 0f)
            }, new[] {
                path.GetControlPoint(4),
                path.GetControlPoint(5),
                path.GetControlPoint(6)
            });
        }

        [Test]
        public void InsertKeyPointAtStart_Insert1KeyPointOnTheLeft()
        {
            // add new key point at the beginning
            path.InsertKeyPointAtStart(new Vector2(-3f, 0));

            // verify added control points, and also that the existing first point was moved correctly
            Assert.AreEqual(new[] {
                new Vector2(-3f, 0f),
                new Vector2(-2f, -1f),
                new Vector2(-1f, -1f),
                new Vector2(0f, 0f)
            }, new[] {
                path.GetControlPoint(0),
                path.GetControlPoint(1),
                path.GetControlPoint(2),
                path.GetControlPoint(3)
            });
        }

        [Test]
        public void RemoveKeyPoint_RemoveStartKeyPoint()
        {
            path.AddKeyPoint(new Vector2(3f, 3f));

            // arrange last control points to avoid depending too much on how AddKeyPoint
            // auto-computes tangents
            path.SetControlPoint(4, new Vector2(4f, 2f));
            path.SetControlPoint(5, new Vector2(2f, 4f));

            // remove start point
            path.RemoveKeyPoint(0);

            Assert.AreEqual(new[] {
                new Vector2(3f, 0f),
                new Vector2(4f, 2f),
                new Vector2(2f, 4f),
                new Vector2(3f, 3f)
            }, path.ControlPoints);
        }

        [Test]
        public void RemoveKeyPoint_RemoveMiddleKeyPoint()
        {
            path.AddKeyPoint(new Vector2(3f, 3f));

            // arrange last control points to avoid depending too much on how AddKeyPoint
            // auto-computes tangents
            path.SetControlPoint(4, new Vector2(4f, 2f));
            path.SetControlPoint(5, new Vector2(2f, 4f));

            // remove mid point
            path.RemoveKeyPoint(1);

            Assert.AreEqual(new[] {
                new Vector2(0f, 0f),
                new Vector2(1f, 1f),
                new Vector2(2f, 4f),
                new Vector2(3f, 3f)
            }, path.ControlPoints);
        }

        [Test]
        public void RemoveKeyPoint_RemoveEndKeyPoint()
        {
            path.AddKeyPoint(new Vector2(3f, 3f));

            // remove end point
            path.RemoveKeyPoint(2);

            // verify added control points
            Assert.AreEqual(new[] {
                new Vector2(0f, 0f),
                new Vector2(1f, 1f),
                new Vector2(2f, -1f),
                new Vector2(3f, 0f)
            }, path.ControlPoints);
        }

        [Test]
        public void InterpolatePathByParameter_Start()
        {
            // Default has 1 curve, which starts at (0, 0)
            Assert.AreEqual(new Vector2(0f, 0f), path.InterpolatePathByParameter(0f));
        }

        [Test]
        public void InterpolatePathByParameter_End()
        {
            // Default has 1 curve, which ends at (3, 0)
            Assert.AreEqual(new Vector2(3f, 0f), path.InterpolatePathByParameter(1f));
        }

        [Test]
        public void InterpolatePathByParameter_MultiCurveNearEnd()
        {
            path.AddKeyPoint(new Vector2(6f, 0f));

            // arrange control points to make a straight line from (3, 0)
            path.SetControlPoint(4, new Vector2(4f, 0f));
            path.SetControlPoint(5, new Vector2(5f, 0f));

            // We now have 2 curves, so the path parameter at the end of the path is 2
            Vector2 position = path.InterpolatePathByParameter(1.999f);
            Assert.AreEqual(5.999, (double)position.x, 0.01);
            Assert.AreEqual(0f, position.y);
        }

        [Test]
        public void InterpolatePathByParameter_MultiCurveEnd()
        {
            path.AddKeyPoint(new Vector2(5f, 0f));
            path.AddKeyPoint(new Vector2(10f, 0f));

            // We now have 3 curves, so the path parameter at the end of the path is 3
            Assert.AreEqual(new Vector2(10f, 0f), path.InterpolatePathByParameter(3f));
        }

        [Test]
        public void InterpolatePathByNormalizedParameter_Start()
        {
            Assert.AreEqual(new Vector2(0f, 0f), path.InterpolatePathByNormalizedParameter(0f));
        }

        [Test]
        public void InterpolatePathByNormalizedParameter_End()
        {
            Assert.AreEqual(new Vector2(3f, 0f), path.InterpolatePathByNormalizedParameter(1f));
        }

        [Test]
        public void InterpolatePathByNormalizedParameter_MultiCurveNearEnd()
        {
            path.AddKeyPoint(new Vector2(6f, 0f));

            // arrange control points to make a straight line from (3, 0)
            path.SetControlPoint(4, new Vector2(4f, 0f));
            path.SetControlPoint(5, new Vector2(5f, 0f));

            Vector2 position = path.InterpolatePathByNormalizedParameter(0.999f);
            Assert.AreEqual(5.999, (double)position.x, 0.01);
            Assert.AreEqual(0f, position.y);
        }

        [Test]
        public void InterpolatePathByNormalizedParameter_MultiCurveEnd()
        {
            path.AddKeyPoint(new Vector2(5f, 0f));
            path.AddKeyPoint(new Vector2(10f, 0f));

            Assert.AreEqual(new Vector2(10f, 0f), path.InterpolatePathByNormalizedParameter(1f));
        }

        [Test]
        public void EvaluateCurveLength_Linear()
        {
            // arrange control points to make a straight line
            path.SetControlPoint(1, new Vector2(1f, 0f));
            path.SetControlPoint(2, new Vector2(2f, 0f));

            // for a straight line, the number of segments doesn't matter, we'll always get the exact length
            Assert.AreEqual(3f, path.EvaluateCurveLength(0, 4));
        }
    }
}
