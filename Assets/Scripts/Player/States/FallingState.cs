using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "States/Falling")]
public class FallingState : APlayerState {

	public JumpState referenceState;

    public override bool CanTransitionInto( Player player ) {
        return player.rigidbody2d.velocity.y < 0;
    }

	public override void Execute(Player player) {
		if (referenceState.airControl) {
			player.rigidbody2d.velocity = new Vector2(Input.GetAxis("Horizontal") * referenceState.airControlSpeed, player.rigidbody2d.velocity.y);
		}
	}
}
