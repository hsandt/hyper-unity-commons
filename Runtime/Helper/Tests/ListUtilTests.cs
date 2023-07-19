using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;

namespace HyperUnityCommons.Tests
{
    [TestFixture]
    public class ListUtilTests
    {
        [Test]
        public void GetLowerBoundIndex_ListIsEmpty()
        {
            var list = new List<float> {};
            Assert.AreEqual(0, list.GetLowerBoundIndex(0f));
        }

        [Test]
        public void GetLowerBoundIndex_ValueIsLessThanListMinimum()
        {
            var list = new List<float> {1f, 2f, 3f};
            Assert.AreEqual(0, list.GetLowerBoundIndex(0f));
        }

        [Test]
        public void GetLowerBoundIndex_ValueIsJustBelowMinimum()
        {
            var list = new List<float> {1f, 2f, 3f};
            Assert.AreEqual(0, list.GetLowerBoundIndex(0.9f));
        }

        [Test]
        public void GetLowerBoundIndex_ValueIsSameAsMinimum()
        {
            var list = new List<float> {1f, 2f, 3f};
            Assert.AreEqual(0, list.GetLowerBoundIndex(1f));
        }

        [Test]
        public void GetLowerBoundIndex_ValueIsSameAsMinimum3x()
        {
            var list = new List<float> {1f, 1f, 1f, 2f, 3f};
            Assert.AreEqual(0, list.GetLowerBoundIndex(1f));
        }

        [Test]
        public void GetLowerBoundIndex_ValueIsJustAboveMinimum()
        {
            var list = new List<float> {1f, 2f, 3f};
            Assert.AreEqual(1, list.GetLowerBoundIndex(1.1f));
        }

        [Test]
        public void GetLowerBoundIndex_ValueIsJustAboveMinimum3x()
        {
            var list = new List<float> {1f, 1f, 1f, 2f, 3f};
            Assert.AreEqual(3, list.GetLowerBoundIndex(1.1f));
        }

        [Test]
        public void GetLowerBoundIndex_ValueIsJustBelowListValueInMiddle()
        {
            var list = new List<float> {1f, 2f, 3f};
            Assert.AreEqual(1, list.GetLowerBoundIndex(1.9f));
        }

        [Test]
        public void GetLowerBoundIndex_ValueIsJustBelowListValueInMiddle3x()
        {
            var list = new List<float> {1f, 2f, 2f, 2f, 3f};
            Assert.AreEqual(1, list.GetLowerBoundIndex(1.9f));
        }

        [Test]
        public void GetLowerBoundIndex_ValueIsSameAsListValueInMiddle()
        {
            var list = new List<float> {1f, 2f, 3f};
            Assert.AreEqual(1, list.GetLowerBoundIndex(2f));
        }

        [Test]
        public void GetLowerBoundIndex_ValueIsSameAsListValueInMiddle3x()
        {
            var list = new List<float> {1f, 2f, 2f, 2f, 3f};
            Assert.AreEqual(1, list.GetLowerBoundIndex(2f));
        }

        [Test]
        public void GetLowerBoundIndex_ValueIsJustAboveListValueInMiddle()
        {
            var list = new List<float> {1f, 2f, 3f};
            Assert.AreEqual(2, list.GetLowerBoundIndex(2.1f));
        }

        [Test]
        public void GetLowerBoundIndex_ValueIsJustAboveListValueInMiddle3x()
        {
            var list = new List<float> {1f, 2f, 2f, 2f, 3f};
            Assert.AreEqual(4, list.GetLowerBoundIndex(2.1f));
        }

        [Test]
        public void GetLowerBoundIndex_ValueIsSameAsListMaximum()
        {
            var list = new List<float> {1f, 2f, 3f};
            Assert.AreEqual(2, list.GetLowerBoundIndex(3f));
        }

        [Test]
        public void GetLowerBoundIndex_ValueIsSameAsListMaximum3x()
        {
            var list = new List<float> {1f, 2f, 3f, 3f, 3f};
            Assert.AreEqual(2, list.GetLowerBoundIndex(3f));
        }

        [Test]
        public void GetLowerBoundIndex_ValueIsGreaterThanListMaximum()
        {
            var list = new List<float> {1f, 2f, 3f};
            Assert.AreEqual(3, list.GetLowerBoundIndex(4f));
        }

