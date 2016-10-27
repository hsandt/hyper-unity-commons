using UnityEngine;
using System.Collections;

using NUnit.Framework;

[TestFixture]
public class VectorUtilTests {

	Vector2 origin;
	Vector2 u;
	Vector2 v;

	[TestFixtureSetUp]
	public void Init () {
		origin = Vector2.zero;
		u = Vector2.right;
		v = Vector2.up;
	}

	// [ExpectedException(typeof(ArgumentException)]
	// public void Method_Argument_ArgumentException () {
	// }

	[Test]
	public void Rotate_UBy60Deg () {
		Assert.That(VectorUtil.Rotate(u, 60f), Is.EqualTo(new Vector2(0.5f, Mathf.Sqrt(3)/2)));
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
	public void PointToSegmentDistance_PointOnTheLeft_DistanceToSegmentStart () {
		Assert.AreEqual(2f, VectorUtil.PointToSegmentDistance(new Vector2(-2f, 0f), origin, u));
	}

}
