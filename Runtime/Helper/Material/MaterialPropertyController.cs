using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace HyperUnityCommons
{
    /// Component that sets properties "_Color" and "_Brightness" on materials via public API, optionally with limited duration
    /// This must be subclassed with a class that specializes into finding materials on components of type TComponent.
    /// See subclasses for more info
    public abstract class MaterialPropertyController<TComponent> : ClearableBehaviour where TComponent: Component
    {
        /* Property hashes */

        private readonly int colorPropertyID = Shader.PropertyToID("_Color");
        private readonly int brightnessPropertyID = Shader.PropertyToID("_Brightness");


        [Header("Parameters")]

        [SerializeField, Tooltip("Determines if we should automatically register TComponent components, " +
                                 "and how far we should search them (not at all, on same game object or on recursive children). " +
                                 "Note that we don't check for duplicates, so if you set this to a non-None value, " +
                                 "do not add the concerned sprite renderers again manually.")]
        private SearchComponentsMode searchComponentsMode = SearchComponentsMode.Self;


        [Header("References")]

        [Tooltip("Additional list of components to apply property changes to, besides the ones found via search following Search Components Mode. " +
                 "Note the script doesn't check for duplicates, so only use this for components that cannot be trivially found in the children hierarchy " +
                 "(such as components on isolated children or grandchildren that should be picked without their siblings, or on a non-child game object).")]
        [FormerlySerializedAs("controlledComponentsWithMaterial")]
        public List<TComponent> additionalControlledComponentsWithMaterial;


        [Header("Dynamic parameters")]

        [Tooltip("When true, set the tint of all controlled material instances to currentTintOverride on Update. " +
                 "Unlike setters, this allows to edit a property via an animation.<br><br>" +
                 "For performance reasons, we recommend to enable this only when needed, for instance during animations " +
                 "that continuously animate the properties.<br><br>" +
                 "When the value becomes false, the component will automatically call ResetTint once to clear any remaining override.")]
        public bool updateTintWithOverride = false;

        [Tooltip("When Update Properties With Override is checked, set all material tints to this value " +
                 "every Update.")]
        public Color currentTintOverride = Color.white;

        [Tooltip("When true, set the tint of all controlled material instances to currentTintOverride on Update. " +
                 "Unlike setters, this allows to edit a property via an animation.<br><br>" +
                 "For performance reasons, we recommend to enable this only when needed, for instance during animations " +
                 "that continuously animate the properties.<br><br>" +
                 "When the value becomes false, the component will automatically call ResetTint once to clear any remaining override.")]
        public bool updateBrightnessWithOverride = false;

        [Tooltip("When Update Properties With Override is checked, set all material brightnesses to this value " +
                 "every Update.")]
        public float currentBrightnessOverride = 0f;


        /* Custom components */

        /// Timer counting down toward end of tint change
        private Timer m_TintChangeEndTimer;

        /// Timer counting down toward end of brightness change
        private Timer m_BrightnessChangeEndTimer;


        /* Cached references */

        /// List of target material instances that are affected by the property changes
        /// Cached on Awake
        private List<Material> m_CachedTargetMaterialInstances;


        /* Initial state */

        /// Dictionary of initial tints, with key: material instance ID
        private readonly Dictionary<EntityId, Color> m_InitialTintDict = new();

        /// Dictionary of initial brightnesses, with key: material instance ID
        private readonly Dictionary<EntityId, float> m_InitialBrightnessDict = new();


        /* Current state */

        /// Flag that tracks whether updateTintWithOverride was true last frame
        /// to detect when it becomes false, so we can ResetTint to reach a clean state
        private bool m_WasUpdatingTintWithOverride = false;

        /// Flag that tracks whether updateBrightnessWithOverride was true last frame
        /// to detect when it becomes false, so we can ResetBrightness to reach a clean state
        private bool m_WasUpdatingBrightnessWithOverride = false;


        #region Methods to override

        /// If child class is using shared material, create unique material instances for every target
        protected virtual void InstantiateMaterials() {}

        /// Return enumerable to target material properties
        protected abstract Material GetTargetMaterialInstance(TComponent component);

        #endregion


        #region MonoBehaviour Methods

        private void Awake()
        {
            m_TintChangeEndTimer = new Timer(callback: ResetTint);
            m_BrightnessChangeEndTimer = new Timer(callback: ResetBrightness);

            // Search for any extra controlled components with material in hierarchy
            GameObjectUtil.FillComponentsSearchingInHierarchy(additionalControlledComponentsWithMaterial, gameObject, searchComponentsMode);

            // Must be called just after FillComponentsSearchingInHierarchy so we instantiate materials for all wanted
            // shared materials if needed, and before setting m_CachedTargetMaterialInstances since
            // GetTargetMaterialInstance needs those material instances.
            InstantiateMaterials();

            // Cache material instances from components
            m_CachedTargetMaterialInstances = additionalControlledComponentsWithMaterial
                .Select(GetTargetMaterialInstance)
                .ToList();

            // Store initial properties of material instances
            for (int i = 0; i < m_CachedTargetMaterialInstances.Count; i++)
            {
                var targetMaterialInstance = m_CachedTargetMaterialInstances[i];

                // Just check m_InitialTintDict, as m_InitialBrightnessDict should have the same keys
                DebugUtil.AssertFormat(!m_InitialTintDict.ContainsKey(targetMaterialInstance.GetEntityId()),
                    this,
                    "[MaterialPropertyController] Awake: m_InitialTintDict already contains key " +
                    "{0}, which means material instance {1} from component {2} has been added twice, " +
                    "or once in Inspector to controlledComponentsWithMaterial, and once via FillComponentsSearchingInHierarchy " +
                    "on {3}. Make sure that the search is not redundant with manual entry.",
                    targetMaterialInstance.GetEntityId(), targetMaterialInstance, additionalControlledComponentsWithMaterial[i], this);

                // Assertion message based on native error message of GetColor/GetFloat when property is missing,
                // but adding game object information
                DebugUtil.AssertFormat(targetMaterialInstance.HasColor(colorPropertyID), additionalControlledComponentsWithMaterial[i].gameObject,
                    "[MaterialPropertyController] Awake: on {0}, Material '{1}' with Shader '{2}' has no color property '_Color'",
                    additionalControlledComponentsWithMaterial[i].gameObject, targetMaterialInstance.name, targetMaterialInstance.shader.name);
                DebugUtil.AssertFormat(targetMaterialInstance.HasFloat(brightnessPropertyID), additionalControlledComponentsWithMaterial[i].gameObject,
                    "[MaterialPropertyController] Awake: on {0}, Material '{1}' with Shader '{2}' has no float property '_Brightness'",
                    additionalControlledComponentsWithMaterial[i].gameObject, targetMaterialInstance.name, targetMaterialInstance.shader.name);

                m_InitialTintDict.Add(targetMaterialInstance.GetEntityId(),
                    targetMaterialInstance.GetColor(colorPropertyID));
                m_InitialBrightnessDict.Add(targetMaterialInstance.GetEntityId(),
                    targetMaterialInstance.GetFloat(brightnessPropertyID));
            }
        }

        private void Update()
        {
            UpdateTint();
            UpdateBrightness();
        }

        private void UpdateTint()
        {
            if (updateTintWithOverride)
            {
                // Update mode: set properties directly from override values
                SetTint(currentTintOverride);
            }
            else
            {
                if (m_WasUpdatingTintWithOverride)
                {
                    // updateTintWithOverride just became false, so we reset tint
                    // to reach a clean state, in case some override tint is left
                    ResetTint();
                }

                // Manual mode: tint is not overriden, so update tint timer
                m_TintChangeEndTimer.CountDown(Time.deltaTime);
            }
        }

        private void UpdateBrightness()
        {
            m_WasUpdatingTintWithOverride = updateTintWithOverride;

            if (updateBrightnessWithOverride)
            {
                // Update mode: set properties directly from override values
                SetBrightness(currentBrightnessOverride);
            }
            else
            {
                if (m_WasUpdatingBrightnessWithOverride)
                {
                    // updateBrightnessWithOverride just became false, so we reset brightness
                    // to reach a clean state, in case some override brightness is left
                    ResetBrightness();
                }

                // Manual mode: brightness is not overriden, so update brightness timer
                m_BrightnessChangeEndTimer.CountDown(Time.deltaTime);
            }

            m_WasUpdatingBrightnessWithOverride = updateBrightnessWithOverride;
        }

        #endregion


        #region ClearableBehaviour override

        public override void Clear()
        {
            ResetProperties();
        }

        #endregion


        #region Own Methods

        /// Set tint property on passed material instance
        private void SetTintOnMaterialInstance(Material materialInstance, Color tint)
        {
            materialInstance.SetColor(colorPropertyID, tint);
        }

        /// Reset tint property on passed material instance to initial color
        private void ResetTintOnMaterialInstance(Material targetMaterialInstance)
        {
            SetTintOnMaterialInstance(targetMaterialInstance,
                m_InitialTintDict[targetMaterialInstance.GetEntityId()]);
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
                m_InitialBrightnessDict[targetMaterialInstance.GetEntityId()]);
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
                SetBrightness(m_InitialBrightnessDict[targetMaterialInstance.GetEntityId()]);
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
        public void SetTintForDuration(Color tint, float duration)
        {
            SetTint(tint);
            m_TintChangeEndTimer.SetTime(duration);
        }

        /// Set sprite material brightness for given duration
        public void SetBrightnessForDuration(float brightness, float duration)
        {
            SetBrightness(brightness);
            m_BrightnessChangeEndTimer.SetTime(duration);
        }

        /// Set all supported material properties for given duration
        public void SetPropertiesForDuration(Color tint, float brightness, float duration)
        {
            SetProperties(tint, brightness);
            m_TintChangeEndTimer.SetTime(duration);
            m_BrightnessChangeEndTimer.SetTime(duration);
        }

        #endregion
    }
}
