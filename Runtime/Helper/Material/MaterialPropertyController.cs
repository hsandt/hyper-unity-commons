using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using HyperUnityCommons;

/// Component that sets properties "Tint" and "Brightness" on materials via public API, optionally with limited duration
/// MAT_Sprite-Unlit (Packages/com.longnguyenhuu.hyper-unity-commons/Runtime/Helper/Shaders/MAT_Sprite-Unlit.mat)
/// is an example of material with those two properties.
/// This must be subclassed with a class that specializes into finding materials on components of type TComponent.
public abstract class MaterialPropertyController<TComponent> : ClearableBehaviour where TComponent: Component
{
    /* Property hashes */

    private readonly int tintPropertyID = Shader.PropertyToID("Tint");
    private readonly int brightnessPropertyID = Shader.PropertyToID("Brightness");


    [Header("Child references")]

    [Tooltip("List of sprites to apply property changes to. If Search Components Mode is not None, this is filled " +
        "automatically on initialization. Note that components found this way are added to the ones already " +
        "set in the inspector, without checking for duplicates.")]
    public List<TComponent> controlledComponentsWithMaterial;


    [Header("Parameters")]

    [SerializeField, Tooltip("Determines if we should automatically register TComponent components, " +
         "and how far we should search them (not at all, on same game object or on recursive children). " +
         "Note that we don't check for duplicates, so if you set this to a non-None value, " +
         "do not add the concerned sprite renderers again manually.")]
    private SearchComponentsMode searchComponentsMode = SearchComponentsMode.Self;


    /* Custom components */

    /// Timer counting down toward end of property changes
    private Timer m_PropertyChangeEndTimer;


    /* Cached references */

    /// List of target material instances that are affected by the property changes
    /// Cached on Awake
    private List<Material> m_CachedTargetMaterialInstances;


    /* Initial state */

    /// Dictionary of initial tints, with key: material instance ID
    private readonly Dictionary<int, Color> m_InitialTintDict = new();

    /// Dictionary of initial brightnesses, with key: material instance ID
    private readonly Dictionary<int, float> m_InitialBrightnessDict = new();



    /* Methods to override */

    /// Do any initialisation required by the child class here, such as registering or creating material instances
    /// if they don't exist or are shared on game start
    protected virtual void Init() {}

    /// Return enumerable to target material properties
    protected abstract Material GetTargetMaterialInstance(TComponent component);


    private void Awake()
    {
        // Call it first, may adjust controlledComponentsWithMaterial
        Init();

        m_PropertyChangeEndTimer = new Timer(callback: ResetProperties);

        // Search for any extra controlled components with material in hierarchy
        GameObjectUtil.FillComponentsSearchingInHierarchy(controlledComponentsWithMaterial, gameObject, searchComponentsMode);

        // Cache material instances from components
        m_CachedTargetMaterialInstances = controlledComponentsWithMaterial?
            .Select(GetTargetMaterialInstance)
            .ToList();

        // Store initial properties of material instances
        foreach (var targetMaterialInstance in m_CachedTargetMaterialInstances)
        {
            m_InitialTintDict.Add(targetMaterialInstance.GetInstanceID(), targetMaterialInstance.GetColor(tintPropertyID));
            m_InitialBrightnessDict.Add(targetMaterialInstance.GetInstanceID(), targetMaterialInstance.GetFloat(brightnessPropertyID));
        }
    }

    private void Update()
    {
        m_PropertyChangeEndTimer.CountDown(Time.deltaTime);
    }


    /* ClearableBehaviour override */

    public override void Clear()
    {
        ResetProperties();
    }


    /* Own methods */

    /// Set tint property on passed material instance
    private void SetTintOnMaterialInstance(Material materialInstance, Color tint)
    {
        materialInstance.SetColor(tintPropertyID, tint);
    }

    /// Reset tint property on passed material instance to initial color
    private void ResetTintOnMaterialInstance(Material targetMaterialInstance)
    {
        SetTintOnMaterialInstance(targetMaterialInstance,
            m_InitialTintDict[targetMaterialInstance.GetInstanceID()]);
    }

    /// Set brightness property on passed material instance
    private void SetBrightnessOnMaterialInstance(Material materialInstance, float brightness)
    {
        materialInstance.SetFloat(brightnessPropertyID, brightness);
    }

    /// Reset brightness property on passed material instance to initial value
    private void ResetBrightnessOnMaterialInstance(Material targetMaterialInstance)
    {
        SetBrightnessOnMaterialInstance(targetMaterialInstance,
            m_InitialBrightnessDict[targetMaterialInstance.GetInstanceID()]);
    }

    /// Set tint property on all target material instances
    public void SetTint(Color tint)
    {
        foreach (var targetMaterialInstance in m_CachedTargetMaterialInstances)
        {
            SetTintOnMaterialInstance(targetMaterialInstance, tint);
        }
    }

    /// Reset tint on all target material instances to initial color
    public void ResetTint()
    {
        foreach (var targetMaterialInstance in m_CachedTargetMaterialInstances)
        {
            ResetTintOnMaterialInstance(targetMaterialInstance);
        }
    }

    /// Set brightness property on all target material instances
    public void SetBrightness(float brightness)
    {
        foreach (var targetMaterialInstance in m_CachedTargetMaterialInstances)
        {
            SetBrightnessOnMaterialInstance(targetMaterialInstance, brightness);
        }
    }

    /// Reset brightness on all target material instances to initial value
    public void ResetBrightness()
    {
        foreach (var targetMaterialInstance in m_CachedTargetMaterialInstances)
        {
            SetBrightness(m_InitialBrightnessDict[targetMaterialInstance.GetInstanceID()]);
        }
    }

    public void SetProperties(Color tint, float brightness)
    {
        SetTint(tint);
        SetBrightness(brightness);
    }

    public void ResetProperties()
    {
        ResetTint();
        ResetBrightness();
    }

    /// Set sprite material tint for given duration
    /// It also resets the timer shared with other properties, so this may lengthen or shorten other property changes.
    public void SetTintForDuration(Color tint, float duration)
    {
        SetTint(tint);
        m_PropertyChangeEndTimer.SetTime(duration);
    }

    /// Set sprite material brightness for given duration
    /// It also resets the timer shared with other properties, so this may lengthen or shorten other property changes.
    public void SetBrightnessForDuration(float brightness, float duration)
    {
        SetBrightness(brightness);
        m_PropertyChangeEndTimer.SetTime(duration);
    }

    public void SetPropertiesForDuration(Color tint, float brightness, float duration)
    {
        SetProperties(tint, brightness);
        m_PropertyChangeEndTimer.SetTime(duration);
    }
}
