using System.Collections;
using UnityEngine;

using NUnit.Framework;

namespace HyperUnityCommons.Tests
{

	[TestFixture]
	public class AnimationCurveUtilTests {

	    AnimationCurve constantZeroCurve;
	    AnimationCurve constantPointCurve;
	    AnimationCurve constantPositiveCurve;
	    AnimationCurve constantMixedCurve;
	    AnimationCurve linearPositiveCurve;
	    AnimationCurve linearMixedCurve;
	    AnimationCurve bezierSpline;
	    AnimationCurve bezierMixedCurve;

	    [OneTimeSetUp]
	    public void Init () {
	        constantZeroCurve = AnimationCurveUtil.CreateStep(new Keyframe(0f, 0f));
	        constantPointCurve = AnimationCurveUtil.CreateStep(new Keyframe(0f, 9999f));
	        constantPositiveCurve = AnimationCurveUtil.CreateStep(new Keyframe(0f, 0f), new Keyframe(1f, 1f), new Keyframe(2f, 3f), new Keyframe(4f, 0f));
	        constantMixedCurve = AnimationCurveUtil.CreateStep(new Keyframe(0f, 0f), new Keyframe(1f, 2f), new Keyframe(2f, -3f), new Keyframe(4f, 1f));
	        linearPositiveCurve = AnimationCurveUtil.CreatePiecewiseLinear(new Keyframe(0f, 0f), new Keyframe(1f, 1f), new Keyframe(2f, 1f), new Keyframe(4f, 0f));
	        linearMixedCurve = AnimationCurveUtil.CreatePiecewiseLinear(new Keyframe(0f, -2f), new Keyframe(1f, 1f), new Keyframe(2f, -1f), new Keyframe(4f, 0f));

	        // default tangents are 0
	        Keyframe k0 = new Keyframe(0f, 1f);
	        Keyframe k1 = new Keyframe(1f, 2f);
	        k1.outTangent = -2f;
	        Keyframe k2 = new Keyframe(2f, 1f);
	        Keyframe k3 = new Keyframe(4f, -2f);
	        k3.inTangent = -2f;

	        bezierSpline = new AnimationCurve(k1, k2);
	        bezierMixedCurve = new AnimationCurve(k0, k1, k2, k3);
	    }

	    [Test]
	    public void GetDuration_ConstantZeroCurve_Zero () {
	        Assert.AreEqual(0f, constantZeroCurve.GetDuration());
	    }

	    [Test]
	    public void GetDuration_ConstantPointCurve_Zero () {
	        Assert.AreEqual(0f, constantPointCurve.GetDuration());
	    }

	    [Test]
	    public void GetDuration_ConstantPositiveCurve_LastKeyTime () {
	        Assert.AreEqual(4f, constantPositiveCurve.GetDuration());
	    }

	    [Test]
	    public void Integral_ConstantZeroCurve_Zero () {
	        Assert.AreEqual(0f, AnimationCurveUtil.Integral(constantZeroCurve));
	    }

	    [Test]
	    public void Integral_ConstantPointCurve_Zero () {
		    // Value is high, but interval on X is zero, so result is 0
	        Assert.AreEqual(0f, AnimationCurveUtil.Integral(constantPointCurve));
	    }

	    [Test]
	    public void Integral_ConstantPositiveCurve_Positive () {
	        Assert.AreEqual(0f + 1f + 6f, AnimationCurveUtil.Integral(constantPositiveCurve));
	    }

	    [Test]
	    public void Integral_ConstantMixedCurve_Signed () {
	        Assert.AreEqual(0f + 2f - 6f, AnimationCurveUtil.Integral(constantMixedCurve));
	    }

	    [Test]
	    public void Integral_LinearPositiveCurve_Positive () {
	        Assert.AreEqual(0.5f + 1f + 1f, AnimationCurveUtil.Integral(linearPositiveCurve));
	    }

	    [Test]
	    public void Integral_LinearMixedCurve_Signed () {
	        Assert.AreEqual(-0.5f + 0f - 1f, AnimationCurveUtil.Integral(linearMixedCurve));
	    }

	    [Test]
	    public void Integral_BezierSpline_Signed () {
	        Assert.That(4f / 3f, Is.EqualTo(AnimationCurveUtil.Integral(bezierSpline)).Within(3e-7f));
	    }

	    [Test]
	    public void Integral_BezierMixedCurve_Signed () {
	        // I computed the various integral parts wih Desmos, using an example of Bezier curve,
	        // placing the control points at 1/3 along the X axis, and converting t coordinate
	        // to x coordinate for the integral with the formula integral(0,1)(g_2(t)g_1'(t)dt)
	        // where g_1(t) is the X coord of the Bezier curve and g_2(t) its Y coord
	        Assert.That(1.5f + 4f / 3f - 1f / 3f, Is.EqualTo(AnimationCurveUtil.Integral(bezierMixedCurve)).Within(5e-7f));
	    }

	}

}

