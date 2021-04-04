using System;
using System.Collections.Generic;
using UnityEngine;

using CommonsHelper;
using Object = UnityEngine.Object;

namespace CommonsPattern
{
    public class Pool<TPooledObject> where TPooledObject : MonoBehaviour, IPooledObject
    {
        /* Parameters */
		
        /// Pooled object prefab
        private GameObject m_PooledObjectPrefab;
        
        /// Parent under which all pooled objects will be created
        private Transform m_PoolTransform;

        
        /* State */
        
        /// List of objects in the pool, active or inactive
        private List<TPooledObject> m_Objects = new List<TPooledObject>();

        
        /// Constructor.
        public Pool(GameObject pooledObjectPrefab, Transform poolTransform)
        {
            m_PooledObjectPrefab = pooledObjectPrefab;
            m_PoolTransform = poolTransform;
        }

        /// Initialise pool with [initialPoolSize] objects
        public void Init(int initialPoolSize)
        {
            for (int i = 0; i < initialPoolSize; ++i)
            {
                InstantiatePooledObject();
            }
        }
        
        /// Instantiate new game object from prefab under poolTransform, initialises it,
        /// releases it (so it is ready for usage) and adds it to pooled objects list, then return reference to it.
        /// Fail if prefab has no TPooledObject component.
        public TPooledObject InstantiatePooledObject()
        {
            GameObject prefabInstance = Object.Instantiate(m_PooledObjectPrefab, m_PoolTransform);
            TPooledObject pooledObject = prefabInstance.GetComponentOrFail<TPooledObject>();
            pooledObject.InitPooled();
            pooledObject.Release();
            
            m_Objects.Add(pooledObject);
            
            return pooledObject;
        }
        
        /// Try to return a released object
        /// If no objects are released, check instantiateNewObjectOnStarvation
        /// - if true, instantiate a new object (with initialisation) and return it
        /// - if false, return null
        public TPooledObject GetObject(bool instantiateNewObjectOnStarvation)
        {
            // O(n)
            foreach (TPooledObject pooledObject in m_Objects)
            {
                if (!pooledObject.IsInUse())
                {
                    return pooledObject;
                }
            }
			
            // Pool starvation: check how we should handle it
            // Performance note: if instantiating object, we are not "smart" (e.g. instantiating a batch of new objects
            // as a List allocation would do, by power of two). We really just instantiate what we need, 1 object.
            return instantiateNewObjectOnStarvation ? InstantiatePooledObject() : null;
        }
        
        /// Return true if any pooled object is in use
        public bool AnyInUse()
        {
            // CURRENT ALGORITHM: for every pooled object type, for every stored object, check if this object is in use
            // O(n), n pool size
            // ALTERNATIVE 1: all objects know their pools, and they notify an increment
            //	or decrement in counter of objects in use, so we can directly answer
            // ALTERNATIVE 2: two lists, one of objects released and one of objects in use
            //	we can immediately check the length of the lists to know if any / all are used
            foreach (TPooledObject pooledObject in m_Objects)
            {
                if (pooledObject.IsInUse())
                {
                    return true;
                }
            }
            return false;
        }
    }
}