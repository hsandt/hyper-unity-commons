using UnityEngine;
using System.Collections;

using NUnit.Framework;

[TestFixture]
public class DirectionTests {

    [Test]
    public void ToHorizontalDirection_Negative_Left () {
        Assert.AreEqual(HorizontalDirection.Left, DirectionUtil.ToHorizontalDirection(-1f));
    }

    [Test]
    public void ToHorizontalDirection_Zero_None () {
        Assert.AreEqual(HorizontalDirection.None, DirectionUtil.ToHorizontalDirection(0f));
    }

    [Test]
    public void ToHorizontalDirection_Positive_Right() {
        Assert.AreEqual(HorizontalDirection.Right, DirectionUtil.ToHorizontalDirection(10f));
    }

    [Test]
    public void ToSignX_Left_Minus1 () {
        Assert.AreEqual(-1f, DirectionUtil.ToSignX(HorizontalDirection.Left));
    }

    [Test]
    public void ToSignX_None_0 () {
        Assert.AreEqual(0f, DirectionUtil.ToSignX(HorizontalDirection.None));
    }

    [Test]
    public void ToSignX_Right_1 () {
        Assert.AreEqual(1f, DirectionUtil.ToSignX(HorizontalDirection.Right));
    }

}
