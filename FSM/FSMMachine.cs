#define DEBUG_FSM_MACHINE

using System;
using System.Collections.Generic;
using UnityEngine;

/*

EXAMPLE:

public enum StateEnum {
	None = 0,
	State1,
	State2
}

public class MyMonoBehaviour : MonoBehaviour {

	FSMMachine<StateEnum> fsmMachine;

	void Awake () {
		fsmMachine = new FSMMachine<StateEnum>();
		fsmMachine.AddState(new MyState1());
		fsmMachine.AddState(new MyState2());
		fsmMachine.SetDefaultStateByKey(StateEnum.State1);
		// or
		fsmMachine.SetDefaultStateByKey(MyState1.Key);
	}

	void Start () {
		fsmMachine.Setup();
	}

	void FixedUpdate () {
		fsmMachine.UpdateMachine();
	}
	
}

We recommend to create a child class of FSMMachine<MyState> to add custom features.
However, the class is not marked as abstract so that you can use it directly to make a simple FSM.

*/

/// Finite-state machine for states represented by TStateKey keys, with states of type TState (must be FSMState<TStateKey>)
/// TStateKey may be any type that can be a dictionary key, which is basically any type in C# due to Comparability and Hashability
/// The default value of TStateKey *must* represent the None state.
/// We recommended to use an Enum which EnumType.None = 0
/// This FSM uses the "next pattern", where the next state is explicitly set by key, without using transition signals.
/// The null -> initial state transition is also applied with the next pattern.
/// It also supports custom transition effects by adding the method OnTransitionFrom() besides OnEnter() and OnExit().
public class FSMMachine<TStateKey, TState> where TState : FSMState<TStateKey, TState> {

	/* Parameters */

	/// Dictionary of all states available, indexed by state key.
	/// The None state is not represented, and handled in a special case in each method.
	protected Dictionary<TStateKey, TState> states = new Dictionary<TStateKey, TState>();

	/// Initial and reset state of the machine. If null, the machine won't start.
	protected TState defaultState;


	/* State vars */

	/// Current state
	public TState CurrentState { get; protected set; }

	/// Next state to enter (on next update)
	public TState NextState { get; protected set; }


	public FSMMachine () {

	}

	public void Setup () {
		// count on the very first UpdateMachine to enter the initial state
		NextState = defaultState;
	}

	public void Clear () {
		if (CurrentState != null) {
			CurrentState.OnExit(null);
			CurrentState = null;
		}
	}

	/// Add a preconstructed state to the dictionary with the right key
	/// Throws an exception if a state with the same has already been added
	/// Ex: myMachine.AddState(new MyState(myStateKey))
	public void AddState(TState state) {
		if (state != null) {
			if (state.HasValidKey()) {
				states.Add(state.Key, state);
				state.RegisterMachine(this);
			}
			else
				Debug.LogWarning("[FSMMachine] Cannot add state for None state key.");
		}
		else {
			Debug.LogWarning("[FSMMachine] Cannot add null state.");
		}
	}

	/// Set the default state of the machine by key.
	/// Call this after adding the corresponding state, or it will not work and log a warning.
	/// Setting the default state key to the None state key is allowed, but should be done for clean up purpose only.
	public void SetDefaultStateByKey(TStateKey key) {
		if (FSMState<TStateKey, TState>.IsValidKey(key)) {
			TState state;
			if (states.TryGetValue(key, out state)) {
				if (state.CanTransitionFrom(null)) {
					defaultState = state;
				}
				else
					Debug.LogWarningFormat("[FSMMachine] Default state cannot be set for key: {0}," +
						"as it cannot transition from the None state.", key);
			}
			else {
				Debug.LogWarningFormat("[FSMMachine] Tried to set default state key to {0} but no corresponding state was added, default state will not be modified.", key);
			}
		}
		else {
			defaultState = null;
		}
	}

	/// Set the next state to enter on next update
	/// The next state cannot be set to null, as the None state is only for pre-initialization
	public void SetNextStateByKey(TStateKey key) {
		if (FSMState<TStateKey, TState>.IsValidKey(key)) {
			TState state;
			if (states.TryGetValue(key, out state)) {
				NextState = state;
			}
			else {
				Debug.LogWarningFormat("[FSMMachine] Tried to set next state key to {0} but no corresponding state was added, next state will not be modified.", key);
			}
		}
		else {
			Debug.LogWarningFormat("[FSMMachine] Cannot set next state to None state.");
		}
	}

	/// Update the machine state, applying any requested transitions to a new state
	public void UpdateMachine() {
		ApplyTransition();
        if (CurrentState != null)
		    CurrentState.UpdateState();
        else
			Debug.LogWarningFormat("[FSMMachine] UpdateMachine: CurrentState is null");
	}

	/// Apply transition to any requested next state
	void ApplyTransition () {
		// check for transitions (HasSameKey includes a check for null state)
		if (NextState != null) {
			if (!NextState.HasSameKey(CurrentState)) {
				if (NextState.CanTransitionFrom(CurrentState)) {
					if (CurrentState != null) {
						CurrentState.OnExit(NextState);
					}
					NextState.OnEnter(CurrentState);  // Initial state enters from None state
					#if DEBUG_FSM_MACHINE
					Debug.LogFormat("[FSMMachine] {0} -> {1}", CurrentState != null ? CurrentState.ToString() : "None", NextState.ToString());
					#endif
					CurrentState = NextState;
				}
				else {
					Debug.LogWarningFormat("[FSMMachine] Cannot transition from current state key {0} to next state key {1}", CurrentState.Key, NextState.Key);
				}
			}
			else {
				Debug.LogWarningFormat("[FSMMachine] Next state key is the same as current state key {0}, ignoring transition", CurrentState.Key);
			}
			NextState = null;
		}
	}

}
