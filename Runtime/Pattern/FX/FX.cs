using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

using HyperUnityCommons;

/// FX component for pooled objects
/// For usage information, see FXPoolManager doc
public abstract class FX : StandardMasterPooledObject
{
    [Header("Parameters")]

    [SerializeField, Tooltip("If checked, SFX will be played using Stack Volume Modifier set on SfxPoolManager. " +
        "Recommended when it's possible to spawn many instances of that FX (or at least playing the same SFX) " +
        "to avoid sound clutter.")]
    private bool sfxUseStackVolumeModifier = true;


    /* Sibling components (optional) */

    private SfxPlayer m_SfxPlayer;


    protected override void Init()
    {
        // Optional
        m_SfxPlayer = GetComponent<SfxPlayer>();
    }

    /// Place FX at passed anchorPosition (this ignores any prefab root local position, so if you need an offset,
    /// place the actual FX Sprite as a child under that root, with local position set to this offset)
    public void WarpTo(Vector3 anchorPosition)
    {
        // Move FX to target anchor position
        transform.position = anchorPosition;
    }

    public abstract Task WaitForPlayOneShotCompletion();

    /// Play any SFX associated at passed sfxVolumeScale
    public void PlaySfxIfAny(float sfxVolumeScale = 1f)
    {
        if (m_SfxPlayer != null)
        {
            m_SfxPlayer.PlaySFX(sfxVolumeScale, sfxUseStackVolumeModifier);
        }
    }
}
