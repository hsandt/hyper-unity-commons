namespace CommonsPattern
{

	public interface IPooledObject
	{
	    /// Acquire the object, making it considered used
	    /// This should only contain basic activation opposed to Release (in most cases, activate a game object)
	    /// and it is the pooled object user's responsibility to finalize the setup (e.g. setting the position
	    /// of the object for a full spawn definition).
	    /// Common implementation defined by StandardPooledObject:
	    /// `gameObject.SetActive(true);`
	    /// Non-standard pooled objects such as Audio Source may need a more specific implementation.
	    /// If there is nothing to do before actual usage (e.g. there is nothing to do before AudioSource.Play)
	    /// then it is fine to leave the implementation empty.
	    /// The implementation doesn't need to check IsInUse either as often, calling Acquire is idempotent,
	    /// but the caller is encouraged to verify IsInUse first in context where it is not clear that
	    /// the object is unused.
	    void Acquire();

	    /// Is the object currently used? It cannot be acquired if true.
	    /// This should return true after calling Acquire + custom use method (see bottom comment),
	    /// and false after calling Release.
	    /// Depending on the pooled object's type, it may be considered used immediately after Acquire
	    /// (e.g. standard objects that are considered used when active) or after the custom use method
	    /// (e.g. Audio Source that are considered used only after calling some Play method).
	    /// Some pooled objects may also auto-release after Acquire/custom use method (e.g. Audio Source
	    /// after a non-looping sound ended).
	    /// In addition, after prefab instantiation by a Pool, there is no assumption on whether
	    /// the object is used or not. For instance, a standard object will be active (assuming the prefab
	    /// was), but an Audio Source will be considered idle.
	    /// Common implementation defined by StandardPooledObject:
	    /// `return gameObject.activeSelf;`
	    /// Non-standard pooled objects such as Audio Source may need a more specific implementation.
	    bool IsInUse();

	    /// Release the object, making it considered unused, so that it can be acquired next time
	    /// Common implementation defined by StandardPooledObject:
	    /// `gameObject.SetActive(false);`
	 	/// Non-standard pooled objects such as Audio Source may need a more specific implementation.
		void Release();

	    // There is no Use method because each type of pooled object may require a differently named
		// custom use method, with different parameters (e.g. Spawn(position, velocity),
		// PlayOneShot(AudioClip clip), etc.).
		// Most pooled object classes should define their own custom use method,
		// even if just to delegate the job to a sibling component.
	}
}
