using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

using HyperUnityCommons;

/// FX Pool Manager
/// SEO: after LocatorManager
public class FXPoolManager : MultiPoolManager<FX, FXPoolManager>
{
    [SerializeField, Tooltip("Tag used to find FX Pool, if Pool Transform is not set in the inspector and this field " +
         "is not empty")]
    private string fxPoolTag = "FXPool";


    protected override void Init()
    {
        if (poolTransform == null)
        {
            poolTransform = LocatorManager.Instance.FindWithTag(fxPoolTag)?.transform;
        }

        base.Init();
    }

    /// Spawn FX by name at [anchorPosition] and play any associated SFX at [sfxVolumeScale]
    public FX SpawnFX(string fxName, Vector3 anchorPosition, float sfxVolumeScale = 1f)
    {
        // Acquire FX. This will activate the game object. We assume the FX starts playing automatically.
        FX fx = AcquireFreeObject(fxName);
        if (fx != null)
        {
            fx.WarpTo(anchorPosition);
            fx.PlaySfxIfAny(sfxVolumeScale);
            return fx;
        }

        Debug.LogErrorFormat(this,
            "[FXPoolManager] SpawnFX: Could not acquire free object for fxName '{0}'",
            fxName);
        return null;
    }

    /// Spawn one-shot (non-looping) FX by name at [anchorPosition] and play any associated SFX at [sfxVolumeScale]
    /// Assume that FX is played automatically on activation and wait for FX end,
    /// deactivating it if needed so it's considered released for reuse in pooling
    public async Task<FX> PlayOneShotFXAsync(string fxName, Vector3 anchorPosition, float sfxVolumeScale = 1f)
    {
        // Start like SpawnFX
        FX fx = AcquireFreeObject(fxName);
        if (fx != null)
        {
            // Setup, as it's a MasterBehaviour, and besides FX subclass may have additional setup
            fx.Setup();

            fx.WarpTo(anchorPosition);
            fx.PlaySfxIfAny(sfxVolumeScale);

            // Then wait for FX to complete (method depends on FX subclass) and release it
            // (important because IsInUse only checks for game object active state, not whether FX is actually over)
            await fx.WaitForPlayOneShotCompletion();
            fx.Release();

            return fx;
        }

        Debug.LogErrorFormat(this,
            "[FXPoolManager] PlayOneShotFXAsync: Could not acquire free object for fxName '{0}'",
            fxName);
        return null;
    }

    public void PauseAllFX()
    {
        foreach (FX fx in GetObjectsInUseInAllPools())
        {
            fx.Pause();
        }
    }

    public void ResumeAllFX()
    {
        foreach (FX fx in GetObjectsInUseInAllPools())
        {
            fx.Resume();
        }
    }
}
