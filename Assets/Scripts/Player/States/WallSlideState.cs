using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "States/WallSlide")]
public class WallSlideState : APlayerState {

	public FallingState FallingPlayerState;
	public JumpState JumpPlayerState;

    public float fallSpeed = 1f;
    public float distanceAfterJumpOff = 0.5f;
    public float delayBetweenTransitions = 0.25f;

    public override bool CanTransitionInto( Player player ) {
        return Input.GetAxis("Horizontal") < 0 && player.left || Input.GetAxis("Horizontal") > 0 && player.right;
    }

    public override void Execute( Player player ) {

	    if (Input.GetButtonDown("Jump")) {

			if (player.left)
				player.transform.position -= Vector3.left * distanceAfterJumpOff;
			else if (player.right)
				player.transform.position -= Vector3.right * distanceAfterJumpOff;

			player.TransferToState(JumpPlayerState);

		} else if ((player.left && Input.GetAxisRaw("Horizontal") < -0.1f) || (player.right && Input.GetAxisRaw("Horizontal") > 0.1f)) {
		    player.rigidbody2d.velocity = new Vector2(0, -fallSpeed);
	    }
	    else {
		    player.TransferToState(FallingPlayerState);
		}

	}

    public override void OnEnter( Player player ) {
        // Set animation
        CanTransitionOutOf = false;
        player.StartCoroutine(WaitToTransitionOut());
    }

    IEnumerator WaitToTransitionOut() {
        yield return new WaitForSeconds(delayBetweenTransitions);
        CanTransitionOutOf = true;
    }    
}
