﻿using UnityEngine;
using Vexe.Runtime.Types;
using System;
using System.Collections;
using System.Collections.Generic;

/// Manager for a heterogeneous pool of game objects sharing a common script that implements IPooledObject
/// TKey is a key that identifies each type of game object
public abstract class MultiPoolManager<TKey, TPooledObject> : BetterBehaviour where TPooledObject : MonoBehaviour, IPooledObject {

	/* external references */
	/// Parent of all the pooled objects
	[Serialize]
	protected Transform poolTransform;

	/// Dictionary of prefabs used to generate pooled objects, per key
	[Serialize]
	protected Dictionary<TKey, GameObject> prefabDict;

	/* parameters */
	/// Max number of objects to pool for each type (multi-pool total size is a multiple)
	[Serialize]
	protected int poolSize = 20;

	/* state variables */
	Dictionary<TKey, List<TPooledObject>> m_MultiPool = new Dictionary<TKey, List<TPooledObject>>();
	// int nbObjectsInUse;

	// Use this for initialization
	void Awake () {
		Init();
	}

	protected void Init () {
		// initialize pool with poolSize objects of each type
		foreach (KeyValuePair<TKey, GameObject> entry in prefabDict)
		{
			m_MultiPool[entry.Key] = new List<TPooledObject>();
			GameObject pooledObjectPrefab = entry.Value;
			for (int i = 0; i < poolSize; ++i) {
				GameObject pooledGameObject = pooledObjectPrefab.InstantiateUnder(poolTransform);
				TPooledObject pooledObject = pooledGameObject.GetComponentOrFail<TPooledObject>();
				pooledObject.Release();
				m_MultiPool[entry.Key].Add(pooledObject);
			}

			// in case prefab reference is a scene instance, deactivate it (no effect if prefab is an asset since runtime)
			pooledObjectPrefab.SetActive(false);
			// nbObjectsInUse = 0;  // uncomment if you choose ALTERNATIVE 1 in AnyInUse()
		}
	}

	public TPooledObject GetObject (TKey objectType) {
		// O(n), n pool size
		for (int i = 0; i < poolSize; ++i) {
			TPooledObject pooledObject = m_MultiPool[objectType][i];
			if (!pooledObject.IsInUse()) {
				return pooledObject;
			}
		}
		// starvation
		Debug.LogWarningFormat("Multi-pool starvation, cannot get released object of type {0}", objectType);
		return null;
	}

	// public void ReleaseObject (TPooledObject pooledObject) {
	// 	pooledObject.Release();
	// }

	/// Return true if any pooled object is in use
	public bool AnyInUse () {
		// CURRENT ALGORITHM: for every pooled object type, for every stored object, check if this object is in use
		// O(mn), m number of types, n pool size
		// ALTERNATIVE 1: all objects know their pools, and they notify an increment
		//	or decrement in counter of objects in use, so we can directly answer
		// ALTERNATIVE 2: two lists, one of objects released and one of objects in use
		//	we can immediately check the length of the lists to know if any / all are used
		foreach (var objectListPair in m_MultiPool)
		{
			for (int i = 0; i < poolSize; ++i) {
				TPooledObject pooledObject = objectListPair.Value[i];
				if (pooledObject.IsInUse()) {
					return true;
				}
			}
		}
		return false;
	}

}
