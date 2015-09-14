using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// Usage: create an instance of DebugCanvas object in the scene. The prefab is provided
/// in the same folder as this script, and contains both objects for the DebugScreenManager and the DebugText scripts
public class DebugScreenManager : SingletonManager<DebugScreenManager> {

	protected DebugScreenManager () {} // guarantee this will be always a singleton only - can't use the constructor!

	// debug text and variable prefabs used to create on-screen debug messages
	[SerializeField]
	GameObject debugTextPrefab;
	[SerializeField]
	GameObject debugVariablePrefab;

	List<DebugText> m_DebugTexts = new List<DebugText>();
	List<DebugVariable> m_DebugVariables = new List<DebugVariable>();

	// parameters
	[SerializeField]
	int nbChannels = 3;  // channels are instantiated on setup, so you cannot change add channels while running; if you want to, run Setup() on value change

	// variable update event
	public delegate void UpdateVariableHandler(string variableName, string valueText);
	public static event UpdateVariableHandler UpdateVariableEvent;

	void Awake () {
		Instance = this;
		Init();
	}

	void Start () {
	}

	public void Init () {
		// prepare one debug text and one debug variable per channel (only one can appear per channel at a time)
		// (similar to pool, but fewer objects and each has a fixed position)

		// channel vertical space is based on font size of debug text prefab; use same or lower size for debug variable
		int fontSize = debugTextPrefab.GetComponentOrFail<DebugText>().text.fontSize;

		for (int i = 0; i < nbChannels; ++i) {
			Vector3 offset = i * fontSize * 1.2f * Vector3.down;

			GameObject debugTextInstance = debugTextPrefab.InstantiateUnderWithOffset(transform, offset);
			var debugText = debugTextInstance.GetComponentOrFail<DebugText>();
			m_DebugTexts.Add(debugText);
			debugTextInstance.SetActive(false);

			GameObject debugVariableInstance = debugVariablePrefab.InstantiateUnderWithOffset(transform, offset);
			var debugVariable = debugVariableInstance.GetComponentOrFail<DebugVariable>();
			m_DebugVariables.Add(debugVariable);
			debugVariableInstance.SetActive(false);
		}

		// deactivate prefab to hide model text (later, also use it as the first instance)
		// don't do it from the prefab or before copying, or Awake() will not be called in copies (they have to be instantiated active)
		debugTextPrefab.SetActive(false);
		debugVariablePrefab.SetActive(false);
	}

	/// Print text on screen on channel for duration in seconds
	public void ShowDebugText(string text, int channel = 0, float duration = 2f) {
		CheckChannelValidity(channel);
		m_DebugVariables[channel].Hide();
		m_DebugTexts[channel].Show(text, duration);
	}

	/// Start displaying value of variable on screen
	public void ShowDebugVariable<T>(string variableName, T value, int channel = 0) {
		CheckChannelValidity(channel);
		m_DebugTexts[channel].Hide();
		m_DebugVariables[channel].Show(variableName, value);
	}

	/// Update value of variable by name
	public void UpdateVariable<T>(string variableName, T value) {
		// send update variable event to all debug variable scripts, they will update if they are concerned
		// alternative: keep a dictionary of DebugVariable per variable name and directly update the one concerned
		UpdateVariableEvent(variableName, value.ToString());
	}

	public void ClearChannel(int channel) {
		CheckChannelValidity(channel);
		m_DebugTexts[channel].Hide();
		m_DebugVariables[channel].Hide();
	}

	void CheckChannelValidity (int channel) {
		if (channel >= nbChannels) {
			throw new ArgumentException(string.Format("Channel #{0} does not exist (#0 - #{1} only)", channel, nbChannels - 1), "channel");
		}
	}

	// TODO: special text display for variables, that is shown in permanence and is updated when the var changes

}
