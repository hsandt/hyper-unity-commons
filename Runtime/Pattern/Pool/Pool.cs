using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Object = UnityEngine.Object;

namespace HyperUnityCommons
{
    public class Pool<TPooledObject> where TPooledObject : MonoBehaviour, IPooledObject
    {
        /* Parameters */

        /// Pooled object component in pooled prefab
        private readonly TPooledObject m_PrefabPooledObject;

        /// Parent under which all pooled objects will be created
        private readonly Transform m_PoolTransform;


        /* State */

        /// List of objects in the pool, active or inactive
        private List<TPooledObject> m_Objects;


        /// Constructor
        public Pool(TPooledObject prefabPooledObject, Transform poolTransform)
        {
            m_PrefabPooledObject = prefabPooledObject;
            m_PoolTransform = poolTransform;
        }

        /// Initialise pool with [initialPoolSize] free objects (not in use)
        /// If the pool transform already contains sample objects (children),
        /// reuse them. This part is mostly useful for OrderedPoolContainer used for UI, as we often want to test
        /// layout in the editor so there are often sample pooled objects left in the scene.
        /// We expect the sample objects to be instances of m_PooledObjectPrefab
        /// with no non-trivial property override, so they are really like new prefab
        /// instances we would create at runtime if there were no sample objects.
        /// We don't want to verify all of that, so we just throw if those objects
        /// don't have a TPooledObject component, which is expected on m_PooledObjectPrefab.
        /// ! This is meant for Single Pool only !
        public void InitCheckingExistingChildren(int initialPoolSize)
        {
            // 1. Create list with capacity = expected count
            // (using the max now will avoid LazyInstantiatePooledObjects having to
            // set capacity again to a greater value later)
            int oldChildCount = m_PoolTransform.childCount;
            m_Objects = new List<TPooledObject>(Mathf.Max(oldChildCount, initialPoolSize));

            // 2. Add all existing pooled objects to the objects list,
            //    expecting instances of m_PooledObjectPrefab
            // (if the pool parent starts empty, this does nothing)
            foreach (Transform child in m_PoolTransform)
            {
                TPooledObject pooledObject = child.GetComponentOrFail<TPooledObject>();
                m_Objects.Add(pooledObject);
            }

            // 3. Instantiate any remaining objects to reach [initialPoolSize]
            // (this calls m_Objects.Add too)
            LazyInstantiatePooledObjects(initialPoolSize);

            // 4. Release all objects so we can use them later
            ReleaseAllObjects();
        }

        /// Initialise pool with [initialPoolSize] free objects (not in use)
        /// ! MultiPoolManager should use this to avoid incorrectly registering pooled objects of type A
        ///   as pooled objects of type B.
        ///   This means that multi-pool transforms should not have any children preset in the scene !
        public void InitIgnoringExistingChildren(int initialPoolSize)
        {
            // 1. Create list with capacity = expected count
            m_Objects = new List<TPooledObject>(initialPoolSize);

            // 2. Instantiate [initialPoolSize] pooled objects
            for (int i = 0; i < initialPoolSize; i++)
            {
                InstantiatePooledObject();
            }

            // 3. Release all objects so we can use them later
            ReleaseAllObjects();
        }

        /// Acquire the first [count] objects under pool transform,
        /// release all the other ones, and return an enumerable to those [count] objects
        /// Instantiate as many new objects as needed.
        /// ! This is meant for Single Pool only !
        public IEnumerable<TPooledObject> AcquireOnlyFirstObjects(int count)
        {
            // 1. Instantiate any remaining objects to reach [count]
            LazyInstantiatePooledObjects(count);

            // Note: at this point, we have no assumption on whether an instantiated object
            // should be in use or not, as it depends on TPooledObject type.
            // In general, at that point they are in an intermediate state where they are
            // ready to use, but really used.
            // To make things clear, we will check IsInUse and Acquire/Release as needed.

            // 2. Acquire all required pooled objects that are not already in use
            for (int i = 0; i < count; i++)
            {
                if (!m_Objects[i].IsInUse())
                {
                    m_Objects[i].Acquire();
                }
            }

            // 3. Release any extra objects in use that we don't need now
            for (int i = count; i < m_Objects.Count; i++)
            {
                if (m_Objects[i].IsInUse())
                {
                    m_Objects[i].Release();
                }
            }

            return m_Objects.Take(count);
        }

        /// Instantiate pooled objects until there are [targetPoolSize] of them
        /// under the pool transform
        /// This also registers created objects in the objects list.
        /// ! This is meant for Single Pool only !
        private void LazyInstantiatePooledObjects(int targetPoolSize)
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            int oldChildCount = m_PoolTransform.childCount;
            Debug.AssertFormat(oldChildCount == m_Objects.Count,
                "[Pool] LazyInstantiatePooledObjects: Pool transform has {0} children, whereas we registered {1} pooled objects. " +
                "Make sure to register all created pooled objects, and not add objects of other types " +
                "under the pool transform",
                oldChildCount, m_Objects.Count);
            #endif

