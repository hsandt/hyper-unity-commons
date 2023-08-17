using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using HyperUnityCommons;

/// Component to place on label associated to a setting arrow choice
/// Requires: AppManager, SettingsManager
/// SEO: (all subclasses) after SettingsManager
public abstract class SettingArrowChoiceLabel<TSettingValue> : BaseSettingLabel
{
    [Header("Asset references")]

    [Tooltip("Data of discrete setting modified by the associated choice")]
    public DiscreteSettingData<TSettingValue> discreteSettingData;

    [Tooltip("(Optional) SFX played when changing choice")]
    public AudioClip sfxChoiceChange;


    [Header("Scene references")]

    [Tooltip("Arrow Left Button, used to select previous choice")]
    public Button arrowLeftButton;

    [Tooltip("Arrow Right Button, used to select next choice")]
    public Button arrowRightButton;


    /* Cached parameters */

    /// List of choice values (stored format)
    protected List<TSettingValue> m_ChoiceValues;

    /// List of choice names corresponding to m_ChoiceValues
    protected List<string> m_ChoiceNames;


    /* State vars */

    /// Index of the entry currently selected
    /// Note: avoid name "m_CurrentIndex", used by Selectable
    protected int m_CurrentChoiceIndex;

    /// Update the text content with the current choice name
    /// Override method to either replace text directly or use a localized text key.
    protected abstract void UpdateText(string currentChoiceName);


