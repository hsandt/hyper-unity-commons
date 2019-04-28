// An Editor script to move anchors and resize rect boundaries at the same time
// Source: https://answers.unity.com/questions/1100603/how-to-make-anchor-snap-to-self-rect-transform-in.html

// Credits
// Phedg1: original script
// stephane.lallee: combined component and editor tool into one script
// hsandt: added toggle to enable sticking anchors to rect only when wanted, and button to immediately stick anchors

using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class AnchorToolsEditor : EditorWindow
{
    /// When true make the anchors match the rect boundaries after a rect resize
    private bool stickAnchorsToRect = false;
        
    AnchorToolsEditor()
    {
        Debug.Log("[AnchorToolsEditor] Registering for anchors update On Scene GUI");
        SceneView.onSceneGUIDelegate += OnScene;
    }
    
    [MenuItem("Tools/Anchor Tools")]
    static void Init()
    {
        AnchorToolsEditor editorScreenshot = GetWindow<AnchorToolsEditor>(title: "Anchor Tools");

        if (EditorPrefs.HasKey("AnchorToolsEditor.screenshotFolderPath"))
            editorScreenshot.stickAnchorsToRect = EditorPrefs.GetBool("AnchorToolsEditor.stickAnchorsToRect");
    }
    
    void OnGUI()
    {
        EditorGUI.BeginChangeCheck();

        stickAnchorsToRect = EditorGUILayout.Toggle("Stick Anchors to Rect", stickAnchorsToRect);

        if (EditorGUI.EndChangeCheck()) {
            EditorPrefs.SetBool("AnchorToolsEditor.stickAnchorsToRect", stickAnchorsToRect);
        }

        if (GUILayout.Button("Stick Anchors to Rect")) UpdateAnchors();
    }

    private void OnScene(SceneView sceneView)
    {
        // detect mouse up button as a resize event; this is not accurate as other actions may be used,
        // and we may modify the rect by inputting values with the keyboard, but works for quick usage
        if (stickAnchorsToRect && Event.current.type == EventType.MouseUp && Event.current.button == 0)
        {
            UpdateAnchors();
        }
    }

    public void OnDestroy()
    {
        Debug.Log("[AnchorToolsEditor] Unregistering for anchors update On Scene GUI");
        SceneView.onSceneGUIDelegate -= OnScene;
    }

    static public Rect anchorRect;
    static public Vector2 anchorVector;
    private static Rect anchorRectOld;
    private static Vector2 anchorVectorOld;
    private static RectTransform currentRectTransform;
    private static RectTransform parentRectTransform;
    private static Vector2 pivotOld;
    private static Vector2 offsetMinOld;
    private static Vector2 offsetMaxOld;

    private static void UpdateAnchors()
    {
        TryToGetRectTransform();
        if (currentRectTransform != null && parentRectTransform != null && ShouldStick())
        {
            Stick();
        }
    }

    private static bool ShouldStick()
    {
        return (
            currentRectTransform.offsetMin != offsetMinOld ||
            currentRectTransform.offsetMax != offsetMaxOld ||
            currentRectTransform.pivot != pivotOld ||
            anchorVector != anchorVectorOld ||
            anchorRect != anchorRectOld
            );
    }

    private static void Stick()
    {
        CalculateCurrentWH();
        CalculateCurrentXY();

        CalculateCurrentXY();
        pivotOld = currentRectTransform.pivot;
        anchorVectorOld = anchorVector;

        AnchorsToCorners();
        anchorRectOld = anchorRect;

        EditorUtility.SetDirty(currentRectTransform.gameObject);
    }

    private static void TryToGetRectTransform()
    {
        if (Selection.activeGameObject != null)
        {
            currentRectTransform = Selection.activeGameObject.GetComponent<RectTransform>();
            if (currentRectTransform != null && currentRectTransform.parent != null)
            {
                parentRectTransform = currentRectTransform.parent.GetComponent<RectTransform>();
            }
            else
            {
                parentRectTransform = null;
            }
        }
        else
        {
            currentRectTransform = null;
            parentRectTransform = null;
        }
    }

    private static void CalculateCurrentXY()
    {
        float pivotX = anchorRect.width * currentRectTransform.pivot.x;
        float pivotY = anchorRect.height * (1 - currentRectTransform.pivot.y);
        Vector2 newXY = new Vector2(currentRectTransform.anchorMin.x * parentRectTransform.rect.width + currentRectTransform.offsetMin.x + pivotX - parentRectTransform.rect.width * anchorVector.x,
                                  -(1 - currentRectTransform.anchorMax.y) * parentRectTransform.rect.height + currentRectTransform.offsetMax.y - pivotY + parentRectTransform.rect.height * (1 - anchorVector.y));
        anchorRect.x = newXY.x;
        anchorRect.y = newXY.y;
        anchorRectOld = anchorRect;
    }

    private static void CalculateCurrentWH()
    {
        anchorRect.width = currentRectTransform.rect.width;
        anchorRect.height = currentRectTransform.rect.height;
        anchorRectOld = anchorRect;
    }

    private static void AnchorsToCorners()
    {
        float pivotX = anchorRect.width * currentRectTransform.pivot.x;
        float pivotY = anchorRect.height * (1 - currentRectTransform.pivot.y);
        currentRectTransform.anchorMin = new Vector2(0f, 1f);
        currentRectTransform.anchorMax = new Vector2(0f, 1f);
        currentRectTransform.offsetMin = new Vector2(anchorRect.x / currentRectTransform.localScale.x, anchorRect.y / currentRectTransform.localScale.y - anchorRect.height);
        currentRectTransform.offsetMax = new Vector2(anchorRect.x / currentRectTransform.localScale.x + anchorRect.width, anchorRect.y / currentRectTransform.localScale.y);
        currentRectTransform.anchorMin = new Vector2(currentRectTransform.anchorMin.x + anchorVector.x + (currentRectTransform.offsetMin.x - pivotX) / parentRectTransform.rect.width * currentRectTransform.localScale.x,
                                                 currentRectTransform.anchorMin.y - (1 - anchorVector.y) + (currentRectTransform.offsetMin.y + pivotY) / parentRectTransform.rect.height * currentRectTransform.localScale.y);
        currentRectTransform.anchorMax = new Vector2(currentRectTransform.anchorMax.x + anchorVector.x + (currentRectTransform.offsetMax.x - pivotX) / parentRectTransform.rect.width * currentRectTransform.localScale.x,
                                                 currentRectTransform.anchorMax.y - (1 - anchorVector.y) + (currentRectTransform.offsetMax.y + pivotY) / parentRectTransform.rect.height * currentRectTransform.localScale.y);
        currentRectTransform.offsetMin = new Vector2((0 - currentRectTransform.pivot.x) * anchorRect.width * (1 - currentRectTransform.localScale.x), (0 - currentRectTransform.pivot.y) * anchorRect.height * (1 - currentRectTransform.localScale.y));
        currentRectTransform.offsetMax = new Vector2((1 - currentRectTransform.pivot.x) * anchorRect.width * (1 - currentRectTransform.localScale.x), (1 - currentRectTransform.pivot.y) * anchorRect.height * (1 - currentRectTransform.localScale.y));

        offsetMinOld = currentRectTransform.offsetMin;
        offsetMaxOld = currentRectTransform.offsetMax;
    }
}
