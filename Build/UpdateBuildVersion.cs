using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Text))]
public class UpdateBuildVersion : MonoBehaviour {

	Text text;

	void Awake () {
		text = this.GetComponentOrFail<Text>();
	}

	void Start () {
		UpdateText();
	}

	void UpdateText () {
		string version = BuildData.GetVersion();
		text.text = version;
	}

}
