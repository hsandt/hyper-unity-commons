namespace CommonsPattern
{

	public interface IPooledObject {
	    /// Initialize pooled object (call only once on creation)
	    void InitPooled();

	    /// Is the object currently used? It cannot be requested if true.
		bool IsInUse();

	    /// Release the object so that it can be used next time
		void Release();
	}

}
