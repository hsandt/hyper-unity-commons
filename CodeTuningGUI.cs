using System;
using UnityEngine;

namespace CommonsDebug
{

	/// Code Tuning GUI
	/// NETWORK: make it a NetworkBehaviour and synchronize the tuned values
	/// so that tuned values at the same everywhere.
	/// Currently, you need to change the values on the Host to affect Command functions
	public class CodeTuningGUI : MonoBehaviour
	{

		void OnGUI () {
			GUILayout.BeginArea(new Rect(500, 50, 500, 200));

			GUILayout.BeginVertical();

			ToggleEntry("active", ref CodeTuning.Instance.active);

			if (CodeTuning.Instance.active)
			{
				IntEntry("branch index", ref CodeTuning.Instance.branchIndex);
				ToggleEntry("bool 1", ref CodeTuning.Instance.bool1);
				ToggleEntry("bool 2", ref CodeTuning.Instance.bool2);
				IntEntry("int 1", ref CodeTuning.Instance.int1);
				IntEntry("int 2", ref CodeTuning.Instance.int2);
				IntEntry("int 3", ref CodeTuning.Instance.int3);
				FloatEntry("float 1", ref CodeTuning.Instance.float1);
				FloatEntry("float 2", ref CodeTuning.Instance.float2);
				FloatEntry("float 3", ref CodeTuning.Instance.float3);
			}

			GUILayout.EndVertical();

			GUILayout.EndArea();
		}

		void ToggleEntry (string name, ref bool value)
		{
	 		GUILayout.BeginHorizontal();

			GUILayout.Label(name, GUILayout.Width(120f));
			// we leave the label on the right as empty as we already add a label on the left
			value = GUILayout.Toggle(value, "");

			GUILayout.EndHorizontal();
		}

		void IntEntry (string name, ref int value)
		{
			GUILayout.BeginHorizontal();

			GUILayout.Label (name, GUILayout.Width(120f));
			value = (int) Mathf.Floor(GUILayout.HorizontalSlider ((float) value, 0.0f, 10.0f, GUILayout.Width(120f)));
			GUILayout.Space(10f);
			GUILayout.Label (value.ToString());

			GUILayout.EndHorizontal();
		}

		void FloatEntry (string name, ref float value)
		{
			GUILayout.BeginHorizontal();

			GUILayout.Label (name, GUILayout.Width(120f));
			value = GUILayout.HorizontalSlider (value, 0.0f, 10.0f, GUILayout.Width(120f));
			GUILayout.Space(10f);
			GUILayout.Label(value.ToString());

			GUILayout.EndHorizontal();
		}

	}

}
