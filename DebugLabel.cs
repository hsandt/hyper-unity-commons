using UnityEngine;

namespace CommonsDebug
{

	/// Proxy static class for DebugLabelManager
	public static class DebugLabel {

	    // All methods start by checking the existence of DebugLabelManager.Instance for safety. In particular, the DebugLabelManager object should be EditorOnly
	    // so it is missing in the standalone

		/// Draw a text label at a given position, with given color and duration
	    public static void DrawText(Vector3 position, string text, Color color, float duration = 0f) {
	        #if UNITY_EDITOR
	        if (DebugLabelManager.Instance != null)
	            DebugLabelManager.Instance.DrawText(position, text, color, duration);
	        #endif
		}

	}

}
