using UnityEngine;
using System.Collections;
using Rewired;

public class SideViewHumanControl : SideViewCharacterControl {

	Animator animator;
	SideViewCharacterMotor motor;

	/// Rewired player
	Player player;

	void Awake () {
		animator = this.GetComponentOrFail<Animator>();
		motor = this.GetComponentOrFail<SideViewCharacterMotor>();
		player = ReInput.players.GetPlayer(0);
	}

	// Update is called once per frame
	void Update () {
		Vector2 moveInputVector = new Vector2(
			player.GetAxis("Move Horizontal"),
			player.GetAxis("Move Vertical")
			);
		m_MoveIntentionVector = moveInputVector * motor.maxSpeed;

		// record jump input if grounded (if false, do nothing so as not to cancel an input if there are multiple Update between two FixedUpdates)
		if (animator.GetBool("Grounded") && player.GetButtonDown("Jump")) {
			m_JumpIntention = true;
			Debug.Log("Jump input");
		}
	}

}
