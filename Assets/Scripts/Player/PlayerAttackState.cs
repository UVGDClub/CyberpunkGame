using System;
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

        OnBeginExit(this, player.stateTransitions.IdleTransition());

        yield return null;
    }
}