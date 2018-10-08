using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class Dialogue : MonoBehaviour {

    public Text dialogueText;
    float displayStartTime = 0;
    float displayTimeLength = 0;
    int MAX_SENTANCE_LENGTH = 80;
    LinkedList<string> queue = new LinkedList<string>();

    // Use this for initialization
    void Start () {
        dialogueText.text = "";
        this.LoadTextFile("Pulp Fiction");
	}
	
	void FixedUpdate () {
        if (Time.fixedTime >= displayStartTime + displayTimeLength) {
            if (queue.Count == 0) {
                dialogueText.text = "";
            } else
            {
                SetDialogue(queue.First.Value, displayTimeLength);
                queue.RemoveFirst();
            }
        }
    }

    /*
     * @param dialogue -> the text to be displayed
     * Will display the text for sixteen seconds
     */
    public void SetDialogue(string dialogue)
    {
        dialogueText.text = dialogue;
        displayStartTime = Time.fixedTime;
        displayTimeLength = 8;
    }
    /*
     * @param textTime -> the amount of time that the text should be on the screen
     */
    public void SetDialogue(string dialogue, float textTime)
    {
        dialogueText.text = dialogue;
        displayStartTime = Time.fixedTime;
        displayTimeLength = textTime;
    }

    public void SetDialogue(string[] dialogue)
    {
        SetDialogue(dialogue, dialogue.Length * 4);
    }

    public void SetDialogue(string[] dialogue, float totalTextTime)
    {
        displayTimeLength = totalTextTime / (float) dialogue.Length;

        foreach (string line in dialogue)
        {
            queue.AddLast(line);
        }
    }

    /**
     * @param location -> the name of the text file within "Resources/Dialogue"
     * Example: "Pulp Fiction" will load "Pulp Fiction.txt" from Resources/Dialogue
     */
    public void LoadTextFile(string name)
    {
        string text = Resources.Load<TextAsset>("Dialgue/" + name).ToString();

        SetDialogue(ParseText(text));

    }

    private string[] ParseText(string text)
    {
		List<string> output = new List<string>{""};

		text = text.Replace("\n\n", "  \n");

		string[] words = text.Split(' ');
		foreach (string word in words)
        {
            if (output[output.Count - 1].Length + word.Length + 1 > MAX_SENTANCE_LENGTH)
            {
                output.Add("");
            }

			//If text file contains multiple lines
			if (word.Contains('\n'))
			{
				string pinch;
				string[] section = word.Split('\n');
				for (int i = 0; i < section.Length; i++)
				{
					pinch = section[i].Trim();
					if (pinch.Length > 0)
					{
						output[output.Count - 1] += " " + pinch;
						if (i < section.Length - 1)
						{
							output.Add("");
						}
					}
				}
				
			} else {
				output[output.Count - 1] += " " + word;
			}
        }
        return output.ToArray();
    }

}
