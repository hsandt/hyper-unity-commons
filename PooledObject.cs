using UnityEngine;
using System.Collections;

public abstract class PooledObject : MonoBehaviour {

	public bool InUse { get { return IsInUse(); } }
	public abstract bool IsInUse();

	public abstract void Release();

}
