using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using HyperUnityCommons;

/// Component to place on label associated to a setting arrow choice
/// Requires: AppManager, SettingsManager
/// SEO: after SettingsManager
public abstract class SettingArrowChoiceLabel<TSettingValue> : Selectable
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
            Setup();
        }
    }

    protected override void OnDisable()
    {
        if (AppManager.IsNotQuitting())
        {
            base.OnDisable();
        }

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
    private void Setup()
    {
        // At this point, SettingsManager must have loaded/initialized all settings (from either prefs or defaults)
        // so it's safe to use the current setting value (it will also retrieve engine value if set to
        // alwaysCheckEngineValue)
        TSettingValue currentSettingValue = SettingsManager.Instance.GetSettingValue(discreteSettingData);

        // Remember we are working with a choice index, not value, so retrieve index from value
        int currentChoiceIndexFromValue = FindChoiceIndex(currentSettingValue);

        if (currentChoiceIndexFromValue < 0)
        {
            DebugUtil.LogWarningFormat(discreteSettingData,
                "[SettingArrowChoiceLabel] Setup: could not find choice index {0} for setting {1} on widget {2}. " +
                "This should not happen in normal usage, as LoadSimpleSettingFromPreferences/LoadResolutionSettingFromPreferences " +
                "should use GetFallbackValueFrom if value from preference was invalid, or GetDefaultValueOnStart if there " +
                "was no preference, and both should guarantee a valid value. It can happen in rare cases when user " +
                "changes hardware during the game (e.g. switching to monitor with fewer resolutions), so trying to fall " +
                "back to closest value with GetFallbackValueFrom.",
                currentSettingValue, discreteSettingData, this);

            // Looks like the old choice is not valid anymore, it can happen (see log message below)
            // In this case, let's revert to a good fallback, assumed to be close enough to the current setting.
            TSettingValue fallbackSettingValue = discreteSettingData.GetFallbackValueFrom(currentSettingValue);
            currentChoiceIndexFromValue = FindChoiceIndex(fallbackSettingValue);

            if (currentChoiceIndexFromValue < 0)
            {
                DebugUtil.LogErrorFormat(discreteSettingData, "[SettingArrowChoiceLabel] Setup: " +
                    "... fallback failed, could not find choice index for fallback value {0}. " +
                    "Trying default value (using current engine state if needed)",
                    fallbackSettingValue);

                TSettingValue defaultSettingValue = discreteSettingData.GetDefaultValueOnStart();
                currentChoiceIndexFromValue = FindChoiceIndex(defaultSettingValue);

                if (currentChoiceIndexFromValue < 0)
                {
                    DebugUtil.LogErrorFormat(discreteSettingData, "[SettingArrowChoiceLabel] Setup: " +
                        "... default failed, could not find choice index for default value {0}. " +
                        "Fall back to choice index 0 as ultimate resort.",
                        defaultSettingValue);

                    currentChoiceIndexFromValue = 0;
                }
            }
        }

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

    /// Select a choice by index
    private void SelectChoice(int index)
    {
        if (index == m_CurrentChoiceIndex)
        {
            return;
        }

        if (m_CurrentChoiceIndex < 0 || m_CurrentChoiceIndex > m_ChoiceNames.Count - 1) {
            Debug.LogErrorFormat(this,
                "[SettingArrowChoiceLabel] SelectChoice: index {0} is out of bounds ({1} entries)",
                m_CurrentChoiceIndex, m_ChoiceNames.Count);
            return;
        }

        SelectChoice_Internal(index);
    }

    private void SelectChoice_Internal(int index)
    {
        // Set choice index
        m_CurrentChoiceIndex = index;

        // Set associated setting
        SettingsManager.Instance.SetSetting(discreteSettingData, m_ChoiceValues[index]);

        // Update view
        UpdateUI();
    }
}
