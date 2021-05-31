using UnityEngine;
using System.Collections;

using NUnit.Framework;

namespace CommonsHelper.Tests
{
    [TestFixture]
    public class VectorUtilTests
    {
        Vector2 origin;
        Vector2 u;
        Vector2 v;

        [OneTimeSetUp]
        public void Init()
        {
            origin = Vector2.zero;
            u = Vector2.right;
            v = Vector2.up;
        }

        [Test]
        public void ProjectParallel_OnDiagonal()
        {
            Assert.AreEqual(new Vector2(-0.5f, -0.5f), VectorUtil.ProjectParallel(-v, u + v));
        }

        [Test]
        public void ProjectOrthogonal_OnDiagonal()
        {
            Assert.AreEqual(new Vector2(0.5f, -0.5f), VectorUtil.ProjectOrthogonal(-v, u + v));
        }

        [Test]
        public void Mirror_OnDiagonal()
        {
            Assert.AreEqual(-u, VectorUtil.Mirror(-v, u + v));
        }

        [Test]
        public void Rotate_UBy60Deg()
        {
            Assert.AreEqual(new Vector2(0.5f, Mathf.Sqrt(3) / 2), VectorUtil.Rotate(u, 60f));
        }

        [Test]
        public void Rotate90CW_U_MinusV()
        {
            Assert.AreEqual(-v, VectorUtil.Rotate90CW(u));
        }

        [Test]
        public void Rotate90CCW_U_V()
        {
            Assert.AreEqual(v, VectorUtil.Rotate90CCW(u));
        }

        [Test]
        public void ClosestPointOnSegmentToPoint_PointOnTheLeft_SegmentStart()
        {
            Assert.AreEqual(origin, VectorUtil.ClosestPointOnSegmentToPoint(origin, u, new Vector2(-2f, 0f)));
        }

        [Test]
        public void ClosestPointOnSegmentToPoint_PointToSegmentDistance_PointProjectedNearTheMiddle_SegmentNearMiddle()
        {
            Assert.AreEqual(new Vector2(0.4f, 0f),
                VectorUtil.ClosestPointOnSegmentToPoint(origin, u, new Vector2(0.4f, 1f)));
        }

        [Test]
        public void ClosestPointOnSegmentToPoint_PointOnTheRight_SegmentEnd()
        {
            Assert.AreEqual(u, VectorUtil.ClosestPointOnSegmentToPoint(origin, u, new Vector2(2f, 1f)));
        }

        [Test]
        public void ClosestPointOnSegmentToPoint_SegmentReducedToPoint_SegmentUniquePoint()
        {
            Assert.AreEqual(u, VectorUtil.ClosestPointOnSegmentToPoint(u, u, new Vector2(10f, -5f)));
        }

        [Test]
        public void PointToSegmentDistance_PointOnTheLeft_DistanceToSegmentStart()
        {
            Assert.AreEqual(2f, VectorUtil.PointToSegmentDistance(new Vector2(-2f, 0f), origin, u));
        }

        [Test]
        public void PointToSegmentDistance_PointProjectedNearTheMiddle_DistanceToProjection()
        {
            Assert.AreEqual(17f, VectorUtil.PointToSegmentDistance(new Vector2(0.6f, -17f), origin, u));
        }

        [Test]
        public void PointToSegmentDistance_PointOnTheRight_DistanceToSegmentEnd()
        {
            Assert.AreEqual(3f, VectorUtil.PointToSegmentDistance(new Vector2(1f, -3f), origin, u));
        }

        [Test]
        public void PointToSegmentDistance_SegmentReducedToPoint_DistanceToSegmentUniquePoint()
        {
            Assert.AreEqual(2f, VectorUtil.PointToSegmentDistance(new Vector2(1f, -2f), u, u));
        }

        [Test]
        public void PointToSegmentDistance_PointOnSegment_Zero()
        {
            Assert.AreEqual(0f, VectorUtil.PointToSegmentDistance(new Vector2(0.6f, 0f), origin, u));
        }

        [Test]
        public void PointToSegmentDistanceOutParamDistance_PointOnTheLeft_DistanceToSegmentStart()
        {
            float paramDistance;
            Assert.AreEqual(2f, VectorUtil.PointToSegmentDistance(new Vector2(-2f, 0f), origin, u, out paramDistance));
            Assert.AreEqual(0f, paramDistance);
        }

        [Test]
        public void PointToSegmentDistanceOutParamDistance_PointProjectedNearTheMiddle_DistanceToProjection()
        {
            float paramDistance;
            Assert.AreEqual(3f, VectorUtil.PointToSegmentDistance(new Vector2(0.7f, 3f), origin, u, out paramDistance));
            Assert.AreEqual(0.7f, paramDistance);
        }

        [Test]
        public void PointToSegmentDistanceOutParamDistance_PointOnTheRight_DistanceToSegmentEnd()
        {
            float paramDistance;
            Assert.AreEqual(9f, VectorUtil.PointToSegmentDistance(new Vector2(10f, 0f), origin, u, out paramDistance));
            Assert.AreEqual(1f, paramDistance);
        }

        [Test]
        public void RoundVector2()
        {
            Assert.AreEqual(new Vector2(1.5f, -1f), VectorUtil.RoundVector2(new Vector2(1.26f, -1.25f), 0.5f));
        }
        
        [Test]
        public void RoundVector3()
        {
            Assert.AreEqual(new Vector3(1.5f, -1f, 1f), VectorUtil.RoundVector3(new Vector3(1.26f, -1.25f, 0.75f), 0.5f));
        }
    }
}