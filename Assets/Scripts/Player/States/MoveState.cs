using System;
using UnityEngine;

[CreateAssetMenu(menuName = "States/MoveState")]
public class MoveState : APlayerState {

    public float moveSpeed = 5f;

    public override void Execute(Player player) {
        player.rigidbody2d.velocity = GetForwardVelocity(Input.GetAxisRaw("Horizontal"), player) * moveSpeed;
    }

    public Vector2 GetForwardVelocity( float dir, Player player) {
        if (dir == 0)
            return Vector2.zero;

        player.facing = dir > 0 ? Direction.Right : Direction.Left;
        RaycastHit2D hitForward = dir > 0 ? player.right : player.left;

        if (hitForward && !Mathf.Approximately(hitForward.normal.y, 0) && hitForward.distance <= player.maxForwardSlopeCastDistance) {
            //Debug.Log("Ascending slope (normal) : " + hitForward.normal);
            return Vector2.Perpendicular(hitForward.normal) * -dir;
        }

        //need max down slope cast distance??
        if (player.bottomHit && !Mathf.Approximately(player.bottomHit.normal.y, 1) && hitForward.distance <= player.maxDownSlopeCastDistance) {
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
