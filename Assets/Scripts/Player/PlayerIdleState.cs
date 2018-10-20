///<summary>
/// Created by Glen McManus September 27, 2018
/// </summary>

using System;
using System.Collections;
using UnityEngine;

public class PlayerIdleState : IState
{
    public Player player;

    public event EventHandler<StateBeginExitEventArgs> OnBeginExit;

    public void BeginEnter()
    {
        //Debug.Log("Begin entering idle state");
    }

    public void EndEnter()
    {
        //Debug.Log("End entering idle state");
    }

    public void EndExit()
    {
        //Debug.Log("End exiting idle state");
    }

    public IEnumerable Execute()
    {
        while(true)
        {
            if(!player.isGrounded())
            {
                OnBeginExit(this, player.stateTransitions.AirTransition());
                yield break;
            }
            else if(Input.GetAxis("Horizontal") != 0)
            {
                //Debug.Log("Horizontal pressed");
                OnBeginExit(this, player.stateTransitions.MoveTransition());
                yield break;
            }
            else if(Input.GetButton("Jump"))
            {
               // player.Jump();
                OnBeginExit(this, player.stateTransitions.JumpToAirTransition());
                yield break;
            }
            else if(Input.GetButton("Attack"))
            {
                OnBeginExit(this, player.stateTransitions.AttackTransition());
                yield break;
            }

            yield return null;
        }
        
    }
}
