using System;
using System.Collections;
using UnityEngine;

public class PlayerDeadState : IState
{
    public Player player;

    public event EventHandler<StateBeginExitEventArgs> OnBeginExit;

    public void BeginEnter()
    {
        Debug.Log("Begin entering dead state");
    }

    public void EndEnter()
    {
        Debug.Log("End entering dead state");
    }

    public void EndExit()
    {
        Debug.Log("End exiting dead state");
    }

    public IEnumerable Execute()
    {
        Debug.Log("You are dead.");

        yield return null;
    }
}
