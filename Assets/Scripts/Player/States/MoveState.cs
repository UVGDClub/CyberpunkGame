using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "States/MoveState")]
public class MoveState : APlayerState {

    public float moveSpeed = 5f;

    public override void Execute(Player player) {
        player.rigidbody2d.velocity = new Vector2(Input.GetAxis("Horizontal")* moveSpeed, player.rigidbody2d.velocity.y);
    }

    public override bool CanTransitionInto( Player player ) {
        if (Input.GetAxis("Horizontal") != 0)
            return true;

        return false;
    }
}
