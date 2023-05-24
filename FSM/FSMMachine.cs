#define DEBUG_FSM_MACHINE

using System.Collections.Generic;
using UnityEngine;

namespace CommonsPattern
{

	/*

	EXAMPLE:

	public enum StateEnum {
		None = 0,
		State1,
		State2
	}
	
	public class MyState1 : MyState {
		// implement methods...
	} 

	public class MyState2 : MyState {
		// implement methods...
	} 

	public class MyMonoBehaviour : MonoBehaviour {

		FSMMachine<MyStateKey, MyState> fsmMachine;

		void Awake () {
			fsmMachine = new FSMMachine<MyStateKey, MyState>();
			fsmMachine.AddState(new MyState1());
			fsmMachine.AddState(new MyState2());
			fsmMachine.SetDefaultStateByKey(MyStateKey.State1);
			// or
			fsmMachine.SetDefaultStateByKey(MyState1.Key);
		}

		void Start () {
			fsmMachine.Setup();
		}

		void FixedUpdate () {
			fsmMachine.UpdateMachine();
		}
	
		// call when MyMonoBehaviour must be cleared/reset but not destroyed
		void Clear () {
			fsmMachine.Clear();
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
	/// It also supports custom transitions between specific states by checking the previous/next state argument in OnEnterFrom() and OnExitTo().
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
	        // set the next state and switch to the latter immediately (required as we call ApplyTransition *after* UpdateState)
	        // since we start from the null state, this is equivalent to:
	        // if (defaultState != null && defaultState.IsTransitionAllowedFrom(null)) {
	        //   defaultState.OnEnterFrom(null); CurrentState = defaultState;
	        // }
			NextState = defaultState;
	        ApplyTransition();
		}

		public void Clear () {
			if (CurrentState != null) {
				CurrentState.OnExitTo(null);
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
	                Debug.LogError("[FSMMachine] Cannot add state for None state key.");
			}
			else {
	            Debug.LogError("[FSMMachine] Cannot add null state.");
			}
		}

		/// Set the default state of the machine by key.
		/// Call this after adding the corresponding state, or it will not work and log a warning.
		/// Setting the default state key to the None state key is allowed, but should be done for clean up purpose only.
		public void SetDefaultStateByKey(TStateKey key) {
			if (FSMState<TStateKey, TState>.IsValidKey(key)) {
				TState state;
				if (states.TryGetValue(key, out state)) {
					if (state.IsTransitionAllowedFrom(null)) {
						defaultState = state;
					}
					else
	                    Debug.LogErrorFormat("[FSMMachine] Default state cannot be set for key: {0}, " +
							"as it cannot transition from the None state.", key);
				}
				else {
	                Debug.LogErrorFormat("[FSMMachine] Tried to set default state key to {0} but no corresponding state was added, default state will not be modified.", key);
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
					Debug.LogErrorFormat("[FSMMachine] Tried to set next state key to {0} but no corresponding state was added, next state will not be modified.", key);
				}
			}
			else {
	            Debug.LogError("[FSMMachine] Cannot set next state to None state.");
			}
		}

		/// Update the machine state, applying any requested transitions to a new state
		public void UpdateMachine() {
	        if (CurrentState != null) {
			    CurrentState.UpdateState();
	            ApplyTransition();  // apply transition after update so that model and animation immediately updated
	        }
	        else {
	            Debug.LogError("[FSMMachine] UpdateMachine: CurrentState is null");
	        }
		}

		/// Apply transition to any requested next state
		void ApplyTransition () {
			// check for transitions (HasSameKey includes a check for null state)
			if (NextState != null) {
				if (!NextState.HasSameKey(CurrentState)) {
					if (NextState.IsTransitionAllowedFrom(CurrentState)) {
						if (CurrentState != null) {
							CurrentState.OnExitTo(NextState);
						}
						NextState.OnEnterFrom(CurrentState);  // Initial state enters from None state
						#if DEBUG_FSM_MACHINE
						Debug.LogFormat("[FSMMachine] {0} -> {1}", CurrentState != null ? CurrentState.ToString() : "None", NextState.ToString());
						#endif
						CurrentState = NextState;
					}
					else {
	                    Debug.LogErrorFormat("[FSMMachine] Transition from {0} to {1} is not allowed", CurrentState.Key, NextState.Key);
					}
				}
				else {
	                Debug.LogErrorFormat("[FSMMachine] Next state key is the same as current state key {0}, ignoring transition", CurrentState.Key);
				}
				NextState = null;
			}
		}

	}

}
