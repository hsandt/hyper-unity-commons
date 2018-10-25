using UnityEngine;
using System.Collections;

namespace CommonsPattern
{

	/// Interface for scripts that should Pause behaviour but not be disabled as MonoBehaviours (e.g. because we still need the Update to check
	/// for an input that will Resume us, or to avoid unregistering OnResume events). Can be used on Master scripts to pause Slave scripts.
	public interface IPausable {

		/// Pause this object and all the objects it manages
		void Pause ();

		/// Resume behaviour for this object and all the objects it manages
		void Resume ();

	}

}
