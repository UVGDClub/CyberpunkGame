using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "States/WallSlide")]
public class WallSlideState : APlayerState
{

    public FallingState FallingPlayerState;
    public JumpState JumpPlayerState;
    public IdleState IdleState;

    public float fallSpeed = 1f;
    public Vector2 distanceAfterJumpOff = new Vector3(0.02f, 0.02f, 0);
    public float delayBetweenTransitions = 0.25f;

    public override bool CanTransitionInto(Player player)
    {
        return !player.bottomHit && ((Input.GetAxisRaw("Horizontal") < 0 && player.left && player.left.normal.normalized.y == 0)
            || (Input.GetAxisRaw("Horizontal") > 0 && player.right && player.right.normal.normalized.y == 0));
    }

    public override void OnEnter( Player player ) {
        player.canDash = true;
        if (Input.GetButton("Jump"))
            processedJump = true;

        player.animator.SetBool("WallSliding", true);

        if (player.facing == Direction.Left)
            player.facing = Direction.Right;
        else
            player.facing = Direction.Left;
    }

    public override void OnExit(Player player)
    {
        player.animator.SetBool("WallSliding", false);
    }

    private bool processedJump;
    public override void Execute(Player player)
    {
        if(player.bottomHit) {
            player.TransferToState(IdleState);
            return;
        }

        if (!Input.GetButton("Jump"))
            processedJump = false;

        if ((Input.GetButton("Jump") || Input.GetButtonDown("Jump")) && !processedJump)
        {
            if (player.facing == Direction.Left)
                player.facing = Direction.Right;
            else
                player.facing = Direction.Left;

            processedJump = true;
            if (player.left)
                player.rigidbody2d.velocity += distanceAfterJumpOff;
            else if (player.right)
                player.rigidbody2d.velocity += new Vector2(-distanceAfterJumpOff.x, distanceAfterJumpOff.y);

            player.TransferToState(JumpPlayerState);

        }
        else if ((player.left && player.left.normal.y == 0 && Input.GetAxisRaw("Horizontal") < -0.1f) || 
            (player.right && player.right.normal.y == 0 && Input.GetAxisRaw("Horizontal") > 0.1f))
        {
            player.rigidbody2d.velocity = new Vector2(0, -fallSpeed);
        }
        else
        {
            player.TransferToState(FallingPlayerState);
        }

    }

}
