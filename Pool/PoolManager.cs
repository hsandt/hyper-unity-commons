using System.Collections.Generic;
using UnityEngine;

namespace Commons.Pattern
{

	/// CRTP: when inheriting from this base class, use the derived class as the T generic argument so that you can access a singleton instance of the derived class
	public abstract class PoolManager<TPooledObject, T> : SingletonManager<T> where TPooledObject : MonoBehaviour, IPooledObject where T : SingletonManager<T> {

		/* external references */
		[SerializeField]
		protected Transform poolTransform;

		/* resource prefabs */
		[SerializeField]
		protected GameObject pooledObjectPrefab;

		/* parameters */
		[SerializeField]
		protected int poolSize = 20;

		/* state variables */
		List<TPooledObject> m_Pool = new List<TPooledObject>();

		// TEMPLATE METHOD FOR DERIVED CLASSES
		// void Awake () {
		//  SetInstanceOrSelfDestruct(this);
		// 	Init();
		// }

		/// <summary>
		/// Initialize pool by creating [poolSize] copies of the pooled object
		/// </summary>
		protected void Init () {
			// Debug.LogFormat("Setup with poolSize: {0}", poolSize);
			// prepare pool with enough bullets
			for (int i = 0; i < poolSize; ++i) {
				GameObject pooledGameObject = pooledObjectPrefab.InstantiateUnder(poolTransform);
				TPooledObject pooledObject = pooledGameObject.GetComponentOrFail<TPooledObject>();
				pooledObject.Release();
				m_Pool.Add(pooledObject);
			}
			// in case prefab reference is a scene instance, deactivate it (no effect if prefab is an asset since runtime)
			pooledObjectPrefab.SetActive(false);
		}

		public TPooledObject GetObject () {
			// O(n)
			for (int i = 0; i < poolSize; ++i) {
				TPooledObject pooledObject = m_Pool[i];
				if (!pooledObject.IsInUse()) {
					return pooledObject;
				}
			}
			// starvation
			return null;
		}

		public void ReleaseObject (TPooledObject pooledObject) {
			pooledObject.Release();
		}

	}

}
