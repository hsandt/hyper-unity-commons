using UnityEngine;
using UnityEditor;
using Vexe.Editor.Drawers;

// class MinMaxSliderVexeDrawer : AttributeDrawer<Vector2, MinMaxSliderVexeAttribute> {

// 	public override void OnGUI () {
// 		Vector2 range = memberValue;
// 		float min = range.x;
// 		float max = range.y;
// 		gui.Prefix(displayText);
// 		// gui.LabelField( "Min Val:", min.ToString());  // ADDED
// 		// gui.LabelField( "Max Val:", max.ToString());  // ADDED
// 		gui.BeginCheck ();
// 		gui.MinMaxSlider (displayText, ref min, ref max, attribute.min, attribute.max);
// 		if (gui.HasChanged ()) {
// 			range.x = min;
// 			range.y = max;
// 			memberValue = range;
// 		}
// 	}

// }
