using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "States/IdleState")]
public class IdleState : APlayerState {

    public override void Execute( Player player ) {
        Debug.Log("Idle");
    }

    public override bool CanTransitionInto( Player player ) {
        if (player.rigidbody2d.velocity.x == 0 && player.rigidbody2d.velocity.y == 0)
            return true;

        return false;
    }
}
