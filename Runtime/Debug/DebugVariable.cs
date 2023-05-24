using UnityEngine;
using UnityEngine.UI;
using System.Collections;

using CommonsHelper;

namespace CommonsDebug
{

	public class DebugVariable : MonoBehaviour {

		// reference to value text
		public Text debugValue;

		// script references
		Text text;  // variable text

		/* State vars */
		/// True if a variable is shown in this container ("the pooled object is used")
		// bool used;

		/// Time it takes to fade color if the text is not updated
		const float colorChangeTime = 2f;

		/// Channel index used by this entry
		public int channelIndex;

		/// Time passed since last update
		float timeSinceLastUpdate;

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


		void Update () {
			timeSinceLastUpdate += Time.deltaTime;
			UpdateColor();
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
			string valueText = value == null ? "null" : value.ToString();
			// REFACTOR: pass context as argument?
	//		string context = DebugScreenManager.Instance.GetContext();
	//		if (!string.IsNullOrEmpty(context)) valueText = string.Format("({0}) {1}", context, valueText);
			debugValue.text = valueText;
			timeSinceLastUpdate = 0f;
			UpdateColor();
		}

		/// Update the color based on the current timeSinceLastUpdate
		void UpdateColor () {
			debugValue.color = Color.Lerp(Color.white, Color.grey, timeSinceLastUpdate / colorChangeTime);
		}

		// void OnUpdateVariable(string variableName, string valueText) {
		// 	if (varName == variableName) {
		// 		debugValue.text = valueText;
		// 	}
		// }

		public void Hide () {
		    gameObject.SetActive(false);
			timeSinceLastUpdate = 0f;
		    // used = false;
		}

		/// Is the object currently used? It cannot be requested if true.
		public bool IsInUse() {
			return gameObject.activeSelf;
		}
	}

}
