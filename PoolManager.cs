using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class PoolManager<TPooledObject> : MonoBehaviour where TPooledObject : PooledObject {

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

	// Use this for initialization
	void Start () {
		Setup();
	}

	protected void Setup () {
		// Debug.LogFormat("Setup with poolSize: {0}", poolSize);
		// prepare pool with enough bullets
		for (int i = 0; i < poolSize; ++i) {
			GameObject pooledGameObject = pooledObjectPrefab.InstantiateUnder(poolTransform);
			TPooledObject pooledObject = pooledGameObject.GetComponentOrFail<TPooledObject>();
			pooledObject.Release();
			m_Pool.Add(pooledObject);
		}
	}

	public TPooledObject GetObject () {
		// O(n)
		for (int i = 0; i < poolSize; ++i) {
			TPooledObject pooledObject = m_Pool[i];
			if (!pooledObject.InUse) {
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
