using UnityEngine;
using UnityEngine.UI;

namespace CommonsHelper
{

	/// Put this script on any game object with a Text reflecting the current build version.
	/// This is convenient to always show the correct build version on the title screen.
	/// The Start method will also ensure that the correct version is displayed when playing,
	/// even if you forgot to click the "Update build version in text" button in the custom inspector.
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
			string version = BuildData.GetVersionStringFromResource();
			text.text = version;
		}

	}

}

