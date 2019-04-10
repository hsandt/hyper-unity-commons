using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonsHelper
{

    /// Component containing a BezierPath2D
    public class BezierPath2DComponent : MonoBehaviour
    {
        
        [SerializeField, ReadOnly]
        private BezierPath2D path = null;
        public BezierPath2D Path
        {
            get { return path; }
        }
        
    }

}
