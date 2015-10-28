using UnityEngine;
using System.Collections;

using Rewired;

public class SideViewCharacter_MotionState : StateMachineBehaviour {

	// scripts
	protected SideViewCharacterControl control;
	protected SideViewCharacterMotor motor;
	protected Rigidbody2D rigidbody2d;

	// Rewired player
	protected Player player;

	virtual public void Init(SideViewCharacterControl control, SideViewCharacterMotor motor, Rigidbody2D rigidbody2d) {
		this.control = control;
		this.motor = motor;
		this.rigidbody2d = rigidbody2d;
		player = ReInput.players.GetPlayer(0);
	}


}
