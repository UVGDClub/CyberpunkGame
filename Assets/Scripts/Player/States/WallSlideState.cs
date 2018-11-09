using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "States/WallSlide")]
public class WallSlideState : APlayerState
{

    public FallingState FallingPlayerState;
    public JumpState JumpPlayerState;

    public float fallSpeed = 1f;
    public Vector2 distanceAfterJumpOff = new Vector3(0.02f, 0.02f, 0);
    public float delayBetweenTransitions = 0.25f;

    public override bool CanTransitionInto(Player player)
    {
        return Input.GetAxis("Horizontal") < 0 && player.left || Input.GetAxis("Horizontal") > 0 && player.right;
    }

    public override void Execute(Player player)
    {

        if (Input.GetButtonDown("Jump"))
        {

            if (player.left)
                player.rigidbody2d.velocity += distanceAfterJumpOff;
            else if (player.right)
                player.rigidbody2d.velocity += new Vector2(-distanceAfterJumpOff.x, distanceAfterJumpOff.y);

            player.TransferToState(JumpPlayerState);

        }
        else if ((player.left && Input.GetAxisRaw("Horizontal") < -0.1f) || (player.right && Input.GetAxisRaw("Horizontal") > 0.1f))
        {
            player.rigidbody2d.velocity = new Vector2(0, -fallSpeed);
        }
        else
        {
            player.TransferToState(FallingPlayerState);
        }

    }

}
