using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

// IMPROVE: when variable has not been updated for several frames, darken the label

/// Usage: create an instance of DebugCanvas object in the scene. The prefab is provided
/// in the same folder as this script, and contains both objects for the DebugScreenManager and the DebugText scripts
public class DebugScreenManager : SingletonManager<DebugScreenManager> {

	protected DebugScreenManager () {} // guarantee this will be always a singleton only - can't use the constructor!

	// debug text and variable prefabs used to create on-screen debug messages
	public GameObject debugTextPrefab;
	public GameObject debugVariablePrefab;

	List<DebugText> m_DebugTexts = new List<DebugText>();
	List<DebugVariable> m_DebugVariables = new List<DebugVariable>();

	/// Dictionary of variable name => debug variable
	Dictionary<string, DebugVariable> m_DebugVariableDict = new Dictionary<string, DebugVariable>();

	// parameters
	[SerializeField]
	int nbChannels = 8;  // channels are instantiated on setup, so you cannot change add channels while running; if you want to, run Init() on value change

	// variable update event
	public delegate void UpdateVariableHandler(string variableName, string valueText);
	public static event UpdateVariableHandler UpdateVariableEvent;

	void Awake () {
		Instance = this;
		Init();
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

	public int GetNextChannelAvailable() {
		// OPTIMIZATION: keep track of 1st channel available by using chained index technique or by checking in advance each time a channel is used or released
		// see Pooling techniques: each object knows the index of the next free object, or just apply this search each time something changes but store the result
		// improve: make text and variable uniforms (inherit from same base class, one object only if possible)
		for (int i = 0; i < nbChannels; ++i) {
			if (!m_DebugTexts[i].IsInUse() && !m_DebugVariables[i].IsInUse()) {
				return i;
			}
		}
		// all channels are busy!
		Debug.LogWarning("All the debug screen channels are already used");
		return -1;
	}

	/// Print text on screen on channel for duration in seconds
	public void ShowDebugText(string text, float duration) {
		int nextChannelAvailable = GetNextChannelAvailable();
		if (nextChannelAvailable == -1) return;
		ShowDebugText(text, nextChannelAvailable, duration);
	}

	/// Print text on screen on channel for duration in seconds
	public void ShowDebugText(string text, int channel, float duration) {
		CheckChannelValidity(channel);
		DebugVariable debugVariableAtChannel = m_DebugVariables[channel];
		if (debugVariableAtChannel.IsInUse()) {
			debugVariableAtChannel.Hide();
			m_DebugVariableDict.Remove(debugVariableAtChannel.VarName);
		}
		m_DebugTexts[channel].Show(text, duration);
	}

	/// Start displaying value of variable on screen, in 1st channel available
	public void ShowDebugVariable<T>(string variableName, T value) {
		int nextChannelAvailable = GetNextChannelAvailable();
		if (nextChannelAvailable == -1) return;
		ShowDebugVariable<T>(variableName, value, nextChannelAvailable);
	}

	/// Show or update variable on screen, in 1st channel available
	public void ShowOrUpdateDebugVariable<T>(string variableName, T value) {
		if (!m_DebugVariableDict.ContainsKey(variableName)) {
			ShowDebugVariable<T>(variableName, value);
		} else {
			UpdateVariable<T>(variableName, value);
		}
	}

	/// Start displaying value of variable on screen
	public void ShowDebugVariable<T>(string variableName, T value, int channel) {
		CheckChannelValidity(channel);
		m_DebugTexts[channel].Hide();
		DebugVariable debugVariableAtChannel = m_DebugVariables[channel];
		if (debugVariableAtChannel.IsInUse()) {
			// no need to hide here, since we'll reuse it soon enough
			m_DebugVariableDict.Remove(debugVariableAtChannel.VarName);
		}
		debugVariableAtChannel.Show(variableName, value);
		m_DebugVariableDict.Add(variableName, debugVariableAtChannel);
	}

	/// Update value of variable by name
	public void UpdateVariable<T>(string variableName, T value) {
		// send update variable event to all debug variable scripts, they will update if they are concerned
		// alternative: keep a dictionary of DebugVariable per variable name and directly update the one concerned
		m_DebugVariableDict[variableName].SetValue<T>(value);
		// UpdateVariableEvent(variableName, value.ToString());
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
