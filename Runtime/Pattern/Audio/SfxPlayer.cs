using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// Component that plays an in-game SFX via SFX Pool Manager on request
public class SfxPlayer : MonoBehaviour
{
    [Header("Asset references")]

    [Tooltip("In-game SFX played when this game object is enabled")]
    public AudioClip sfx;


    private void Awake()
    {
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.AssertFormat(sfx != null, this, "[SfxSpawner] Awake: sfx is not set on {0}", this);
        #endif
    }

    public void PlaySFX(float volumeScale, bool useStackVolumeModifier = false)
    {
        InGameSfxPoolManager.Instance.PlaySfx(sfx, volumeScale, useStackVolumeModifier, context: this, debugClipName: "sfx");
    }
}
