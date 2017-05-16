using UnityEngine;
using System.Collections;

public interface IPausable {

	/// Pause this object and all the objects it manages
	void Pause ();

	/// Resume behaviour for this object and all the objects it manages
	void Resume ();

}
