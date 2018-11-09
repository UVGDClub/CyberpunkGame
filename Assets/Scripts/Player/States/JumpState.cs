using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "States/JumpState")]
public class JumpState : APlayerState {

    public int maxTicksForJump = 6;
    public float JumpVelocityAddition = 0.6f;

    public bool retainVelocityY = false;
    public bool airControl = true;
	public float airControlSpeed = 5f;

    public override bool CanTransitionInto( Player player ) {
        if (Input.GetButtonDown("Jump"))
            return true;

        return false;
    }

    public override void OnEnter( Player player ) {
        CanTransitionOutOf = false;
        player.StartCoroutine(Jump(player));
    }

	public override void Execute(Player player) {
		Vector2 vel = player.rigidbody2d.velocity;
		if (airControl) {
			vel.x = Input.GetAxis("Horizontal") * airControlSpeed;
		}
		player.rigidbody2d.velocity = vel;
	}

	IEnumerator Jump(Player player) {

        int numTicks = 0;

        player.rigidbody2d.velocity = new Vector2(player.rigidbody2d.velocity.x, retainVelocityY ? player.rigidbody2d.velocity.y : 0);

        while (Input.GetButton("Jump") && numTicks <= maxTicksForJump) {

            Vector2 vel = player.rigidbody2d.velocity;
            vel += new Vector2(0, JumpVelocityAddition);

            player.rigidbody2d.velocity = vel;
            numTicks++;
            yield return null;
        }

        CanTransitionOutOf = true;
    }

}
