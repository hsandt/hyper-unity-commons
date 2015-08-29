public static class DebugScreen {

	/// Print text on screen for duration in seconds
	public static void Print(string text, float duration = 1f) {
		DebugScreenManager.Instance.Print(text, duration: duration);
	}

}
