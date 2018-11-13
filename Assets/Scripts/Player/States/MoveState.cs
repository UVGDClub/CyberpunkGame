using System;
using UnityEngine;

[CreateAssetMenu(menuName = "States/MoveState")]
public class MoveState : APlayerState {

    public float moveSpeed = 5f;
    public float maxSlopeDegree = 46f;

    public override void Execute(Player player) {
        player.rigidbody2d.velocity = GetForwardVelocity(Input.GetAxisRaw("Horizontal"), player) * moveSpeed;
    }

    public override void OnEnter(Player player)
    {
        player.animator.SetBool("Running", true);
    }

    public override void OnExit(Player player)
    {
        player.animator.SetBool("Running", false);
    }

    public Vector2 GetForwardVelocity( float dir, Player player) {
        if (dir == 0)
            return Vector2.zero;

        player.facing = dir > 0 ? Direction.Right : Direction.Left;
        RaycastHit2D hitForward = dir > 0 ? player.right : player.left;

        float SlopeDeg = Vector2.Angle(dir > 0 ? Vector2.left : Vector2.right, hitForward.normal);

        if (SlopeDeg > maxSlopeDegree) {
            return Vector2.zero;
        }

        if (hitForward && hitForward.distance <= player.maxForwardSlopeCastDistance) {
            if(Mathf.Approximately(hitForward.normal.y, 0)) {
                return Vector2.right * dir;
            }
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
        if (!player.bottomHit) return false;

        // If you're trying to walk up something you can't, stop trying.
        if (Input.GetAxisRaw("Horizontal") > 0f && (!player.right || Vector2.Angle(Vector2.left, player.right.normal) < maxSlopeDegree)) return true;
        if (Input.GetAxisRaw("Horizontal") < 0f && (!player.left || Vector2.Angle(Vector2.right, player.left.normal) < maxSlopeDegree)) return true;
        return false;
    }
}
