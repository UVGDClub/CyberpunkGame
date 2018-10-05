using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTesting : MonoBehaviour {

    public Dialogue dialogue;


    // Use this for initialization
    void Start() {
    
    }

    // Update is called once per frame
    void Update() {

    }


    [ContextMenu("Wizard Man")]
    void SetWizardMan()
    {
        dialogue.SetDialogue("Alexander is the wizard");
    }
}
