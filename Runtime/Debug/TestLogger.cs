using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HyperUnityCommons
{
    /// This is just a sample script for quick testing UnityEvent and other things that need a MonoBehaviour and
    /// a function to call to see the effect
    public class TestLogger : MonoBehaviour
    {
        public void TestLog()
        {
            Debug.LogFormat(this, "TestLog called on {0}", this);
        }
    }
}
