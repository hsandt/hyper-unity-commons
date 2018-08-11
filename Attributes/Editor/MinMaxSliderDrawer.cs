// https://gist.github.com/frarees/9791517
using UnityEngine;
using UnityEditor;

namespace Commons.Helper
{

	[CustomPropertyDrawer (typeof (MinMaxSliderAttribute))]
	class MinMaxSliderDrawer : PropertyDrawer {

		public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {

			if (property.propertyType == SerializedPropertyType.Vector2) {
				Vector2 range = property.vector2Value;
				float min = range.x;
				float max = range.y;
				MinMaxSliderAttribute attr = attribute as MinMaxSliderAttribute;
				EditorGUI.BeginChangeCheck ();
				EditorGUI.MinMaxSlider (position, label, ref min, ref max, attr.min, attr.max);
				if (EditorGUI.EndChangeCheck ()) {
					range.x = min;
					range.y = max;
					property.vector2Value = range;
				}
				position.y += 20;
				EditorGUI.LabelField(position, "Min Val:", min.ToString());  // ADDED
				position.y += 20;
				EditorGUI.LabelField(position, "Max Val:", max.ToString());  // ADDED

			} else {
				EditorGUI.LabelField (position, label, "Use only with Vector2");
			}
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
		    var extraHeight = 2 * 20f;
		    return base.GetPropertyHeight(property, label) + extraHeight;
		}

	}

}

