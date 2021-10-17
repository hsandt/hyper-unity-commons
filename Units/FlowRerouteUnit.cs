// Adapted from Bolt Addons Community FlowReroute unit
// https://github.com/RealityStop/Bolt.Addons.Community/blob/master/Runtime/Fundamentals/Units/Utility/FlowReroute.cs
// https://github.com/RealityStop/Bolt.Addons.Community/blob/master/LICENSE (MIT License)
// Unfortunately, with Unity VisualScripting it doesn't appear as compact as with Bolt

using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace CommonsVisualScripting
{
    [UnitCategory("Utility")]
    [UnitTitle("Flow Reroute")]
    [UnitShortTitle("Flow Reroute")]
    public class FlowRerouteUnit : Unit
    {
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlInput input;
        
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlOutput output;
        
        protected override void Definition()
        {
            input = ControlInput("in", flow => output);
            output = ControlOutput("out");
            Succession(input, output);
        }
    }
}