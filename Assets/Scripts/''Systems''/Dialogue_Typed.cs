using System.Collections;
using System.Linq;
using UnityEngine;

public class Dialogue_Typed : Dialogue {
	
	private char fullBlock = '\x2588';
	string finalText;
	public float charWaitTime = 0.05f;
	public int blinkDelay = 6;
	public bool adaptiveDisplayTime = true;
	const float endPause = 2.0f;

	/**
	 * Gets the next string in the queue
	 * and starts the TypeText coroutine
	 */
	protected override void NextInQueue()
	{
		finalText = queue.ElementAt(0).Trim();
		if (adaptiveDisplayTime && displayTime < charWaitTime * MAX_SENTANCE_LENGTH)
		{
			displayTime = charWaitTime * finalText.Length + endPause;
		}
		queue.RemoveFirst();
		dialogueText.text = fullBlock.ToString();
		StartCoroutine(TypeText());
	}

	/**
	 * Reveals the text one character at a time
	 */
	public IEnumerator TypeText()
	{
		int blink = -1;
		bool allCharRevealed = false;
		while (!allCharRevealed)
		{
			string currentText = dialogueText.text;
			int index = currentText.Length-1;
			if (currentText.Length - 1 > finalText.Length)
			{
				allCharRevealed = true;
			}
			else if (index != -1)
			{
				currentText = currentText.Substring(0, index);
				currentText += finalText[index].ToString();
				if (blink < 0)
				{
					currentText += fullBlock;
				} else if (blink < blinkDelay)
				{
					currentText += ' ';
				} else
				{
					blink = -blinkDelay;
				}
				blink++;
			}

			dialogueText.text = currentText;
			yield return new WaitForSeconds(charWaitTime);
		}
	}
}