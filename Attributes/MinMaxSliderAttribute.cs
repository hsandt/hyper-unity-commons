// https://gist.github.com/frarees/9791517
using System;
using UnityEngine;

public class MinMaxSliderAttribute : PropertyAttribute {

	/// Min of the slider itself
	public readonly float max;
	/// Max of the slider itself
	public readonly float min;

	public MinMaxSliderAttribute (float min, float max) {
		this.min = min;
		this.max = max;
	}
}
