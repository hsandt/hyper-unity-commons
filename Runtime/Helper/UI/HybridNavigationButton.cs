using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HyperUnityCommons
{
    /// Button doesn't support mixing Explicit navigation combined with Automatic/Horizontal/Vertical navigation
    /// as fallback, so this class adds this functionality.
    /// It ignores navigation.mode for directions (it will still check it for different things, such as disable
    /// selection if navigation mode is None in Selectable.OnPointerDown), acting like Explicit for
    /// a given direction when Selectable has been set, and falling back to Automatic detection when not set.
    /// It's not possible to force set no Selectable in a given direction, as null is considered a no-override,
    /// but this should cover most cases of bad Automatic navigation.
    /// The concept is the same as answers on
    /// https://stackoverflow.com/questions/48075615/unity-use-automatic-navigation-in-explicit-navigation
    /// but minimalistic, as it reuses existing selectOnXXX fields.
    /// For a complete directional detection rewrite, see
    /// https://forum.unity.com/threads/better-automatic-navigation-algorithm.1232001/
    public class HybridNavigationButton : Button
    {
        // In each directional method, use selectable set for navigation as an override
        // If not set, fallback to Automatic navigation, but caution: we cannot just use the base implementation,
        // as this one checks navigation.mode. And navigation.mode is often set on Explicit on this component,
        // since Inspector in Normal mode only shows the selectOnXXX fields in Explicit mode.
        // So, to avoid having user to set it back to Automatic each time after editing, we call FindSelectable,
        // the method used in Automatic mode, directly.

        public override Selectable FindSelectableOnLeft()
        {
            return navigation.selectOnLeft != null ? navigation.selectOnLeft : FindSelectable(Vector3.left);
        }

        public override Selectable FindSelectableOnRight()
        {
            return navigation.selectOnRight != null ? navigation.selectOnRight : FindSelectable(Vector3.right);
        }

        public override Selectable FindSelectableOnUp()
        {
            return navigation.selectOnUp != null ? navigation.selectOnUp : FindSelectable(Vector3.up);
        }

        public override Selectable FindSelectableOnDown()
        {
            return navigation.selectOnDown != null ? navigation.selectOnDown : FindSelectable(Vector3.down);
        }
    }
}
