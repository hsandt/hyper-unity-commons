using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

using HyperUnityCommons;

/// MaterialPropertyController for Sprite Renderers
public class SpriteMaterialPropertyController : MaterialPropertyController<SpriteRenderer>
{
    [Header("Child references")]

    [Tooltip("Old array of additional sprites to apply brighten to (besides the optional sibling sprite renderer). " +
        "We now use a list, and added flags to automatically add sibling and children components, so we only kept " +
        "this for version transition, until designers switched completely to the new flags + list.")]
    [FormerlySerializedAs("additionalSpriteRenderers")]
    public SpriteRenderer[] OLD_AdditionalSpriteRenderers;

    [Tooltip("Old list of sprites to apply brighten to. If Search Components Mode is not None, this is filled " +
        "automatically on initialization. Note that components found this way are added to the ones already " +
        "set in the inspector, without checking for duplicates.")]
    [FormerlySerializedAs("controlledSpriteRenderers")]
    public List<SpriteRenderer> OLD_controlledSpriteRenderers;


    protected override void UpdateVersion()
    {
        DebugUtil.AssertListElementsNotNull(OLD_AdditionalSpriteRenderers, this, nameof(OLD_AdditionalSpriteRenderers));
        DebugUtil.AssertListElementsNotNull(OLD_controlledSpriteRenderers, this, nameof(OLD_controlledSpriteRenderers));

        // Version upgrade: for projects which have not transferred all sprite renderers from
        // old lists OLD_AdditionalSpriteRenderers and OLD_controlledSpriteRenderers to new list
        // controlledComponentsWithMaterial (on base class) yet, add entries from the old array/list
        if (controlledComponentsWithMaterial.Count == 0)
        {
            if (OLD_AdditionalSpriteRenderers.Length > 0)
            {
                Debug.LogWarningFormat(this,
                    "[SpriteMaterialPropertyController] Init: {0} still using entries in old array " +
                    "OLD_AdditionalSpriteRenderers, please move them to new list controlledComponentsWithMaterial",
                    this);
                controlledComponentsWithMaterial.AddRange(OLD_AdditionalSpriteRenderers);
            }

            if (OLD_controlledSpriteRenderers.Count > 0)
            {
                Debug.LogWarningFormat(this,
                    "[SpriteMaterialPropertyController] Init: {0} still using entries in old list " +
                    "OLD_controlledSpriteRenderers, please move them to new list controlledComponentsWithMaterial",
                    this);
                controlledComponentsWithMaterial.AddRange(OLD_controlledSpriteRenderers);
            }
        }
    }

    protected override Material GetTargetMaterialInstance(SpriteRenderer spriteRenderer)
    {
        // Sprite Renderer material is always a material instance
        return spriteRenderer.material;
    }
}
