/// From HierarchyGeneration.cs found online, but removed the last 2 methods that were added natively to Unity

using UnityEngine;
using UnityEditor;

namespace CommonsHelper.Editor
{

	public class HierarchyGeneration : MonoBehaviour {

	    [MenuItem("GameObject/Create Empty Parent #&e")]
	    static void CreateEmptyParent() {

	        GameObject go = new GameObject("GameObject");

	        if (Selection.activeTransform != null)
	        {
	            go.transform.parent = Selection.activeTransform.parent;
	            go.transform.Translate(Selection.activeTransform.position);
	            Selection.activeTransform.parent = go.transform;
	        }

	    }

	    [MenuItem("GameObject/Create Empty Duplicate #&d")]
	    static void CreateEmptySibling() {

	        GameObject go = new GameObject("GameObject");

	        if (Selection.activeTransform != null)
	        {
	            go.transform.parent = Selection.activeTransform.parent;
	            go.transform.Translate(Selection.activeTransform.position);
	        }

	    }

	}

}

