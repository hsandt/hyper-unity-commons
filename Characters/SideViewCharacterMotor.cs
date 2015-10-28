using UnityEngine;
using System.Collections;

using UnityConstants;

// SEO: after Control scripts to have a motion as fast as possible
[RequireComponent(typeof (Rigidbody2D))]
[RequireComponent(typeof (Animator))]
public class SideViewCharacterMotor : MonoBehaviour {

	Rigidbody2D m_Rigidbody2D;
	Animator animator;
	SideViewCharacterControl control;

	/* Parameters */

	/// Character max speed
	[SerializeField] float m_MaxSpeed = 1.0f;
	public float maxSpeed { get { return m_MaxSpeed; } }

	/// Jump linear momentum (p = mass * velocity impulse)
	[SerializeField] float m_JumpMomentum = 3.0f;
	public float jumpMomentum { get { return m_JumpMomentum; } }

	/// Raycast distance to check ground status
	[SerializeField] float m_GroundCheckDistance = 0.2f;

	/* State vars */

	/// Is the character grounded?
	bool m_IsGrounded;
	public bool isGrounded { get { return m_IsGrounded; } }

	/// Normal to the current ground, if any
	Vector2 m_GroundNormal;

	HorizontalDirection m_HorizontalDirection;
	/// Current direction
	public HorizontalDirection horizontalDirection { get { return m_HorizontalDirection; } }

	void Awake () {
		m_Rigidbody2D = this.GetComponentOrFail<Rigidbody2D>();
		animator = this.GetComponentOrFail<Animator>();
		control = this.GetComponentOrFail<SideViewCharacterControl>();
	}

	void OnEnable () {
		// initialize state behaviours
		foreach (var stateBehaviour in animator.GetBehaviours<SideViewCharacter_MotionState>()) {
			stateBehaviour.Init(control, this, m_Rigidbody2D);
		}
	}

	void Start () {
		Setup();
	}

	public void Setup () {
		m_IsGrounded = true;
		m_GroundNormal = Vector2.up;

		DebugScreen.PrintVar<Vector2>(1, "moveVector", Vector2.zero);
	}

	void FixedUpdate () {
		Move(control.moveIntentionVector);
		UpdateAnimatorVelocity();
	}

	/// Return true if ground is present under the character's feet (adapted from Unity ThirdPersonCharacter to 2D)
	public void CheckGroundStatus () {

		// 0.1f of offset to make sure we don't miss the ground if the foot is just on or a little inside the ground
		RaycastHit2D hitInfo = Physics2D.Raycast((Vector2) transform.position + Vector2.up * 0.1f, Vector2.down, m_GroundCheckDistance, Layers.GroundMask);
#if UNITY_EDITOR
		Debug.DrawRay((Vector2) transform.position + Vector2.up * 0.1f, Vector2.down * m_GroundCheckDistance, Color.red);
#endif

		if (hitInfo.collider != null) {
			// Debug.LogFormat("CheckGroundStatus hitInfo.collider: {0}", hitInfo.collider);
			m_IsGrounded = true;
			m_GroundNormal = hitInfo.normal;
			Debug.DrawRay(hitInfo.point, hitInfo.normal, Color.red, 1f);
		} else {
			m_IsGrounded = false;
			m_GroundNormal = Vector2.up;  // in the air, move straight along X axis
		}

	}

	/// Update animator parameters VelocityX and VelocityY
	public void UpdateAnimatorVelocity () {
		animator.SetFloat("VelocityX", m_Rigidbody2D.velocity.x);
		animator.SetFloat("VelocityY", m_Rigidbody2D.velocity.y);
	}

	/// Apply move, a vector in the world coordinates with magnitude equal to actual speed wanted, by clamping and projecting on slope
	public void Move (Vector2 baseMoveVector) {
		// determine the move velocity based on intention magnitude, but clamp if higher than max speed
		Vector2 moveVector = Vector2.ClampMagnitude(baseMoveVector, m_MaxSpeed);

		// project move vector along slope
		moveVector = VectorUtil.ProjectOrthogonal(moveVector, m_GroundNormal);
		DebugScreen.UpdateVar<Vector2>("moveVector", moveVector);

		// apply new velocity in X axis only
		m_Rigidbody2D.velocity = new Vector2(moveVector.x, m_Rigidbody2D.velocity.y);

		// use boolean to determine if character is walking for animator (redundant with velocity, but convenient)
		animator.SetBool("IsWalking", moveVector != Vector2.zero);

		// update horizontal direction based on motion along X
		if (moveVector.x > 0) {
			m_HorizontalDirection = HorizontalDirection.Right;
		} else if (moveVector.x < 0) {
			m_HorizontalDirection = HorizontalDirection.Left;
		}
		animator.SetInteger("Direction", (int) m_HorizontalDirection);

	}

	/// Start jumping
	public void Jump () {
		// apply vertical jump velocity
		DebugScreen.Print(0, "JUMP");
		m_Rigidbody2D.AddForce(jumpMomentum * Vector2.up, ForceMode2D.Impulse);
	}

}
