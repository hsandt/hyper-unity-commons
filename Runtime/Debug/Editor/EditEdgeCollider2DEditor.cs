﻿// From http://stackoverflow.com/questions/29819697/manually-edit-unity3d-collider-coordinates
// Raphael Marques
// Adapted by Long Nguyen Huu (hsandt)
// +/- buttons code inspired by Vectrosity by Starscene Software

using UnityEngine;
using UnityEditor;
using System.Collections;

namespace HyperUnityCommons.Editor
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(EditEdgeCollider2D))]
	public class EditEdgeCollider2DEditor : UnityEditor.Editor {

		public override void OnInspectorGUI() {
			DrawDefaultInspector();

			if (GUILayout.Button("Round all coordinates to 1/16 px"))
			{
				var script = (EditEdgeCollider2D) target;
				EdgeCollider2D collider = script.GetComponent<EdgeCollider2D>();

				if (collider != null)
				{
					Undo.RecordObject(collider, "Snap edge collider 2D coordinates to 1/16 px");

					// .points return a temporary array copy, so we can work on it,
					// but we must re-assign it to collider.points at the end
					Vector2[] points = collider.points;

					for (int i = 0; i < points.Length; i++)
					{
						points[i] = VectorUtil.RoundVector2(points[i], 1f/16f);
					}

					collider.points = points;
				}
			}

			/*
			 * This custom inspector is now obsolete in Unity 5.4 where coordinates can be manually edited in the main component, in Normal view
			 * I may restore this code if I add something to make it better than the native Unity coordinate editor, such as +/- buttons to insert and remove points

			// Assign the edge collider component reference directly instead of depending on a member variable of EditEdgeCollider2D storing
			// the component reference in its Awake(), since Awake may not be called before this method in the editor
			collider = ((EditEdgeCollider2D) target).GetComponent<EdgeCollider2D>();

			if (collider != null) {

				var points = collider.points;
				for (int i = 0; i < points.Length; i++) {
					EditorGUILayout.BeginHorizontal();

					points[i] = UnityEditor.EditorGUILayout.Vector2Field(i.ToString(), points[i]);
					if (points[i] != collider.points[i]) {
						Debug.LogFormat("points[i]: {0}", points[i]);
						// collider.points[i] = points[i];  // does not work
						collider.points = points;
						UpdateColliderPoints();
					}

					// does not always work
					// if (GUILayout.Button("+", EditorStyles.miniButton, GUILayout.Width(40))) {
					// 	InsertPoint(i, points[i]);  // insert a copy of the previous vector
					// }

					// GUILayout.FlexibleSpace();

					EditorGUILayout.EndHorizontal();
				}

			}

			*/
		}

		// void InsertPoint (int i, Vector2 point) {
		// 	var pointsList = collider.points.ToList<Vector2>();
		// 	pointsList.Insert(i, point);
		// 	collider.points = pointsList.ToArray<Vector2>();
		// 	UpdateColliderPoints();
		// }

		// void UpdateColliderPoints () {
		// 	EditorUtility.SetDirty(collider);
		// }

	}
}
