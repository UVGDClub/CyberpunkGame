using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class APlayerState : ScriptableObject {

    public List<APlayerState> Transitions;
    public bool CanTransitionOutOf = true;

    public void InputCheck(Player player) {
        if (CanTransitionOutOf) {
            for (int i = 0; i < Transitions.Count; i++) {

                if (Transitions[i].CanTransitionInto(player)) {
                    Debug.Log("Exiting: " + this);
                    Debug.Log("Entering: " + Transitions[i]);
                    OnExit(player);
                    player.currentState = Transitions[i];
                    Transitions[i].OnEnter(player);
                    return;
                }

            }

        }
    }

    public virtual void OnEnter(Player player) {

    }

    public virtual void OnExit( Player player ) {

    }

    public virtual void Execute(Player player) {
        
    }

    public virtual bool CanTransitionInto(Player player) {
        throw new System.NotImplementedException();
    }
}
