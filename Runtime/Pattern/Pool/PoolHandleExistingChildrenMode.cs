using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HyperUnityCommons
{
    /// Enum determining how to handle children already set up under a pool transform in the scene
    /// (generally for previewing purpose)
    /// Note that all existing children to be use should have the correct IPooledObject component,
    /// and all unused children should be destroyed on initialization to avoid desync between child count and
    /// pooled object count.
    public enum PoolHandleExistingChildrenMode
    {
        /// Register all existing children as valid pooled objects
        /// The children should have the correct IPooledObject component. This doesn't verify that the children are
        /// actually like new pooled object prefab instances would be, so be careful with bad instance values,
        /// especially UI Transform overrides that prevent usage of UI prefab properties after change.
        UseAllExistingChildren,
        /// Register all children that are active (self) under the pool transform, and destroy all inactive children
        /// Like UseAllChildren, the active children should have the correct IPooledObject component, and this doesn't
        /// guarantee the same properties as new pooled object prefab instances.
        UseActiveExistingChildren,
        /// Don't register existing children at all, and destroy them all before creating proper pooled objects
        /// Use this when you have objects too different from the pooled object prefab, or if you don't want to rely on
        /// preview object values, e.g. because of bad override UI Transform properties
        [InspectorName("Don't Use Children")]
        DontUseExistingChildren
    }
}
