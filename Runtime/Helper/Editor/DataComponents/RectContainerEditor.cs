using UnityEngine;
using UnityEditor;
using System;
using System.Collections;

namespace HyperUnityCommons.Editor
{

	[CustomEditor(typeof(RectContainer), true), CanEditMultipleObjects]
	public class RectContainerEditor : UnityEditor.Editor {

		void OnSceneGUI ()
		{
			RectContainer rectContainer = (RectContainer) target;

			Undo.RecordObject(rectContainer, "Resized Rect");
			HandlesUtil.DrawRectHandle(ref rectContainer.rect, rectContainer.transform, rectContainer.drawColor);
		}

	}

}

