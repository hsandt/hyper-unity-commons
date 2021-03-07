namespace CommonsPattern
{

	public interface IPooledObject
	{
	    /// Initialize pooled object (called only once on creation).
	    /// Implementation may be empty.
	    void InitPooled();

	    /// Is the object currently used? It cannot be requested if true.
	    /// Common implementation: `return gameObject.activeSelf;`
		bool IsInUse();

	    /// Release the object so that it can be used next time
	    /// Common implementation: `gameObject.SetActive(false);`
		void Release();
		
		// There is no Spawn method because each kind of pooled object will require
		// different parameters (position, velocity, etc.). Most pooled object classes
		// should define their own Spawn method, often starting with `gameObject.SetActive(false);`
	}

}
