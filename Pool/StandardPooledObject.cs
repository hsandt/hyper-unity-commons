using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonsPattern
{
    /// Common implementation of IPooledObject for MonoBehaviour, based on game object activation
    /// This component can be added directly to any pooled object considered active when game object is active
    /// (most of them work like that), and whose logic (including positioning on activation) is done elsewhere
    /// (via direct code or inside other components)
    /// Usage:
    /// For each new type of Pooled Object that can work with this common implementation, create a new PoolManager:
    /// public class MyPoolManager : PoolManager&lt;StandardPooledObject, MyPoolManager> {}
    /// If it doesn't have particular functionality, you don't even need to add methods.
    /// The only reason we must create a new class is because PoolManager inherits from SingletonManager,
    /// so we need a brand new class with its own unique Instance each time.
    public class StandardPooledObject : MonoBehaviour, IPooledObject
    {
        /* IPooledObject interface */

        public void InitPooled()
        {
        }

        public bool IsInUse()
        {
            return gameObject.activeSelf;
        }

        public void Release()
        {
            gameObject.SetActive(false);
        }


        /* Own methods */

        public void Activate()
        {
            gameObject.SetActive(true);
        }
    }
}
