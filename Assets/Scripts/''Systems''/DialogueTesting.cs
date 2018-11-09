using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DialogueTesting : MonoBehaviour {

    public Dialogue dialogue;

    // Use this for initialization
    void Start() {
		dialogue.AddDialogue(dialogue.LoadTextFile("Pulp Fiction"));
		StartCoroutine(dialogue.UpdateText());
	}

}
