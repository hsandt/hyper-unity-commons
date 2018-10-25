using System;

namespace CommonsHelper
{

	public class ControlUtil
	{
		/// Return the boolean value passed by reference and reset it to false.
		/// Use this with a buffered controller boolean input (GetButtonDown, GetButtonUp) or a control intention (jumpIntention, etc.) if you don't use an input buffer
		/// Note that using intention directly without input buffer may result in delaying an intention until it is consumed (e.g. jumping intention delayed until landing)
		public static bool ConsumeBool(ref bool value) {
			if (value) {
				value = false;
				return true;
			} else {
				return false;
			}
		}
	}

}
