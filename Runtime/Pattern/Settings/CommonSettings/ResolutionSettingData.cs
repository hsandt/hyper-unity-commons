using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HyperUnityCommons
{
	[CreateAssetMenu(fileName = "ResolutionSettingData", menuName = "Settings/Resolution Setting Data")]
	public class ResolutionSettingData : DiscreteSettingData<Resolution>
	{
		[Tooltip("String format used to display value as text. There must be 3 arguments, representing resp. width, " +
			"height and refresh rate. If empty or whitespace, fallback to Unity's native Resolution.ToString() i.e. " +
			"'{0} x {1} @ {2}Hz', which is also the default value.")]
		public string overrideStringFormat = "{0} x {1} @ {2}Hz";


		public override Resolution GetDefaultValueOnStart()
		{
			// This is called during SettingsManager.Init, so default value is the one set in engine on start
			return Screen.currentResolution;
		}

		public override Resolution GetFallbackValueFrom(Resolution referenceResolution)
		{
			Resolution[] resolutions = Screen.resolutions;

			// Initialize candidate resolution matching width and height with default struct
			// We will know if it has been filled later by checking if any field is not 0
			Resolution dimensionMatchingResolution = default;

			for (int i = 0; i < resolutions.Length; i++)
			{
				Resolution resolution = resolutions[i];

				// Look for the closest match in the available resolutions. Focus on dimensions first.
				if (resolution.width == referenceResolution.width && resolution.height == referenceResolution.height)
				{
					// Now compare refresh rate in case there are multiple ones available
					if (resolution.refreshRate == referenceResolution.refreshRate)
					{
						// We found a perfect match among available resolutions, use it
						return referenceResolution;
					}
					else
					{
						// We found a perfect match for width and height, not resolution, so that's still the second best
						// candidate. Remember it, and pick it unless we find a complete perfect match later (i.e.
						// except for the return above, all other paths leading to return should check
						// if dimensionMatchingResolution has been filled first).
						// Don't bother picking the one with the closest referenceResolution.refreshRate, we'll just
						// remember the last one (so probably the one with biggest refresh rate).
						dimensionMatchingResolution = resolution;
					}
				}
				else if (resolution.width > referenceResolution.width)
				{
					// This is a return path, so check for filled dimensionMatchingResolution first
					if (dimensionMatchingResolution.width > 0)
					{
						return dimensionMatchingResolution;
					}

					// Unity guarantees that resolutions are ordered by width ASC, so we just crossed the width we wanted
					// Pick the resolution just below, so its width will be less than or equal to the reference one
					// If this is the first entry, we cannot pick the previous entry, so clamp the index after decrement
					// Note that this doesn't take height into account, so we may get a pretty different height
					// If you want to fix this, prefer an algorithm that searches available resolution with minimal
					// distance to reference resolution
					int clampedPreviousIndex = Mathf.Max(0, i - 1);
					return resolutions[clampedPreviousIndex];
				}
			}

			// This is a return path, so check for filled dimensionMatchingResolution first
			if (dimensionMatchingResolution.width > 0)
			{
				return dimensionMatchingResolution;
			}

			// We couldn't find a matching resolution, nor one with greater or equal width, so just pick the last one,
			// as it will be the greatest, so still the closest to the reference resolution (at least for width)
			return resolutions[^1];
		}

		public override void OnSetValue(Resolution storedValue)
        {
            // Preserve FullScreen Mode (set via another setting), and set the other 3 settings from Resolution fields
            Screen.SetResolution(storedValue.width, storedValue.height, Screen.fullScreenMode, storedValue.refreshRate);
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
    }
}
