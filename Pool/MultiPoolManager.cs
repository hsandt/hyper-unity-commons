//#define DEBUG_MULTI_POOL_MANAGER

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CommonsPattern
{

	/// Manager for a heterogeneous pool of game objects sharing a common script TPooledObject that implements IPooledObject
	/// Unlike PoolManager, this manager loads the prefabs by resource name (dictionary cannot be serialized directly and list of references are long to drag and drop)
	/// When inheriting from this base class, use the derived class as the T generic argument so that you can access a singleton instance of the derived class
	public abstract class MultiPoolManager<TPooledObject, T> : SingletonManager<T> where TPooledObject : MonoBehaviour, IPooledObject where T : SingletonManager<T> {

		[Header("External scene references")]

		[Tooltip("Parent under which all pooled objects will be created. If not set in inspector, must be set in code, " +
		         "in override Init() but before base.Init().")]
		public Transform poolTransform;


		[Header("Parameters")]

		[SerializeField, Tooltip("Path of gameobject prefabs to load, starting just after Resources/")]
		protected string resourcePrefabsPath = "";

		[SerializeField, Tooltip("Initial number of objects to pool for each type (multi-pool total size is a multiple). " +
		                         "More pooled objects may be instantiated dynamically if Instantiate New Object On Starvation is true.")]
		protected int initialPoolSize = 10;

		[SerializeField, Tooltip("Should the manager instantiate a new pooled object in case of starvation? " +
		                         "This will increase the pool size dynamically and may slow down on spawn, " +
		                         "but avoids not spawning an object at all. Recommended for gameplay objects.")]
		protected bool instantiateNewObjectOnStarvation = false;


		/* Cached parameters */

		/// Dictionary of resource prefabs used to generate pooled objects, per name. Derived from resourcePrefabsPath.
		protected readonly Dictionary<string, GameObject> prefabLibrary = new Dictionary<string, GameObject>();


		/* State */

		/// Dictionary of sub-pools managed internally, indexed by prefab name
		protected readonly Dictionary<string, Pool<TPooledObject>> m_MultiPool = new Dictionary<string, Pool<TPooledObject>>();


		protected override void Init() {
			// Load all prefabs from Resources
			LoadAllPrefabs();

			// Generate a sub-pool of objects for each prefab
			foreach (var entry in prefabLibrary) {
				string prefabName = entry.Key;
				GameObject prefab = entry.Value;
				GeneratePool(prefabName, prefab);
			}
		}

		/// Load and store all prefabs found in resourcePrefabsPath in prefab library
		private void LoadAllPrefabs()
		{
			GameObject[] prefabs = Resources.LoadAll<GameObject>(resourcePrefabsPath);
			foreach (GameObject prefab in prefabs)
			{
#if UNITY_EDITOR
				if (prefabLibrary.ContainsKey(prefab.name))
				{
					Debug.LogWarningFormat("[MultiPoolManager] Prefab library already contains prefab named '{0}', " +
					                       "the last one found will overwrite this entry. " +
					                       "Make sure to give unique names to the Pooled Object Prefabs managed by " +
					                       "the same MultiPoolManager, even if they are in different sub-folders",
											prefab.name);
				}
#endif

#if DEBUG_MULTI_POOL_MANAGER
				Debug.LogFormat("[MultiPoolManager] Adding {0}/{1} to prefab library", resourcePrefabsPath, prefab.name);
#endif

				prefabLibrary.Add(prefab.name, prefab);
			}
		}

		/// Initialize and fill pool of [initialPoolSize] objects of type [prefabName]
		private void GeneratePool(string prefabName, GameObject pooledObjectPrefab)
		{
#if DEBUG_MULTI_POOL_MANAGER
			Debug.LogFormat("[MultiPoolManager] Setup prefab pool of size {0} for object type '{1}'", initialPoolSize, prefabName);
#endif
			m_MultiPool.Add(prefabName, new Pool<TPooledObject>(pooledObjectPrefab, poolTransform));
			m_MultiPool[prefabName].Init(initialPoolSize);  // includes call to InitPooled and Release on each object
		}

		[Obsolete("Use GetFreeObject")]
		public TPooledObject GetObject(string prefabName)
		{
			return GetFreeObject(prefabName);
		}

		/// Retrieve a released instance in the pool of objects called prefabName
		public TPooledObject GetFreeObject(string prefabName)
		{
			if (m_MultiPool.TryGetValue(prefabName, out Pool<TPooledObject> pool))
	        {
		        return pool.GetFreeObject(instantiateNewObjectOnStarvation);
	        }

			Debug.LogErrorFormat(this, "Prefab '{0}' not found in multi pool dictionary of {1}", prefabName, name);
			return null;
		}

		/// Return an enumerable to all objects in use in all pools
		public IEnumerable<TPooledObject> GetObjectsInUseInAllPools()
		{
			return m_MultiPool.Values.SelectMany(pool => pool.GetObjectsInUse());
		}

		/// Return true if any pooled object is in use
		public bool AnyInUse ()
		{
			// CURRENT ALGORITHM: for every pooled object type, for every stored object, check if this object is in use
			// O(mn), m number of types, n pool size
			// ALTERNATIVE 1: all objects know their pools, and they notify an increment
			//	or decrement in counter of objects in use, so we can directly answer
			// ALTERNATIVE 2: two lists, one of objects released and one of objects in use
			//	we can immediately check the length of the lists to know if any / all are used
			return m_MultiPool.Values.Any(pool => pool.AnyInUse());
		}

		/// Release all objects in use in all sub-pools
		public void ReleaseAllObjects()
		{
			foreach (Pool<TPooledObject> pool in m_MultiPool.Values)
			{
				pool.ReleaseAllObjects();
			}
		}
	}
}
