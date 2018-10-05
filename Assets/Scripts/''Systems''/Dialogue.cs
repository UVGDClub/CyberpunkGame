using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class Dialogue : MonoBehaviour {

    public Text dialogueText;
    float stopDisplayTime;
    int MAX_SENTANCE_LENGTH = 50;

    // Use this for initialization
    void Start () {
        stopDisplayTime = 0;
        dialogueText.text = "Assume cat is flat";
        this.LoadTextFile("Pulp Fiction");
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
     * Will display the text for sixteen seconds
     */
    public void SetDialogue(string dialogue)
    {
        dialogueText.text = dialogue;
        stopDisplayTime = Time.fixedTime + 16;
    }
    /*
     * @param textTime -> the amount of time that the text should be on the screen
     */
    public void SetDialogue(string dialogue, float textTime)
    {
        dialogueText.text = dialogue;
        stopDisplayTime = Time.fixedTime + textTime;
    }

    /**
     * @param location -> the name of the text file within "Resources/Dialogue"
     * Example: "Pulp Fiction" will load "Pulp Fiction.txt" from Resources/Dialogue
     */
    public void LoadTextFile(string name)
    {
        string text = Resources.Load<TextAsset>("Dialgue/Pulp Fiction").ToString();
        string[] sentances = this.ParseText(text);

        SetDialogue(text); //Change this once parsing is complete
    }

    private string[] ParseText(string text)
    {
        List<string> output = new List<string>();
        string[] words = text.Split(' ');

        int sentanceNumber = 0;
        foreach (string word in words)
        {
            if (output[sentanceNumber].Length + word.Length > MAX_SENTANCE_LENGTH)
            {
                sentanceNumber++;
                output.Add("");
            }
            output[sentanceNumber] += word;
        }

        return output.ToArray();
    }

}
