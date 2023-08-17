// Created by Long Nguyen Huu
// 2016.05.15
// Original code under MIT License
// However, specific methods have been taking from various places and may have their own authors/licenses,
// see comments above each

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

		// Default parameters for render and transparent screenshots
		private const int defaultRenderWidth = 1920;
		private const int defaultRenderHeight = 1080;

		private string screenshotFolderPath = defaultScreenshotFolderPath;
		private string screenshotFilenamePrefix = defaultScreenshotFilenamePrefix;
		private int nextScreenshotIndex = 0;

		// Parameters for render and transparent screenshots
		public int renderWidth = defaultRenderWidth;
		public int renderHeight = defaultRenderHeight;

		[Tooltip("If true, use RGBA32 instead of RGB24, allowing alpha transparency when camera uses a transparent " +
			"solid color as background")]
		public bool renderTransparent = false;


		[MenuItem("Window/Hyper Unity Commons/Editor Screenshot")]
		private static void Init()
		{
			GetOrCreateWindow();
		}

		[MenuItem("Tools/Take Screenshot _F11")]
		private static void StaticTakeScreenshot()
		{
			GetOrCreateWindow().TakeStandardScreenshot();
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

			if (EditorPrefs.HasKey($"EditorScreenshot.{Application.productName}.renderWidth"))
			{
				editorScreenshot.renderWidth = EditorPrefs.GetInt($"EditorScreenshot.{Application.productName}.renderWidth");
			}

			if (EditorPrefs.HasKey($"EditorScreenshot.{Application.productName}.renderHeight"))
			{
				editorScreenshot.renderHeight = EditorPrefs.GetInt($"EditorScreenshot.{Application.productName}.renderHeight");
			}

			if (EditorPrefs.HasKey($"EditorScreenshot.{Application.productName}.renderTransparent"))
			{
				editorScreenshot.renderTransparent = EditorPrefs.GetInt($"EditorScreenshot.{Application.productName}.renderTransparent") > 0;
			}

			// if one dimension is 0, revert to default
			if (editorScreenshot.renderWidth == 0 || editorScreenshot.renderHeight == 0)
			{
				editorScreenshot.renderWidth = defaultRenderWidth;
				editorScreenshot.renderHeight = defaultRenderHeight;
				EditorPrefs.SetInt($"EditorScreenshot.{Application.productName}.renderWidth", defaultRenderWidth);
				EditorPrefs.SetInt($"EditorScreenshot.{Application.productName}.renderHeight", defaultRenderHeight);

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
			renderWidth = EditorGUILayout.IntField("Render width", renderWidth);
			renderHeight = EditorGUILayout.IntField("Render height", renderHeight);
			renderTransparent = EditorGUILayout.Toggle("Render transparent", renderTransparent);

			if (EditorGUI.EndChangeCheck()) {
				EditorPrefs.SetString($"EditorScreenshot.{Application.productName}.screenshotFolderPath", screenshotFolderPath);
				EditorPrefs.SetString($"EditorScreenshot.{Application.productName}.screenshotFilenamePrefix", screenshotFilenamePrefix);
				EditorPrefs.SetInt($"EditorScreenshot.{Application.productName}.nextScreenshotIndex", nextScreenshotIndex);
				EditorPrefs.SetInt($"EditorScreenshot.{Application.productName}.renderWidth", renderWidth);
				EditorPrefs.SetInt($"EditorScreenshot.{Application.productName}.renderHeight", renderHeight);
				EditorPrefs.SetInt($"EditorScreenshot.{Application.productName}.renderTransparent", renderTransparent ? 1 : 0);
			}

			if (GUILayout.Button("Take standard screenshot")) TakeStandardScreenshot();
			if (GUILayout.Button("Take render screenshot")) TakeRenderScreenshot();
			if (GUILayout.Button("Take transparent screenshot via RGBA32")) TakeSimpleTransparentScreenshot();
			if (GUILayout.Button("Take transparent screenshot via black/white comparison")) TakeAdvancedTransparentScreenshot();
			if (GUILayout.Button("Open Screenshots folder")) OpenScreenshotsFolder();
		}

		private string ConstructScreenshotPath(string suffix)
		{
			// Add title and version if available
			BuildData buildData = Build.GetBuildData();
			string appPrefix = buildData ? $"{buildData.appName} {buildData.GetVersionString()} " : "";

			// Ex: "Dragon Raid v0.2.0 screenshot_17 (render)"
			return $"{screenshotFolderPath}/{appPrefix}{screenshotFilenamePrefix}{nextScreenshotIndex:00}{suffix}.png";
		}

		private void TakeStandardScreenshot()
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

				string path = ConstructScreenshotPath("");
				ScreenCapture.CaptureScreenshot(path);

				Debug.LogFormat("Screenshot recorded at {0} ({1})", path, UnityStats.screenRes);

				IncrementScreenshotIndex();
			}
			catch (IOException ex)
			{
				Console.WriteLine(ex.Message);
			}
		}

		// EXPERIMENTAL: render screenshot, does not capture UI
		// http://answers.unity3d.com/questions/22954/how-to-save-a-picture-take-screenshot-from-a-camer.html
		// But the first answer I picked from is kinda old, I had to improve it with GetTemporary/ReleaseTemporary
		// and this can be improved further with the second answer to support JPG, etc.
		// For transparency, I took inspiration from http://answers.unity3d.com/questions/12070/capture-rendered-scene-to-png-with-background-tran.html
		// SimpleCaptureTransparentScreenshot (also pasted more below) and just added a flag renderTransparent to use
		// TextureFormat.RGBA32, which enables alpha transparency and works in simple cases (when there is a single
		// camera with solid color already set to Color.clear, basically)
		private void TakeRenderScreenshot ()
		{
			Camera camera = Camera.main;
			if (camera == null) {
				Debug.LogWarning("No main camera found");
				return;
			}

			RenderTexture rt = RenderTexture.GetTemporary(renderWidth, renderHeight, 24);
			camera.targetTexture = rt;
			TextureFormat textureFormat = renderTransparent ? TextureFormat.RGBA32 : TextureFormat.RGB24;
			Texture2D screenShot = new Texture2D(renderWidth, renderHeight, textureFormat, false);
			camera.Render();
			RenderTexture.active = rt;
			screenShot.ReadPixels(new Rect(0, 0, renderWidth, renderHeight), 0, 0);
			camera.targetTexture = null;
			RenderTexture.active = null; // JC: added to avoid errors
			RenderTexture.ReleaseTemporary(rt);

			byte[] bytes = screenShot.EncodeToPNG();

			try
			{
				if (!Directory.Exists(screenshotFolderPath))
				{
					Directory.CreateDirectory(screenshotFolderPath);
				}

				string path = ConstructScreenshotPath(" (render)");
				File.WriteAllBytes(path, bytes);

				Debug.LogFormat("Render Screenshot recorded at {0} ({1})", path, UnityStats.screenRes);

				IncrementScreenshotIndex();
			}
			catch (IOException ex)
			{
				Console.WriteLine(ex.Message);
			}
		}

		private void TakeSimpleTransparentScreenshot()
		{
			Camera camera = Camera.main;
			if (camera == null) {
				Debug.LogWarning("No main camera found");
				return;
			}

			string path = ConstructScreenshotPath(" (simple transparent)");

			SimpleCaptureTransparentScreenshot(camera, renderWidth, renderHeight, path);
		}

		private void TakeAdvancedTransparentScreenshot()
		{
			Camera camera = Camera.main;
			if (camera == null) {
				Debug.LogWarning("No main camera found");
				return;
			}

			string path = ConstructScreenshotPath(" (advanced transparent)");

			CaptureTransparentScreenshot(camera, renderWidth, renderHeight, path);
		}

		// CaptureTransparentScreenshot and SimpleCaptureTransparentScreenshot come from:
		// https://answers.unity.com/questions/12070/capture-rendered-scene-to-png-with-background-tran.html
		// http://answers.unity.com/answers/1612933/view.html
		// CaptureTransparentScreenshot compares two screenshots on black and white background, computing difference
		// to deduce alpha.
		// SimpleCaptureTransparentScreenshot is a cleaner version of TakeRenderScreenshot that automatically
		// sets background color to Color.clear if needed, and reverts camera parameters from backup if needed.

		// The MIT License (MIT)
		// Copyright (c) 2014 Brad Nelson and Play-Em Inc.
		// CaptureScreenshot is based on Brad Nelson's MIT-licensed AnimationToPng: http://wiki.unity3d.com/index.php/AnimationToPNG
		// AnimationToPng is based on Twinfox and bitbutter's Render Particle to Animated Texture Scripts.
		public static void CaptureTransparentScreenshot(Camera cam, int width, int height, string screengrabfile_path)
		{
			// This is slower, but seems more reliable.
			var bak_cam_targetTexture = cam.targetTexture;
			var bak_cam_clearFlags = cam.clearFlags;
			var bak_RenderTexture_active = RenderTexture.active;

			var tex_white = new Texture2D(width, height, TextureFormat.ARGB32, false);
			var tex_black = new Texture2D(width, height, TextureFormat.ARGB32, false);
			var tex_transparent = new Texture2D(width, height, TextureFormat.ARGB32, false);
			// Must use 24-bit depth buffer to be able to fill background.
			var render_texture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32);
			var grab_area = new Rect(0, 0, width, height);

			RenderTexture.active = render_texture;
			cam.targetTexture = render_texture;
			cam.clearFlags = CameraClearFlags.SolidColor;

			cam.backgroundColor = Color.black;
			cam.Render();
			tex_black.ReadPixels(grab_area, 0, 0);
			tex_black.Apply();

			cam.backgroundColor = Color.white;
			cam.Render();
			tex_white.ReadPixels(grab_area, 0, 0);
			tex_white.Apply();

			// Create Alpha from the difference between black and white camera renders
			for (int y = 0; y < tex_transparent.height; ++y)
			{
				for (int x = 0; x < tex_transparent.width; ++x)
				{
					float alpha = tex_white.GetPixel(x, y).r - tex_black.GetPixel(x, y).r;
					alpha = 1.0f - alpha;
					Color color;
					if (alpha == 0)
					{
						color = Color.clear;
					}
					else
					{
						color = tex_black.GetPixel(x, y) / alpha;
					}

					color.a = alpha;
					tex_transparent.SetPixel(x, y, color);
				}
			}

			// Encode the resulting output texture to a byte array then write to the file
			byte[] pngShot = ImageConversion.EncodeToPNG(tex_transparent);
			File.WriteAllBytes(screengrabfile_path, pngShot);

			cam.clearFlags = bak_cam_clearFlags;
			cam.targetTexture = bak_cam_targetTexture;
			RenderTexture.active = bak_RenderTexture_active;
			RenderTexture.ReleaseTemporary(render_texture);

			Texture2D.Destroy(tex_black);
			Texture2D.Destroy(tex_white);
			Texture2D.Destroy(tex_transparent);
		}

		public static void SimpleCaptureTransparentScreenshot(Camera cam, int width, int height,
			string screengrabfile_path)
		{
			// Depending on your render pipeline, this may not work.
			var bak_cam_targetTexture = cam.targetTexture;
			var bak_cam_clearFlags = cam.clearFlags;
			var bak_RenderTexture_active = RenderTexture.active;

			var tex_transparent = new Texture2D(width, height, TextureFormat.ARGB32, false);
			// Must use 24-bit depth buffer to be able to fill background.
			var render_texture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32);
			var grab_area = new Rect(0, 0, width, height);

			RenderTexture.active = render_texture;
			cam.targetTexture = render_texture;
			cam.clearFlags = CameraClearFlags.SolidColor;

			// Simple: use a clear background
			cam.backgroundColor = Color.clear;
			cam.Render();
			tex_transparent.ReadPixels(grab_area, 0, 0);
			tex_transparent.Apply();

			// Encode the resulting output texture to a byte array then write to the file
			byte[] pngShot = ImageConversion.EncodeToPNG(tex_transparent);
			File.WriteAllBytes(screengrabfile_path, pngShot);

			cam.clearFlags = bak_cam_clearFlags;
			cam.targetTexture = bak_cam_targetTexture;
			RenderTexture.active = bak_RenderTexture_active;
			RenderTexture.ReleaseTemporary(render_texture);

			Texture2D.Destroy(tex_transparent);
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
