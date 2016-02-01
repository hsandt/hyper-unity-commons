using UnityEngine;

public static class DebugScreen {

	/// Print text on screen with default duration
	public static void Print(int channel, string text) {
		if (DebugScreenManager.Instance != null)
			DebugScreenManager.Instance.ShowDebugText(text, channel);
	}

	/// Print formatted stext on screen with default duration
	public static void Print(int channel, string text, params object[] args) {
		if (DebugScreenManager.Instance != null)
			DebugScreenManager.Instance.ShowDebugText(string.Format(text, args), channel);
	}

	// REFACTOR: one single function PrintVar for both init and update
	// use a dict to track which vars are observed by name, and if a var is already tracked update, else init / show
	public static void PrintVar<T>(int channel, string variableName, T value) {
		if (DebugScreenManager.Instance != null)
			DebugScreenManager.Instance.ShowDebugVariable(variableName, value, channel);
	}

	public static void UpdateVar<T>(string variableName, T value) {
		if (DebugScreenManager.Instance != null)
			DebugScreenManager.Instance.UpdateVariable(variableName, value);
	}

}
