using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// Abstract class for finite-state machine state. TStateKey can be any comparable, although we recommend using an Enum.
/// TState, the base class of all your states, must be provided, and therefore you need to create a base class
/// that inherits from FSMState<> as an intermediary:
/// 	public abstract class MyStateClass : FSMState<MyStateKey, MyStateClass>
/// In any case, the default value of TStateKey should represent the None state and never be used for an actual state.
/// Thus, for an enum, the 1st value (= 0) should always be None.
public abstract class FSMState<TStateKey, TState> where TState : FSMState<TStateKey, TState> {

	// TEMPLATE for child class
	/*
	public MyState () {
		previousStates = new HashSet<MyStateKey> {
			MyStateKey.None,
			MyStateKey.Falling
		};
	}
	*/


	/* Parameters */

	/// Unique key representing this state. Defined in child state class.
	/// Do not return the default value of TStateKey, which is reserved by the conceptual None state.
	public abstract TStateKey Key { get; }

	/// List of previous states allowed for transitions, bound the state definition. Defined in child class constructor.
	protected HashSet<TStateKey> previousStates;


	/* State vars */

	/// FSMMachine managing this state
	protected FSMMachine<TStateKey, TState> machine;


	public override string ToString ()
	{
		return Key.ToString();
	}

	/// Return true if the state key is valid (i.e. a non-default-value key), false otherwise.
	public static bool IsValidKey (TStateKey key) {
		return !EqualityComparer<TStateKey>.Default.Equals(key, default(TStateKey));
	}

	/// Return true if this state has a valid key (i.e. a non-default-value key), false otherwise.
	public bool HasValidKey () {
		// To support generic comparison, use CompareTo
		return !EqualityComparer<TStateKey>.Default.Equals(Key, default(TStateKey));
	}

	/// Return true if the state is represented by key
	public bool HasKey (TStateKey otherKey) {
		return EqualityComparer<TStateKey>.Default.Equals(Key, otherKey);
	}

	/// Return true if this state and the other state have the same keys
	public bool HasSameKey (FSMState<TStateKey, TState> other) {
		return other != null && EqualityComparer<TStateKey>.Default.Equals(Key, other.Key);
	}

	/// Return true if a transition is allowed from the previous state to this state
	public bool CanTransitionFrom(FSMState<TStateKey, TState> state) {
		return previousStates.Contains(state != null ? state.Key : default(TStateKey));
	}

	/// Set the machine and call OnAddedToMachine
	public void RegisterMachine (FSMMachine<TStateKey, TState> machine) {
		this.machine = machine;
		OnAddedToMachine(machine);
	}

	/// Set the machine (override to set child class fields from machine)
	protected virtual void OnAddedToMachine (FSMMachine<TStateKey, TState> machine) {}

	/// Enter state callback
	public virtual void OnEnter (TState previousState) {}

	/// Update state behaviour
	public virtual void UpdateState () {}

	/// Exit state callback
	public virtual void OnExit (TState nextState) {}

}
