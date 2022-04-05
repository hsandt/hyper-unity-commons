using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonsHelper
{
    /// Component containing a CatmullRomPath2D
    public class CatmullRomPath2DComponent : Path2DComponent
    {
        [SerializeField, Tooltip("Embedded Catmull-Rom Path. Coordinates are writable to allow numerical placement of " +
             "control points, but do not add/delete/duplicate points with +/- button or right-click command as " +
             "the number of points would become invalid. Instead, use either the Add New Control Point at Origin button " +
             "to add a point, or edit the path visually (see tooltip by hovering Edit Path button). " +
             "If you change Control Points Size manually, make sure to set a number N >= 4.")]
        private CatmullRomPath2D m_Path = new CatmullRomPath2D();
        public CatmullRomPath2D CatmullRomPath => m_Path;

        public override Path2D Path => m_Path;
    }
}
