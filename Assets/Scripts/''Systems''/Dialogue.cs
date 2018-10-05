using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Dialogue : MonoBehaviour {

    public Text dialogueText;

	// Use this for initialization
	void Start () {
        dialogueText.text = "Default";
	}
	
	// Update is called once per frame
	void Update () {
    
	}

    public void SetDialogue(string dialogue)
    {
        dialogueText.text = dialogue;
    }

}
