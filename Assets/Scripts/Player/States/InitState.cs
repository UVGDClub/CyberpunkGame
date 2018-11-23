using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "States/InitState")]
public class InitState : APlayerState {

    public override void OnEnter( Player player ) {
        CanTransitionOutOf = false;
        player.rigidbody2d.gravityScale = 0;

        player.animator.SetBool("Idle", true);
        player.StartCoroutine(Initialize(player));
    }

    IEnumerator Initialize(Player player) {
        while (player.fileDetails == null)
            yield return null;

        PlayerSpawnInfo spawn = new PlayerSpawnInfo(Vector2Int.zero, new Vector2(player.fileDetails.position_x, player.fileDetails.position_y), Direction.Right);
        player.Spawn(spawn);

        player.rigidbody2d.gravityScale = 1;
        player.initialized = true;
        CanTransitionOutOf = true;
    }
}
