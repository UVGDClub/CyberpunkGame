using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "States/Falling")]
public class FallingState : APlayerState {

    public override bool CanTransitionInto( Player player ) {
        return player.rigidbody2d.velocity.y < 0;
    }

    public override void Execute( Player player ) {
        Debug.Log("falling");
    }

}
