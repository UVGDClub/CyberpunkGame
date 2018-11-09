/*using System;
using System.Collections;
using UnityEngine;

public class PlayerAttackState : IState
{
    public Player player;

    public event EventHandler<StateBeginExitEventArgs> OnBeginExit;

    public void BeginEnter()
    {

    }

    public void EndEnter()
    {

    }

    public void EndExit()
    {

    }

    public IEnumerable Execute()
    {
        // Debug.Log("Attack!");

        Vector2 direction = player.facing == Direction.Left ? Vector2.left : Vector2.right;

        Debug.DrawRay(player.rigidbody2d.position, direction * player.maxAttackDistance, Color.red);

        RaycastHit2D[] hits = Physics2D.BoxCastAll(player.rigidbody2d.position, player.attackBoxSize, 0, direction, player.maxAttackDistance, player.attackLayerMask);

        foreach(RaycastHit2D hit in hits)
        {
            //maintain a list of objects that can be attacked in a scriptable object?
            //using:
            //hit.collider.GetInstanceID()
            //and a scriptable object?
            BreakableTileInstance bti = hit.collider.GetComponent<BreakableTileInstance>();
            if (bti == null)
                continue;

            bti.StartCoroutine(bti.BreakTile());
        }


        if (player.isGrounded())
            OnBeginExit(this, player.stateTransitions.IdleTransition());
        else
            OnBeginExit(this, player.stateTransitions.AirTransition());

        yield return null;
    }
}*/