using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class Dialogue : MonoBehaviour {

    public Text dialogueText;
    float stopDisplayTime;

    // Use this for initialization
    void Start () {
        stopDisplayTime = 0;
        dialogueText.text = "Assume cat is flat";
	}
	
	void FixedUpdate () {
        if (Time.fixedTime >= stopDisplayTime) {
            if (!IsDisplaying()) {
                dialogueText.text = "";
            }
        }
    }

    public bool IsDisplaying()
    {
        return dialogueText.text == "";
    }

    /*
     * @param dialogue -> the text to be displayed
     * Will display the text for eight seconds
     */
    public void SetDialogue(string dialogue)
    {
        dialogueText.text = dialogue;
        stopDisplayTime = Time.fixedTime + 8;
    }
    /*
     * @param textTime -> the amount of time that the text should be on the screen
     */
    public void SetDialogue(string dialogue, float textTime)
    {
        dialogueText.text = dialogue;
        stopDisplayTime = Time.fixedTime + textTime;
    }

}
