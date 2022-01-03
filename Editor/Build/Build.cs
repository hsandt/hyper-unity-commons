using UnityEngine;
using UnityEditor;
using Reporting = UnityEditor.Build.Reporting;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// TODO: add pre-build and post-build hooks via callback methods (create another non-static class to override)
// It could be useful e.g. to delete debug objects when building for release, or platform-specific objects when
// building for other platforms (the EditorOnly tag is not enough for these particular cases)

namespace CommonsHelper.Editor
{

	public static class Build {

		/// Path to expected BuildData asset inside some Resources folder, without ".asset"
		/// (important to work with Resources.Load)
		private const string buildDataPathInResources = "Build/BuildData";

		/// Path to Resources folder where new BuildData will be created if no BuildData is found
		private const string defaultResourcesDirectoryPath = "Resources";

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
			{ BuildTarget.iOS, new BuildTargetDerivedData("iOS", "iOS", platformSpecificOptions: BuildOptions.SymlinkSources) },
			{ BuildTarget.WebGL, new BuildTargetDerivedData("WebGL", "WebGL") },
		};

		const BuildOptions autoRunOption = BuildOptions.AutoRunPlayer;
		
		// options for dev build on Standalone (PC and mobile)
		const BuildOptions standAloneDevelopmentOptions = BuildOptions.Development | BuildOptions.AllowDebugging;
		
		// WebGL dev build only should not rely on AllowDebugging aka Script Debugging
		// (despite being only visible for Standalone in Build Settings UI), so don't use this option on WebGL dev build
		// See https://forum.unity.com/threads/cannot-build-in-development-mode.691183/#post-6793151
		// Instead, we need to Connect to Host or it will fail to run (and it must be run with localhost)
		const BuildOptions webGLDevelopmentOptions = BuildOptions.Development | BuildOptions.ConnectToHost;

		/// Return all the scenes checked in the Build Settings
		static string[] GetScenes () {
			return EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
		}

