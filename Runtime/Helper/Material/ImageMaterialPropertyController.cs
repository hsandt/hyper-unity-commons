using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;

/// MaterialPropertyController for Images
public class ImageMaterialPropertyController : MaterialPropertyController<Image>
{
    [Header("Sibling & child references")]

    [Tooltip("Old images to apply property changes to")]
    [FormerlySerializedAs("images")]
    public Image[] OLD_images;


    protected override void Init()
    {
        // Version upgrade: for projects which have not transferred all sprite renderers from
        // old list OLD_images to new list controlledComponentsWithMaterial (on base class) yet,
        // add entries from the old list
        if (controlledComponentsWithMaterial.Count == 0)
        {
            if (OLD_images.Length > 0)
            {
                Debug.LogWarningFormat(this,
                    "[ImageMaterialPropertyController] Init: {0} still using entries in old array " +
                    "OLD_images, please move them to new list controlledComponentsWithMaterial",
                    this);
                controlledComponentsWithMaterial.AddRange(OLD_images);
            }
        }

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
