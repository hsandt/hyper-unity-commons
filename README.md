# Hyper Unity Commons

A collection of utility scripts for Unity.

I keep this repository to use as Git submodule in my personal and team projects. I consider most scripts to be stable, but I regularly add features and update the API to fit new versions of Unity, or the needs of my current project; sometimes breaking compatibility with previous projects.

For this reason, this is not meant to be used as a strong dependency for projects where I am not part of the team. If you need a more stable, documented utility repository, you can have a look at other projects like https://github.com/Deadcows/MyBox (general) and https://github.com/Unity-UI-Extensions/com.unity.uiextensions (UI).

However, if you found some scripts that would benefit your project in this repository, you're welcome to use them under the current license (see [LICENSE](#License)). Because scripts are under active development, I recommend people who want to use them but who are not working with me to either:

a. clone this repository as submodule, but stick to a certain commit for a given project (or at least pull new commits carefully, paying attention to those flagged "! API BREAKING !")
b. download and adapt individual scripts as needed (just copy the MIT LICENSE along)

Improvement suggestions are welcome. I don't take pull requests at the moment, but you can reach me at hs@gamedesignshortcut.com.

## Dependencies

- Unity.InputSystem
- Unity.TextMeshPro

## Optional assemblies and Scripting Define Symbols

To avoid breaking compilation due to scripts referencing assets/packages that have not been downloaded/imported in your project, we created sub-assemblies for scripts that reference other scripts/assemblies not present in all projects. Those sub-assemblies have Define Constraints, so that they are optional and only activate when certain symbols are defined in the project.

In most cases, the Define Constraints are defined automatically using Version Defines to automatically detect installed package versions (see [documentation](https://docs.unity3d.com/Manual/ScriptCompilationAssemblyDefinitionFiles.html#define-symbols)).

The following packages are optional:

- [UnityExtensions.InspectInline (d4160's fork)](https://github.com/d4160/UnityExtensions.InspectInline) (available on [openupm](https://openupm.com/packages/garettbass.unity-extensions.inspect-in-line/); if present, UNITY_EXTENSIONS_INSPECT_IN_LINE is defined)
- [nl.elraccoone.tweens](https://github.com/jeffreylanters/unity-tweens) (if present, NL_ELRACCOONE_TWEENS is defined)
- [com.eflatun.scenereference](https://github.com/starikcetin/Eflatun.SceneReference) (if present, COM_EFLATUN_SCENEREFERENCE is defined)
- [com.e7.introloop](https://exceed7.com/introloop) (paid asset; if present, COM_E7_INTROLOOP is defined)

However, in the case of assets like DOTween which have a peculiar install process, you must define the Define Constraints manually: go to Project Settings > Player > Script Compilation > Scripting Define Symbols and enter the required symbols in the list.

Below is the list of symbols of define for each optional sub-assembly:

* CommonsHelper.DOTween: To enable scripts in Extensions/DOTween, make sure to install DOTween, and define project symbol: HYPER_UNITY_COMMONS_DOTWEEN

## Optional project settings

To use custom input composite 'Binding With Invertible Modifier' from file InvertibleModifierComposite.cs, you must
enable unsafe code in Project Settings > Player > Other Settings > Script Compilation > Allow 'unsafe' Code.

## License

See [LICENSE](LICENSE) for all original scripts.

Some scripts are based on code snippets found online, sometimes with multiple contributors before myself. In this case, known contributors are listed at the beginning of the file, and the code license effective on the platform at the time the code snippet was posted applies instead (for instance, CC BY-SA 4.0 on Stack Overflow).

Sometimes there is a complete folder copied from another open source project, generally because development stopped so this was the only way to salvage it. In this case, said folder will have its own LICENSE file.

The Runtime/Helper/Collider2D/PolygonColliderSimplification-master-2017-06-09, Runtime/Helper/Types/Unity, Editor/StagPoint, Editor/UnityToolbag folders have their own LICENSE.

* Runtime/Helper/Collider2D/PolygonColliderSimplification-master-2017-06-09 contains a partial frozen copy of third-party repository https://github.com/j-bbr/PolygonColliderSimplification (stripped of ProjectSettings)
* Runtime/Helper/Types/Unity contains files from the com.unity.live-capture package
* UnityToolbag contains a partial copy of https://github.com/kellygravelyn/UnityToolbag
* StagPoint contains a salvaged copy of StagPoint's ImmediateWindow before development stopped and the project was abandoned. It has a copyright, but considering their tool was available for free it was abandoned and made unavailable, I considered it fair use to keep a copy here.
