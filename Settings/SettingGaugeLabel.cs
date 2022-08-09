using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

using CommonsHelper;

public class SettingGaugeLabel : Selectable
{
    [Header("Asset references")]

    [Tooltip("Data of setting modified by the associated gauge")]
    public SettingData<float> floatSettingData;


    [Header("External references")]

    [Tooltip("Slider associated to this label")]
    [FormerlySerializedAs("optionGaugeSlider")]
    public Slider gaugeSlider;


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
            Debug.AssertFormat(gaugeSlider != null, this, "[SettingGaugeLabel] No Gauge Slider set on {0}.", this);
            #endif

            gaugeSlider.onValueChanged.AddListener(OnSliderValueChanged);

            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.AssertFormat(floatSettingData != null, this, "[SettingGaugeLabel] No floatSettingData found on {0}.", gameObject);
            #endif

            Setup();
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        gaugeSlider.onValueChanged.RemoveListener(OnSliderValueChanged);
    }

    private void Setup()
    {
        // Initialize visual to match model

        // First, get the readable value from settings: by convention, it's a ratio, so still a normalized value
        float normalizedSliderValue = SettingsManager.Instance.GetFloatSettingAsReadableValue(floatSettingData);

        // In this case we are setting up slider value without user interaction, so to avoid an unnecessary callback
        // OnSliderValueChanged (which could try to set value again, or even play SFX), change value silently.
        // Therefore, we cannot use normalizedValue setter directly, so we will denormalize manually.
        float sliderValue = Mathf.Lerp(gaugeSlider.minValue, gaugeSlider.maxValue, normalizedSliderValue);
        gaugeSlider.SetValueWithoutNotify(sliderValue);
    }

    private void OnSliderValueChanged(float value)
    {
        // Slider value has changed, so update value in settings
        // By convention, it's a ratio, so pass the normalized value
        SettingsManager.Instance.SetFloatSettingFromReadableValue(floatSettingData, gaugeSlider.normalizedValue);
    }

    public override void OnMove(AxisEventData eventData)
    {
        if (gaugeSlider.direction == Slider.Direction.LeftToRight)
        {
            // Capture horizontal move
            if (eventData.moveDir is MoveDirection.Left or MoveDirection.Right)
            {
                // Delegate move to native Slider to change value
                // Rely on registered OnSliderValueChanged to then set the value we need to
                gaugeSlider.OnMove(eventData);
                return;
            }
        }
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        else
        {
            Debug.LogWarningFormat(this, "[SettingGaugeLabel] OnMove: slider has direction {0}, " +
                "only LeftToRight is supported", gaugeSlider.direction);
        }
        #endif

        // Move was not captured, call base implementation to navigate between setting labels and other selectables
        base.OnMove(eventData);
    }
}
