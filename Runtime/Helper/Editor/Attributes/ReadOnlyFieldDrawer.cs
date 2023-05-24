// Thanks to various contributors, we are eventually using FuzzyLogic's 1st variant
// for field types that don't already have a Custom Property Drawer, but using DisabledScope for memory efficiency:
// https://answers.unity.com/questions/489942/how-to-make-a-readonly-property-in-inspector.html

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace HyperUnityCommons.Editor
{
    /// ReadOnlyField will gray out elements it is used on.
    /// When used on an array, it will gray out the array elements but not the array itself, so the size can still be
    /// changed. Elements can also still be added/removed with the +/- buttons.
    /// There is currently no work around, so the user must be careful not to modify the size manually.
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
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
        }
    }
}
