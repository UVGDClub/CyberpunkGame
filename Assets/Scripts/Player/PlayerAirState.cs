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
            //@TODO implement aerial abilities
            yield return null;
        }

        OnBeginExit(this, player.stateTransitions.IdleTransition());
    }
}
