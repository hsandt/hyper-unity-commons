using UnityEngine;

public static class AssertUtil {

	// TODO: create a string.Format version
	[System.Diagnostics.Conditional( "DEBUG" )]
	public static void Assert( bool condition, string message ) {
	    if( !condition ) {
	        Debug.LogError( message );
	    }
	}

}
