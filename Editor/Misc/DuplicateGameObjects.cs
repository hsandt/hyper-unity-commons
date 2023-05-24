// References:
// - ReplaceGameObjects.cs in same folder for prefab linking and support for additive scenes
// - https://answers.unity.com/questions/168580/how-do-i-properly-duplicate-an-object-in-a-editor.html
//   (Anisoropos's answer)

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HyperUnityCommons.Editor
{
    public static class DuplicateGameObjects
    {
        [MenuItem("Edit/Duplicate Below %#d", priority = 120)]
        private static void DuplicateSelectionBelow()
        {
            PrefabStage currentPrefabStage = null;

            // Check current stage
            Stage currentStage = StageUtility.GetCurrentStage();
            if (currentStage is PrefabStage prefabStage)
            {
                // Prefab edit mode
                currentPrefabStage = prefabStage;
            }
            else if (currentStage is not MainStage)
            {
                // Not Prefab edit mode nor Main scene, must be a custom stage
                Debug.LogWarning("Duplicate Below is not supported in custom stages");
                return;
            }

            var clones = new List<GameObject>();

            // Note that Selection.transforms, unlike Selection.objects, only keeps the top-most parent
            // if you selected both a parent and a direct or indirect child under it.
            // But since duplicating children modifies the parent content, it means that we would get different results
            // when duplicating the parent or the child first, so it's more stable to just ignore children.
            // "Prefab Mode in Context" pseudo-game-object is also ignored by Selection.transforms,
            // unlike Selection.objects, so it's safer on that side too.
            foreach (Transform selectedTransform in Selection.transforms)
            {
                GameObject selectedGameObject = selectedTransform.gameObject;

                Transform cloneParent;
                int cloneSiblingIndex;

                if (currentPrefabStage != null && selectedGameObject == currentPrefabStage.prefabContentsRoot)
                {
                    // We are selecting Prefab root in Prefab Stage (Prefab Edit Mode)
                    // Selection.transforms ignores children, so, if this is the case, we know that we are only iterating
                    // on that Prefab root. However, during prefab editing, we cannot duplicate the root itself to get
                    // a copy under the same parent (the context), since the prefab root must have no siblings.
                    // So, if this happens, just like native Duplicate, create the clone under the prefab root itself
                    // (selectedTransform), effectively duplicating the prefab root as its own child.
                    // However, while Duplicate will place it as last child, Duplicate Below will place it as
                    // first child (0).
                    // IMPORTANT: this will not create a prefab linked instance, but a normal game object, exactly
                    // like native Duplicate. And this is what we want, because otherwise, it would create a cyclic
                    // dependency from a prefab to itself.
                    cloneParent = selectedTransform;
                    cloneSiblingIndex = 0;
                }
                else
                {
                    // In normal cases, place the clone under the same parent, as next sibling
                    cloneParent = selectedTransform.parent;
                    cloneSiblingIndex = selectedTransform.GetSiblingIndex() + 1;
                }

                // Get new game object name following Project Settings > Editor > Numbering Scheme
                string newGameObjectName = GameObjectUtility.GetUniqueNameForSibling(selectedTransform.parent,
                    selectedGameObject.name);

                GameObject clone;

                // Support prefab instances:
                // (same as ReplaceGameObjects)
                // check if selected game object is an actual prefab instance root in the Scene (from model, regular or variant prefab)
                // use IsAnyPrefabInstanceRoot to make sure it is a prefab root (including a prefab instance root parented to
                // another prefab instance), and not a non-prefab object parented to a prefab instance
                if (PrefabUtility.GetPrefabInstanceStatus(selectedGameObject) == PrefabInstanceStatus.Connected &&
                    PrefabUtility.IsAnyPrefabInstanceRoot(selectedGameObject))
                {
                    // (similar to ReplaceGameObjects)
                    // Make sure to get the actual prefab for this game object, by using GetPrefabAssetPathOfNearestInstanceRoot.
                    // This will work for both outermost and inner prefab roots.
                    // Other methods like GetCorrespondingObjectFromSource will return the outermost prefab only,
                    // which, in the case of duplication, will not only duplicate the wrong object, but also place it
                    // on the wrong parent (maybe to avoid some cyclic dependency although in prefab instances, this
                    // only becomes a problem when applying overrides)
                    string prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(selectedGameObject);
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

                    // Create another prefab instance under same parent as duplicated object, with same properties override
                    // The stage will be set later with PlaceGameObjectInCurrentStage, the parent can still be set now.
                    // If working on the Main Stage, the scene will also be set later with MoveGameObjectToScene,
                    // so don't bother passing the Scene instead of the parent Transform when selectedTransform.parent
                    // is null.
                    clone = (GameObject)PrefabUtility.InstantiatePrefab(prefab, cloneParent);
                    PrefabUtility.SetPropertyModifications(clone, PrefabUtility.GetPropertyModifications(selectedGameObject));
                }
                else
                {
                    // The duplicated object is not a prefab, create a standard clone under the same parent
                    clone = Object.Instantiate(selectedGameObject, cloneParent);
                }

                // Place clone in current stage
                // This only matters when editing in Prefab Mode
                StageUtility.PlaceGameObjectInCurrentStage(clone);

                // When selected object is at scene root (only possible in Main Stage, not Prefab Stage,
                // since we do nothing in case root is selected, but checking it just in case),
                // it will be placed in the active scene by default, so move it to the same scene as the selected object
                if (currentPrefabStage == null && selectedTransform.parent == null)
                {
                    SceneManager.MoveGameObjectToScene(clone, selectedTransform.gameObject.scene);
                }

                // TODO: if contiguous selection, clone everything below, not intertwined

                // Move clone right under selected object
                // This works even with multiple selection under the same parent as GetSiblingIndex is reevaluated
                // after the last child insertion
                clone.transform.SetSiblingIndex(cloneSiblingIndex);

                // Rename clone with Numbering Scheme
                clone.name = newGameObjectName;

                Undo.RegisterCreatedObjectUndo(clone, "Duplicate below");

                clones.Add(clone);
            }

            // Select new objects, if any
            if (clones.Count > 0)
            {
                Selection.objects = clones.Cast<Object>().ToArray();
            }
        }
    }
}