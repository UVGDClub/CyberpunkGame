using UnityEngine;

[CreateAssetMenu(menuName = "States/Falling")]
public class FallingState : APlayerState {

	public JumpState referenceState;

    public override bool CanTransitionInto( Player player ) {
        return !player.bottomHit && player.rigidbody2d.velocity.y < 0.01f;
    }

	public override void Execute(Player player) {
		if (referenceState.airControl) {
			player.rigidbody2d.velocity = new Vector2(
                Input.GetAxisRaw("Horizontal") * referenceState.airControlSpeed, 
                player.rigidbody2d.velocity.y);
		}
	}
}
