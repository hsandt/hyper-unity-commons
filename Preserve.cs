using UnityEngine.Scripting;

// We only need to add this to a single file in the CommonsVisualScripting assembly
// to make it link this assembly *and* preserve all types and members inside it.
// This is a trick to avoid having to define link.xml in every project using the custom Units
// defined in CommonsVisualScripting, at the (relatively low) cost of never stripping any custom Units,
// even if they are not used in the project.
// See https://docs.unity3d.com/2022.1/Documentation/Manual/ManagedCodeStripping.html
[assembly: AlwaysLinkAssembly, Preserve]
