using System;
using System.Collections.Generic;
using UnityEngine;

using CommonsHelper;

namespace CommonsPattern
{

	/// CRTP: when inheriting from this base class, use the derived class as the T generic argument so that you can access a singleton instance of the derived class
	public abstract class PoolManager<TPooledObject, T> : SingletonManager<T> where TPooledObject : MonoBehaviour, IPooledObject where T : SingletonManager<T> {

		/* External references */
		
		[SerializeField, Tooltip("Pool transform")]
		protected Transform poolTransform;

		
		/* Resource prefabs */
		
		[SerializeField, Tooltip("Pooled object prefab")]
		protected GameObject pooledObjectPrefab;

		
		/* Parameters */
		
		[SerializeField, Tooltip("Initial pool size (may change if Instantiate New Object On Starvation is true)")]
		[UnityEngine.Serialization.FormerlySerializedAs("poolSize")]
		protected int initialPoolSize = 20;
		
		[SerializeField, Tooltip("Should the manager instantiate a new pooled object in case of starvation? " +
		                         "This will increase the pool size dynamically and may slow down on spawn, " +
		                         "but avoids not spawning an object at all. Recommended for gameplay objects.")]
		protected bool instantiateNewObjectOnStarvation = false;

		/* state variables */
		List<TPooledObject> m_Pool = new List<TPooledObject>();

		/// <summary>
		/// Initialize pool by creating [initialPoolSize] copies of the pooled object
		/// </summary>
		protected override void Init () {
			// Debug.LogFormat("Setup with initialPoolSize: {0}", initialPoolSize);
			// prepare pool with enough bullets
			for (int i = 0; i < initialPoolSize; ++i)
			{
				InstantiatePooledObject();
			}
			
			// in case prefab reference is a scene instance, deactivate it (no effect if prefab is an asset since runtime)
			pooledObjectPrefab.SetActive(false);
		}

		private TPooledObject InstantiatePooledObject()
		{
			GameObject pooledGameObject = Instantiate(pooledObjectPrefab, poolTransform);
			TPooledObject pooledObject = pooledGameObject.GetComponentOrFail<TPooledObject>();
			pooledObject.InitPooled();
			pooledObject.Release();
			m_Pool.Add(pooledObject);
			return pooledObject;
		}

		public TPooledObject GetObject () {
			// O(n)
			for (int i = 0; i < m_Pool.Count; ++i) {
				TPooledObject pooledObject = m_Pool[i];
				if (!pooledObject.IsInUse()) {
					return pooledObject;
				}
			}
			
			// pool starvation
			if (instantiateNewObjectOnStarvation)
			{
				// Performance note: we are not "smart", instantiating a batch of new objects
				// as a List allocation would do, by power of two. We really just instantiate what we need.
				return InstantiatePooledObject();
			}
			
			return null;
		}

		[Obsolete("Use pooledObject.Release() instead.")]
		public void ReleaseObject (TPooledObject pooledObject) {
			pooledObject.Release();
		}

	}

}
