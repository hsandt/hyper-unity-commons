// We combined the best elements of two solutions:
// https://answers.unity.com/questions/489942/how-to-make-a-readonly-property-in-inspector.html (includeChildren: true)
// https://forum.unity.com/threads/serialize-readonly-field.426525/ (previousGUIState)

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace CommonsHelper.Editor
{
    /// ReadOnlyField will gray out elements it is used on.
    /// When used on an array, it will gray out the array elements but not the array itself, so the size can still be
    /// changed. There is currently no work around, so the user must be careful not to modify the size manually.
    /// See https://answers.unity.com/questions/1150679/custom-propertydrawer-for-attributes-on-array-memb.html
    [CustomPropertyDrawer(typeof(ReadOnlyFieldAttribute))]
    public class ReadOnlyFieldDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            bool previousGUIState = GUI.enabled;
            GUI.enabled = false;
 
            EditorGUI.PropertyField(position, property, label, true);
 
            GUI.enabled = previousGUIState;
        }
    }
}
