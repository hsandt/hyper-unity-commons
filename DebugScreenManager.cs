using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// Usage: create an instance of DebugCanvas object in the scene. The prefab is provided
/// in the same folder as this script, and contains both objects for the DebugScreenManager and the DebugText scripts
public class DebugScreenManager : SingletonManager<DebugScreenManager> {

	protected DebugScreenManager () {} // guarantee this will be always a singleton only - can't use the constructor!

	// debug text prefab used to create on-screen debug messages
	[SerializeField]
	GameObject debugTextPrefab;

	List<DebugText> m_DebugTexts = new List<DebugText>();

	// parameters
	[SerializeField]
	int nbChannels = 3;

	void Awake () {
		Instance = this;
	}

	void Start () {
		Setup();
	}

	public void Setup () {
		// prepare one debug text per channel (similar to pool, but fewer objects and each has a fixed position)
		for (int i = 0; i < nbChannels; ++i) {
			// wait to get text component -> font size before computing offset
			GameObject debugTextInstance = debugTextPrefab.InstantiateUnderWithOffset(transform, Vector2.zero);
			var debugText = debugTextInstance.GetComponentOrFail<DebugText>();
			// each channel occupies a vertical space proportional to the text font size
			debugTextInstance.transform.localPosition += new Vector3(0f, -i * debugText.text.fontSize * 1.2f, 0f);
			m_DebugTexts.Add(debugText);
			debugTextInstance.SetActive(false);
		}
		// deactivate prefab to hide model text (later, also use it as the first instance)
		// don't do it from the prefab, or Awake() will not be called
		// don't do it before copying, or Awake() will not be called in copies (they have to be instantiated active)
		debugTextPrefab.SetActive(false);
	}

	/// Print text on screen on channel for duration in seconds
	public void ShowDebugText(string text, int channel = 0, float duration = 2f) {
		if (channel >= nbChannels) {
			throw new ArgumentException(string.Format("Channel #{0} does not exist (#0 - #{1} only)", channel, nbChannels - 1), "channel");
		}
		var debugText = m_DebugTexts[channel];
		debugText.Show(text, duration);
	}

	// TODO: special text display for variables, that is shown in permanence and is updated when the var changes

}
