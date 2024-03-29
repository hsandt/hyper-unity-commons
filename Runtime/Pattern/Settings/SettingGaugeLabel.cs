using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

using HyperUnityCommons;

/// Component to place on label associated to a setting gauge slider
/// Requires: AppManager, SettingsManager
/// SEO: after SettingsManager
public class SettingGaugeLabel : BaseSettingLabel
{
    [Header("Asset references")]

    [Tooltip("Data of continuous setting modified by the associated gauge")]
    public ContinuousSettingData<float> floatSettingData;

    [Tooltip("(Optional) SFX played when moving the gauge slider (via either directional input or mouse drag). " +
        "This is subject to throttling to avoid SFX spam when moving via mouse drag submitting. " +
        "Useful to feedback SFX or voice volume change.")]
    public AudioClip sfxSliderMove;


    [Header("External references")]

    [Tooltip("Slider of gauge associated to this label")]
    [FormerlySerializedAs("optionGaugeSlider")]
    public Slider gaugeSlider;


    /* UIBehaviour */

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (gaugeSlider != null)
        {
            gaugeSlider.onValueChanged.RemoveAllListeners();
        }
    }


    /* BaseSettingData */

    public override BaseSettingData GetSettingData()
    {
        return floatSettingData;
    }

    public override void SetSettingData(BaseSettingData settingData)
    {
        floatSettingData = settingData as ContinuousSettingData<float>;

        DebugUtil.AssertFormat(floatSettingData != null,
            "[SettingGaugeLabel] SetSettingData: passed setting data {0} is not a ContinuousSettingData<float>",
            settingData);
    }

    protected override void OnInit()
    {
        DebugUtil.AssertFormat(gaugeSlider != null, this, "[SettingGaugeLabel] No Gauge Slider set on {0}.", this);

        gaugeSlider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    /// Initialize visual to match model, i.e. slider to show current setting as readable value
    public override void Setup()
    {
        // First, get the readable value from settings: by convention, it's a ratio, so still a normalized value
        float normalizedSliderValue = SettingsManager.Instance.GetSettingAsReadableValue(floatSettingData);

        // Second, set slider value
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
        SettingsManager.Instance.SetSettingFromReadableValue(floatSettingData, gaugeSlider.normalizedValue);

        if (sfxSliderMove != null)
        {
            UISfxPoolManager.Instance.PlaySfx(sfxSliderMove, useThrottle: true,
                context: this, debugClipName: "sfxSliderMove");
        }
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
                eventData.Use();
                return;
            }
        }
        else
        {
            DebugUtil.LogWarningFormat(this, "[SettingGaugeLabel] OnMove: slider has direction {0}, " +
                "only LeftToRight is supported", gaugeSlider.direction);
        }

        // Move was not captured, call base implementation to navigate between setting labels and other selectables
        base.OnMove(eventData);
    }
}
