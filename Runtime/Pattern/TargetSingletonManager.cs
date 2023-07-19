// Based on http://wiki.unity3d.com/index.php/Singleton

using UnityEngine;

namespace HyperUnityCommons
{
	/// A variant of SingletonManager useful when the class of the component you need to access "globally" already exists,
	/// so you cannot change it into a SingletonManager (and you cannot inherit from it to add SingletonManager
	/// as C# doesn't support multi-inheritance), or you prefer to keep that component separate in case you need
	/// to use it a non-singleton elsewhere, and generally to favor composition over inheritance.
	/// In this case, keep the component you need to access globally as a normal Component (often MonoBehaviour),
	/// and create a TargetSingletonManager subclass where TTarget = type of that component.
	/// The managed component is called "target" and this manager is almost empty, only meant to provide access to
	/// the target.
	public abstract class TargetSingletonManager<TSingleton, TTarget> : SingletonManager<TSingleton>
		where TSingleton : TargetSingletonManager<TSingleton, TTarget>
		where TTarget : Component
	{
		[Tooltip("Component targeted by the manager. You can set anything you want in the inspector, or even at " +
			"runtime if needed, but it's preferable to put the target component on the same object or some child, " +
			"so you can turn this game object into a prefab that preserves this reference. It would also guarantee " +
			"that their lifetimes are synchronized.")]
		public TTarget target;

		/// Shortcut for instance target, i.e. the component this TargetSingletonManager provided access too
		public static TTarget Target => Instance.target;
	}
}
