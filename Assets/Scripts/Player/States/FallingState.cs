using UnityEngine;

[CreateAssetMenu(menuName = "States/Falling")]
public class FallingState : APlayerState {

	public JumpState referenceState;

    public override void OnEnter(Player player)
    {
        player.animator.SetBool("Airborne", true);
    }

    public override void OnExit(Player player)
    {
        player.animator.SetBool("Airborne", false);
    }

    public override bool CanTransitionInto( Player player ) {
        return !player.bottomHit && player.rigidbody2d.velocity.y < 0.01f;
    }

	public override void Execute(Player player) {
		if (referenceState.airControl) {
            float dir = Input.GetAxisRaw("Horizontal");

            if (dir < 0)
                player.facing = Direction.Left;
            else if (dir > 0)

                player.facing = Direction.Right;
            player.rigidbody2d.velocity = new Vector2(
                dir * referenceState.airControlSpeed, 
                player.rigidbody2d.velocity.y);
		}
	}
}
