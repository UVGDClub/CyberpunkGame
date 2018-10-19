using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DialogueTesting : MonoBehaviour {

    public Dialogue_Typed dialogue;

    // Use this for initialization
    void Start() {
	}

    // Update is called once per frame
    void Update() {
        if (dialogue.dialogueText.text == "")
		{
			dialogue.AddDialogue(dialogue.LoadTextFile("Pulp Fiction"), 4);
		}
    }

}
