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
        Assert.IsFalse(0f.IsNotAlmostZero());
	}

	[Test]
    public void IsNotAlmostZero_1_True () {
        Assert.IsTrue(1f.IsNotAlmostZero());
	}

    [Test]
    public void IsAlmost_PI_3c14159274True () {
        Assert.IsTrue(Mathf.PI.IsAlmost(3.14159274f));
    }

    [Test]
    public void IsAlmost_PI_3c141593_False () {
        Assert.IsFalse(Mathf.PI.IsAlmost(3.141593f));
    }

    [Test]
    public void IsNotAlmost_1c5_1c5_False () {
        Assert.IsFalse(1.5f.IsNotAlmost(1.5f));
    }

    [Test]
    public void IsNotAlmost_1c5_1c6_True () {
        Assert.IsTrue(1.5f.IsNotAlmost(1.6f));
    }

}
