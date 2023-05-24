# Unity Commons - Helper

===

THE REPOSITORY HAS BEEN MERGED WITH OTHER UTILITY REPOSITORIES INTO THE CENTRALIZED REPOSITORY at https://bitbucket.org/hsandt/hyper-unity-commons

It is however preserved so we can still access the version history. We may eventually open a new centralized repository that has both the version history of each original repository, and the latest scripts.

===

A collection of helper scripts for Unity. Includes a few utilities for custom editor scripts.

I keep this repository to use as Git submodule in my personal and team projects. I consider most scripts to be stable,
but I regularly add features and update the API to fit new versions of Unity, or the needs of my current project;
sometimes breaking compatibility with previous projects.

For this reason, I recommend people who are interested in the scripts but not working directly with me to either

* download a frozen copy of master branch and stick to it for a given project
* or download and adapt individual scripts as they need (just copy the MIT LICENSE along)

Improvement suggestions are welcome. I don't take pull requests at the moment, but you can reach me at
hs@gamedesignshortcut.com.

## Optional assemblies and Scripting Define Symbols

To avoid breaking compilation due to scripts referencing assets/packages that have not been downloaded/imported in your project, we created sub-assemblies for scripts that reference other scripts/assemblies not present in all projects. Those sub-assemblies have Define Constraints, so that they are optional and only activate when certain symbols are defined in the project. This way, you can import the Unity Commons Helper package with no compilation errors, install assets and optional packages at your pacing, then define those symbols when you're ready, to start using the optional assemblies.

To define symbols in the project, go to Project Settings > Player > Script Compilation > Scripting Define Symbols and enter them in the list.

Below is the list of symbols of define for each optional sub-assembly:

* CommonsHelper.DOTween: To enable scripts in Extensions/DOTween, make sure to install DOTween, and define project symbol: COMMONS_HELPER_DOTWEEN
* CommonsHelper.InputSystemHelper: To enable scripts in InputSystemHelper, make sure to import Unity's Input System package, and define project symbol: COMMONS_HELPER_INPUT_SYSTEM

## License

See [LICENSE](LICENSE) for all original scripts.

Some scripts are based on code snippets found online, sometimes with multiple contributors before myself. In this case, known contributors are listed at the beginning of the file, and the code license effective on the platform at the time the code snippet was posted applies instead (for instance, CC BY-SA 4.0 on Stack Overflow).
