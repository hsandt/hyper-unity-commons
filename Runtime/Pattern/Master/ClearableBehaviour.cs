using UnityEngine;
using System.Collections;

namespace CommonsPattern
{

	/// Base class for behaviours that can be cleared and restarted from their initial states. Recommended for all MonoBehaviours with state variables.
	public abstract class ClearableBehaviour : MonoBehaviour {

		/// Setup the object's state vars so that it reaches its initial state. This should be called on Start.
		public virtual void Setup () {}

		/// Clear object's state vars such as lists and remaining particles.
		public virtual void Clear () {}

		/// Restart the object's state.
		public virtual void Restart () {
			Clear();
			Setup();
		}

	}

}
