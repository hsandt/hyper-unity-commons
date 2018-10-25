// original: http://answers.unity3d.com/questions/148812/is-there-a-toggle-for-snap-to-grid.html

using UnityEngine;
using UnityEditor;
// using System.Reflection;

namespace CommonsEditor
{

	public class AutoSnap : EditorWindow
	{
		// singleton window instance
		static AutoSnap window;

		Vector3 prevPosition;
		Vector3 prevRotation;
		bool doSnap = false;
		// bool doRotateSnap = false;
		float snapValue = 1f;
		// float snapRotateValue = 45f;

		[MenuItem("Edit/Auto Snap %l")]
		static void Init()
		{
			// window = GetWindow<AutoSnap>();
			window = GetWindowWithRect<AutoSnap>(new Rect (100, 100, 275, 60), utility: true, title: "Auto Snap");
			// if (window == null) {
			// 	window = ScriptableObject.CreateInstance<AutoSnap>();
			// 	Vector2 windowSize = new Vector2(275, 100);
			// 	window.minSize = windowSize;
			// 	window.maxSize = windowSize;
			// }

			if (EditorPrefs.HasKey("AutoSnap.doSnap"))
				window.doSnap = EditorPrefs.GetBool("AutoSnap.doSnap");
			if (EditorPrefs.HasKey("AutoSnap.snapValue"))
				window.snapValue = EditorPrefs.GetFloat("AutoSnap.snapValue");
			// if (EditorPrefs.HasKey("AutoSnap.doRotateSnap"))
			// 	window.doRotateSnap = EditorPrefs.GetBool("AutoSnap.doRotateSnap");
			// if (EditorPrefs.HasKey("AutoSnap.snapRotateValue"))
			// 	window.snapRotateValue = EditorPrefs.GetFloat("AutoSnap.snapRotateValue");

			// window.Show();
			// window.ShowUtility();
			window.Focus();
		}

		void OnGUI()
		{
			doSnap = EditorGUILayout.Toggle("Auto Snap", doSnap);
			// doRotateSnap = EditorGUILayout.Toggle ("Auto Snap Rotation", doRotateSnap);
			snapValue = EditorGUILayout.Slider("Snap Value", snapValue, 0f, 10f);
			// snapRotateValue = EditorGUILayout.FloatField ("Rotation Snap Value", snapRotateValue);
			if (GUILayout.Button("Save")) Save();
		}

		void Save() {
			// Debug.LogFormat("Save by {0}", GetInstanceID());
			EditorPrefs.SetBool("AutoSnap.doSnap", doSnap);
			EditorPrefs.SetFloat("AutoSnap.snapValue", snapValue);
			// EditorPrefs.SetBool("AutoSnap.doRotateSnap", doRotateSnap);
			// EditorPrefs.SetFloat("AutoSnap.snapRotateValue", snapRotateValue);
		}

		void SnapUpdate()
		{
			// Snap selected parents (children will not be snapped individually)
			// Debug.LogFormat("SnapUpdate by {0}", GetInstanceID());
			// Debug.LogFormat("doSnap: {0}", doSnap);
			if (doSnap && snapValue > 0
				&& !EditorApplication.isPlaying
				&& Selection.transforms.Length > 0
				&& Selection.transforms[0].position != prevPosition)
			{
				Snap();
				prevPosition = Selection.transforms[0].position;
			}

			/*
			// rotation snap is not reliable, too small changes in rotation
			//   will be ignored
			// in mixed rotations, rounding rotations on individual axes
			//   cause problems too
			if (doRotateSnap && snapRotateValue > 0
				&& !EditorApplication.isPlaying
				&& Selection.transforms.Length > 0
				&& Selection.transforms[0].eulerAngles != prevRotation)
			{
				RotateSnap();
				prevRotation = Selection.transforms[0].eulerAngles;
	                 //Debug.Log("Value of rotation " + Selection.transforms[0].rotation);
	                 //Debug.Log ("Value of old Rotation " + prevRotation);
			}
			*/
		}

		// When windows is opened, let editor snap regularly (100 FPS)
		void OnEnable() { EditorApplication.update += SnapUpdate; }

		void OnDisable() { EditorApplication.update -= SnapUpdate; }

		void Snap()
		{
			foreach (Transform transform in Selection.transforms)
			{
				// Debug.Log(string.Format("Snapping {0}", transform));
				Vector3 pos = transform.localPosition;
				pos.x = Round(pos.x);
				pos.y = Round(pos.y);
				pos.z = Round(pos.z);
				transform.localPosition = pos;
			}
		}

		// void RotateSnap()
		// {
		// 	foreach (Transform transform in Selection.transforms)
		// 	{
		// 		Vector3 rot = transform.eulerAngles;
		// 		rot.x = RotRound(rot.x);
		// 		rot.y = RotRound(rot.y);
		// 		rot.z = RotRound(rot.z);
		// 		transform.eulerAngles = rot;
		// 	}
		// }

		float Round(float input)
		{
			return snapValue * Mathf.Round(input / snapValue);
		}

		// float RotRound(float input)
		// {
		// 	// Debug.Log("The division is: " + (input / snapRotateValue) );
		// 	// Debug.Log("The rounding is: " + Mathf.Round(input / snapRotateValue) );
		// 	// Debug.Log("The return is: " + (snapRotateValue * Mathf.Round(input / snapRotateValue)) );
		// 	return snapRotateValue * Mathf.Round(input / snapRotateValue);
		// }
	}

}
