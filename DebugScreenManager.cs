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
			GameObject debugTextInstance = debugTextPrefab.InstantiateUnderAtOn(transform, new Vector2(0f, i * -10f));
			var debugText = debugTextInstance.GetComponentOrFail<DebugText>();
			debugTextInstance.SetActive(false);
			m_DebugTexts.Add(debugText);
		}
	}

	public void ShowDebugText(string text, int channel = 0, float duration = 1f) {
		if (channel >= nbChannels) {
			throw new ArgumentException(string.Format("Channel #{0} does not exist (#0 - #{1} only)", channel, nbChannels - 1), "channel");
		}
		var debugText = m_DebugTexts[channel];
		debugText.Show(text, duration);
	}

	/// Print text on screen for duration in seconds
	public void Print(string text, int channel = 0, float duration = 1f) {
		ShowDebugText(text, channel, duration);
	}

	// TODO: special text display for variables, that is shown in permanence and is updated when the var changes

}
