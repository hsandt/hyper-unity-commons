using UnityEngine;
using System;
using System.Collections;

/// Internal clock counting down and triggering some callback when over
class Timer {

	private Action callback; // delegate with no parameters and no return value

	private float time; // current time on the internal clock

	public Timer (Action _callback, float _time = 0) {
		callback = _callback;
		time = _time; // default value 0 to start with a stopped timer
	}

	/// Set the current time of the Timer
	/// null or negative value: stop the timer
	/// positive value: relaunch the timer until it reaches 0 and triggers callback
	public void SetTime (float _time) {
		time = _time;
	}

	/// Reset timer to 0 without calling the callback
	public void Stop () {
		time = 0;
	}

	// alternative: use Timer : MonoBehavior + FixedUpdate
	// alternative 2: use a TimerManager that knows each Timer object and updates them
	/// Countdown called by each script containing a timer, in its Update or FixedUpdate
	public void CountDown (float deltaTime) {
		// if time is positive, decrease time of deltaTime
		// (if time already 0, leave it so)
		if (time > 0) {
			time -= deltaTime;
			// if the countdown has reached 0 (or less), callback
			if (time <= 0) {
				time = 0; // optional
				callback();
			}
		}

	}

}

// TODO: looped countdown (repeat event); basicaller a Timer with a maxTime and time = maxTime when it reaches 0
