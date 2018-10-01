using System;
using System.Collections;
using UnityEngine;

public class PlayerDodgeState : IState
{
    public Player player;

    public event EventHandler<StateBeginExitEventArgs> OnBeginExit;

    public void BeginEnter()
    {
        Debug.Log("Begin entering idle state");
    }

    public void EndEnter()
    {
        Debug.Log("End entering idle state");
    }

    public void EndExit()
    {
        Debug.Log("End exiting idle state");
    }

    public IEnumerable Execute()
    {
        //wait for something

        while (true)
        {
            if (Input.GetAxis("Horizontal") != 0)
            {

            }

            yield return null;
        }

    }
}
