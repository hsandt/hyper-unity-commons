using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.LiveCapture;

namespace HyperUnityCommons.Tests
{
    [TestFixture]
    public class CircularBufferTests
    {
        [Test]
        public void Capacity_Initial()
        {
            var buffer = new CircularBuffer<int>(2);
            Assert.AreEqual(2, buffer.Capacity);
        }

        [Test]
        public void Count_Initial()
        {
            var buffer = new CircularBuffer<int>(2);
            Assert.AreEqual(0, buffer.Count);
        }

        [Test]
        public void Count_AfterPushBackWithoutOverwrite()
        {
            var buffer = new CircularBuffer<int>(2);
            buffer.PushBack(1);
            buffer.PushBack(2);
            Assert.AreEqual(2, buffer.Count);
        }

        [Test]
        public void Back_AfterPushBackWithoutOverwrite()
        {
            var buffer = new CircularBuffer<int>(2);
            buffer.PushBack(1);
            buffer.PushBack(2);
            Assert.AreEqual(2, buffer.Back());
            Assert.AreEqual(2, buffer[^1]);
        }

        [Test]
        public void IndexAccess_AfterPushBackWithoutOverwrite()
        {
            var buffer = new CircularBuffer<int>(2);
            buffer.PushBack(1);
            buffer.PushBack(2);
            Assert.AreEqual(1, buffer[0]);
            Assert.AreEqual(2, buffer[1]);
        }

        [Test]
        public void Count_AfterOverwrite()
        {
            var buffer = new CircularBuffer<int>(2);
            buffer.PushBack(1);
            buffer.PushBack(2);
            buffer.PushBack(3);
            Assert.AreEqual(2, buffer.Count);
        }

        [Test]
        public void IndexAccess_AfterOverwrite()
        {
            var buffer = new CircularBuffer<int>(2);
            buffer.PushBack(1);
            buffer.PushBack(2);
            buffer.PushBack(3);
            Assert.AreEqual(2, buffer[0]);
            Assert.AreEqual(3, buffer[1]);
        }
    }
}
