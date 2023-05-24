using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace CommonsHelper.Editor
{
    /// LayerField will display a dropdown to select a layer among the list of layers defined
    /// at edit time, using a native method.
    [CustomPropertyDrawer(typeof(LayerFieldAttribute))]
    public class LayerFieldDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Use property scope to enable prefab override styling
            using (new EditorGUI.PropertyScope(position, label, property))
            {
                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    int newLayer = EditorGUI.LayerField(position, label, property.intValue);

                    if (check.changed)
                    {
                        property.intValue = newLayer;
                    }
                }
            }
        }
    }
}
