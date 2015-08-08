using UnityEngine;
using System;
using System.Collections;

/* Internal clock counting down and triggering some callback when over */
class Timer {

	private Action callback; // delegate with no parameters and no return value

	private float time; // current time on the internal clock

	public Timer (Action _callback, float _time = 0) {
		callback = _callback;
		time = _time; // default value 0 to start with a stopped timer
	}

	/* set the current time of the Timer
	 * null or negative value: stop the timer
	 * positive value: relaunch the timer until it reaches 0 and triggers callback */
	public void setTime(float _time) {
		time = _time;
	}

	/* countdown called by each script containing a timer
	 * alternative: use Timer : MonoBehavior + FixedUpdate
	 * alternative 2: use a TimerManager that knows each Timer object and updates them */
	public void countDown(float deltaTime) {
		// if time is positive, decrease time of deltaTime
		// (if time already 0, leave it so)
		if (time > 0) {
			time -= Time.deltaTime;
			// if the countdown has reached 0 (or less), callback
			if (time <= 0) {
				time = 0; // optional
				callback();
			}
		}
		
	}

}