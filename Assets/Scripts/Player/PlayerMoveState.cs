///<summary>
/// Created by Glen McManus September 27, 2018
/// </summary>

using System;
using System.Collections;
using UnityEngine;

public class PlayerMoveState : IState
{
    public Player player;

    public event EventHandler<StateBeginExitEventArgs> OnBeginExit;

    public void BeginEnter()
    {
        //Debug.Log("Begin entering move state");
    }

    public void EndEnter()
    {
        //Debug.Log("End entering move state");
    }

    public void EndExit()
    {
        //Debug.Log("End exiting move state");
    }

    public IEnumerable Execute()
    {
        while (true)
        {
            if(!player.isGrounded())
            {
                OnBeginExit(this, player.stateTransitions.AirTransition());
                yield break;
            }

            if (Input.GetAxis("Horizontal") != 0)
            {
                player.rigidbody2d.velocity = new Vector2(Input.GetAxis("Horizontal") * player.moveSpeed, player.rigidbody2d.velocity.y);
            }
            else
            {
                player.rigidbody2d.velocity = new Vector2(0, player.rigidbody2d.velocity.y);
                OnBeginExit(this, player.stateTransitions.IdleTransition());
                yield break;
            }                

            if(Input.GetButton("Jump") && player.isGrounded())
            {
               // player.Jump();
                OnBeginExit(this, player.stateTransitions.JumpToAirTransition());
                yield break;
            }

            if(Input.GetMouseButtonDown(0))
            {
                OnBeginExit(this, player.stateTransitions.AttackTransition());
                yield break;
            }

            OnBeginExit(this, player.stateTransitions.IdleTransition());
            yield return null;
        }

    }
}
