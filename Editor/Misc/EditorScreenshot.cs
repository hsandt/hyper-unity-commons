// Created by Long Nguyen Huu
// 2016.05.15
// MIT License

using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace HyperUnityCommons.Editor
{
	public class EditorScreenshot : EditorWindow
	{
		private const string defaultScreenshotFolderPath = "Screenshots";
		private const string defaultScreenshotFilenamePrefix = "screenshot_";

		// EXPERIMENTAL: default parameters for hi-res screenshot
		private const int defaultHiResWidth = 3840;
		private const int defaultHiResHeight = 2160;

		private string screenshotFolderPath = defaultScreenshotFolderPath;
		private string screenshotFilenamePrefix = defaultScreenshotFilenamePrefix;
		private int nextScreenshotIndex = 0;

		// EXPERIMENTAL: parameters for hi-res screenshot
		public int hiResWidth = defaultHiResWidth;
		public int hiResHeight = defaultHiResHeight;

		[MenuItem("Window/Hyper Unity Commons/Editor Screenshot")]
		private static void Init()
		{
			GetOrCreateWindow();
		}

		[MenuItem("Tools/Take Screenshot _F11")]
		private static void StaticTakeScreenshot()
		{
			GetOrCreateWindow().TakeScreenshot();
		}

		private static EditorScreenshot GetOrCreateWindow()
		{
			EditorScreenshot editorScreenshot = GetWindow<EditorScreenshot>(title: "Screenshot");

			if (EditorPrefs.HasKey($"EditorScreenshot.{Application.productName}.screenshotFolderPath"))
			{
				editorScreenshot.screenshotFolderPath = EditorPrefs.GetString($"EditorScreenshot.{Application.productName}.screenshotFolderPath");
			}

			// if empty, revert to default
			if (string.IsNullOrWhiteSpace(editorScreenshot.screenshotFolderPath))
			{
				editorScreenshot.screenshotFolderPath = defaultScreenshotFolderPath;
				EditorPrefs.SetString($"EditorScreenshot.{Application.productName}.screenshotFolderPath", defaultScreenshotFolderPath);
			}

			if (EditorPrefs.HasKey($"EditorScreenshot.{Application.productName}.screenshotFilenamePrefix"))
			{
				editorScreenshot.screenshotFilenamePrefix = EditorPrefs.GetString($"EditorScreenshot.{Application.productName}.screenshotFilenamePrefix");
			}

			// if empty, revert to default
			if (string.IsNullOrWhiteSpace(editorScreenshot.screenshotFilenamePrefix))
			{
				editorScreenshot.screenshotFilenamePrefix = defaultScreenshotFilenamePrefix;
				EditorPrefs.SetString($"EditorScreenshot.{Application.productName}.screenshotFilenamePrefix", defaultScreenshotFilenamePrefix);
			}

			if (EditorPrefs.HasKey($"EditorScreenshot.{Application.productName}.nextScreenshotIndex"))
			{
				editorScreenshot.nextScreenshotIndex = EditorPrefs.GetInt($"EditorScreenshot.{Application.productName}.nextScreenshotIndex");
			}

			if (EditorPrefs.HasKey($"EditorScreenshot.{Application.productName}.hiResWidth"))
			{
				editorScreenshot.hiResWidth = EditorPrefs.GetInt($"EditorScreenshot.{Application.productName}.hiResWidth");
			}

			if (EditorPrefs.HasKey($"EditorScreenshot.{Application.productName}.hiResHeight"))
			{
				editorScreenshot.hiResHeight = EditorPrefs.GetInt($"EditorScreenshot.{Application.productName}.hiResHeight");
			}

			// if one dimension is 0, revert to default
			if (editorScreenshot.hiResWidth == 0 || editorScreenshot.hiResHeight == 0)
			{
				editorScreenshot.hiResWidth = defaultHiResWidth;
				editorScreenshot.hiResHeight = defaultHiResHeight;
				EditorPrefs.SetInt($"EditorScreenshot.{Application.productName}.hiResWidth", defaultHiResWidth);
				EditorPrefs.SetInt($"EditorScreenshot.{Application.productName}.hiResHeight", defaultHiResHeight);

			}

			return editorScreenshot;
		}

		private void OnGUI()
		{
			EditorGUI.BeginChangeCheck();

			GUIContent savePathLabel = new GUIContent("Save path", "Save path of the screenshots, relative from the project root");
			screenshotFolderPath = EditorGUILayout.TextField(savePathLabel, screenshotFolderPath);
			screenshotFilenamePrefix = EditorGUILayout.TextField("Screenshot prefix", screenshotFilenamePrefix);
			nextScreenshotIndex = EditorGUILayout.IntField("Next screenshot index", nextScreenshotIndex);
			hiResWidth = EditorGUILayout.IntField("Hi-res width", hiResWidth);
			hiResHeight = EditorGUILayout.IntField("Hi-res height", hiResHeight);

			if (EditorGUI.EndChangeCheck()) {
				EditorPrefs.SetString($"EditorScreenshot.{Application.productName}.screenshotFolderPath", screenshotFolderPath);
				EditorPrefs.SetString($"EditorScreenshot.{Application.productName}.screenshotFilenamePrefix", screenshotFilenamePrefix);
				EditorPrefs.SetInt($"EditorScreenshot.{Application.productName}.nextScreenshotIndex", nextScreenshotIndex);
				EditorPrefs.SetInt($"EditorScreenshot.{Application.productName}.hiResWidth", hiResWidth);
				EditorPrefs.SetInt($"EditorScreenshot.{Application.productName}.hiResHeight", hiResHeight);
			}

			if (GUILayout.Button("Take screenshot")) TakeScreenshot();
			if (GUILayout.Button("Take hi-res screenshot")) TakeHiresScreenshot();
			if (GUILayout.Button("Open Screenshots folder")) OpenScreenshotsFolder();
		}

		private string ConstructScreenshotPath(bool hires)
		{
			// Add title and version if available
			BuildData buildData = Build.GetBuildData();
			string appPrefix = buildData ? $"{buildData.appName} {buildData.GetVersionString()} " : "";

			// Add resolution suffix for hires
			string optionalResolutionSuffix = hires ? " (hires)" : "";

			// Ex: "Dragon Raid v0.2.0 screenshot_17 (hires)"
			return $"{screenshotFolderPath}/{appPrefix}{screenshotFilenamePrefix}{nextScreenshotIndex:00}{optionalResolutionSuffix}.png";
		}

		private void TakeScreenshot()
		{
			if (string.IsNullOrWhiteSpace(screenshotFolderPath))
			{
				Debug.LogWarning("Screenshot Folder Path is empty, cannot take screenshot.");
				return;
			}

			// get name of current focused window, which should be "  (UnityEditor.GameView)" if it is a Game view
			string focusedWindowName = focusedWindow.ToString();
			if (!focusedWindowName.Contains("UnityEditor.GameView")) {
				// since no Game view is focused right now, focus on any Game view, or create one if needed
				Type gameViewType = Type.GetType("UnityEditor.GameView,UnityEditor");
				GetWindow(gameViewType);
			}

			// Tried getting the last focused window, but does not always work (even for focused window!)
			// Type gameViewType = Type.GetType("UnityEditor.GameView,UnityEditor");
			// EditorWindow lastFocusedGameView = (EditorWindow) gameViewType.GetField("s_LastFocusedGameView", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
			// if (lastFocusedGameView != null) {
			// 	lastFocusedGameView.Focus();
			// } else {
			// 	// no Game view created since editor launch, create one
			// 	Type gameViewType = Type.GetType("UnityEditor.GameView,UnityEditor");
			// 	EditorWindow.GetWindow(gameViewType);
			// }

			try
			{
				if (!Directory.Exists(screenshotFolderPath))
				{
					Directory.CreateDirectory(screenshotFolderPath);
				}

				string path = ConstructScreenshotPath(false);
				ScreenCapture.CaptureScreenshot(path);

				Debug.LogFormat("Screenshot recorded at {0} ({1})", path, UnityStats.screenRes);

				IncrementScreenshotIndex();
			}
			catch (IOException ex)
			{
				Console.WriteLine(ex.Message);
			}
		}

		// EXPERIMENTAL: hi-res screenshot
		// http://answers.unity3d.com/questions/22954/how-to-save-a-picture-take-screenshot-from-a-camer.html
		// For transparency, insert code from http://answers.unity3d.com/questions/12070/capture-rendered-scene-to-png-with-background-tran.html
		private void TakeHiresScreenshot ()
		{
			Camera camera = Camera.main;
			if (camera == null) {
				Debug.LogWarning("No main camera found");
				return;
			}

			RenderTexture rt = new RenderTexture(hiResWidth, hiResHeight, 24);
			camera.targetTexture = rt;
			Texture2D screenShot = new Texture2D(hiResWidth, hiResHeight, TextureFormat.RGB24, false);
			camera.Render();
			RenderTexture.active = rt;
			screenShot.ReadPixels(new Rect(0, 0, hiResWidth, hiResHeight), 0, 0);
			camera.targetTexture = null;
			RenderTexture.active = null; // JC: added to avoid errors
			if (EditorApplication.isPlaying)  // ADDED
				Destroy(rt);
			else
				DestroyImmediate(rt);
			byte[] bytes = screenShot.EncodeToPNG();

			try
			{
				if (!Directory.Exists(screenshotFolderPath))
				{
					Directory.CreateDirectory(screenshotFolderPath);
				}

				string path = ConstructScreenshotPath(true);
				File.WriteAllBytes(path, bytes);

				Debug.LogFormat("Hires Screenshot recorded at {0} ({1})", path, UnityStats.screenRes);

				IncrementScreenshotIndex();
			}
			catch (IOException ex)
			{
				Console.WriteLine(ex.Message);
			}
		}

		private void IncrementScreenshotIndex()
		{
			++nextScreenshotIndex;
			EditorPrefs.SetInt($"EditorScreenshot.{Application.productName}.nextScreenshotIndex", nextScreenshotIndex);
		}

		private void OpenScreenshotsFolder()
		{
			// Application.dataPath ends with Assets/ so we need to go one directory up to get the project root
			string projectRootPath = Path.GetDirectoryName(Application.dataPath);
			string screenshotFolderFullPath = Path.Combine(projectRootPath, screenshotFolderPath);

			// Create directory if needed
			if (!Directory.Exists(screenshotFolderFullPath))
			{
				Directory.CreateDirectory(screenshotFolderFullPath);
			}

			// Open Screenshots folder
			// https://forum.unity.com/threads/editorutility-revealinfinder-inconsistency.383939/#post-8431145
			EditorUtility.OpenWithDefaultApp(screenshotFolderFullPath);
		}
	}
}
