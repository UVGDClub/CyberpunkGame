using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class APlayerState : ScriptableObject {

    public List<APlayerState> Transitions;
    public bool CanTransitionOutOf = true;

    public void InputCheck(Player player) {
        if (CanTransitionOutOf) {
            Debug.Log("Can transition");
            for (int i = 0; i < Transitions.Count; i++) {

                if (Transitions[i].CanTransitionInto(player)) {
                    player.currentState = Transitions[i];
                    Transitions[i].Execute(player);
                }

            }

        }
    }

    public virtual void Execute(Player player) {
        
    }

    public virtual bool CanTransitionInto(Player player) {
        Debug.Log("WTF");
        return false;
    }
}
