using UnityEngine;
using UnityEditor;
using System;
using System.Collections;

[CanEditMultipleObjects()]
[CustomEditor(typeof(RectContainer), true)]
public class RectContainerEditor : Editor {

	void OnSceneGUI ()
	{
		RectContainer rectContainer = (RectContainer) target;

		Undo.RecordObject(rectContainer, "Resized Rect");
		HandlesUtil.DrawRect(ref rectContainer.rect, rectContainer.transform, rectContainer.drawColor);
	}

}
