using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// TODO: add pre-build and post-build hooks via callback methods (create another non-static class to override)

public static class Build {

	struct BuildTargetDerivedData {

		public string platformName;				// Raw target name, used as parent folder name: "Windows", "OSX", "Android", etc.
		public string targetName;					// Readable target name: "Windows 64", "OSX 32", "Android", etc.
		public string extension;						// Optional extension (empty for target folders): ".exe", "", ".apk", etc.
		public BuildOptions platformSpecificOptions;	// Extra options added automatically for this platform

		public BuildTargetDerivedData(string platformName, string targetName, string extension = "", BuildOptions platformSpecificOptions = BuildOptions.None) {
			this.platformName = platformName;
			this.targetName = targetName;
			this.extension = extension;
			this.platformSpecificOptions = platformSpecificOptions;
		}

	}

	static Dictionary<BuildTarget, BuildTargetDerivedData> buildTargetDerivedDataDict = new Dictionary<BuildTarget, BuildTargetDerivedData> {
		{ BuildTarget.StandaloneWindows64, new BuildTargetDerivedData("Windows", "Windows 64", ".exe") },
		{ BuildTarget.Android, new BuildTargetDerivedData("Android", "Android", ".apk") },
		{ BuildTarget.iOS, new BuildTargetDerivedData("iOS", "iOS", platformSpecificOptions: BuildOptions.SymlinkLibraries) },
	};

	const BuildOptions autoRunOption = BuildOptions.AutoRunPlayer;
	const BuildOptions developmentOptions = BuildOptions.Development | BuildOptions.AllowDebugging;

	/// Return all the scenes checked in the Build Settings
	static string[] GetScenes () {
		return EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
	}

	/// Build the player for a target, with a build platform name (Windows, OSX, Android, etc.), a build target name (Windows 64, OSX 32, Android),
	/// whether it is a development build, and extra options.
	/// This requires to have a BuildData ScriptableObject asset in some Resources/Build folder.
	static void BuildPlayerWithVersion (BuildTarget buildTarget, bool developmentMode, BuildOptions extraOptions) {
		BuildTargetDerivedData buildTargetDerivedData;
		if (!buildTargetDerivedDataDict.TryGetValue(buildTarget, out buildTargetDerivedData))
		{
			Debug.LogWarningFormat("[Build] Build target {0} has no entry in Build.buildTargetDerivedDataDict. Stop.", buildTarget);
			return;
		}

		BuildData buildData = Resources.Load<BuildData>("Build/BuildData");
		if (buildData == null) {
			Debug.LogWarning("[Build] No BuildData found at Resources/Build/BuildData. Stop.");
			return;
		}

		BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
		buildPlayerOptions.scenes = GetScenes();

		// Example: "Build/Windows/Tactical Ops v3.1.7 - Windows 64 dev.exe"
		buildPlayerOptions.locationPathName = string.Format("Build/{0}/{1} v{2}.{3}.{4} - {5}{6}{7}",
			buildTargetDerivedData.platformName,
			buildData.appName, buildData.majorVersion, buildData.minorVersion, buildData.stageVersion,
			buildTargetDerivedData.targetName,
			developmentMode ? " dev" : "",
			buildTargetDerivedData.extension
			);

		buildPlayerOptions.target = buildTarget;
		buildPlayerOptions.options = autoRunOption | extraOptions;
		if (developmentMode)
			buildPlayerOptions.options |= developmentOptions;

		Debug.LogFormat("Building {0}...", buildPlayerOptions.locationPathName);
		double startTime = EditorApplication.timeSinceStartup;

		string errorMessage = BuildPipeline.BuildPlayer(buildPlayerOptions);

		double endTime = EditorApplication.timeSinceStartup;
		Debug.LogFormat("Finished building {0} {1} in {2:0.00}s", buildPlayerOptions.locationPathName,
			errorMessage == "" ? "successfully" : "with error", endTime - startTime);
	}

	/// Build Windows 64
	[MenuItem("Build/Build Windows 64 (Development)")]
	static void BuildWindows64Development () {
		BuildPlayerWithVersion(BuildTarget.StandaloneWindows64, true, BuildOptions.None);
	}

	/// Build Windows 64
	[MenuItem("Build/Build Windows 64 _F10")]
	static void BuildWindows64 () {
		BuildPlayerWithVersion(BuildTarget.StandaloneWindows64, false, BuildOptions.None);
	}

	// Build Android
	[MenuItem("Build/Build Android (Development)")]
	static void BuildAndroidDevelopment()
	{
		BuildPlayerWithVersion(BuildTarget.Android, true, BuildOptions.None);
	}
	// Build Android
	[MenuItem("Build/Build Android")]
	static void BuildAndroid()
	{
		BuildPlayerWithVersion(BuildTarget.Android, false, BuildOptions.None);
	}
}
