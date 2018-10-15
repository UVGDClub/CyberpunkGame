using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class DialogueTester : MonoBehaviour {

    public Dialogue Dialogue;
    public DialogueDef Text;

    public void SetText() {
        Dialogue.SetDialog(Text.Text);
    }

#if UNITY_EDITOR
    private void OnEnable() {
        Dialogue = Object.FindObjectOfType<Dialogue>();
    }
#endif
}
