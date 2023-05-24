// https://answers.unity.com/questions/1589226/showing-an-array-with-enum-as-keys-in-the-property.html
// Authors:
// - bonzairob: original code
// - idbrii: improve robustness
// - huulong (hsandt): simplified index extraction

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace CommonsHelper.Editor
{
    /// Property drawer for EnumNamedArrayAttribute
    /// See usage in EnumNamedArrayAttribute docstring
    [CustomPropertyDrawer(typeof(EnumNamedArrayAttribute))]
    public class DrawerEnumNamedArray : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EnumNamedArrayAttribute enumNames = (EnumNamedArrayAttribute) attribute;
            // Initial idea was to extract index X from something like "Stats.baseConsumableStats.Array.data[X]" with IndexOf and Replace,
            // but when embedded, propertyPath becomes something like "StatsArray.Array.data[1].baseConsumableStats.Array.data[X]",
            // requiring LastIndexOf instead.
            // However, it turns out that displayName is "Element X" where X is the index, so despite being slightly
            // less reliable than a propertyPath (as prone to localization changes), it is more simple to extract X
            // from there, so we're using displayName instead.

            // Ex: Element 2
            string value = property.displayName.Replace("Element ", "");

            // Ex: 2
            int index = System.Convert.ToInt32(value);

            if (index < enumNames.names.Length)
            {
                //change the label
                label.text = enumNames.names[index];
            }

            // Draw field
            EditorGUI.PropertyField( position, property, label, true );
        }
    }
}
