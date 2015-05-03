// original: http://answers.unity3d.com/questions/148812/is-there-a-toggle-for-snap-to-grid.html

using UnityEngine;
using UnityEditor;

public class AutoSnap : EditorWindow
{
	Vector3 prevPosition;
	Vector3 prevRotation;
	bool doSnap = true;
	bool doRotateSnap = true;
	float snapValue = 1f;
	float snapRotateValue = 1f;

	[MenuItem( "Edit/Auto Snap %l" )]
	static void Init()
	{
		AutoSnap window = (AutoSnap) EditorWindow.GetWindow(typeof(AutoSnap));
		window.maxSize = new Vector2(200, 100);
	}

	void OnGUI()
	{
		doSnap = EditorGUILayout.Toggle("Auto Snap", doSnap);
		doRotateSnap = EditorGUILayout.Toggle ("Auto Snap Rotation", doRotateSnap);
		snapValue = EditorGUILayout.FloatField("Snap Value", snapValue);
		snapRotateValue = EditorGUILayout.FloatField ("Rotation Snap Value", snapRotateValue);
	}

	void SnapUpdate()
	{
		// Snap selected parents (children will not be snapped individually)
		// Debug.Log("SnapUpdate");
		if (doSnap
			&& !EditorApplication.isPlaying
			&& Selection.transforms.Length > 0
			&& Selection.transforms[0].position != prevPosition)
		{
			Snap();
			prevPosition = Selection.transforms[0].position;
		}

		// rotation snap is not reliable, too small changes in rotation
		//   will be ignored
		// in mixed rotations, rounding rotations on individual axes
		//   cause problems too
		if (doRotateSnap
			&& !EditorApplication.isPlaying
			&& Selection.transforms.Length > 0
			&& Selection.transforms[0].eulerAngles != prevRotation)
		{
			RotateSnap();
			prevRotation = Selection.transforms[0].eulerAngles;
                 //Debug.Log("Value of rotation " + Selection.transforms[0].rotation);
                 //Debug.Log ("Value of old Rotation " + prevRotation);
		}
	}

	// When windows is opened, let editor snap regularly (100 FPS)
	// do not remove the event in OnDisable() if you want snapping to
	//   be applied even when the window is closed
	void OnEnable() { EditorApplication.update += SnapUpdate; }

	void OnSceneGUI() {
		Debug.Log("OnSceneGUI");
	}

	void OnDrawGizmos() {
		Debug.Log("OnDrawGizmos");
	}

	void Snap()
	{
		foreach (Transform transform in Selection.transforms)
		{
			// Debug.Log(string.Format("Snapping {0}", transform));
			Vector3 pos = transform.position;
			pos.x = Round(pos.x);
			pos.y = Round(pos.y);
			pos.z = Round(pos.z);
			transform.position = pos;
		}
	}

	void RotateSnap()
	{
		foreach (Transform transform in Selection.transforms)
		{
			Vector3 rot = transform.eulerAngles;
			rot.x = RotRound(rot.x);
			rot.y = RotRound(rot.y);
			rot.z = RotRound(rot.z);
			transform.eulerAngles = rot;
		}
	}

	float Round(float input)
	{
		return snapValue * Mathf.Round(input / snapValue);
	}

	float RotRound(float input)
	{
		// Debug.Log("The division is: " + (input / snapRotateValue) );
		// Debug.Log("The rounding is: " + Mathf.Round(input / snapRotateValue) );
		// Debug.Log("The return is: " + (snapRotateValue * Mathf.Round(input / snapRotateValue)) );
		return snapRotateValue * Mathf.Round(input / snapRotateValue);
	}
}
