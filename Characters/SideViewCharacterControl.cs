using UnityEngine;
using System.Collections;

public class SideViewCharacterControl : MonoBehaviour {

	/// Vector characterizing move intention, in speed unit, same scale as motor speed
	protected Vector2 m_MoveIntentionVector;
	public Vector2 moveIntentionVector { get { return m_MoveIntentionVector; } }

	protected bool m_JumpIntention;
	public bool jumpIntention { get { return m_JumpIntention; } }

	void Start () {
		Setup();
	}

	public void Setup () {
		m_MoveIntentionVector = Vector2.zero;
		m_JumpIntention = false;
	}

	/// Set JUMP action input to false if true, and return original value
	/// Usage: if (ConsumeJumpIntention()) { /* apply action */ }
	public bool ConsumeJumpIntention() {
		if (m_JumpIntention) {
			Debug.Log("Consume Jump Intention true");
			m_JumpIntention = false;
			return true;
		} else {
			return false;
		}
	}

}
