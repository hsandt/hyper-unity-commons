using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

// IMPROVE: when variable has not been updated for several frames, darken the label

/// Usage: create an instance of DebugCanvas object in the scene. The prefab is provided
/// in the same folder as this script, and contains both objects for the DebugScreenManager and the DebugText scripts
public class DebugScreenManager : SingletonManager<DebugScreenManager> {

#if UNITY_EDITOR
	[System.NonSerialized] bool initialized;  // Hot reload support
#endif

	protected DebugScreenManager () {} // guarantee this will be always a singleton only - can't use the constructor!

	/* Prefabs */

	// debug text and variable prefabs used to create on-screen debug messages
	public GameObject debugTextPrefab;
	public GameObject debugVariablePrefab;

	/* Parameters */

	[SerializeField]
	int nbChannels = 8;  // channels are instantiated on Awake, so you cannot change add channels while running; if you want to, run Init() on value change

	/* State vars */

	/// Context game object, used to differenciate debug texts called from the same line but by different objects that are not accessible from the method calling the debug
	/// Can also be used by other debug functions
	public GameObject context = null;

	// Texts and variables used to debug
	List<DebugText> m_DebugTexts = new List<DebugText>();
	List<DebugVariable> m_DebugVariables = new List<DebugVariable>();

	/// Dictionary of variable name => debug variable
	Dictionary<string, DebugVariable> m_DebugVariableDict = new Dictionary<string, DebugVariable>();

	// variable update event
//	public delegate void UpdateVariableHandler(string variableName, string valueText);
//	public static event UpdateVariableHandler UpdateVariableEvent;

	void Awake () {
#if UNITY_EDITOR
		initialized = true;  // Hot reload support
#endif
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

			// GameObject debugTextInstance = debugTextPrefab.InstantiateUnderWithOffset(transform, offset);

			GameObject debugTextInstance = Instantiate(debugTextPrefab, transform) as GameObject;
			debugTextInstance.transform.localPosition += offset;

			var debugText = debugTextInstance.GetComponentOrFail<DebugText>();
			debugText.channelIndex = i;
			m_DebugTexts.Add(debugText);
			debugTextInstance.SetActive(false);

			// GameObject debugVariableInstance = debugVariablePrefab.InstantiateUnderWithOffset(transform, offset);
			GameObject debugVariableInstance = Instantiate(debugVariablePrefab,  transform) as GameObject;
			debugVariableInstance.transform.localPosition += offset;

			var debugVariable = debugVariableInstance.GetComponentOrFail<DebugVariable>();
			debugVariable.channelIndex = i;
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

	// IMPROVE: categorize debug texts by string category ("character", "item", etc.) rather than numeric channel

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
		if (context != null) text = string.Format("({0}) {1}", context.name, text);
		m_DebugTexts[channel].Show(text, duration);
	}

	// OPTIMIZATION: only update text as human-visible time (if pausing step by step in the editor, update each frame, ie consider real time)

	/// Show or update variable on screen, in 1st channel available
	public void ShowOrUpdateDebugVariable<T>(string variableName, T value) {
		if (context != null) variableName = string.Format("({0}) {1}", context.name, variableName);
		if (!m_DebugVariableDict.ContainsKey(variableName)) {
			ShowDebugVariable<T>(variableName, value);
		} else {
			UpdateVariable<T>(variableName, value);
		}
	}

	/// Start displaying value of variable on screen, in 1st channel available
	public void ShowDebugVariable<T>(string variableName, T value) {
		int nextChannelAvailable = GetNextChannelAvailable();
		if (nextChannelAvailable == -1) return;
		ShowDebugVariable<T>(variableName, value, nextChannelAvailable);
	}

	/// Start displaying value of variable on screen
	public void ShowDebugVariable<T>(string variableName, T value, int channel) {
		CheckChannelValidity(channel);
		m_DebugTexts[channel].Hide();
		DebugVariable debugVariableAtChannel = m_DebugVariables[channel];
		if (debugVariableAtChannel.IsInUse()) {
			// no need to hide here, since we'll reuse it now
			m_DebugVariableDict.Remove(debugVariableAtChannel.VarName);
		}
		// REFACTOR: Show only shows, call SetValue to update separately
		debugVariableAtChannel.Show(variableName, value);
		m_DebugVariableDict.Add(variableName, debugVariableAtChannel);
	}

	/// Update value of variable by name
	void UpdateVariable<T>(string variableName, T value) {
		// REFACTOR: delegate to DebugVariable.UpdateValue() method
		m_DebugVariableDict[variableName].SetValue<T>(value);
		// UpdateVariableEvent(variableName, value.ToString());
	}

	public void ClearChannel(int channel) {
		CheckChannelValidity(channel);
		m_DebugTexts[channel].Hide();
		m_DebugVariables[channel].Hide();
	}

	public void ClearAllChannels() {
		for (int i = 0; i < nbChannels; ++i) {
			CheckChannelValidity(i);
			m_DebugTexts[i].Hide();
			m_DebugVariables[i].Hide();
		}
	}

	void CheckChannelValidity (int channel) {
		if (channel >= nbChannels) {
			throw new ArgumentException(string.Format("Channel #{0} does not exist (#0 - #{1} only)", channel, nbChannels - 1), "channel");
		}
	}

#if UNITY_EDITOR
	// Hot reload support
	void OnEnable () {
		if (!initialized) {
			Debug.Log("[DEBUG SCREEN MANAGER] Hot Reload");
			Instance = this;
			ClearAllChannels();
		}
	}
#endif

}
