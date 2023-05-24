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
        public void GetLowerBound_0_0()
        {
            var list = new List<int> {1, 3};
            Assert.AreEqual(0, list.GetLowerBound(0));
        }

        [Test]
        public void GetLowerBound_1_0()
        {
            var list = new List<int> {1, 2, 3};
            Assert.AreEqual(0, list.GetLowerBound(1));
        }

        [Test]
        public void GetLowerBound_2_1()
        {
            var list = new List<int> {1, 2, 3};
            Assert.AreEqual(1, list.GetLowerBound(2));
        }
    }
}
