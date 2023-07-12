/// Enum defining how to search components on a given game object and its relatives
/// Use this in scripts that can automate component search with GetComponents or GetComponentsInChildren
public enum SearchComponentsMode
{
    /// Do not search
    None,
    /// Only search components on the same game object
    Self,
    /// Search components on the same game object and all its children, recursively
    SelfAndChildren,
}
