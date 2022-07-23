using System;
using System.Collections.Generic;
using UnityEngine;

using CommonsHelper;

namespace CommonsPattern
{

	/// CRTP: when inheriting from this base class, use the derived class as the T generic argument so that you can access a singleton instance of the derived class
	public abstract class PoolManager<TPooledObject, T> : SingletonManager<T> where TPooledObject : MonoBehaviour, IPooledObject where T : SingletonManager<T> {

		[Header("Prefabs")]

		[Tooltip("Pooled object prefab")]
		public GameObject pooledObjectPrefab;


		[Header("External scene references")]

		[Tooltip("Parent under which all pooled objects will be created. If not set in inspector, must be set in code, " +
		         "in override Init() but before base.Init().")]
		public Transform poolTransform;


		[Header("Parameters")]

		[SerializeField, Tooltip("Initial pool size (may change if Instantiate New Object On Starvation is true)")]
		protected int initialPoolSize = 20;

		[SerializeField, Tooltip("Should the manager instantiate a new pooled object in case of starvation? " +
		                         "This will increase the pool size dynamically and may slow down on spawn, " +
		                         "but avoids not spawning an object at all. Recommended for gameplay objects.")]
		protected bool instantiateNewObjectOnStarvation = false;


		/* State */

		/// Pool managed internally
		protected Pool<TPooledObject> m_Pool;


		protected override void Init ()
		{
			TPooledObject prefabPooledObject = pooledObjectPrefab.GetComponentOrFail<TPooledObject>();

			m_Pool = new Pool<TPooledObject>(prefabPooledObject, poolTransform);

			// Check for initial pool size override (generally not useful for single pool, but checked
			// for consistency)
			int initialPoolSizeOverride = prefabPooledObject.InitialPoolSizeOverride;
			int actualInitialPoolSize = initialPoolSizeOverride > 0 ? initialPoolSizeOverride : initialPoolSize;

			m_Pool.InitCheckingExistingChildren(actualInitialPoolSize);
		}

		[Obsolete("Use AcquireFreeObject (then no need to Acquire/activate manually)")]
		public TPooledObject GetObject()
		{
			return AcquireFreeObject();
		}

		[Obsolete("Use AcquireFreeObject (then no need to Acquire/activate manually)")]
		public TPooledObject GetFreeObject()
		{
			return AcquireFreeObject();
		}

		/// Retrieve a released instance in the pool of objects, acquire it and return it
		public TPooledObject AcquireFreeObject()
		{
			return m_Pool.AcquireFreeObject(instantiateNewObjectOnStarvation);
		}

		/// Return an enumerable to all objects in use
		public IEnumerable<TPooledObject> GetObjectsInUse()
		{
			return m_Pool.GetObjectsInUse();
		}

		/// Release all objects in use
		public void ReleaseAllObjects()
		{
			m_Pool.ReleaseAllObjects();
		}
	}
}
