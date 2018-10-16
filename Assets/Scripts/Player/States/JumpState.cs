using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "States/JumpState")]
public class JumpState : APlayerState {

    public int maxTicksForJump = 6;
    public float JumpVelocityAddition = 0.6f;
    public bool airControl = true;

    public override bool CanTransitionInto( Player player ) {
        if (Input.GetButton("Jump"))
            return true;

        return false;
    }

    public override void OnEnter( Player player ) {
        CanTransitionOutOf = false;
        player.StartCoroutine(Jump(player));
    }


    IEnumerator Jump(Player player) {
        Debug.Log("Jump");
        int numTicks = 0;
        player.rigidbody2d.velocity = new Vector2(player.rigidbody2d.velocity.x, 0);

        while (Input.GetButton("Jump") && numTicks <= maxTicksForJump) {

            Vector2 vel = player.rigidbody2d.velocity;
            vel += new Vector2(0, JumpVelocityAddition);

            if (airControl) {
                vel.x = Input.GetAxis("Horizontal");
            }

            player.rigidbody2d.velocity = vel;
            numTicks++;
            yield return null;
        }

        CanTransitionOutOf = true;
    }

}
