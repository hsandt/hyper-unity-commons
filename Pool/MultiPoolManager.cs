using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using CommonsHelper;

namespace CommonsPattern
{

	/// Manager for a heterogeneous pool of game objects sharing a common script TPooledObject that implements IPooledObject
	/// Unlike PoolManager, this manager loads the prefabs by resource name (dictionary cannot be serialized directly and list of references are long to drag and drop)
	/// When inheriting from this base class, use the derived class as the T generic argument so that you can access a singleton instance of the derived class
	public abstract class MultiPoolManager<TPooledObject, T> : SingletonManager<T> where TPooledObject : MonoBehaviour, IPooledObject where T : SingletonManager<T> {

		/* External references */

		/// Parent of all the pooled objects
		public Transform poolTransform;

		/* Parameters */

		[Tooltip("Path of gameobject prefabs to load, starting just after Resources/")]
		[SerializeField] protected string resourcePrefabsPath = "";

		[Tooltip("Max number of objects to pool for each type (multi-pool total size is a multiple)")]
		[SerializeField] protected int poolSize = 10;

		/* State variables */

		/// Dictionary of resource prefabs used to generate pooled objects, per name
		protected Dictionary<string, GameObject> prefabLibrary = new Dictionary<string, GameObject>();

		/// Pool of object instances, per name
		Dictionary<string, List<TPooledObject>> m_MultiPool = new Dictionary<string, List<TPooledObject>>();

		// int nbObjectsInUse;

		// TEMPLATE
		// void Awake () {
		//  SetInstanceOrSelfDestruct(this);

		// 	Init();
		// }

		protected void Init () {
			LoadAllPrefabs();

			foreach (var entry in prefabLibrary) {
				string prefabName = entry.Key;
				GameObject prefab = entry.Value;
				GeneratePool(prefabName, prefab);
			}
		}

		void LoadAllPrefabs () {
			GameObject[] prefabs = Resources.LoadAll<GameObject>(resourcePrefabsPath);
			for (int i = 0; i < prefabs.Length; ++i) {
				Debug.LogFormat("[MultiPoolManager] Adding {0}/{1} to prefab library", resourcePrefabsPath, prefabs[i].name);
				prefabLibrary.Add(prefabs[i].name, prefabs[i]);
			}
		}

		/// Initialize and fill pool [prefabName] with [poolSize] prefab instances
		void GeneratePool (string prefabName, GameObject prefab) {
			Debug.LogFormat("[MultiPoolManager] Setup prefab pool of size {0} for {1}", poolSize, prefab.name);
			m_MultiPool.Add(prefabName, new List<TPooledObject>());
			for (int i = 0; i < poolSize; ++i) {
				GameObject sourceObject = Instantiate(prefab, poolTransform) as GameObject;
				TPooledObject pooledObject = sourceObject.GetComponentOrFail<TPooledObject>();
			    pooledObject.InitPooled();
				pooledObject.Release();
				m_MultiPool[prefabName].Add(pooledObject);
			}
		}

		/// Retrieve a released instance in the pool of objects called prefabName
		public TPooledObject GetObject (string prefabName) {
			// O(n), n pool size
	        List<TPooledObject> pooledObjects;
	        if (m_MultiPool.TryGetValue(prefabName, out pooledObjects)) {
	            Debug.AssertFormat(poolSize == pooledObjects.Count, this, "[CODE] Pool list of {0} in multipool has {1} elements but pool size is {2}.", prefabName, pooledObjects.Count, poolSize);
				for (int i = 0; i < poolSize; ++i) {
	                TPooledObject pooledObject = pooledObjects[i];
					if (!pooledObject.IsInUse()) {
						return pooledObject;
					}
	            }
	        }
	        else {
	            Debug.LogWarningFormat(this, "Prefab {0} not found in multi pool dictionary", prefabName);
	            return null;
	        }
			// starvation
			Debug.LogWarningFormat(this, "Multi-pool starvation, cannot get a released instance of object {0}", prefabName);
			return null;
		}

		/// Return true if any pooled object is in use
		public bool AnyInUse () {
			// CURRENT ALGORITHM: for every pooled object type, for every stored object, check if this object is in use
			// O(mn), m number of types, n pool size
			// ALTERNATIVE 1: all objects know their pools, and they notify an increment
			//	or decrement in counter of objects in use, so we can directly answer
			// ALTERNATIVE 2: two lists, one of objects released and one of objects in use
			//	we can immediately check the length of the lists to know if any / all are used
			foreach (var nameListPair in m_MultiPool)
			{
				for (int i = 0; i < poolSize; ++i) {
					TPooledObject pooledObject = nameListPair.Value[i];
					if (pooledObject.IsInUse()) {
						return true;
					}
				}
			}
			return false;
		}

	}

}
