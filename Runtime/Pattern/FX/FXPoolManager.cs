using System.Collections;
using System.Collections.Generic;
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

    public FX SpawnFX(string fxName, Vector3 anchorPosition, float sfxVolumeScale = 1f)
    {
        FX fx = AcquireFreeObject(fxName);
        if (fx != null)
        {
            fx.Setup(anchorPosition, sfxVolumeScale);
            return fx;
        }

        Debug.LogErrorFormat(this,
            "[FXPoolManager] SpawnFX: Could not acquire free object for fxName '{0}'",
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
