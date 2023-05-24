using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HyperUnityCommons
{
    /// Adaptation of StandardPooledObject for MasterBehaviour
    public class StandardMasterPooledObject : MasterBehaviour, IPooledObject
    {
        /* IPooledObject interface */

        public void Acquire()
        {
            gameObject.SetActive(true);
        }

        public bool IsInUse()
        {
            return gameObject.activeSelf;
        }

        public void Release()
        {
            gameObject.SetActive(false);
        }
    }
}