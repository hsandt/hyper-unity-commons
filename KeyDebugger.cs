using UnityEngine;
using System.Collections;

public class KeyDebugger : MonoBehaviour {

	void OnGUI()
	{
		if (Event.current != null) {
			KeyCode keyCode = GetKeyCode(Event.current);
			if (keyCode != KeyCode.None) { Debug.Log("You pressed/released: " + keyCode); }
		}
	}

	public KeyCode GetKeyCode(Event e)
	{
		if (e.isKey && e.keyCode != KeyCode.None) {
			return e.keyCode;
		}
		return KeyCode.None;
	}

}
