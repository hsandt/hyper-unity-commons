using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HyperUnityCommons
{
	[CreateAssetMenu(fileName = "ResolutionSettingData", menuName = "Settings/Resolution Setting Data")]
	public class ResolutionSettingData : DiscreteSettingData<Resolution>, IEngineSetting<Resolution>
	{
		[Tooltip("String format used to display value as text. There must be 3 arguments, representing resp. width, " +
			"height and refresh rate. If empty or whitespace, fallback to Unity's native Resolution.ToString() i.e. " +
			"'{0} x {1} @ {2}Hz', which is also the default value.")]
		public string overrideStringFormat = "{0} x {1} @ {2}Hz";


		/* SettingData<Resolution> */

		public override Resolution GetFallbackValueFrom(Resolution referenceResolution)
		{
			Resolution[] resolutions = Screen.resolutions;

			// Min search pattern: search closest resolution using custom distance measurement
			float closestResolutionDistance = float.PositiveInfinity;
			Resolution closestResolution = default;

			foreach (Resolution resolution in resolutions)
			{
				// Define custom distance measurement here
				// Dimensions are the most important, so they use square distance,
				// then refresh rate is secondary, so represent it with a fraction (always lower than dimension
				// distances, since width and height are integers) by dividing by a number of the scale of the highest
				// refresh rates (60Hz is common, 144 Hz is high end and very rarely 240Hz, so 200 is good enough,
				// esp. as differences between dimensions are already pretty big)
				float resolutionDistance = Mathf.Pow(referenceResolution.width - resolution.width, 2f) +
					Mathf.Pow(referenceResolution.height - resolution.height, 2f) +
					Mathf.Abs(referenceResolution.refreshRate - resolution.refreshRate) / 200f;

				// Look for the closest match in the available resolutions. Focus on dimensions first.
				if (resolutionDistance == 0f)
				{
					// We found a perfect match among available resolutions, use it
					return referenceResolution;
				}
				else if (resolutionDistance < closestResolutionDistance)
				{
					closestResolutionDistance = resolutionDistance;
					closestResolution = resolution;
				}
			}

			return closestResolution;
		}

        public override string RepresentedValueToText(Resolution representedValue)
        {
            // Add refresh rate suffix, unless not indicated (value 0)
            if (!string.IsNullOrWhiteSpace(overrideStringFormat))
            {
	            return string.Format(overrideStringFormat,
		            representedValue.width, representedValue.height, representedValue.refreshRate);
            }
            else
            {
	            // No string format override, use default conversion
	            // (can also return base.RepresentedValueToText(representedValue))
	            return representedValue.ToString();
            }
        }


        /* DiscreteSettingData<Resolution> */

        public override List<Resolution> GetAvailableValues()
        {
            Resolution[] resolutions = Screen.resolutions;

            #if UNITY_ANDROID || UNITY_IOS
            DebugUtil.AssertFormat(resolutions.Length > 0,
                "[ResolutionSettingData] GetAvailableValues: no resolutions available. This is expected on a " +
                "mobile platform, so avoid displaying Resolution setting on mobile.");
            #else
            DebugUtil.AssertFormat(resolutions.Length > 0,
                "[ResolutionSettingData] GetAvailableValues: no resolutions available, yet we are not on mobile.");
            #endif

            // Show refresh rate if meaningful (not 0)
            return resolutions.ToList();
        }


        /* IEngineSetting */

        public Resolution GetValue()
        {
	        // Screen.currentResolution returns native display resolution in any windowed mode (including fullscreen
	        // window), so this is not what we want here. Instead, we must get dimensions from Screen.width/height.
	        // We need the refresh rate though, so this one is extracted from Screen.currentResolution.
	        // See https://forum.unity.com/threads/screen-setresolution-not-changing-resolution.654817/
	        return new Resolution
	        {
		        width = Screen.width,
		        height = Screen.height,
		        refreshRate = Screen.currentResolution.refreshRate
	        };
        }

        public void SetValue(Resolution resolution)
        {
	        #if UNITY_EDITOR
	        DebugUtil.LogFormat("[FullScreenSettingData] Set screen resolution to {0} (ignored in Editor)", resolution);
	        #endif

	        // Preserve FullScreen Mode (set via another setting), and set the other 3 settings from Resolution fields
	        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode, resolution.refreshRate);
        }
	}
}
