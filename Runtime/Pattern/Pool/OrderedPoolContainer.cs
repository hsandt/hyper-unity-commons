using System;
using System.Collections.Generic;
using UnityEngine;

namespace HyperUnityCommons
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
		protected int initialPoolSize = 5;


		/* State */

		/// Pool managed internally
		protected Pool<TPooledObject> m_Pool;


		private void Awake()
		{
			TPooledObject prefabPooledObject = pooledObjectPrefab.GetComponentOrFail<TPooledObject>();
			m_Pool = new Pool<TPooledObject>(prefabPooledObject, transform);

			switch (handleExistingChildrenMode)
			{
				case PoolHandleExistingChildrenMode.UseAllExistingChildren:
					// Register all existing children
					m_Pool.InitCheckingExistingChildren(initialPoolSize);
					break;

				case PoolHandleExistingChildrenMode.UseActiveExistingChildren:
					// Destroy deactivated children to be clear
					// Pool.InitCheckingExistingChildren will iterate on children right after this, and it also itself
					// calls Pool.LazyInstantiatePooledObjects which has an assert comparing child count and registered
					// object count,
					// but Destroy applies at the end of the frame, so for child iteration and count to be correct,
					// we must detach the parent (immediate operation) too.
					foreach (Transform child in transform)
					{
						if (!child.gameObject.activeSelf)
						{
							child.SetParent(null);
							Destroy(child.gameObject);
						}
					}

					// Now we can register the remaining children, which are all active
					m_Pool.InitCheckingExistingChildren(initialPoolSize);
					break;

				case PoolHandleExistingChildrenMode.DontUseExistingChildren:
					// Destroy all children to be clear
					foreach (Transform child in transform)
					{
						Destroy(child.gameObject);
					}

					m_Pool.InitIgnoringExistingChildren(initialPoolSize);
					break;

				default:
					throw new ArgumentOutOfRangeException();
			}
		}

        /// Acquire the first [count] objects under pool transform,
        /// release all the other ones, and return an enumerable to those [count] objects
        /// Instantiate as many new objects as needed, but warn on new instantiation
        /// so we can spot pools where we set an initial size too low.
		public IEnumerable<TPooledObject> AcquireOnlyFirstObjects(int count)
		{
			return m_Pool.AcquireOnlyFirstObjects(count);
		}

        /// Retrieve a released instance in the pool of objects, acquire it and return it
        /// Instantiate new object if needed, but warn on new instantiation
        /// so we can spot pools where we set an initial size too low.
        /// While OrderedPoolContainer is normally used for a range of contiguous active objects starting from index 0,
        /// providing this method makes the container a little more flexible and usable for cases when some objects
        /// can be released in an undefined order. Ultimately, we will probably make this class support the complete
        /// single pool API, effectively working like PoolManager but without being a singleton.
        public TPooledObject AcquireFreeObject()
        {
	        return m_Pool.AcquireFreeObject(instantiateNewObjectOnStarvation: true);
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
