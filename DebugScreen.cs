using UnityEngine;

/// Static class for easy debugging
public static class DebugScreen {

	// All methods start by checking the existence of DebugScreenManager.Instance for safety. In particular, the DebugScreenManager object is EditorOnly so the standalone would crash without

	/// Print text on screen with default duration
	public static void Print(int channel, string text) {
		if (DebugScreenManager.Instance != null)
			DebugScreenManager.Instance.ShowDebugText(text, channel, 2f);
	}

	/// Print formatted text on screen with default duration of 2f
	public static void Print(int channel, string text, params object[] args) {
		if (DebugScreenManager.Instance != null)
			DebugScreenManager.Instance.ShowDebugText(string.Format(text, args), channel, 2f);
	}

	/// Print formatted text on screen with default duration, on first available channel
	public static void Print(string text, params object[] args) {
		if (DebugScreenManager.Instance != null)
			DebugScreenManager.Instance.ShowDebugText(string.Format(text, args), 2f);
	}

	public static void PrintVar<T>(int channel, string variableName, T value) {
		if (DebugScreenManager.Instance != null)
			DebugScreenManager.Instance.ShowDebugVariable(variableName, value, channel);
	}

	public static void UpdateVar<T>(string variableName, T value) {
		if (DebugScreenManager.Instance != null)
			DebugScreenManager.Instance.UpdateVariable(variableName, value);
	}

	// Print or update variable on screen, on first available channel
	public static void PrintVar<T>(string variableName, T value) {
		if (DebugScreenManager.Instance != null)
			DebugScreenManager.Instance.ShowOrUpdateDebugVariable(variableName, value);
	}


}
