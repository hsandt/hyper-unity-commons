using System;
using UnityEngine;
using Vexe.Runtime.Types;

public class MinMaxSliderVexeAttribute : DrawnAttribute {

	/// Min of the slider itself
	public readonly float max;
	/// Max of the slider itself
	public readonly float min;

	public MinMaxSliderVexeAttribute (float min, float max) {
		this.min = min;
		this.max = max;
	}
}
