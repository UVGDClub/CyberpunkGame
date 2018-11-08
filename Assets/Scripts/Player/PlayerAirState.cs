///<summary>
/// Created by Glen McManus September 27, 2018
/// </summary>

using System;
using System.Collections;
using UnityEngine;

public class PlayerAirState : IState
{
    public Player player;

    public event EventHandler<StateBeginExitEventArgs> OnBeginExit;

    public void BeginEnter()
    {
    //    Debug.Log("Begin entering air state");
    }

    public void EndEnter()
    {
    //    Debug.Log("End entering air state");
    }

    public void EndExit()
    {
    //    Debug.Log("End exiting air state");
    }

    public IEnumerable Execute()
    {
        while (!player.isGrounded())
        {
            if (Input.GetButton("Attack"))
                OnBeginExit(this, player.stateTransitions.AttackTransition());

            player.rigidbody2d.velocity += new Vector2(Input.GetAxis("Horizontal") * 0.1f, 0);

            if (Mathf.Abs(player.rigidbody2d.velocity.x) > player.moveSpeed)
                player.rigidbody2d.velocity = new Vector2(player.moveSpeed * Mathf.Sign(player.rigidbody2d.velocity.x), player.rigidbody2d.velocity.y);

            //@TODO implement aerial abilities
            yield return null;
        }

        OnBeginExit(this, player.stateTransitions.IdleTransition());
    }
}
