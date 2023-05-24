// https://gist.github.com/frarees/9791517
// Original code by frarees
// Changelog from the gist code of 2022-03-12:
// hsandt (2022-05-08): added namespace HyperUnityCommons (only for this repository!)

using System;
using UnityEngine;

namespace HyperUnityCommons
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class MinMaxSliderAttribute : PropertyAttribute
    {
        public float Min { get; set; }
        public float Max { get; set; }
        public bool DataFields { get; set; } = true;
        public bool FlexibleFields { get; set; } = true;
        public bool Bound { get; set; } = true;
        public bool Round { get; set; } = true;

        public MinMaxSliderAttribute() : this(0, 1)
        {
        }

        public MinMaxSliderAttribute(float min, float max)
        {
            Min = min;
            Max = max;
        }
    }
}