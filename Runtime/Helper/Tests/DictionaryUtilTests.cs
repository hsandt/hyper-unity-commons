using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace HyperUnityCommons.Tests
{
    [TestFixture]
    public class DictionaryUtilTests
    {
        [Test]
        public void Clone_EmptyDictionary_EqualButNotSameReference()
        {
            var dict = new Dictionary<int, int> {};
            Dictionary<int,int> clone = dict.Clone();
            Assert.AreEqual(dict, clone);
            Assert.AreNotSame(dict, clone);
        }

        [Test]
        public void Clone_FilledDictionary_EqualButNotSameReference()
        {
            var dict = new Dictionary<int, int> {{1, 10}, {2, 20}};
            Dictionary<int,int> clone = dict.Clone();
            Assert.AreEqual(dict, clone);
            Assert.AreNotSame(dict, clone);
        }

        [Test]
        public void Clone_FilledDictionaryWithObjects_ShallowCopy()
        {
            var keyList = new List<int> {1, 10};
            var valueList = new List<int> {2, 20};
            var dict = new Dictionary<List<int>, List<int>> { { keyList, valueList } };
            Dictionary<List<int>, List<int>> clone = dict.Clone();
            Assert.AreEqual(dict, clone);
            Assert.AreNotSame(dict, clone);

            // Shallow copy means objects in keys and values are copied by reference
            Assert.AreEqual(keyList, clone.Keys.First() );
            Assert.AreSame(keyList, clone.Keys.First() );
            Assert.AreEqual(valueList, clone[keyList] );
            Assert.AreSame(valueList, clone[keyList] );
        }
    }
}
