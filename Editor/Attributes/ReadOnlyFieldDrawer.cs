using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace CommonsHelper.Editor
{

    /// ReadOnlyField will gray out elements it is used on.
    /// When used on an array, it will gray out the array elements but not the array itself, so the size can still be
    /// changed. There is currently no work around, so the user must be careful not to modify the size manually.
    /// https://answers.unity.com/questions/1150679/custom-propertydrawer-for-attributes-on-array-memb.html
    [CustomPropertyDrawer(typeof(ReadOnlyFieldAttribute))]
    public class ReadOnlyFieldDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property,
            GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position,
            SerializedProperty property,
            GUIContent label)
        {
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }
    }

}
