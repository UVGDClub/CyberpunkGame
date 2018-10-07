///<summary>
/// Created by Glen McManus September 27, 2018
/// </summary>

using System.Collections;

public class PlayerStateTransitions {

    public Player player;
    public PlayerStateTransition emptyTransition = new PlayerStateTransition();

    public StateBeginExitEventArgs AirTransition()
    {
        return new StateBeginExitEventArgs(player.airState, emptyTransition);
    }

    //could pass in attack button as paramater, or "action" button depending, then set a variable in attack state
    public StateBeginExitEventArgs AttackTransition()
    {
        //player.attackState.settingSomeFakeVariable = ThisFakeValue;
        return new StateBeginExitEventArgs(player.attackState, emptyTransition);
    }

    public StateBeginExitEventArgs DeadTransition()
    {
        return new StateBeginExitEventArgs(player.deadState, emptyTransition);
    }

    public StateBeginExitEventArgs DodgeTransition()
    {
        return new StateBeginExitEventArgs(player.dodgeState, emptyTransition);
    }

    public StateBeginExitEventArgs IdleTransition()
    {
        return new StateBeginExitEventArgs(player.idleState, emptyTransition);
    }

    public StateBeginExitEventArgs JumpToAirTransition()
    {
        player.StartCoroutine(player.Jump());
        return AirTransition();
    }

    public StateBeginExitEventArgs MoveTransition()
    {
        return new StateBeginExitEventArgs(player.moveState, emptyTransition);
    }

    public StateBeginExitEventArgs StruckTransition()
    {
        return new StateBeginExitEventArgs(player.struckState, emptyTransition);
    }
}

/// <summary>
/// This is a general and empty transition which adds nothing between states.
/// Multiple transitions could be written to be run specifically between two states,
/// or each time a state is transitioned to/from.
/// </summary>
public class PlayerStateTransition : IStateTransition
{
    public IEnumerable Enter()
    {
        /*
         * Can implement things to be done while starting a transition
         */ 
        yield return null;
    }

    public IEnumerable Exit()
    {
        /*
         * Can implement things to be done while ending a transition
         */
        yield return null;
    }
}
