using UnityEngine;
using UnityEditor;
using Reporting = UnityEditor.Build.Reporting;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// TODO: add pre-build and post-build hooks via callback methods (create another non-static class to override)

namespace CommonsHelper.Editor
{

	public static class Build {

		struct BuildTargetDerivedData {

			public string platformName;						// Raw target name, used as parent folder name: "Windows", "OSX", "Android", etc.
			public string targetName;						// Readable target name: "Windows 64", "OSX", "Android", etc.
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
			{ BuildTarget.StandaloneOSX, new BuildTargetDerivedData("OSX", "OSX", ".app") },
			{ BuildTarget.StandaloneLinux64, new BuildTargetDerivedData("Linux", "Linux 64", ".x86_64") },
			{ BuildTarget.Android, new BuildTargetDerivedData("Android", "Android", ".apk") },
			{ BuildTarget.iOS, new BuildTargetDerivedData("iOS", "iOS", platformSpecificOptions: BuildOptions.SymlinkLibraries) },
			{ BuildTarget.WebGL, new BuildTargetDerivedData("WebGL", "WebGL") },
		};

		const BuildOptions autoRunOption = BuildOptions.AutoRunPlayer;
		const BuildOptions developmentOptions = BuildOptions.Development | BuildOptions.AllowDebugging;

		/// Return all the scenes checked in the Build Settings
		static string[] GetScenes () {
			return EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
		}

		/// Build the player for a target, with a build platform name (Windows, OSX, Android, etc.), a build target name (Windows 64, OSX, Android),
		/// whether it is a development build, and extra options (not used in this script, but useful for command line scripts using Unity headless mode)
		/// This requires to have a BuildData ScriptableObject asset in some Resources/Build folder.
		public static void BuildPlayerWithVersion (BuildTarget buildTarget, bool developmentMode, BuildOptions extraOptions = BuildOptions.None) {
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

			// Example: "Tactical Ops v3.1.7 - Windows 64 dev"
			string baseName = $"{buildData.appName} v{buildData.majorVersion}.{buildData.minorVersion}.{buildData.stageVersion} - " +
				$"{buildTargetDerivedData.targetName}{(developmentMode ? " dev" : "")}";
			
			// For build configs generating an executable file (and exceptionally an .app folder on OSX),
			// build inside a directory with the same name as the executable basename.
			// This allows better build isolation and facilitates correct folder/zip uploading (e.g. on Windows, we need to upload UnityPlayer.dll).
			// Example of locationPathName: "Build/Windows/Tactical Ops v3.1.7 - Windows 64 dev/Tactical Ops v3.1.7 - Windows 64 dev.exe"
			
			// For build configs generating a whole folder, do not add an extra level of directory.
			// Example of locationPathName: "Build/WebGL/Tactical Ops v3.1.7 - WebGL"
			
			string extraParentDir = string.IsNullOrEmpty(buildTargetDerivedData.extension) ? "" : $"{baseName}/";
			
			BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
			{
				scenes = GetScenes(),
				locationPathName = $"Build/{buildTargetDerivedData.platformName}/{extraParentDir}{baseName}{buildTargetDerivedData.extension}",
				target = buildTarget,
				options = autoRunOption | buildTargetDerivedData.platformSpecificOptions | extraOptions
			};


			if (developmentMode)
				buildPlayerOptions.options |= developmentOptions;

			Debug.LogFormat("Building {0}...", buildPlayerOptions.locationPathName);

			Reporting.BuildReport buildReport = BuildPipeline.BuildPlayer(buildPlayerOptions);
			Reporting.BuildSummary buildSummary = buildReport.summary;
			
			Debug.LogFormat("Build result: {0} ({3:c} from {1} to {2})", buildSummary.result,
				buildSummary.buildStartedAt, buildSummary.buildEndedAt, (buildSummary.buildEndedAt - buildSummary.buildStartedAt));
		}
		
		/// Build Windows 64
	# if UNITY_EDITOR_WIN
		[MenuItem("Build/Build Windows 64 _F10")]
	#else
		[MenuItem("Build/Build Windows 64")]
	#endif
		static void BuildWindows64 () {
			BuildPlayerWithVersion(BuildTarget.StandaloneWindows64, false);
		}

		/// Build Windows 64 development
		[MenuItem("Build/Build Windows 64 (Development)")]
		static void BuildWindows64Development () {
			BuildPlayerWithVersion(BuildTarget.StandaloneWindows64, true);
		}

		/// Build OS X
	# if UNITY_EDITOR_OSX
		[MenuItem("Build/Build OS X _F10")]
	#else
		[MenuItem("Build/Build OS X")]
	#endif
		static void BuildOSX () {
			BuildPlayerWithVersion(BuildTarget.StandaloneOSX, false);
		}

		/// Build OS X development
		[MenuItem("Build/Build OS X (Development)")]
		static void BuildOSXDevelopment () {
			BuildPlayerWithVersion(BuildTarget.StandaloneOSX, true);
		}

		/// Build Linux
		# if UNITY_EDITOR_LINUX
		[MenuItem("Build/Build Linux 64 _F10")]
		#else
		[MenuItem("Build/Build Linux 64")]
		#endif
		static void BuildLinux64 () {
			BuildPlayerWithVersion(BuildTarget.StandaloneLinux64, false);
		}

		/// Build Linux development
		[MenuItem("Build/Build Linux 64 (Development)")]
		static void BuildLinux64Development () {
			BuildPlayerWithVersion(BuildTarget.StandaloneLinux64, true);
		}

		/// Build Android
		[MenuItem("Build/Build Android")]
		static void BuildAndroid()
		{
			BuildPlayerWithVersion(BuildTarget.Android, false);
		}

		/// Build Android development
		[MenuItem("Build/Build Android (Development)")]
		static void BuildAndroidDevelopment()
		{
			EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
			BuildPlayerWithVersion(BuildTarget.Android, true);
		}

		/// Build WebGL
		[MenuItem("Build/Build WebGL")]
		static void BuildWebGL()
		{
			BuildPlayerWithVersion(BuildTarget.WebGL, false);
		}

		/// Build WebGL development
		[MenuItem("Build/Build WebGL (Development)")]
		static void BuildWebGLDevelopment()
		{
			BuildPlayerWithVersion(BuildTarget.WebGL, true);
		}

	}

}

