using UnityEngine;

[CreateAssetMenu(menuName = "States/IdleState")]
public class IdleState : APlayerState {

    public override void OnEnter(Player player)
    {
        player.rigidbody2d.constraints = RigidbodyConstraints2D.FreezeAll;
    }

    public override void OnExit(Player player)
    {
        player.rigidbody2d.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    public override void Execute(Player player)
    {
        if (player.bottomHit)
            return;

        player.rigidbody2d.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    public override bool CanTransitionInto( Player player ) {
        return Mathf.Approximately(Input.GetAxisRaw("Horizontal"), 0) && player.bottomHit;
    }
}
