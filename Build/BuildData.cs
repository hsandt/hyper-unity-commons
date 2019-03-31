using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonsHelper
{

	/// Build version data. Create one asset in Resources/Build.
	[CreateAssetMenu(fileName = "BuildData", menuName = "Data/Build Data", order = 1)]
	public class BuildData : ScriptableObject {

		// Example: My App 3.0.27 -> appName: "My App", majorVersion: 3, minorVersion: 0, stageVersion: 27
		public string appName = "My App";
		public int majorVersion = 0;
		public int minorVersion = 0;
		public int stageVersion = 1;

		public string GetVersionString() {
			return $"v{majorVersion}.{minorVersion}.{stageVersion}";
		}

		public static string GetVersionStringFromResource() {
			BuildData buildData = ResourcesUtil.LoadOrFail<BuildData>("Build/BuildData");
			return buildData.GetVersionString();
		}

	}

}

