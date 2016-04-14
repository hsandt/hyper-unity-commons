using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Linq;
using System.Reflection;

public class EditorScreenshot : EditorWindow
{

	string screenshotFolderPath = "Screenshots";
	int nextScreenshotIndex = 0;

	[MenuItem("Window/Editor Screenshot")]
	static void Init()
	{
		EditorScreenshot editorScreenshot = GetWindow<EditorScreenshot>(title: "Screenshot");

		if (EditorPrefs.HasKey("EditorScreenshot.screenshotFolderPath"))
			editorScreenshot.screenshotFolderPath = EditorPrefs.GetString("EditorScreenshot.screenshotFolderPath");
		// if (EditorPrefs.HasKey("AutoSnap.snapValue"))
		// 	window.snapValue = EditorPrefs.GetFloat("AutoSnap.snapValue");
		// if (EditorPrefs.HasKey("AutoSnap.doRotateSnap"))
		// 	window.doRotateSnap = EditorPrefs.GetBool("AutoSnap.doRotateSnap");
		// if (EditorPrefs.HasKey("AutoSnap.snapRotateValue"))
		// 	window.snapRotateValue = EditorPrefs.GetFloat("AutoSnap.snapRotateValue");
	}

	void OnGUI()
	{
		EditorGUI.BeginChangeCheck ();
		screenshotFolderPath = EditorGUILayout.TextField("Save path: ", screenshotFolderPath);
		if (EditorGUI.EndChangeCheck ())
			EditorPrefs.SetString("EditorScreenshot.screenshotFolderPath", screenshotFolderPath);

		if (GUILayout.Button("Take screenshot")) TakeScreenshot();
	}

	[MenuItem("Tools/Take Screenshot _F11")]
	static void StaticTakeScreenshot()
	{
		GetWindow<EditorScreenshot>(title: "Screenshot").TakeScreenshot();
	}

	void TakeScreenshot()
	{
		// get name of current focused window, which should be "  (UnityEditor.GameView)" if it is a Game view
		string focusedWindowName = EditorWindow.focusedWindow.ToString();
		if (!focusedWindowName.Contains("UnityEditor.GameView")) {
			// since no Game view is focused right now, focus on any Game view, or create one if needed
			Type gameViewType = Type.GetType("UnityEditor.GameView,UnityEditor");
			EditorWindow.GetWindow(gameViewType);
		}

		// Tried getting the last focused window, but does not always work (even for focused window!)
		// EditorWindow lastFocusedGameView = (EditorWindow) gameViewType.GetField("s_LastFocusedGameView", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
		// if (lastFocusedGameView != null) {
		// 	lastFocusedGameView.Focus();
		// } else {
		// 	// no Game view created since editor launch, create one
		// 	Type gameViewType = Type.GetType("UnityEditor.GameView,UnityEditor");
		// 	EditorWindow.GetWindow(gameViewType);
		// }

		string path = string.Format("{0}/screen_{1:00}.png", screenshotFolderPath, nextScreenshotIndex);
		Application.CaptureScreenshot(path);

		Debug.LogFormat("Screenshot recorded in {0}", path);
		++nextScreenshotIndex;
	}

	[MenuItem("Tools/Screenshot/Reset screenshot count")]
	void ResetCount()
	{
		nextScreenshotIndex = 0;
	}

}
