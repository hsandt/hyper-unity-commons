using System;
using System.Collections.Generic;
using UnityEngine;

namespace HyperUnityCommons
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

		[SerializeField, Tooltip("Determines how to handle children already set up under a pool transform " +
			 "in the scene (generally for previewing purpose). UseActiveExistingChildren is a good default because it " +
			 "means we are pragmatic, and only use the existing children that are already visible in the scene. " +
			 "Note that all existing children to be use should have the correct IPooledObject component, and all " +
			 "unused children are destroyed on initialization to avoid desync between child count and " +
			 "pooled object count. " +
			 "Generally, existing children are instances of pooledObjectPrefab, but they may have some undesired overrides, " +
			 "so if you want to use them, you must make sure to revert such overrides in the scene.")]
		protected PoolHandleExistingChildrenMode handleExistingChildrenMode = PoolHandleExistingChildrenMode.UseActiveExistingChildren;

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

			m_Pool.InitHandlingExistingChildren(handleExistingChildrenMode, actualInitialPoolSize);
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
