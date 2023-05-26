using System.Collections;
using System.Collections.Generic;
using HyperUnityCommons;
using UnityEngine;

/// Place a Persistent Managers Generator game object with this component on each scene
/// It should not be DontDestroyOnLoad itself. It will spawn managers that work across all scenes.
/// Some really need to live across scenes to convey information (e.g. a Session Manager),
/// others are simply needed by every scene and it's more convenient to generate them once and for all than place them
/// manually in every scene, although they don't convey information across scenes (e.g. a Constants Manager).
/// SEO: before non-persistent managers (that may use the instantiated persistent managers)
/// Indeed, each persistent manager will call Init right after creation from this generator, so the SEO of this script
/// is more important than the one of each persistent manager.
/// That said, we also recommend to set this script's SEO before the persistent managers themselves so that if some
/// persistent managers are incorrectly placed in the scene, the scene managers will be the ones to self-destruct
/// with a warning.
/// Multi-scene: if working with multiple scenes (Active + Additive ones), scene order takes priority over SEO.
/// The active scene's objects' Awake methods will be called before other scenes' objects' Awake methods,
/// so make sure to place the PersistentManagersGenerator on the active scene.
/// (SEO still matters to order execution between objects in the same scene)
public class PersistentManagersGenerator : MonoBehaviour
{
    [Header("Assets")]

    [Tooltip("List of manager prefabs to instantiate on Awake, if not already created")]
    public List<GameObject> managerPrefabs;


    private void Awake()
    {
        DebugUtil.AssertListElementsNotNull(managerPrefabs, this, nameof(managerPrefabs));

        // Only instantiate manager prefabs if not already done
        // This allows us to place a PersistentManagersGenerator in every scene, so we can play from any scene in the
        // editor, and the scene we play from will instantiate all persistent managers; while further scenes won't
        // instantiate any more of them.
        // If done correctly, persistent managers are truly the "global singletons" you'd find in other engines.
        if (PersistentManagersCreationToken.Instance == null)
        {
            // Create each persistent manager and flag it DontDestroyOnLoad so it's actually persistent across scenes
            foreach (GameObject managerPrefab in managerPrefabs)
            {
                DontDestroyOnLoad(Instantiate(managerPrefab));
            }

            // Instantiate a creation token so we remember not to re-instantiate those managers in next scene
            PlaceCreationToken();
        }
    }

    private void PlaceCreationToken()
    {
        DontDestroyOnLoad(new GameObject("PersistentManagersCreationToken",
            typeof(PersistentManagersCreationToken)));
    }
}
