using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// Abstract class for finite-state machine state. TStateKey can be any comparable, although we recommend using an Enum.
public abstract class FSMState<TStateKey> {

	/* Parameters */

	/// State key (should be unique per state machine)
	public TStateKey Key { get; protected set; }

	/// List of previous states allowed for transitions
	/// Fill with AddTransitionFrom
	protected HashSet<TStateKey> previousStates;


	/* State vars */

	/// FSMMachine managing this state
	protected FSMMachine<TStateKey> machine;


	/// Create an FSM state with a given key. In general, one FSMState class corresponds to one key,
	/// but if two states have the same behavior, you can share the same class and differenciate them by key only.
	/// Do not pass the default value (None state) key, or the machine will refuse to add this state.
	public FSMState (TStateKey key) {
		Key = key;
	}

	/// Return true if the state key is valid (i.e. a non-default-value key), false otherwise.
	public static bool IsValidKey (TStateKey key) {
		return EqualityComparer<TStateKey>.Default.Equals(key, default(TStateKey));
	}

	/// Return true if this state has a valid key (i.e. a non-default-value key), false otherwise.
	public bool HasValidKey () {
		// To support generic comparison, use CompareTo
		return EqualityComparer<TStateKey>.Default.Equals(Key, default(TStateKey));
	}

	/// Return true if the state is represented by key
	public bool HasKey (TStateKey otherKey) {
		return EqualityComparer<TStateKey>.Default.Equals(Key, otherKey);
	}

	/// Return true if this state and the other state have the same keys
	public bool HasSameKey (FSMState<TStateKey> other) {
		return other != null && EqualityComparer<TStateKey>.Default.Equals(Key, other.Key);
	}

	/// Return true if a transition is allowed from the previous state to this state
	public bool CanTransitionFrom(FSMState<TStateKey> state) {
		return previousStates.Contains(state != null ? state.Key : default(TStateKey));
	}

	/// Set the machine
	protected void OnAddedToMachine (FSMMachine<TStateKey> machine) {
		this.machine = machine;
	}

	/// Return true if the state has been added to an FSM machine
	protected bool HasMachine () {
		return machine != null;
	}

	/// Transition callback
	public virtual void OnTransitionFrom (TStateKey key) {}

	/// Enter state callback
	public virtual void OnEnter() {}

	/// Update state behaviour
	public virtual void UpdateState() {}

	/// Exit state callback
	public virtual void OnExit() {}

}
