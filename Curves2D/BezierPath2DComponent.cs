using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonsHelper
{

    /// Component containing a BezierPath2D
    public class BezierPath2DComponent : MonoBehaviour
    {
#if UNITY_EDITOR
        [Tooltip("Should the editable path be drawn? Uncheck this or fold the component to hide the editable path. " +
                 "This is also automatically cleared by master components that already draw this as part of their children")]
        public bool shouldDrawEditablePath = true;
#endif
        
        [SerializeField, ReadOnlyField]
        private BezierPath2D path = new BezierPath2D();
        public BezierPath2D Path => path;
    }

}