            // 1. Prepare capacity if needed
            if (targetPoolSize > m_Objects.Capacity)
            {
                m_Objects.Capacity = targetPoolSize;
            }

            // 2. Instantiate just enough instances to reach targetCount
            // (if we have enough objects, this does nothing)
            for (int i = m_Objects.Count; i < targetPoolSize; i++)
            {
                InstantiatePooledObject();
            }
        }

        /// Instantiate new game object from prefab under poolTransform, initialise it
        /// and add it to pooled objects list, then return reference to it
        /// Fail if prefab has no TPooledObject component.
        /// The caller is responsible for Releasing/Acquiring the new object depending on the needs.
        private TPooledObject InstantiatePooledObject()
        {
            // Instantiate prefab under pool transform and get TPooledObject component
            // (Instantiate<Component> instantiates the game object with the component, then returns the component
            // of the new instance)
            TPooledObject prefabInstancePooledObject = Object.Instantiate(m_PrefabPooledObject, m_PoolTransform);

            // Append count to name to make it easier to distinguish pooled objects
            // Note that name property will set game object name
            // Ex: Projectile(Clone) 3
            prefabInstancePooledObject.name += $" {m_Objects.Count}";

            m_Objects.Add(prefabInstancePooledObject);

            return prefabInstancePooledObject;
        }

        [Obsolete("Use AcquireFreeObject instead, and remove Acquire/SetActive(true) call following this on the caller side")]
        public TPooledObject GetFreeObject(bool instantiateNewObjectOnStarvation)
        {
            return AcquireFreeObject(instantiateNewObjectOnStarvation);
        }

        /// Try to get a free object and Acquire it, if we got any
        /// If no objects are free, check instantiateNewObjectOnStarvation
        /// - if true, instantiate a new object (with initialisation), Acquire it and return it
        /// - if false, return null
        public TPooledObject AcquireFreeObject(bool instantiateNewObjectOnStarvation)
        {
            TPooledObject pooledObject = GetFreeObject();

            if (pooledObject == null && instantiateNewObjectOnStarvation)
            {
                // Pool starvation, but we should instantiate a new object as it is considered important
                // Performance note: we are not "smart" (e.g. instantiating a batch of new objects
                // as a List allocation would do, by power of two). We really just instantiate what we need, 1 object.
                // So in counterpart with log a warning, as generally such last minute instantiation is not intended and
                // only a fallback, so we want to notify developer they may want to increase initial pool size instead.
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogWarningFormat("[Pool] AcquireFreeObject: pool for prefab pooled object '{0}' is starving at size {1} but " +
                                       "instantiateNewObjectOnStarvation is true, so we will instantiate a new object as fallback. " +
                                       "Consider increasing pool size to at least {2} to avoid this situation.",
                    m_PrefabPooledObject, m_Objects.Count, m_Objects.Count + 1);
                #endif

                pooledObject = InstantiatePooledObject();
            }

            if (pooledObject != null)
            {
                pooledObject.Acquire();
            }

            return pooledObject;
        }


        /// Return a free object if any, else null
        private TPooledObject GetFreeObject()
        {
            // O(n)
            // Consider improving performance by tracking list of free objects
            foreach (TPooledObject pooledObject in m_Objects)
            {
                if (!pooledObject.IsInUse())
                {
                    return pooledObject;
                }
            }

            return null;
        }

        /// Return the count of all objects, active or inactive
        public int CountAllObjects()
        {
            return m_Objects.Count;
        }

        /// Return the object at a given index (it may be active or inactive)
        public TPooledObject GetObject(int index)
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.AssertFormat(index < m_Objects.Count,
                "[Pool] GetObject: index {0} is out of bounds, as there are only {1} objects",
                index, m_Objects.Count);
            #endif

            return m_Objects[index];
        }

        /// Return an enumerable to all objects in use
        public IEnumerable<TPooledObject> GetObjectsInUse()
        {
            // O(n)
            // Consider improving performance by tracking list of objects in use
            return m_Objects.Where(pooledObject => pooledObject.IsInUse());
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
            return m_Objects.Any(pooledObject => pooledObject.IsInUse());
        }

        /// Release all objects in use
        public void ReleaseAllObjects()
        {
            foreach (TPooledObject pooledObject in m_Objects)
            {
                if (pooledObject.IsInUse())
                {
                    pooledObject.Release();
                }
            }
        }
    }
}