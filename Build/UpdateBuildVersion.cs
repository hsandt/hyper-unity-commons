using UnityEngine;
using UnityEngine.UI;

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