        [Test]
        public void GetUpperBoundIndex_ListIsEmpty()
        {
            var list = new List<float> {};
            Assert.AreEqual(0, list.GetUpperBoundIndex(0f));
        }

        [Test]
        public void GetUpperBoundIndex_ValueIsLessThanListMinimum()
        {
            var list = new List<float> {1f, 2f, 3f};
            Assert.AreEqual(0, list.GetUpperBoundIndex(0f));
        }

        [Test]
        public void GetUpperBoundIndex_ValueIsJustBelowMinimum()
        {
            var list = new List<float> {1f, 2f, 3f};
            Assert.AreEqual(0, list.GetUpperBoundIndex(0.9f));
        }

        [Test]
        public void GetUpperBoundIndex_ValueIsSameAsMinimum()
        {
            var list = new List<float> {1f, 2f, 3f};
            Assert.AreEqual(1, list.GetUpperBoundIndex(1f));
        }

        [Test]
        public void GetUpperBoundIndex_ValueIsSameAsMinimum3x()
        {
            var list = new List<float> {1f, 1f, 1f, 2f, 3f};
            Assert.AreEqual(3, list.GetUpperBoundIndex(1f));
        }

        [Test]
        public void GetUpperBoundIndex_ValueIsJustAboveMinimum()
        {
            var list = new List<float> {1f, 2f, 3f};
            Assert.AreEqual(1, list.GetUpperBoundIndex(1.1f));
        }

        [Test]
        public void GetUpperBoundIndex_ValueIsJustAboveMinimum3x()
        {
            var list = new List<float> {1f, 1f, 1f, 2f, 3f};
            Assert.AreEqual(3, list.GetUpperBoundIndex(1.1f));
        }

        [Test]
        public void GetUpperBoundIndex_ValueIsJustBelowListValueInMiddle()
        {
            var list = new List<float> {1f, 2f, 3f};
            Assert.AreEqual(1, list.GetUpperBoundIndex(1.9f));
        }

        [Test]
        public void GetUpperBoundIndex_ValueIsJustBelowListValueInMiddle3x()
        {
            var list = new List<float> {1f, 2f, 2f, 2f, 3f};
            Assert.AreEqual(1, list.GetUpperBoundIndex(1.9f));
        }

        [Test]
        public void GetUpperBoundIndex_ValueIsSameAsListValueInMiddle()
        {
            var list = new List<float> {1f, 2f, 3f};
            Assert.AreEqual(2, list.GetUpperBoundIndex(2f));
        }

        [Test]
        public void GetUpperBoundIndex_ValueIsSameAsListValueInMiddle3x()
        {
            var list = new List<float> {1f, 2f, 2f, 2f, 3f};
            Assert.AreEqual(4, list.GetUpperBoundIndex(2f));
        }

        [Test]
        public void GetUpperBoundIndex_ValueIsJustAboveListValueInMiddle()
        {
            var list = new List<float> {1f, 2f, 3f};
            Assert.AreEqual(2, list.GetUpperBoundIndex(2.1f));
        }

        [Test]
        public void GetUpperBoundIndex_ValueIsJustAboveListValueInMiddle3x()
        {
            var list = new List<float> {1f, 2f, 2f, 2f, 3f};
            Assert.AreEqual(4, list.GetUpperBoundIndex(2.1f));
        }

        [Test]
        public void GetUpperBoundIndex_ValueIsSameAsListMaximum()
        {
            var list = new List<float> {1f, 2f, 3f};
            Assert.AreEqual(3, list.GetUpperBoundIndex(3f));
        }

        [Test]
        public void GetUpperBoundIndex_ValueIsSameAsListMaximum3x()
        {
            var list = new List<float> {1f, 2f, 3f, 3f, 3f};
            Assert.AreEqual(5, list.GetUpperBoundIndex(3f));
        }

        [Test]
        public void GetUpperBoundIndex_ValueIsGreaterThanListMaximum()
        {
            var list = new List<float> {1f, 2f, 3f};
            Assert.AreEqual(3, list.GetUpperBoundIndex(4f));
        }
    }
}
