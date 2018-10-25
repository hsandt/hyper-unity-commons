﻿// Created by Long Nguyen Huu
// 2016.05.15
// MIT License

using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;

namespace CommonsEditor
{

	public class EditorScreenshot : EditorWindow
	{

		string screenshotFolderPath = "Screenshots";
		string screenshotFilenamePrefix = "screenshot_";
		int nextScreenshotIndex = 0;

		[MenuItem("Window/Editor Screenshot")]
		static void Init()
		{
			GetOrCreateWindow();
		}

		[MenuItem("Tools/Take Screenshot _F11")]
		static void StaticTakeScreenshot()
		{
			GetOrCreateWindow().TakeScreenshot();
		}

		static EditorScreenshot GetOrCreateWindow()
		{
			EditorScreenshot editorScreenshot = GetWindow<EditorScreenshot>(title: "Screenshot");

			if (EditorPrefs.HasKey("EditorScreenshot.screenshotFolderPath"))
				editorScreenshot.screenshotFolderPath = EditorPrefs.GetString("EditorScreenshot.screenshotFolderPath");
			if (EditorPrefs.HasKey("EditorScreenshot.screenshotFilenamePrefix"))
				editorScreenshot.screenshotFilenamePrefix = EditorPrefs.GetString("EditorScreenshot.screenshotFilenamePrefix");
			if (EditorPrefs.HasKey("EditorScreenshot.nextScreenshotIndex"))
				editorScreenshot.nextScreenshotIndex = EditorPrefs.GetInt("EditorScreenshot.nextScreenshotIndex");

			return editorScreenshot;
		}

		void OnGUI()
		{
			EditorGUI.BeginChangeCheck();

			GUIContent savePathLabel = new GUIContent("Save path", "Save path of the screenshots, relative from the project root");
			screenshotFolderPath = EditorGUILayout.TextField(savePathLabel, screenshotFolderPath);
			screenshotFilenamePrefix = EditorGUILayout.TextField("Screenshot prefix", screenshotFilenamePrefix);
			nextScreenshotIndex = EditorGUILayout.IntField("Next screenshot index", nextScreenshotIndex);

			if (EditorGUI.EndChangeCheck()) {
				EditorPrefs.SetString("EditorScreenshot.screenshotFolderPath", screenshotFolderPath);
				EditorPrefs.SetString("EditorScreenshot.screenshotFilenamePrefix", screenshotFilenamePrefix);
				EditorPrefs.SetInt("EditorScreenshot.nextScreenshotIndex", nextScreenshotIndex);
			}

			if (GUILayout.Button("Take screenshot")) TakeScreenshot();
			if (GUILayout.Button("Take hires screenshot")) TakeHiresScreenshot();
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
				string path = string.Format("{0}/{1}{2:00}.png", screenshotFolderPath, screenshotFilenamePrefix, nextScreenshotIndex);
				ScreenCapture.CaptureScreenshot(path);

				Debug.LogFormat("Screenshot recorded at {0} ({1})", path, UnityStats.screenRes);

				++nextScreenshotIndex;
				EditorPrefs.SetInt("EditorScreenshot.nextScreenshotIndex", nextScreenshotIndex);
			}
			catch (IOException ex)
			{
				Console.WriteLine(ex.Message);
			}
		}

		// EXPERIMENTAL: hi-res screenshot
		// http://answers.unity3d.com/questions/22954/how-to-save-a-picture-take-screenshot-from-a-camer.html
		// For transparency, insert code from http://answers.unity3d.com/questions/12070/capture-rendered-scene-to-png-with-background-tran.html

		public int resWidth = 2550;
		public int resHeight = 3300;

		void TakeHiresScreenshot ()
		{
			Camera camera = Camera.main;
			if (camera == null) {
				Debug.LogWarning("No main camera found");
				return;
			}

			RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
			camera.targetTexture = rt;
			Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
			camera.Render();
			RenderTexture.active = rt;
			screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
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
				string path = string.Format("{0}/{1}{2:00} (hires).png", screenshotFolderPath, screenshotFilenamePrefix, nextScreenshotIndex);
				System.IO.File.WriteAllBytes(path, bytes);

				Debug.LogFormat("Hires Screenshot recorded at {0} ({1})", path, UnityStats.screenRes);

				++nextScreenshotIndex;
				EditorPrefs.SetInt("EditorScreenshot.nextScreenshotIndex", nextScreenshotIndex);
			}
			catch (IOException ex)
			{
				Console.WriteLine(ex.Message);
			}
		}

	}

}
