using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Dialogue_Typed : Dialogue
{
	// Use this for initialization
	private char fullBlock = '\x2588';
	bool allCharRevealed = true;
	string finalText;
	WaitForSeconds charRevealTime;
	public float charWaitTime = 0.1f;

	new void Awake()
	{
		base.Awake();
		charRevealTime = new WaitForSeconds(charWaitTime);
	}

	protected override void NextInQueue()
	{
		finalText = queue.ElementAt(0).Trim();
		if (displayTime < charWaitTime * finalText.Length)
		{
			waitTime = new WaitForSeconds(charWaitTime * finalText.Length + 0.5f);
			displayTime = charWaitTime * finalText.Length + 0.5f;
		}
		queue.RemoveFirst();
		dialogueText.text = fullBlock.ToString();
		StartCoroutine(TypeText());
	}

	public IEnumerator TypeText()
	{
		allCharRevealed = false;
		while (!allCharRevealed)
		{
			string currentText = dialogueText.text;
			int index = currentText.IndexOf(fullBlock);
			if (currentText.Length > finalText.Length)
			{
				allCharRevealed = true;
			}
			else
			{
				if (index != -1)
				{
					currentText = currentText.Substring(0, index);
					currentText += finalText[index].ToString() + fullBlock.ToString();
				}
			}

			dialogueText.text = currentText;
			yield return charRevealTime;
		}
	}
}