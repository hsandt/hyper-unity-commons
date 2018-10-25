using UnityEngine;
using System;
using System.Collections;

namespace CommonsHelper
{

	/// Internal clock counting down and triggering some optional callback when over
	public class Timer {

	    /* Parameters */

	    /// Function to call when the timer has counted down to 0. Default to null so there is no callback.
		Action callback;


	    /* State vars */

	    /// Remaining time of the timer counting down. When time <=0, the timer is stopped. Default to 0 so the timer starts stopped.
		float time;


		// TODO: add looping boolean parameter for auto-loop
	    public Timer (float _time = 0, Action _callback = null) {
			callback = _callback;
			time = _time;
		}

		/// Set the current time of the Timer
		/// if _timer <= 0: stop the timer
	    /// if _timer > 0: restart the timer until it reaches 0 and triggers callback
		public void SetTime (float _time) {
			time = _time;
		}

	    /// Reset timer to 0 without calling the callback (shortcut for SetTime(0))
		public void Stop () {
			time = 0;
		}

		/// Countdown the time of deltaTime. Must be called by each script containing a timer in its Update or FixedUpdate
		public bool CountDown (float deltaTime) {
			// if time is positive, decrease time of deltaTime
			if (time > 0) {
				time -= deltaTime;
				if (time <= 0) {
					time = 0; // clean-up
	                if (callback != null)
	    				callback();
					return true;
				}
			}
	        // timer was stopped or counted down but didn't reach 0
			return false;
		}

	}

}

