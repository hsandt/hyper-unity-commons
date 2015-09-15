using UnityEngine;
using UnityEditor;
using System.Reflection;

public static class FaceCamera
{

	[MenuItem( "GameObject/Align Rotation with Camera" )]
	static void AlignRotationWithCamera()
	{
		Camera camera = Camera.main;
		if (camera != null) {
			foreach (GameObject go in Selection.gameObjects) {
				Undo.RecordObject (go.transform, "Face Camera");
				// use Vector3.forward if the object's Z points toward the back of the screen (e.g. sprites), else Vector3.back
				go.transform.LookAt(go.transform.position + camera.transform.rotation * Vector3.forward, camera.transform.rotation * Vector3.up);
			}
		}
		else {
			Debug.LogWarning("No main camera found, cannot align game object rotation with camera.");
		}
	}

}
