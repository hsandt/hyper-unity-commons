// original: http://answers.unity3d.com/questions/148812/is-there-a-toggle-for-snap-to-grid.html
// Opens a utility window where you can enable auto positional snapping with a custom grid interval
// Currently, the grid interval is uniform on XYZ
// We used to be able to snap rotation but it was unreliable as too small changes in rotation
// would be ignored and in mixed rotations, rounding rotations on individual axes would cause problems.

using UnityEngine;
using UnityEditor;

using CommonsHelper;

namespace CommonsEditor
{
	public class AutoSnap : EditorWindow
	{
		// singleton window instance
		static AutoSnap window;

		private Vector3 prevPosition = Vector3.zero;  // this is always a point on the grid, so won't prevent initial snap
		private bool doSnap = false;
		private float snapValue = 1f;

		[MenuItem("Edit/Auto Snap %l")]
		private static void Init()
		{
			window = GetWindowWithRect<AutoSnap>(new Rect (100, 100, 275, 60), utility: true, title: "Auto Snap");

			if (EditorPrefs.HasKey("AutoSnap.doSnap"))
				window.doSnap = EditorPrefs.GetBool("AutoSnap.doSnap");
			if (EditorPrefs.HasKey("AutoSnap.snapValue"))
				window.snapValue = EditorPrefs.GetFloat("AutoSnap.snapValue");
			
			window.Focus();
		}

		private void OnGUI()
		{
			doSnap = EditorGUILayout.Toggle("Auto Snap", doSnap);
			snapValue = EditorGUILayout.Slider("Snap Value", snapValue, 0f, 10f);
			if (GUILayout.Button("Save")) Save();
		}

		private void Save() {
			EditorPrefs.SetBool("AutoSnap.doSnap", doSnap);
			EditorPrefs.SetFloat("AutoSnap.snapValue", snapValue);
		}

		private void SnapUpdate()
		{
			// Snap selected parents (children will not be snapped individually)
			// this is disabled during Play
			if (doSnap && snapValue > 0
				&& !EditorApplication.isPlaying
				&& Selection.transforms.Length > 0
				&& Selection.transforms[0].position != prevPosition)
			{
				Snap();
				prevPosition = Selection.transforms[0].position;
			}
		}

		// When windows is opened, let editor snap regularly (100 FPS)
		private void OnEnable() { EditorApplication.update += SnapUpdate; }

		private void OnDisable() { EditorApplication.update -= SnapUpdate; }

		private void Snap()
		{
			foreach (Transform transform in Selection.transforms)
			{
				// Undo in case you mis-snapped an object, but remember to disable Auto Snap before undoing or it will
				// resnap immediately!
				Undo.RecordObject(transform, "Auto-snap position");
				
				transform.localPosition = VectorUtil.RoundVector3(transform.localPosition, snapValue);
			}
		}
	}
}
