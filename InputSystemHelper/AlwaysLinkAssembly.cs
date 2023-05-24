using UnityEngine.Scripting;

// We only need to add this to a single file in the CommonsHelper.InputSystemHelper assembly
// to make it link this assembly so [RuntimeInitializeOnLoadMethod] (as well as [InitializeOnLoad] for the editor)
// works properly, and Input Processor and Input Binding Composite are registered correctly in both editor and runtime.
// https://docs.unity3d.com/ScriptReference/Scripting.AlwaysLinkAssemblyAttribute.html
[assembly: AlwaysLinkAssembly]
