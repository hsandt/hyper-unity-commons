using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CommonsEditor
{

	public static class EditorGUIHelper
	{
	    /// Draw a property field from the previous control position and update the Y position according to the field height, with optional extra Y spacing
	    public static bool PropertyField (SerializedProperty property, GUIContent label, ref Rect position, float spacing = 0f)
	    {
	        float height = EditorGUI.GetPropertyHeight (property, label);
	        position.height = height;
	        bool expandedChildren = EditorGUI.PropertyField (position, property, label);
	        position.y += height + spacing;
	        return expandedChildren;
	    }
	}
}
