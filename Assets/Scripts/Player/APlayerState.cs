using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class APlayerState : ScriptableObject {

    public Color DebugColor;

    public List<APlayerState> Transitions;
    protected bool CanTransitionOutOf = true;

    public void InputCheck(Player player) {
        if (CanTransitionOutOf) {
            for (int i = 0; i < Transitions.Count; i++) {

                if (Transitions[i].CanTransitionInto(player)) {
                    player.TransferToState(Transitions[i]);
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
