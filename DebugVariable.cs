using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DebugVariable : MonoBehaviour {

	// script references
	Text text;  // variable text

	// reference to value text
	[SerializeField]
	Text debugValue;

	// state vars
	[SerializeField]
	string varName;

	// Use this for initialization
	void Awake () {
		text = this.GetComponentOrFail<Text>();
	}

	void Start () {
	}


	void FixedUpdate () {
	}

	/// Show variable with initial value
	public void Show<T> (string variableName, T value) {
	    // make text visible
	    gameObject.SetActive(true);
	    // update variable name
	    varName = variableName;
		text.text = variableName;
	    // update value
	    debugValue.text = value.ToString();
	    DebugScreenManager.UpdateVariableEvent += new DebugScreenManager.UpdateVariableHandler(OnUpdateVariable);
	}

	public void SetVariableName(string variableName) {
		text.text = variableName;
	}

	public void SetValue<T>(T value) {
		debugValue.text = value.ToString();
	}

	void OnUpdateVariable(string variableName, string valueText) {
		if (varName == variableName) {
			debugValue.text = valueText;
		}
	}

	public void Hide () {
	    gameObject.SetActive(false);
	}
}