		/// Build the player for a target, with a build platform name (Windows, OSX, Android, etc.), a build target name (Windows 64, OSX, Android),
		/// whether it is a development build, and extra options (not used in this script, but useful for command line scripts using Unity headless mode)
		/// This requires to have a BuildData ScriptableObject asset in some Resources/Build folder.
		public static void BuildPlayerWithVersion (BuildTarget buildTarget, bool developmentMode, BuildOptions extraOptions = BuildOptions.None) {
			if (!buildTargetDerivedDataDict.TryGetValue(buildTarget, out var buildTargetDerivedData))
			{
				Debug.LogWarningFormat("[Build] Build target {0} has no entry in Build.buildTargetDerivedDataDict. Stop.", buildTarget);
				return;
			}

			BuildTargetGroup buildTargetGroup = BuildPipeline.GetBuildTargetGroup(buildTarget);

			BuildData buildData = Resources.Load<BuildData>(buildDataPathInResources);
			if (buildData == null)
			{
				string fullBuildDataPath = Path.Combine("Assets", defaultResourcesDirectoryPath, buildDataPathInResources);
				Debug.Log($"[Build] No BuildData found at any Resources/{buildDataPathInResources}. Creating one at {fullBuildDataPath}.");

				// create directory recursively if it doesn't exist yet
				string buildDataDirectory = Path.GetDirectoryName(fullBuildDataPath);
				if (!Directory.Exists(buildDataDirectory))
				{
					Directory.CreateDirectory(buildDataDirectory);
				}

				// create missing BuildData asset
				buildData = ScriptableObject.CreateInstance<BuildData>();
				buildData.appName = PlayerSettings.productName;
				// AssetDatabase.CreateAsset needs extension .asset to create with correct file name
				AssetDatabase.CreateAsset(buildData, $"{fullBuildDataPath}.asset");
			}

			// Example: "Tactical Ops v3.1.7 (WIP) - Windows 64 dev"
			string baseName = $"{buildData.appName} {buildData.GetVersionString()} - " +
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
			
			// store original config to restore after build (and avoid unwanted changes in Player Settings that will show in VCS)
			var originalScriptingBackend = PlayerSettings.GetScriptingBackend(buildTargetGroup);
			var originalIl2CppCompilerConfiguration = PlayerSettings.GetIl2CppCompilerConfiguration(buildTargetGroup);
			var originalManagedStrippingLevel = PlayerSettings.GetManagedStrippingLevel(buildTargetGroup);

			if (developmentMode)
			{
				// Debug/Development build: do not optimize for faster build iterations

				if (buildTarget != BuildTarget.WebGL)
				{
					buildPlayerOptions.options |= standAloneDevelopmentOptions;
					
					// use Mono for faster build
					PlayerSettings.SetScriptingBackend(buildTargetGroup, ScriptingImplementation.Mono2x);
				}
				else
				{
					buildPlayerOptions.options |= webGLDevelopmentOptions;
					
					// WebGL uses WebAssembly anyway, so at least set C++ config to debug
					PlayerSettings.SetIl2CppCompilerConfiguration(buildTargetGroup, Il2CppCompilerConfiguration.Debug);
				}

				// Pass stripping level for development build, which has been made tunable as different project
				// setups allow different levels of stripping.
				// Since we store stripping level with a custom enum in Build Data to make it compile for runtime,
				// we must cast it to UnityEditor.ManagedStrippingLevel. It works, because enum values are ordered
				// exactly the same way.
				ManagedStrippingLevel managedStrippingLevel = (ManagedStrippingLevel) buildData.devBuildStrippingLevel;
				PlayerSettings.SetManagedStrippingLevel(buildTargetGroup, managedStrippingLevel);
			}
			else
			{
				// Release build

				bool useIL2CPP;
				
				// WebGL uses WebAssembly anyway so no need to set IL2CPP
				if (buildTarget == BuildTarget.WebGL)
				{
					useIL2CPP = true;
				}
				else
				{
					// only try to build IL2CPP if target platform IL2CPP is supported on editor platform
					// else, use Mono
					// currently, we only know that a given editor platform supports itself as IL2CPP target,
					//  and in addition, Windows can build IL2CPP for Linux (Unity 2020+)
					// make sure to install any IL2CPP modules available for your version of Unity
					bool shouldBuildIL2CPP;
					
					// Editor must be running on one of those, so editorPlatform should be defined
#if UNITY_EDITOR_WIN
#	if UNITY_2020_1_OR_NEWER
					shouldBuildIL2CPP = buildTarget == BuildTarget.StandaloneWindows64 || buildTarget == BuildTarget.StandaloneLinux64;
#	else
					shouldBuildIL2CPP = buildTarget == BuildTarget.StandaloneWindows64;
#	endif
#elif UNITY_EDITOR_OSX
					shouldBuildIL2CPP = buildTarget == BuildTarget.StandaloneOSX;
#elif UNITY_EDITOR_LINUX
					shouldBuildIL2CPP = buildTarget == BuildTarget.StandaloneLinux64;
#endif
					if (shouldBuildIL2CPP)
					{  
						PlayerSettings.SetScriptingBackend(buildTargetGroup, ScriptingImplementation.IL2CPP);
						useIL2CPP = true;
					}
					else
					{
						PlayerSettings.SetScriptingBackend(buildTargetGroup, ScriptingImplementation.Mono2x);
						useIL2CPP = false;
					}
				}

				if (useIL2CPP)
				{
					// unfortunately, IL2CPP Master build fails on Linux, so we stick to Release even for non-development mode
					// if Master works on Windows/WebGL, though, consider using it for their Releases
					PlayerSettings.SetIl2CppCompilerConfiguration(buildTargetGroup, Il2CppCompilerConfiguration.Release);
				}
				
				// Same remark as with dev build, but we pass the release config this time
				ManagedStrippingLevel managedStrippingLevel = (ManagedStrippingLevel) buildData.releaseBuildStrippingLevel;
				
				// IL2CPP needs at least Low stripping level
				if (useIL2CPP && managedStrippingLevel == ManagedStrippingLevel.Disabled)
				{
					Debug.LogWarning("[Build] Release build done with IL2CPP, which needs at least Low stripping level, " +
					                 "but stripping level is set to Disabled. Automatically setting it to Low.");
					managedStrippingLevel = ManagedStrippingLevel.Low;
				}
				PlayerSettings.SetManagedStrippingLevel(buildTargetGroup, managedStrippingLevel);
			}

			// Note: at this point, PlayerSettings have been changed, so unlike passing BuildPlayerOptions this has
			// a side-effect on the current configuration.

			Debug.LogFormat("Building {0}...", buildPlayerOptions.locationPathName);

			Reporting.BuildReport buildReport = BuildPipeline.BuildPlayer(buildPlayerOptions);
			Reporting.BuildSummary buildSummary = buildReport.summary;

			Debug.LogFormat(@"Build result: {0} (took {3:hh\:mm\:ss} from {1} to {2})", buildSummary.result,
				buildSummary.buildStartedAt, buildSummary.buildEndedAt, buildSummary.buildEndedAt - buildSummary.buildStartedAt);

			// restore original settings
			if (buildTarget != BuildTarget.WebGL)
			{
				PlayerSettings.SetScriptingBackend(buildTargetGroup, originalScriptingBackend);
			}
			
			PlayerSettings.SetIl2CppCompilerConfiguration(buildTargetGroup, originalIl2CppCompilerConfiguration);
			PlayerSettings.SetManagedStrippingLevel(buildTargetGroup, originalManagedStrippingLevel);
		}

		/// Build Windows 64
		[MenuItem("Build/Build Windows 64")]
		static void BuildWindows64 () {
			// safety check to avoid building by accident while playing
			// (in particular because F10 is used as a debugging key in some IDEs)
			if (!Application.isPlaying)
			{
				BuildPlayerWithVersion(BuildTarget.StandaloneWindows64, false);
			}
		}

		/// Build Windows 64 development
		[MenuItem("Build/Build Windows 64 (Development)")]
		static void BuildWindows64Development () {
			BuildPlayerWithVersion(BuildTarget.StandaloneWindows64, true);
		}

		/// Build OS X
		[MenuItem("Build/Build OS X")]
		static void BuildOSX () {
			if (!Application.isPlaying)
			{
				BuildPlayerWithVersion(BuildTarget.StandaloneOSX, false);
			}
		}

		/// Build OS X development
		[MenuItem("Build/Build OS X (Development)")]
		static void BuildOSXDevelopment () {
			BuildPlayerWithVersion(BuildTarget.StandaloneOSX, true);
		}

		/// Build Linux
		[MenuItem("Build/Build Linux 64")]
		static void BuildLinux64 () {
			if (!Application.isPlaying)
			{
				BuildPlayerWithVersion(BuildTarget.StandaloneLinux64, false);
			}
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
