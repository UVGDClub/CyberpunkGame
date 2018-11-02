using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "States/WallSlide")]
public class WallSlideState : APlayerState {

    public float fallSpeed = 1f;
    public float distanceAfterJumpOff = 0.5f;
    public float delayBetweenTransitions = 0.25f;

    public override bool CanTransitionInto( Player player ) {
        return Input.GetAxis("Horizontal") < 0 && player.left || Input.GetAxis("Horizontal") > 0 && player.right;
    }

    public override void Execute( Player player ) {
        player.rigidbody2d.velocity = new Vector2(0, -fallSpeed);
    }

    public override void OnEnter( Player player ) {
        // Set animation
        CanTransitionOutOf = false;
        player.StartCoroutine(WaitToTransitionOut());
    }

    IEnumerator WaitToTransitionOut() {
        yield return new WaitForSeconds(delayBetweenTransitions);
        CanTransitionOutOf = true;
    }

    public override void OnExit( Player player ) {
        if(Input.GetButtonDown("Jump")) {
            if(player.left) player.transform.position -= Vector3.left * distanceAfterJumpOff;
            if (player.right) player.transform.position -= Vector3.right * distanceAfterJumpOff;
        }
    }

    
}
