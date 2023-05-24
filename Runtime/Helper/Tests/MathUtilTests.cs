using UnityEngine;
using System.Collections;

using NUnit.Framework;

namespace HyperUnityCommons.Tests
{
    [TestFixture]
    public class MathUtilTests
    {
        [Test]
        public void ToTernary_Negative()
        {
            Assert.AreEqual(-1, MathUtil.ToTernary(-1f));
        }

        [Test]
        public void ToTernary_Zero()
        {
            Assert.AreEqual(0, MathUtil.ToTernary(0f));
        }

        [Test]
        public void ToTernary_Positive()
        {
            Assert.AreEqual(1, MathUtil.ToTernary(1f));
        }

        [Test]
        public void Truncate_Negative()
        {
            Assert.AreEqual(-1, MathUtil.Truncate(-1.9f));
        }

        [Test]
        public void Truncate_Zero()
        {
            Assert.AreEqual(0, MathUtil.Truncate(0f));
        }

        [Test]
        public void Truncate_Positive()
        {
            Assert.AreEqual(1, MathUtil.Truncate(1.9f));
        }

        [Test]
        public void Round_Negative_Exact()
        {
            Assert.AreEqual(-4f, MathUtil.Round(-4f, 2f));
        }

        [Test]
        public void Round_Negative_Middle1()
        {
            // Banker's round always rounds to nearest even, so: -0.5 -> 0
            // and at scale 2x: -1 -> 0
            Assert.AreEqual(0f, MathUtil.Round(-1f, 2f));
        }

        [Test]
        public void Round_Negative_Middle2()
        {
            Assert.AreEqual(-4f, MathUtil.Round(-3f, 2f));
        }

        [Test]
        public void Round_Negative_Middle3_Negative_Snap()
        {
            Assert.AreEqual(0f, MathUtil.Round(-1f, -2f));
        }

        [Test]
        public void Round_Zero()
        {
            Assert.AreEqual(0f, MathUtil.Round(0f, 2f));
        }

        [Test]
        public void Round_Positive_Exact()
        {
            Assert.AreEqual(4f, MathUtil.Round(4f, 2f));
        }

        [Test]
        public void Round_Positive_Middle1()
        {
            // Banker's round always rounds to nearest even, so: 0.5 -> 0
            // and at scale 2x: 1 -> 0
            Assert.AreEqual(0f, MathUtil.Round(1f, 2f));
        }

        [Test]
        public void Round_Positive_Middle2()
        {
            Assert.AreEqual(4f, MathUtil.Round(3f, 2f));
        }

        [Test]
        public void Round_Positive_Middle3_Negative_Snap()
        {
            Assert.AreEqual(0f, MathUtil.Round(1f, -2f));
        }

        [Test]
        public void Remap_BeyondLeftBound()
        {
            Assert.AreEqual(30f, MathUtil.Remap(1f, 2f, 30f, 40f, 0f));
        }

        [Test]
        public void Remap_BeyondRightBound()
        {
            Assert.AreEqual(40f, MathUtil.Remap(1f, 2f, 30f, 40f, 3f));
        }

        [Test]
        public void Remap_Middle()
        {
            Assert.AreEqual(35f, MathUtil.Remap(1f, 2f, 30f, 40f, 1.5f));
        }

        [Test]
        public void Remap_SameXSameY()
        {
            // If xA == xB AND yA == yB, we exceptionally tolerate the case and return yA without assert.
            // This is useful to handle degenerated cases such as path evaluation with key points located at the same place, without errors.
            Assert.AreEqual(30f, MathUtil.Remap(99f, 99f, 30f, 30f, 99f));
        }

        [Test]
        public void RemapUnclamped_BeyondLeftBound()
        {
            Assert.AreEqual(20f, MathUtil.RemapUnclamped(1f, 2f, 30f, 40f, 0f));
        }

        [Test]
        public void RemapUnclamped_BeyondRightBound()
        {
            Assert.AreEqual(50f, MathUtil.RemapUnclamped(1f, 2f, 30f, 40f, 3f));
        }

        [Test]
        public void RemapUnclamped_Middle()
        {
            Assert.AreEqual(35f, MathUtil.RemapUnclamped(1f, 2f, 30f, 40f, 1.5f));
        }

        [Test]
        public void RemapUnclamped_SameXSameY()
        {
            // Same as Remap_SameXSameY, but testing that x outside range still works
            Assert.AreEqual(30f, MathUtil.RemapUnclamped(99f, 99f, 30f, 30f, 199f));
        }

        [TestCase(0f, 1f)]
        [TestCase(-80f, 0f)]
        [TestCase(-90f, 0f)]
        public void VolumeDbToFactor_Db_Factor(float db, float factor)
        {
            Assert.AreEqual(factor, MathUtil.VolumeDbToFactor(db));
        }

        [Test]
        public void VolumeDbToFactor_ClampedAt20Db()
        {
            float factor20Db = MathUtil.VolumeDbToFactor(20f);
            float factor30Db = MathUtil.VolumeDbToFactor(30f);
            Assert.AreEqual(factor20Db, factor30Db);
        }

        [Test]
        public void VolumeDbToFactor_Minus6_Approx_Half()
        {
            Assert.That(MathUtil.VolumeDbToFactor(-6f), Is.EqualTo(0.5f).Within(0.005f));
        }

        [TestCase(1f, 0f)]
        [TestCase(0f, -80f)]
        [TestCase(-1f, -80f)]
        public void VolumeFactorToDb_Factor_Db(float db, float factor)
        {
            Assert.AreEqual(factor, MathUtil.VolumeFactorToDb(db));
        }

        [Test]
        public void VolumeFactorToDb_ClampedAt20Db()
        {
            float factor20Db = MathUtil.VolumeDbToFactor(20f);
            Assert.AreEqual(20f, MathUtil.VolumeFactorToDb(factor20Db + 1f));
        }

        [Test]
        public void VolumeFactorToDb_Half_Approx_Minus6()
        {
            Assert.That(MathUtil.VolumeFactorToDb(0.5f), Is.EqualTo(-6f).Within(0.05f));
        }
    }
}
