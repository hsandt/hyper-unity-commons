// Based on http://wiki.unity3d.com/index.php/Singleton

using UnityEngine;

namespace HyperUnityCommons
{
	/*
	A generic singleton base class for manager game objects that are present only
	once in the scene.

	Usage:

	1. Create a subclass script using the curiously recurrent template pattern:

	```

	public class MyManager : SingletonManager<MyManager> {
		// if you need to override behavior on Awake
		protected override void Init() {
			// init your stuff
		}
	}
	```

	Alternatively, if you prefer defining your own Awake:

	```

	public class MyManager : SingletonManager<MyManager> {
		// if you need to override behavior on Awake
		private void Awake() {
			// replaces InitSingleton() for subclasses, more efficient with static type check
			SetInstanceOrSelfDestruct(this);

			// init your stuff
		}
	}
	```

	2. Create a Game Object (we recommend a Prefab) with your script.
	Either place it in a scene that is always loaded (once), or instantiate your prefab (once) at runtime.

	3. If you need to customize OnDestroy, override OnDestroyInternal instead

	Manager instances will automatically be registered as static instance, accessed via the Instance property.
	Step 2 is necessary, as a manager game object will *not* created if it does not already exist.

	If no instances have been registered, Instance returns null without error.
	If more than 1 instance is detected, any extra instance will self-destruct with a warning.
	On game object/component destruction, the instance is cleared.
	*/
	public abstract class SingletonManager<T> : MonoBehaviour where T : SingletonManager<T> {

		private static T _instance;
		public static T Instance => _instance;

		private void Awake() {
			InitSingleton();
			Init();
		}

		private void InitSingleton () {
			T thisAsT = this as T;
			if (thisAsT != null) {
				SetInstanceOrSelfDestruct(thisAsT);
			}
			else {
				Debug.LogErrorFormat("{0} inherits from SingletonManager<{1}> but is not a {1} itself. " +
						"Make sure to inherit from SingletonManager using the curiously recurrent template pattern.",
					GetType(), typeof(T));
			}
		}

		protected static void SetInstanceOrSelfDestruct (T value) {
			if (_instance == null) {
				_instance = value;
			} else {
				Destroy(value.gameObject);
				Debug.LogWarningFormat("Instance of {0} already exists, this new instance will self-destruct." +
						"Please remove any extra instances of Manager in the scene.",
					typeof(T));
			}
		}

		/// Override this method to customize Awake behavior while preserving singleton logic
		protected virtual void Init() {}

		private void OnDestroy()
		{
			// if this was the registered instance, clear singleton instance
			// (it's cleaner than relying on Unity's null object after destruction)
			// otherwise, it was probably self-destructing with warning due to duplicate singletons, so do nothing else
			if (_instance == this)
			{
				_instance = null;
			}
			OnDestroyInternal();
		}

		/// Override this method to customize OnDestroy behavior while preserving singleton logic
		protected virtual void OnDestroyInternal() {}
	}

}
