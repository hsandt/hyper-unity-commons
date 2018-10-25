// Created by Nikolay Dyankov
// 31.05.2014
// Version 1.0
// modified by hsandt

using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Reflection;
using System.Collections.Generic;

namespace CommonsEditor
{

	public class RestartWindow : EditorWindow {

		[MenuItem("Window/Restart")]
		private static void ShowWindow () {
			EditorWindow.GetWindow(typeof(RestartWindow));
			// RestartWindow window = (RestartWindow)EditorWindow.GetWindow(typeof(RestartWindow));
			// window.autoRepaintOnSceneChange = true;
		}

		// void OnInspectorUpdate() {
		// 	Repaint();
	 //    }

	    void OnGUI () {
	    	if (Application.isPlaying) {
					GameObject selectedGameObject = Selection.activeGameObject;
					if (selectedGameObject != null) {
						if (GUILayout.Button("Restart")) selectedGameObject.SendMessage("Restart");
					}
	    	}
	    }

	}

}
