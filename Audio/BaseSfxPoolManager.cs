using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using CommonsPattern;

/// Base class for Pool Manager allowing to easily play SFX
/// Has a feature to adjust volume automatically when many SFX of the same type are played
/// Usage: create a prefab [Category]SfxPoolManager (where [Category] = UI, InGame, etc.),
/// where you assign pooledObjectPrefab to some prefab with an Audio Source and an SFX component.
/// We recommend creating a variant of the SfxAudioSource_Base prefab provided in Commons Pattern/Audio,
/// named SfxAudioSource_Variant_[Category], and set parameters there
/// (e.g. set Audio Source Output Mixer to some project-specific SFX channel for [Category]),
/// This way, you benefit from the latest changes done on the SfxAudioSource_Base prefab, while keeping your
/// project-specific overrides.
public abstract class BaseSfxPoolManager<T> : PoolManager<Sfx, T> where T : BaseSfxPoolManager<T>
{
    [Header("Parameters")]

    [SerializeField,
     Tooltip("Factor determining volume of SFX played in overlap with other SFX using the same clip " +
         "(simultaneously or with delay). Only used when calling PlaySfx with useStackVolumeModifier. " +
         "Reduce this number to avoid audio clutter when the same SFX is played over many instances. Formula:\n" +
         "(volume scale of N-th SFX using same clip)\n" +
         "= (sameClipStackVolumeModifierFactor)^(N-1)\n" +
         "= (sameClipStackVolumeModifierFactor)^(count of SFX still playing same clip)\n" +
         "0: no stacking, don't play SFX with same clip as SFX still playing\n" +
         "0.5: play 2nd SFX with same clip at volume scale 0.5, 3rd SFX at volume scale 0.25, etc.\n" +
         "1: full stacking, play SFX ignoring those already playing\n" +
         "We can also derive the total volume formula for N SFX played simultaneously with the same clip " +
         "(at base volume scale = 1). It is a geometric series:\n" +
         "(total volume scale of N SFX using same clip)\n" +
         "= (1 - sameClipStackVolumeModifierFactor^N) / (1-sameClipStackVolumeModifierFactor)")]
    [Range(0f, 1f)]
    private float sameClipStackVolumeModifierFactor = 1f;


    protected override void Init()
    {
        if (poolTransform == null)
        {
            // just spawn SFX as children of the manager object
            poolTransform = transform;
        }

        base.Init();
    }

    /// Play SFX clip on pooled SFX object at volumeScale, with optional context and debugClipName to log error
    /// if SFX could not be acquired.
    /// If useStackVolumeModifier is true, apply stack volume modifier for this clip based on
    /// sameClipStackVolumeModifierFactor and count of other SFXs playing same clip
    /// Return played SFX unless it could not be acquired, or the same clip stack volume modifier is 0.
    public Sfx PlaySfx(AudioClip clip, float volumeScale = 1f, bool useStackVolumeModifier = false, Object context = null, string debugClipName = null)
    {
        if (clip != null)
        {
            Sfx sfx = AcquireFreeObject();

            if (sfx != null)
            {
                // If useStackVolumeModifier is true, apply same clip stack volume modifier, else default to 1
                // If it is 0, do not play anything to spare a pooled SFX, and return null
                float sameClipStackVolumeModifier = useStackVolumeModifier
                    ? ComputeSameClipStackVolumeModifier(clip)
                    : 1f;

                if (sameClipStackVolumeModifier > 0)
                {
                    sfx.PlayStoringClip(clip, sameClipStackVolumeModifier * volumeScale);
                    return sfx;
                }
            }
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            else
            {
                Debug.LogWarningFormat(this, "[SfxPoolManager] PlaySfx: Cannot play clip '{0}' due to either " +
                    "missing prefab or pool starvation. In case of pool starvation, consider setting " +
                    "Consider setting instantiateNewObjectOnStarvation: true on SfxPoolManager, or increasing its pool size.",
                    clip);
            }
            #endif
        }
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        else
        {
            if (context != null && debugClipName != null)
            {
                Debug.LogWarningFormat(context, "[SfxPoolManager] PlaySfx: passed clip is null, " +
                    "make sure that {0} is defined on {1}",
                    debugClipName, context);
            }
            else if (context != null)
            {
                Debug.LogWarningFormat(context, "[SfxPoolManager] PlaySfx: passed clip is null, " +
                    "make sure that clip is defined on {0}",
                    context);
            }
            else if (debugClipName != null)
            {
                Debug.LogWarningFormat("[SfxPoolManager] PlaySfx: passed clip is null, " +
                    "make sure that {0} is defined (no context)",
                    debugClipName);
            }
            else
            {
                Debug.LogWarning("[SfxPoolManager] PlaySfx: passed clip is null, " +
                    "make sure that clip is defined");
            }
        }
        #endif

        return null;
    }

    private float ComputeSameClipStackVolumeModifier(AudioClip clip)
    {
        if (sameClipStackVolumeModifierFactor >= 1f)
        {
            // Preserve original volume scale
            return 1f;
        }

        // Count SFX still playing the same clip
        // Note that we can only detect them if we set clip on their AudioSource then called Play,
        // instead of PlayOneShot. That is why we recommend playing all SFX with the same clip via
        // SfxPoolManager.Instance.PlaySfx, which calls Sfx.PlayStoringClip.
        int sameClipPlayingCount = GetObjectsInUse().Count(sfx => sfx.AudioSource.clip == clip);

        // Note that as 0^0 = 1 and 0^n = 0 when n >= 1, so when sameClipStackVolumeModifierFactor == 0,
        // we play the first SFX normally then skip all further SFX with the same clips, as expected.
        return Mathf.Pow(sameClipStackVolumeModifierFactor, sameClipPlayingCount);
    }
}
