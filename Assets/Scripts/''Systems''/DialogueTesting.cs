using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DialogueTesting : MonoBehaviour {

    public Dialogue dialogue;
    


    // Use this for initialization
    void Start() {
        
    }

    // Update is called once per frame
    void Update() {
        
    }


    [ContextMenu("Who is a wizard?")]
    void WhosTheWizard()
    {
        dialogue.SetDialogue("Alexander is the wizard.");
    }

}
