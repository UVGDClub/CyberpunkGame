using System;
using System.Collections;
using UnityEngine;

public class PlayerStruckState : IState
{
    public Player player;

    public event EventHandler<StateBeginExitEventArgs> OnBeginExit;

    public void BeginEnter()
    {
        Debug.Log("Begin entering struck state");
    }

    public void EndEnter()
    {
        Debug.Log("End entering struck state");
    }

    public void EndExit()
    {
        Debug.Log("End exiting struck state");
    }

    public IEnumerable Execute()
    {
        while (true)
        {

            OnBeginExit(this, player.stateTransitions.IdleTransition());

            yield return null;
        }

    }
}