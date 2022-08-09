using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using CommonsHelper;

public class OptionGaugeLabel : Selectable
{
    [Header("Asset references")]

    [Tooltip("Data of setting modified by the associated gauge")]
    public SettingData<float> floatSettingData;


    [Header("External references")]

    [Tooltip("Slider associated to this label")]
    public Slider optionGaugeSlider;


    /* Sibling components */

    /// Value handler for this setting


    // Bug IN-10813: Awake is not called on Play, only on Stop (in editor), when inheriting from Selectable
    // Start sometimes work, but is not reliable either.
    // So we use OnEnable instead, but then must do symmetrical operations in OnDisable
    protected override void OnEnable()
    {
        base.OnEnable();

        // In the editor, on Application Quit, OnEnable is called again to setup values in the editor,
        // but we do not want to touch the slider and modify the scene! So we check that we are not exiting Play Mode
        // (which would often mean MainMenuManager instance has been destroyed, and to be extra safe we also check that
        // we are not quitting play mode if it is not destroyed yet).
        // This should be unnecessary for builds, but the check makes sense for builds anyway so we didn't bother
        // surrounding the condition with #if UNITY_EDITOR here.
        if (AppManager.IsNotQuitting())
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.AssertFormat(optionGaugeSlider != null, this, "[OptionGaugeLabel] No Option Gauge Slider set on {0}.", this);
            #endif

            optionGaugeSlider.onValueChanged.AddListener(OnSliderValueChanged);

            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.AssertFormat(floatSettingData != null, this, "[OptionGaugeLabel] No floatSettingData found on {0}.", gameObject);
            #endif

            // Setup could be done in OptionsMenu (need to reference all option widgets), but for now just do it in OnEnable
            Setup();
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        optionGaugeSlider.onValueChanged.RemoveListener(OnSliderValueChanged);
    }

    private void Setup()
    {
        // Initialize visual to match model
        // In this case we are setting slider value to represent actual value, so to avoid an unnecessary callback
        // trying to change current value to what it already is, change value silently
        optionGaugeSlider.SetValueWithoutNotify( SettingsManager.Instance.GetFloatSettingAsReadableValue(floatSettingData));
    }

    private void OnSliderValueChanged(float value)
    {
        // This is only called on Application Quit if OnEnable called AddListener, which we already prevented
        // when exiting Play Mode, so no need to add the check here too.
        SettingsManager.Instance.SetFloatSettingFromReadableValue(floatSettingData, optionGaugeSlider.value);
    }

    public override void OnMove(AxisEventData eventData)
    {
        if (optionGaugeSlider.direction == Slider.Direction.LeftToRight)
        {
            // Capture horizontal move
            if (eventData.moveDir is MoveDirection.Left or MoveDirection.Right)
            {
                // Delegate move to native Slider to change value
                // Rely on registered OnSliderValueChanged to then set the value we need to
                optionGaugeSlider.OnMove(eventData);
                return;
            }
        }
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        else
        {
            Debug.LogWarningFormat(this, "[OptionGaugeLabel] OnMove: slider has direction {0}, " +
                "only LeftToRight is supported", optionGaugeSlider.direction);
        }
        #endif

        // Move was not captured, call base implementation to navigate between option labels
        base.OnMove(eventData);
    }
}
