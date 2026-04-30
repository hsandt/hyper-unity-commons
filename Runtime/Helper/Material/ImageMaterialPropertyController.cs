using UnityEngine;
using UnityEngine.UI;

/// MaterialPropertyController for Images
/// MAT_UI_Default-with-Brightness (Packages/com.longnguyenhuu.hyper-unity-commons/Runtime/Helper/Shaders/MAT_UI_Default-with-Brightness.mat)
/// is an example of material for images with the properties _Color and _Brightness.
public class ImageMaterialPropertyController : MaterialPropertyController<Image>
{
    protected override void InstantiateMaterials()
    {
        // MaterialPropertyController.Awake also calls DebugUtil.AssertListElementsNotNull so no need to log errors
        // on null entries again, just check for not null
        if (controlledComponentsWithMaterial != null)
        {
            for (int i = 0; i < controlledComponentsWithMaterial.Count; i++)
            {
                if (controlledComponentsWithMaterial[i] != null)
                {
                    // Image.material is a shared material, unlike SpriteRenderer, so we need to create a temporary copy
                    // for each image, so we can work on material instances
                    // https://forum.unity.com/threads/image-material-being-treated-like-renderer-sharedmaterial-any-workaround.279723/#post-7811535
                    controlledComponentsWithMaterial[i].material = new Material(controlledComponentsWithMaterial[i].material);
                }
            }
        }
    }

    protected override Material GetTargetMaterialInstance(Image component)
    {
        return component.material;
    }
}
