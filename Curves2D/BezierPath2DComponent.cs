using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonsHelper
{

    /// Component containing a BezierPath2D
    public class BezierPath2DComponent : MonoBehaviour
    {
        
        [SerializeField, ReadOnly]
        private BezierPath2D path = new BezierPath2D();
        public BezierPath2D Path => path;
    }

}
