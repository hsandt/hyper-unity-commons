// From HierarchyGeneration.cs found online, but removed the last 2 methods that were added natively to Unity

using UnityEngine;
using UnityEditor;

namespace HyperUnityCommons.Editor
{
	public class HierarchyGeneration : MonoBehaviour
	{
	    [MenuItem("GameObject/Create Empty Sibling #&d", priority = 0)]
	    static void CreateEmptySibling()
	    {
	        GameObject go = new GameObject("GameObject");

	        if (Selection.activeTransform != null)
	        {
		        // Move object under parent with normal transform local values
	            go.transform.SetParent(Selection.activeTransform.parent, true);
	            go.transform.localPosition = Vector3.zero;
	            go.transform.localRotation = Quaternion.identity;
	            go.transform.localScale = Vector3.one;

	            Undo.RegisterCreatedObjectUndo(go, "Create Empty Sibling");
	        }
	    }
	}
}

