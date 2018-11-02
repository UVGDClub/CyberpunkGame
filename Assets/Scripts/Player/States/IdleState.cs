using UnityEngine;

[CreateAssetMenu(menuName = "States/IdleState")]
public class IdleState : APlayerState {

    public override bool CanTransitionInto( Player player ) {
        if (Mathf.Abs(player.rigidbody2d.velocity.x) < 0.1f && Mathf.Abs(player.rigidbody2d.velocity.y) < 0.1f)
            return true;

        return false;
    }
}
