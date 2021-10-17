// Adapted from Bolt Addons Community ValueReroute unit
// https://github.com/RealityStop/Bolt.Addons.Community/blob/master/Runtime/Fundamentals/Units/Utility/ValueReroute.cs
// https://github.com/RealityStop/Bolt.Addons.Community/blob/master/LICENSE (MIT License)
// Unfortunately, with Unity VisualScripting it doesn't appear as compact as with Bolt

using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace CommonsVisualScripting
{
    [UnitCategory("Utility")]
    [UnitTitle("Value Reroute")]
    [UnitShortTitle("Value Reroute")]
    public sealed class ValueReroute : Unit
    {
        [DoNotSerialize]
        [PortLabelHidden]
        public ValueInput input;
        
        [DoNotSerialize]
        [PortLabelHidden]
        public ValueOutput output;
        
        [Serialize]
        public Type portType = typeof(object);

        protected override void Definition()
        {
            input = ValueInput(portType, "in");
            output = ValueOutput(portType, "out", flow => flow.GetValue(input));
            Requirement(input, output);
        }
    }
}