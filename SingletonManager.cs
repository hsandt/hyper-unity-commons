// Based on http://wiki.unity3d.com/index.php/Singleton

using UnityEngine;

/// <summary>
/// A singleton generic base class for game objects that are present only
/// once in the scene. A game object is *not* created if it does not already exist,
/// and an exception is thrown if an instance is called but does not exist yet.
/// Each subclass must record itself in Awake(), using `SetInstanceOrSelfDestruct(this)`
///
/// Be aware this will not prevent a non singleton constructor
///   such as `T myT = new T();`
/// To prevent that, add `protected T () {}` to your singleton class.
/// </summary>
public class SingletonManager<T> : MonoBehaviour where T : MonoBehaviour {

	// TEMPLATE
	// protected T () {}
	// void Awake () {
	// 	SetInstanceOrSelfDestruct(this);
	// }

	private static T _instance;

	public static T Instance {
		get {
			return _instance;
		}
		protected set {
			SetInstanceOrSelfDestruct(value);
			Debug.LogWarningFormat("[{0}] Instance::set is deprecated, please use SetInstanceOrSelfDestruct(this).", typeof(T).ToString());
		}
	}

	protected static void SetInstanceOrSelfDestruct (T value) {
		if (_instance == null) {
			_instance = value;
		} else {
			Destroy(value.gameObject);
			Debug.LogFormat("Instance of {0} already exists, existing instance will self-destruct." +
				"Remove any instance of Manager generated by ManagerGenerator in the scenes before releasing.", typeof(T).ToString());
		}
	}

}
