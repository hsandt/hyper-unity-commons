using UnityEngine;
using System.Collections;

using UnityConstants;

public class SideViewCharacter_GroundedState : SideViewCharacter_MotionState {

	// OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
	//override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}

	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {

		motor.CheckGroundStatus();

		// if character wants to jump, let it jump (even if starts to fall this frame)
		// IMPROVE: add even more margin before character actually falls (see Game Feel and articles on character motion tolerance)
		if (control.ConsumeJumpIntention()) {
			Debug.Log("Jump!");
			motor.Jump();
			animator.SetBool("Grounded", false);
		}
		else {
			if (!motor.isGrounded) {
				// nothing below feet, go airborne
				animator.SetBool("Grounded", false);
				Debug.Log("Grounded <- false");
			}
		}

	}

	// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
	//override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}

	// OnStateMove is called right after Animator.OnAnimatorMove(). Code that processes and affects root motion should be implemented here
	//override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}

	// OnStateIK is called right after Animator.OnAnimatorIK(). Code that sets up animation IK (inverse kinematics) should be implemented here.
	//override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}


}
