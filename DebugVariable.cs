using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DebugVariable : MonoBehaviour {

	// reference to value text
	public Text debugValue;

	// script references
	Text text;  // variable text

	/* State vars */
	/// True if a variable is shown in this container ("the pooled object is used")
	// bool used;

	/// Name of the current variable represented, if any
	string varName;
	public string VarName { get { return varName; } }

	// Use this for initialization
	void Awake () {
		text = this.GetComponentOrFail<Text>();
	}

	void Start () {
		// used = false;
	}


	void FixedUpdate () {
	}

	/// Show variable with initial value
	public void Show<T> (string variableName, T value) {
	    // make text visible
	    gameObject.SetActive(true);
	    // used = true;
	    // update variable name
	    varName = variableName;
		text.text = variableName;
	    // update value
	    SetValue<T>(value);
	    // DebugScreenManager.UpdateVariableEvent += new DebugScreenManager.UpdateVariableHandler(OnUpdateVariable);
	}

	public void SetVariableName(string variableName) {
		text.text = variableName;
	}

	public void SetValue<T>(T value) {
		debugValue.text = value.ToString();
	}

	// void OnUpdateVariable(string variableName, string valueText) {
	// 	if (varName == variableName) {
	// 		debugValue.text = valueText;
	// 	}
	// }

	public void Hide () {
	    gameObject.SetActive(false);
	    // used = false;
	}

	/// Is the object currently used? It cannot be requested if true.
	public bool IsInUse() {
		return gameObject.activeSelf;
	}
}
