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

namespace CommonsEditor
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

                if (currentPrefabStage != null && selectedGameObject == currentPrefabStage.prefabContentsRoot)
                {
                    // Prefab root is selected in Prefab Stage
                    // In a prefab, we cannot duplicate the root under itself, since it must have no siblings,
                    // so skip it. And since Selection.transforms ignores children, we know it's the only selection,
                    // so just break.
                    break;
                }

                // Get new game object name following Project Settings > Editor > Numbering Scheme
                string newGameObjectName = GameObjectUtility.GetUniqueNameForSibling(selectedTransform.parent,
                    selectedGameObject.name);

                GameObject clone;

                // Support prefab instances
                GameObject prefab = PrefabUtility.GetCorrespondingObjectFromSource(selectedGameObject);
                if (prefab != null)
                {
                    // Create another prefab instance under same parent as duplicated object
                    // The stage will be set later with PlaceGameObjectInCurrentStage, the parent can still be set now.
                    // If working on the Main Stage, the scene will also be set later with MoveGameObjectToScene,
                    // so don't bother passing the Scene instead of the parent Transform when selectedTransform.parent
                    // is null.
                    clone = (GameObject)PrefabUtility.InstantiatePrefab(prefab, selectedTransform.parent);
                    PrefabUtility.SetPropertyModifications(clone, PrefabUtility.GetPropertyModifications(selectedGameObject));
                }
                else
                {
                    // The duplicated object is not a prefab, create a standard clone under the same parent
                    clone = Object.Instantiate(selectedGameObject, selectedTransform.parent);
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

                // Move clone right under selected object
                // This works even with multiple selection under the same parent as GetSiblingIndex is reevaluated
                // after the last child insertion
                clone.transform.SetSiblingIndex(selectedTransform.GetSiblingIndex() + 1);

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