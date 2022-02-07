using System;
using System.Collections.Generic;
using UnityEngine;

namespace CommonsPattern
{
	/// Ordered pool container component
	/// Add this component directly to the parent object that contains all pooled objects.
	/// Use an ordered pool when you must always instantiate and activate N pooled objects
	/// in a certain order, as it provides methods to lazily initialise exactly N pooled objects,
	/// and deactivate extra pooled objects already active that we don't need.
	/// This is particularly useful for UI, when you add this component to a game object with a
	/// certain Layout, and you need to add N widgets under that Layout.
	/// Ex: Menu has Vertical Layout with a dynamic number of buttons, such as a number of save slots.
	/// Compared to PoolManager, it is not a singleton and it is placed directly on the pool parent,
	/// so it doesn't need to define a Pool Transform. It also always instantiate on starvation,
	/// since when we want an ordered pool, it is generally for critical objects such as menu items.
	public abstract class OrderedPoolContainer<TPooledObject> : MonoBehaviour where TPooledObject : MonoBehaviour, IPooledObject
	{
		[Header("Prefabs")]

		[Tooltip("Pooled object prefab")]
		public GameObject pooledObjectPrefab;


		[Header("Parameters")]

		[SerializeField, Tooltip("Initial pool size (may change if Instantiate New Object On Starvation is true)")]
		protected int initialPoolSize = 5;


		/* State */

		/// Pool managed internally
		protected Pool<TPooledObject> m_Pool;


		private void Awake()
		{
			m_Pool = new Pool<TPooledObject>(pooledObjectPrefab, transform);
			m_Pool.Init(initialPoolSize);
		}

        /// Acquire the first [count] objects under pool transform,
        /// release all the other ones, and return an enumerable to those [count] objects
        /// Instantiate as many new objects as needed.
		public IEnumerable<TPooledObject> AcquireOnlyFirstObjects(int count)
		{
			return m_Pool.AcquireOnlyFirstObjects(count);
		}

        /// Return the count of all objects, active or inactive
        public int CountAllObjects()
        {
	        return m_Pool.CountAllObjects();
        }

        /// Return the object at a given index (it may be active or inactive)
        public TPooledObject GetObject(int index)
        {
	        return m_Pool.GetObject(index);
        }

        /// Release all objects in use
        public void ReleaseAllObjects()
        {
	        m_Pool.ReleaseAllObjects();
        }
	}
}
