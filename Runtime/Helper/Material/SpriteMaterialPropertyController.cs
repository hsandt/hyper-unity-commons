using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// MaterialPropertyController for Sprite Renderers
/// MAT_Sprite-Unlit (Packages/com.longnguyenhuu.hyper-unity-commons/Runtime/Helper/Shaders/MAT_Sprite-Unlit.mat)
/// is an example of material for sprites with the properties _Color and _Brightness.
public class SpriteMaterialPropertyController : MaterialPropertyController<SpriteRenderer>
{
    protected override Material GetTargetMaterialInstance(SpriteRenderer spriteRenderer)
    {
        // Sprite Renderer material is always a material instance
        return spriteRenderer.material;
    }
}
