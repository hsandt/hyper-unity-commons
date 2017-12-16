using UnityEngine;
using System.Collections;

using NUnit.Framework;

[TestFixture]
public class VectorUtilTests {

	Vector2 origin;
	Vector2 u;
	Vector2 v;

	[OneTimeSetUp]
	public void Init () {
		origin = Vector2.zero;
		u = Vector2.right;
		v = Vector2.up;
	}

	[Test]
	public void Rotate_UBy60Deg () {
        Assert.AreEqual(VectorUtil.Rotate(u, 60f), new Vector2(0.5f, Mathf.Sqrt(3)/2));
	}

	[Test]
	public void Rotate90CW_U_MinusV () {
		Assert.AreEqual(-v, VectorUtil.Rotate90CW(u));
	}

	[Test]
	public void Rotate90CCW_U_V () {
		Assert.AreEqual(v, VectorUtil.Rotate90CCW(u));
	}

	[Test]
	public void ClosestPointOnSegmentToPoint_PointOnTheLeft_SegmentStart () {
		Assert.AreEqual(origin, VectorUtil.ClosestPointOnSegmentToPoint(origin, u, new Vector2(-2f, 0f)));
	}

	[Test]
	public void ClosestPointOnSegmentToPoint_PointToSegmentDistance_PointProjectedNearTheMiddle_SegmentNearMiddle () {
		Assert.AreEqual(new Vector2(0.4f, 0f), VectorUtil.ClosestPointOnSegmentToPoint(origin, u, new Vector2(0.4f, 1f)));
	}

	[Test]
	public void ClosestPointOnSegmentToPoint_PointOnTheRight_SegmentEnd () {
		Assert.AreEqual(u, VectorUtil.ClosestPointOnSegmentToPoint(origin, u, new Vector2(2f, 1f)));
	}

	[Test]
	public void ClosestPointOnSegmentToPoint_SegmentReducedToPoint_SegmentUniquePoint () {
		Assert.AreEqual(u, VectorUtil.ClosestPointOnSegmentToPoint(u, u, new Vector2(10f, -5f)));
	}

	[Test]
	public void PointToSegmentDistance_PointOnTheLeft_DistanceToSegmentStart () {
		Assert.AreEqual(2f, VectorUtil.PointToSegmentDistance(new Vector2(-2f, 0f), origin, u));
	}

	[Test]
	public void PointToSegmentDistance_PointProjectedNearTheMiddle_DistanceToProjection () {
		Assert.AreEqual(17f, VectorUtil.PointToSegmentDistance(new Vector2(0.6f, -17f), origin, u));
	}

	[Test]
	public void PointToSegmentDistance_PointOnTheRight_DistanceToSegmentEnd () {
		Assert.AreEqual(3f, VectorUtil.PointToSegmentDistance(new Vector2(1f, -3f), origin, u));
	}

	[Test]
	public void PointToSegmentDistance_SegmentReducedToPoint_DistanceToSegmentUniquePoint () {
		Assert.AreEqual(2f, VectorUtil.PointToSegmentDistance(new Vector2(1f, -2f), u, u));
	}

	[Test]
	public void PointToSegmentDistance_PointOnSegment_Zero () {
		Assert.AreEqual(0f, VectorUtil.PointToSegmentDistance(new Vector2(0.6f, 0f), origin, u));
	}

	[Test]
	public void PointToSegmentDistanceOutParamDistance_PointOnTheLeft_DistanceToSegmentStart () {
		float paramDistance;
		Assert.AreEqual(2f, VectorUtil.PointToSegmentDistance(new Vector2(-2f, 0f), origin, u, out paramDistance));
		Assert.AreEqual(0f, paramDistance);
	}

	[Test]
	public void PointToSegmentDistanceOutParamDistance_PointProjectedNearTheMiddle_DistanceToProjection () {
		float paramDistance;
		Assert.AreEqual(3f, VectorUtil.PointToSegmentDistance(new Vector2(0.7f, 3f), origin, u, out paramDistance));
		Assert.AreEqual(0.7f, paramDistance);
	}

	[Test]
	public void PointToSegmentDistanceOutParamDistance_PointOnTheRight_DistanceToSegmentEnd () {
		float paramDistance;
		Assert.AreEqual(9f, VectorUtil.PointToSegmentDistance(new Vector2(10f, 0f), origin, u, out paramDistance));
		Assert.AreEqual(1f, paramDistance);
	}

}
