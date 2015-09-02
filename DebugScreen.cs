public static class DebugScreen {

	/// Print text on screen with default duration
	public static void Print(int channel, string text) {
		DebugScreenManager.Instance.ShowDebugText(text, channel);
	}

	/// Print formatted stext on screen with default duration
	public static void Print(int channel, string text, params object[] args) {
		DebugScreenManager.Instance.ShowDebugText(string.Format(text, args), channel);
	}



}
