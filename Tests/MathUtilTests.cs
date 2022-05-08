using UnityEngine;
using System.Collections;

using NUnit.Framework;

namespace CommonsHelper.Tests
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

    }
}
