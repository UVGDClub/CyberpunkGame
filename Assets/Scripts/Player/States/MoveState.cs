using System;
using UnityEngine;

[CreateAssetMenu(menuName = "States/MoveState")]
public class MoveState : APlayerState {

    public float moveSpeed = 5f;


    public override void Execute(Player player) {
        player.rigidbody2d.velocity = GetForwardVelocity(Input.GetAxisRaw("Horizontal") * moveSpeed, player);
    }

    public Vector2 GetForwardVelocity( float dir, Player player) {
        if (dir == 0)
            return Vector2.zero;

        Direction facing = dir < 0 ? Direction.Left : Direction.Right;
        RaycastHit2D hitForward = facing == Direction.Right ? player.right : player.left;

        if (hitForward && !Mathf.Approximately(hitForward.normal.y, 0)) {
            //Debug.Log("Ascending slope (normal) : " + hitForward.normal);
            return Vector2.Perpendicular(hitForward.normal) * -dir;
        }

        if (player.bottomHit && !Mathf.Approximately(player.bottomHit.normal.y, 1)) {
            //Debug.Log("Descending slope (normal) : " + hitDown.normal );
            return Vector2.Perpendicular(player.bottomHit.normal) * -dir;
        }

        //Debug.Log("Not on slope");
        return dir * Vector2.right;
    }


    public override bool CanTransitionInto( Player player ) {
	    return Math.Abs(Input.GetAxisRaw("Horizontal")) > 0.01f && player.bottomHit;
    }
}
