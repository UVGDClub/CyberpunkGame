using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/*
 * Written by Alexander Aldridge
 * for the use of UVic GameDev "Cyberpunk" 2018-19 project
 * Modified by: Nobody so far
 * Last Updated 2018-11-08
 * 
 * Known bugs to fix:
 * -Words longer than MAX_SENTANCE_LENGTH untested
 */
public class Dialogue : MonoBehaviour {

    public Text dialogueText;
	protected WaitForSeconds waitTime;
	public float displayTime = 3.0f;


	const int MAX_SENTANCE_LENGTH = 80;
	protected LinkedList<string> queue = new LinkedList<string>();	//LinkedList should hypothetically be nice and efficient for the queue

    // Use this for initialization
    protected void Awake () {
		waitTime = new WaitForSeconds(displayTime);
	}
	
	public IEnumerator UpdateText() {
		while (queue.Count() > 0)
		{
			NextInQueue();
			yield return waitTime;
		}
	}

	protected virtual void NextInQueue()
	{
		dialogueText.text = queue.ElementAt(0);
		queue.RemoveFirst();
	}


	/*
     * @param dialogue -> the text to be displayed
     */
	public virtual void AddDialogue(string dialogue)
    {
		AddDialogue(ParseText(dialogue));
    }

	/*
	 * @param dialogue -> the text to be displayed
     * @param textTime -> the amount of time that the text should be on the screen
     */
	public virtual void AddDialogue(string[] dialogue)
    {
        foreach (string line in dialogue)
        {
            queue.AddLast(line);
        }
    }

	/*
	 * Removes current dialogue from the screen
	 * Untested!
	 */
	public void ClearDialogue()
	{
		queue.Clear();
		dialogueText.text = "";
	}

    /**
     * @param location -> the name of the text file within "Resources/Dialogue"
     * Example: "Pulp Fiction" will load "Pulp Fiction.txt" from Resources/Dialogue
     */
    public string[] LoadTextFile(string name)
    {
        return this.ParseText(Resources.Load<TextAsset>("Dialgue/" + name).ToString());

    }

	/*
	 * This function splits the text up into individual words (based off of
	 * where spaces are) and then puts the words together into sentances. 
	 * A sentance ends when it runs out of characters or if it reaches the
	 * end of the line (ie '\n')
	 * 
	 * It outputs an array of strings where each string is a sentance and
	 * string[n].Length <= MAX_SENTANCE_LENGTH
	 * 
	 * Untested for edgecases such as: words > MAX_SENTANCE_LENGTH
	 */
	private string[] ParseText(string text)
    {
		List<string> output = new List<string> { "" };

		string[] words = text.Split(' ');
		foreach (string word in words)
        {
			if (word.Length > MAX_SENTANCE_LENGTH)
			{
				Debug.Log("Word longer than MAX_SENTANCE_LENGTH");
			}
			if (output[output.Count - 1].Length + word.Length > MAX_SENTANCE_LENGTH)
            {
                output.Add("");
            }

			//If text file contains multiple lines
			if (word.Contains('\n'))
			{
				string s;
				string[] section = word.Split('\n');
				for (int i = 0; i < section.Length; i++)
				{
					s = section[i].Trim();
					if (s.Length > 0)
					{
						output[output.Count - 1] += " " + s;
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
