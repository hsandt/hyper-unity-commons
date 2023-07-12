using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using HyperUnityCommons;

/// Component that sets properties "_Color" and "_Brightness" on materials via public API, optionally with limited duration
/// This must be subclassed with a class that specializes into finding materials on components of type TComponent.
/// See subclasses for more info
public abstract class MaterialPropertyController<TComponent> : ClearableBehaviour where TComponent: Component
{
    /* Property hashes */

    private readonly int colorPropertyID = Shader.PropertyToID("_Color");
    private readonly int brightnessPropertyID = Shader.PropertyToID("_Brightness");


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

    /// If child class has old members, process them to be compatible with new version
    /// (e.g. filling controlledComponentsWithMaterial with old component array content)
    protected virtual void UpdateVersion() {}

    /// If child class is using shared material, create unique material instances for every target
    protected virtual void InstantiateMaterials() {}

    /// Return enumerable to target material properties
    protected abstract Material GetTargetMaterialInstance(TComponent component);


    private void Awake()
    {
        // Call it first, may adjust controlledComponentsWithMaterial
        UpdateVersion();

        m_PropertyChangeEndTimer = new Timer(callback: ResetProperties);

        // Search for any extra controlled components with material in hierarchy
        GameObjectUtil.FillComponentsSearchingInHierarchy(controlledComponentsWithMaterial, gameObject, searchComponentsMode);

        // Must be called just after FillComponentsSearchingInHierarchy so we instantiate materials for all wanted
        // shared materials if needed, and before setting m_CachedTargetMaterialInstances since
        // GetTargetMaterialInstance needs those material instances.
        InstantiateMaterials();

        // Cache material instances from components
        m_CachedTargetMaterialInstances = controlledComponentsWithMaterial?
            .Select(GetTargetMaterialInstance)
            .ToList();

        // Store initial properties of material instances
        for (int i = 0; i < m_CachedTargetMaterialInstances.Count; i++)
        {
            var targetMaterialInstance = m_CachedTargetMaterialInstances[i];

            // Just check m_InitialTintDict, as m_InitialBrightnessDict should have the same keys
            DebugUtil.AssertFormat(!m_InitialTintDict.ContainsKey(targetMaterialInstance.GetInstanceID()),
                this,
                "[MaterialPropertyController] Awake: m_InitialTintDict already contains key " +
                "{0}, which means material instance {1} from component {2} has been added twice, " +
                "or once in Inspector to controlledComponentsWithMaterial, and once via FillComponentsSearchingInHierarchy " +
                "on {2}. Make sure that the search is not redundant with manual entry.",
                targetMaterialInstance.GetInstanceID(), targetMaterialInstance, controlledComponentsWithMaterial[i], this);

            m_InitialTintDict.Add(targetMaterialInstance.GetInstanceID(),
                targetMaterialInstance.GetColor(colorPropertyID));
            m_InitialBrightnessDict.Add(targetMaterialInstance.GetInstanceID(),
                targetMaterialInstance.GetFloat(brightnessPropertyID));
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
        materialInstance.SetColor(colorPropertyID, tint);
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
