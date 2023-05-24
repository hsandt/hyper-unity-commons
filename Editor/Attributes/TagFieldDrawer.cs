using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace CommonsHelper.Editor
{
    /// TagField will display a dropdown to select a tag among the list of tags defined
    /// at edit time, using a native method.
    [CustomPropertyDrawer(typeof(TagFieldAttribute))]
    public class TagFieldDrawer : PropertyDrawer
    {
        private static readonly Color invalidTextColor = new Color(1f, 0.63f, 0.63f);
        private static readonly GUIStyle invalidTagGUIStyle = new GUIStyle(EditorStyles.popup)
        {
            normal = new GUIStyleState { textColor = invalidTextColor },
            hover = new GUIStyleState { textColor = invalidTextColor },
            focused = new GUIStyleState { textColor = invalidTextColor }
        };

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Use property scope to enable prefab override styling
            using (new EditorGUI.PropertyScope(position, label, property))
            {
                // Get all tags and check if current value is valid
                string[] tags = UnityEditorInternal.InternalEditorUtility.tags;
                bool isTagValid = tags.Contains(property.stringValue);

                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    // Draw native Tag Field, with invalid style if tag is, else default style
                    string newTag = EditorGUI.TagField(position, label, property.stringValue,
                        isTagValid ? EditorStyles.popup : invalidTagGUIStyle);

                    if (check.changed)
                    {
                        // Only update tag on actual change to avoid unwanted value uniformization
                        // during multi-select
                        property.stringValue = newTag;
                    }
                }
            }
        }
    }
}