    // Bug IN-10813: when inheriting from Selectable, in the editor:
    // - Awake is not called on Play, only on Stop
    // - Start sometimes work, but is not reliable
    // - OnEnable is called when expected, but also on Application Quit to setup values in the editor
    // => The most reliable is to check and initialize members in OnEnable, after base call, but only if application is
    // not quitting; and also do any required symmetrical operations like even unregistration in OnDisable.
    protected override void OnEnable()
    {
        base.OnEnable();

        if (AppManager.IsNotQuitting())
        {
            DebugUtil.AssertFormat(discreteSettingData != null, this, "[SettingArrowChoiceLabel] No discreteSettingData found on {0}.", this);
            DebugUtil.AssertFormat(arrowLeftButton != null, this, "[SettingArrowChoiceLabel] No arrowLeftButton found on {0}.", this);
            DebugUtil.AssertFormat(arrowRightButton != null, this, "[SettingArrowChoiceLabel] No arrowRightButton found on {0}.", this);

            RegisterEvents();

            Init();
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        UnregisterEvents();
    }

    private void Init()
    {
        // Initialize choices
        m_ChoiceValues = discreteSettingData.GetAvailableValues();
        DebugUtil.AssertFormat(m_ChoiceValues.Count > 0, discreteSettingData,
            "[SettingArrowChoiceLabel] Init: discreteSettingData.GetAvailableValues() returned empty list " +
            "of choices");

        m_ChoiceNames = m_ChoiceValues.Select(value => discreteSettingData
            .StoredValueToRepresentedText(value)).ToList();
    }

    private void RegisterEvents()
    {
        arrowLeftButton.onClick.AddListener(OnLeftArrowClick);
        arrowRightButton.onClick.AddListener(OnRightArrowClick);
    }

    private void UnregisterEvents()
    {
        if (arrowLeftButton != null)
        {
            arrowLeftButton.onClick.RemoveListener(OnLeftArrowClick);
        }

        if (arrowRightButton != null)
        {
            arrowRightButton.onClick.RemoveListener(OnRightArrowClick);
        }
    }

    private void OnLeftArrowClick()
    {
        SelectPreviousChoice();
    }

    private void OnRightArrowClick()
    {
        SelectNextChoice();
    }

    /// Set up choice to match current setting
    /// This is called when this widget is shown so the initial selection is correct
    public override void Setup()
    {
        // At this point, SettingsManager must have loaded/initialized all settings (from either prefs or defaults)
        // so it's safe to use the current setting value (it will also retrieve engine value if set to
        // alwaysCheckEngineValue)
        TSettingValue currentSettingValue = SettingsManager.Instance.GetSettingValue(discreteSettingData);

        // Remember we are working with a choice index, not value, so retrieve index from value
        int currentChoiceIndexFromValue = FindChoiceIndex(currentSettingValue);

        if (currentChoiceIndexFromValue < 0)
        {
            #if UNITY_EDITOR
            DebugUtil.LogWarningFormat(discreteSettingData,
                "[SettingArrowChoiceLabel] Setup: could not find choice index {0} for setting {1} on widget {2}. " +
                "This can happen in editor when testing with non-standard resolutions. " +
                "Falling back to choice index 0 so we can at least display something, but note that displayed choice " +
                "will not match the actual setting. ",
                currentSettingValue, discreteSettingData, this);
            #else
            DebugUtil.LogWarningFormat(discreteSettingData,
                "[SettingArrowChoiceLabel] Setup: could not find choice index {0} for setting {1} on widget {2}. " +
                "This should not happen in normal usage, as InitializeSimpleSetting/InitializeResolutionSetting " +
                "should use GetFallbackValueFrom if value from preference was invalid, or get custom default value "+
                "or engine current value if there was no preference, and both should guarantee a valid value. " +
                "Falling back to choice index 0 so we can at least display something, but note that displayed choice " +
                "will not match the actual setting.",
                currentSettingValue, discreteSettingData, this);
            #endif

            currentChoiceIndexFromValue = 0;
        }

        // Select choice in UI to match current setting
        // (this means we don't need to call SetSettingForCurrentChoice besides)
        SelectChoice_Internal(currentChoiceIndexFromValue);
    }

    private void UpdateUI()
    {
        if (m_ChoiceNames == null)
        {
            Debug.LogError("[SettingArrowChoiceLabel] UpdateUI: m_ChoiceNames is null, STOP", this);
            return;
        }

        if (m_CurrentChoiceIndex < 0 || m_CurrentChoiceIndex > m_ChoiceNames.Count - 1)
        {
            Debug.LogErrorFormat(this,
                "[SettingArrowChoiceLabel] UpdateUI: index {0} is out of bounds ({1} entries), STOP",
                m_CurrentChoiceIndex, m_ChoiceNames.Count);
            return;
        }

        string currentChoiceName = m_ChoiceNames[m_CurrentChoiceIndex];
        if (currentChoiceName == null) {
            Debug.LogErrorFormat(this,
                "[SettingArrowChoiceLabel] UpdateUI: m_ChoiceNames[{0}] is null, STOP",
                m_CurrentChoiceIndex);
            return;
        }

        UpdateText(currentChoiceName);
    }

    public override void OnMove(AxisEventData eventData)
    {
        // Capture horizontal move
        switch (eventData.moveDir)
        {
            case MoveDirection.Left:
                SelectPreviousChoice();
                eventData.Use();
                return;
            case MoveDirection.Right:
                SelectNextChoice();
                eventData.Use();
                return;
        }

        // Move was not captured, call base implementation to navigate between setting labels and other selectables
        base.OnMove(eventData);
    }

    /// Return choice index corresponding to passed value, or -1 if not found
    private int FindChoiceIndex(TSettingValue value)
    {
        for (int i = 0; i < m_ChoiceValues.Count; i++)
        {
            // We should use POD (primitives and struct of primitives) as setting value,
            // so default equality should be good
            if (EqualityComparer<TSettingValue>.Default.Equals(m_ChoiceValues[i], value))
            {
                return i;
            }
        }

        return -1;
    }

    /// Try to select previous choice, do nothing else (also used by arrow button)
    private void SelectPreviousChoice()
    {
        if (m_CurrentChoiceIndex > 0)
        {
            SelectChoice(m_CurrentChoiceIndex - 1);

            if (sfxChoiceChange != null)
            {
                UISfxPoolManager.Instance.PlaySfx(sfxChoiceChange, context: this, debugClipName: "sfxSliderMove");
            }
        }
    }

    /// Try to select next choice, do nothing else (also used by arrow button)
    private void SelectNextChoice()
    {
        if (m_CurrentChoiceIndex < m_ChoiceNames.Count - 1)
        {
            SelectChoice(m_CurrentChoiceIndex + 1);

            if (sfxChoiceChange != null)
            {
                UISfxPoolManager.Instance.PlaySfx(sfxChoiceChange, context: this, debugClipName: "sfxSliderMove");
            }
        }
    }

    /// Select a new choice by index
    private void SelectChoice(int index)
    {
        if (index == m_CurrentChoiceIndex)
        {
            return;
        }

        SelectChoice_Internal(index);

        // Actual change: for now, we apply setting immediately
        SetSettingForCurrentChoice();
    }

    /// Select choice, set associated setting and update UI, no matter what
    /// UB unless index is valid
    private void SelectChoice_Internal(int index)
    {
        if (m_CurrentChoiceIndex < 0 || m_CurrentChoiceIndex > m_ChoiceNames.Count - 1) {
            Debug.LogErrorFormat(this,
                "[SettingArrowChoiceLabel] SelectChoice: index {0} is out of bounds ({1} entries)",
                m_CurrentChoiceIndex, m_ChoiceNames.Count);
            return;
        }

        // Set choice index
        m_CurrentChoiceIndex = index;

        // Update view
        UpdateUI();
    }

    private void SetSettingForCurrentChoice()
    {
        // This is called when choice actually changes, so we want to apply engine changes (call OnSetValue)
        // and save new preference
        SettingsManager.Instance.SetSettingValue(discreteSettingData, m_ChoiceValues[m_CurrentChoiceIndex], immediatelySavePreference: true);
    }
}
