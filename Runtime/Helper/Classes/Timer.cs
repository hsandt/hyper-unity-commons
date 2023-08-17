using UnityEngine;
using System;
using System.Collections;

namespace HyperUnityCommons
{
	/// Internal clock counting down and triggering some optional callback when over
	/// Usage: create new Timer(initial time, callback), call CountDown on it every Update or FixedUpdate,
	/// and process case when it returns true if you don't rely on the callback.
	/// Call SetTime(duration) when you want to (re)start the timer.
	/// It is only serializable in editor for debug, do not count on it for production
	#if UNITY_EDITOR
	[Serializable]
	#endif
	public struct Timer
	{
	    /* Parameters */

	    /// Function to call when the countdown is over, i.e. timer has reached 0. Defaults to null.
	    /// If null, you must detect when the countdown is over by checking the return value of CountDown.
	    private readonly Action m_Callback;


	    /* State */

	    /// Remaining time until countdown is over. When time left is positive, timer is considered running.
	    /// Defaults to 0 (stopped).
		private float m_TimeLeft;

		/// Has the timer any time left?
		public bool HasTimeLeft => m_TimeLeft > 0;


		/// Construct new timer with initial time and callback
		/// Both are optional since we may want a timer with manual handling of count down over,
		/// or start countdown later.
	    public Timer(float initialDuration = 0, Action callback = null)
	    {
			m_TimeLeft = initialDuration;
			m_Callback = callback;
		}

		/// Set the current time
		public void SetTime(float duration)
		{
			m_TimeLeft = duration;
		}

	    /// Reset timer to 0 without calling the callback (shortcut for SetTime(0))
		public void Stop()
	    {
			m_TimeLeft = 0;
		}

		/// Countdown the time of deltaTime
		/// Must be called by each script containing a timer in its Update or FixedUpdate
		public bool CountDown(float deltaTime)
		{
			if (m_TimeLeft > 0)
			{
				// timer is running, count it down
				m_TimeLeft -= deltaTime;

				if (m_TimeLeft <= 0)
				{
					m_TimeLeft = 0;
					m_Callback?.Invoke();
					return true;
				}
			}

	        // timer was either stopped, or counted down but didn't reach 0
			return false;
		}
	}
}
