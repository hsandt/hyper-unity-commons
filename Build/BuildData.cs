using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonsHelper
{
	/// Build version data. Create one asset in Resources/Build to let the Build window access it.
	[CreateAssetMenu(fileName = "BuildData", menuName = "Data/Build Data", order = 1)]
	public class BuildData : ScriptableObject
	{
		// Example: My App 3.0.27 (WIP)
		// <= appName: "My App", majorVersion: 3, minorVersion: 0, stageVersion: 27, workInProgress: true
		[Tooltip("Application name used in build path (may differ from product name)")]
		public string appName = "My App";
		
		[Tooltip("Major version in semantic versioning")]
		public int majorVersion = 0;
		
		[Tooltip("Minor version in semantic versioning")]
		public int minorVersion = 1;
		
		[Tooltip("Stage version in semantic versioning")]
		public int stageVersion = 0;
		
		[Tooltip("Check this if you are actively working on this version. This allows the developer to immediately " +
		         "bump version after a release to avoid overwriting the build folder containing the last release, " +
		         "while not giving the impression that the next version is ready already. Uncheck for release.")]
		public bool workInProgress = true;

		public string GetVersionString()
		{
			string baseVersionString = $"v{majorVersion}.{minorVersion}.{stageVersion}";
			
			if (workInProgress)
			{
				baseVersionString += " WIP";
			}

			return baseVersionString;
		}

		public static string GetVersionStringFromResource()
		{
			BuildData buildData = ResourcesUtil.LoadOrFail<BuildData>("Build/BuildData");
			return buildData.GetVersionString();
		}
	}
}

