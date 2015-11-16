// https://gist.github.com/frarees/9791517
using UnityEngine;
using UnityEditor;
using Vexe.Editor.Drawers;

[CustomPropertyDrawer (typeof (MinMaxSliderAttribute))]
class MinMaxSliderDrawer : PropertyDrawer {

	public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {

		if (property.propertyType == SerializedPropertyType.Vector2) {
			Vector2 range = property.vector2Value;
			float min = range.x;
			float max = range.y;
			EditorGUI.LabelField(position, "Min Val:", min.ToString());  // ADDED
			EditorGUI.LabelField(new Rect(position.x,position.y+20,position.width,position.height), "Max Val:", max.ToString());  // ADDED
			MinMaxSliderAttribute attr = attribute as MinMaxSliderAttribute;
			EditorGUI.BeginChangeCheck ();
			EditorGUI.MinMaxSlider (label, new Rect(position.x,position.y+40,position.width,position.height), ref min, ref max, attr.min, attr.max);
			if (EditorGUI.EndChangeCheck ()) {
				range.x = min;
				range.y = max;
				property.vector2Value = range;
			}
		} else {
			EditorGUI.LabelField (position, label, "Use only with Vector2");
		}
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
	    var extraHeight = 40;
	    return base.GetPropertyHeight(property, label) + extraHeight;
	}

}
