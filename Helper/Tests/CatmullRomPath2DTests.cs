using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace CommonsHelper.Tests
{
    public class CatmullRomPath2DStaticTests
    {
        private static readonly Vector2 o = Vector2.zero;
        private static readonly Vector2 u = Vector2.right;
        private static readonly Vector2 v = Vector2.up;
        private static readonly Vector2 w = u + v;

        [Test]

        public void InterpolateCatmullRom_Start()
        {
            // Catmull-Rom curve starts at the second control point
            Assert.AreEqual(v, CatmullRomPath2D.InterpolateCatmullRom(o, v, w, u, 0.5f, 0f));
        }

        [Test]
        public void InterpolateCatmullRom_End()
        {
            // Catmull-Rom curve ends at the pre-last control point
            Assert.AreEqual(w, CatmullRomPath2D.InterpolateCatmullRom(o, v, w, u, 0.5f, 1f));
        }

        [Test]
        public void InterpolateCatmullRom_Quarter()
        {
            Vector2 quarterInterpolation = CatmullRomPath2D.InterpolateCatmullRom(o, v, w, u, 0.5f, 0.25f);
            Assert.AreEqual(new Vector2(0.203125f, 1.09375f), quarterInterpolation);
        }
        [Test]

        public void InterpolateCatmullRom_Half()
        {
            Vector2 halfInterpolation = CatmullRomPath2D.InterpolateCatmullRom(o, v, w, u, 0.5f, 0.5f);
            Assert.AreEqual(new Vector2(0.5f, 1.125f), halfInterpolation);
        }

        [Test]
        public void InterpolateCatmullRom_ThreeQuarter()
        {
            Vector2 threeQuarterInterpolation = CatmullRomPath2D.InterpolateCatmullRom(o, v, w, u, 0.5f, 0.75f);
            Assert.AreEqual(new Vector2(1f-0.203125f, 1.09375f), threeQuarterInterpolation);
        }
    }

    [TestFixture]
    public class CatmullRomPath2DTests {

        CatmullRomPath2D path;

        [SetUp]
        public void Init () {
            // This will initialize a path with 4 control points, including 2 key points at (0, 0) and (3, 0).
            // This is important as tests rely on the initial state.
            path = new CatmullRomPath2D();
        }

        [Test]
        public void GetControlPointsCount_Initial()
        {
            Assert.AreEqual(4, path.GetControlPointsCount());
        }

        [Test]
        public void GetControlPointsCount_5ControlPoints()
        {
            path.AddControlPoint(new Vector2(5f, 0));

            Assert.AreEqual(5, path.GetControlPointsCount());
        }

        [Test]
        public void SetControlPoint_FirstControlPoint()
        {
            path.SetControlPoint(0, new Vector2(5f, 0));

            Assert.AreEqual(new Vector2(5f, 0), path.GetControlPoint(0));
        }

        [Test]
        public void GetNearestKeyPointIndex_CloseTo1st()
        {
            Assert.AreEqual( 0, path.GetNearestControlPointIndex(new Vector2(-1.5f, -1.5f)));
        }

        [Test]
        public void GetNearestKeyPointIndex_CloseTo2nd()
        {
            Assert.AreEqual( 1, path.GetNearestKeyPointIndex(new Vector2(-0.5f, 0f)));
        }

        [Test]
        public void GetNearestKeyPointIndex_CloseTo2ndAnd3rd()
        {
            // It's a draw, so we return the lowest index
            Assert.AreEqual( 1, path.GetNearestKeyPointIndex(new Vector2(0.5f, 0f)));
        }

        [Test]
        public void GetNearestKeyPointIndex_CloseTo3rd()
        {
            Assert.AreEqual( 2, path.GetNearestKeyPointIndex(new Vector2(1f, -0.5f)));
        }

        [Test]
        public void GetCurvesCount_Initial()
        {
            Assert.AreEqual(1, path.GetCurvesCount());
        }

        [Test]
        public void GetCurvesCount_5ControlPoints()
        {
            path.AddControlPoint(new Vector2(5f, 0f));

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
            path.SetControlPoint(1, new Vector2(0f, -1f));
            path.SetControlPoint(2, new Vector2(0f, 1f));
            path.SetControlPoint(3, new Vector2(2f, 1f));
            path.AddControlPoint(new Vector2(5f, 0));

            // verify last curve
            Assert.AreEqual(new[] {
                new Vector2(0f, -1f),
                new Vector2(0f, 1f),
                new Vector2(2f, 1f),
                new Vector2(5f, 0f)
            }, path.GetCurve(1));
        }

        [Test]
        public void AddKeyPoint_AddControlPointOnTheRight()
        {
            // add new control point
            path.AddControlPoint(new Vector2(6f, 0));

            // verify added control point
            Assert.AreEqual(new Vector2(6f, 0f), path.GetControlPoint(4));
        }

        [Test]
        public void InsertKeyPointAtStart_Insert1KeyPointOnTheLeft()
        {
            // add new key point at the beginning
            path.InsertKeyPointAtStart(new Vector2(-3f, 0));

            // verify added control point, and also that the existing first point was moved correctly
            Assert.AreEqual(new[] {
                new Vector2(-3f, 0f),
                new Vector2(-1f, -1f)
            }, new[] {
                path.GetControlPoint(0),
                path.GetControlPoint(1)
            });
        }

        [Test]
        public void RemoveControlPoint_RemoveStartControlPoint()
        {
            path.AddControlPoint(new Vector2(3f, 3f));

            // remove start point
            path.RemoveControlPoint(0);

            Assert.AreEqual(new[] {
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(2f, -1f),
                new Vector2(3f, 3f),
            }, path.ControlPoints);
        }

        [Test]
        public void RemoveControlPoint_RemoveMiddleControlPoint()
        {
            path.AddControlPoint(new Vector2(3f, 3f));

            // remove mid point
            path.RemoveControlPoint(1);

            Assert.AreEqual(new[] {
                new Vector2(-1f, -1f),
                new Vector2(1f, 0f),
                new Vector2(2f, -1f),
                new Vector2(3f, 3f)
            }, path.ControlPoints);
        }

        [Test]
        public void RemoveControlPoint_RemoveEndControlPoint()
        {
            path.AddControlPoint(new Vector2(3f, 3f));

            // remove end point
            path.RemoveControlPoint(4);

            // verify added control points
            Assert.AreEqual(new[] {
                new Vector2(-1f, -1f),
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(2f, -1f)
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
            // Default has 1 curve, which ends at (1, 0)
            Assert.AreEqual(new Vector2(1f, 0f), path.InterpolatePathByParameter(1f));
        }

        [Test]
        public void InterpolatePathByParameter_MultiCurveNearEnd()
        {
            path.AddControlPoint(new Vector2(6f, 0f));   // actual path last point
            path.AddControlPoint(new Vector2(99f, 0f));  // remember, last point is not part of interpolated curve

            // We now have 3 curves, so the path parameter at the end of the path is 3
            Vector2 position = path.InterpolatePathByParameter(2.999f);
            Assert.AreEqual(5.999, (double)position.x, 0.01);
            // we didn't flatten curve so y is also approximate
            Assert.AreEqual(0, (double)position.y, 0.01);
        }

        [Test]
        public void InterpolatePathByParameter_MultiCurveEnd()
        {
            path.AddControlPoint(new Vector2(5f, 0f));   // actual path last point
            path.AddControlPoint(new Vector2(99f, 0f));  // remember, last point is not part of interpolated curve

            // We now have 3 curves, so the path parameter at the end of the path is 3
            Assert.AreEqual(new Vector2(5f, 0f), path.InterpolatePathByParameter(3f));
        }

        [Test]
        public void InterpolatePathByNormalizedParameter_Start()
        {
            Assert.AreEqual(new Vector2(0f, 0f), path.InterpolatePathByNormalizedParameter(0f));
        }

        [Test]
        public void InterpolatePathByNormalizedParameter_End()
        {
            Assert.AreEqual(new Vector2(1f, 0f), path.InterpolatePathByNormalizedParameter(1f));
        }

        [Test]
        public void InterpolatePathByNormalizedParameter_MultiCurveNearEnd()
        {
            path.AddControlPoint(new Vector2(6f, 0f));   // actual path last point
            path.AddControlPoint(new Vector2(99f, 0f));  // remember, last point is not part of interpolated curve

            Vector2 position = path.InterpolatePathByNormalizedParameter(0.999f);
            Assert.AreEqual(5.999, (double)position.x, 0.02);
            Assert.AreEqual(0f, position.y, 0.01);
        }

        [Test]
        public void InterpolatePathByNormalizedParameter_MultiCurveEnd()
        {
            path.AddControlPoint(new Vector2(5f, 0f));
            path.AddControlPoint(new Vector2(10f, 0f));  // actual path last point
            path.AddControlPoint(new Vector2(99f, 0f));  // remember, last point is not part of interpolated curve

            Assert.AreEqual(new Vector2(10f, 0f), path.InterpolatePathByNormalizedParameter(1f));
        }

        [Test]
        public void EvaluateCurveLength_Linear()
        {
            // arrange control points external to curve interpolation to make a straight line
            path.SetControlPoint(0, new Vector2(-1f, 0f));
            path.SetControlPoint(3, new Vector2(2f, 0f));

            // for a straight line, the number of segments doesn't matter, we'll always get the exact length
            Assert.AreEqual(1f, path.EvaluateCurveLength(0, 4));
        }
    }
}
