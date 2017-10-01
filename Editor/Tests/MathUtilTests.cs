using UnityEngine;
using System.Collections;

using NUnit.Framework;

[TestFixture]
public class MathUtilTests {

	[Test]
    public void IsAlmostZero_0_True () {
        Assert.IsTrue(0f.IsAlmostZero());
	}

	[Test]
    public void IsAlmostZero_1_False () {
        Assert.IsFalse(1f.IsAlmostZero());
	}

	[Test]
    public void IsNotAlmostZero_0_False () {
        Assert.IsFalse(0f.IsAlmostZero());
	}

	[Test]
    public void IsNotAlmostZero_1_True () {
        Assert.IsTrue(1f.IsNotAlmostZero());
	}

}
