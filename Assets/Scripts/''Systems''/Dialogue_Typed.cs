using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Dialogue_Typed : Dialogue
{
	// Use this for initialization
	private char fullBlock = '\x2588';
	bool allCharRevealed = true;
	double charRevealTime = 0.1;
	double charRevealStartTime = 0;
	string finalText;

	new void Start()
	{
		base.Start();
	}

	protected override void FixedUpdate()
	{
		if (!allCharRevealed)
		{
			if (Time.fixedTime >= charRevealStartTime + charRevealTime)
			{
				RevealNextChar();
			}
			return;
		} else
		{
			charRevealStartTime = Time.fixedTime;
			base.FixedUpdate();
		}
	}

	

	void RevealNextChar()
	{
		charRevealStartTime = Time.fixedTime;
		string currentText = dialogueText.text;
		int index = currentText.IndexOf(fullBlock);
		if (currentText.Length > finalText.Length)
		{
			allCharRevealed = true;
		} else {
			if (index != -1)
			{
				currentText = currentText.Substring(0, index);
				currentText += finalText[index] + fullBlock.ToString();
			}
		}

		dialogueText.text = currentText;
	}

	IEnumerable ThisIsACoroutine()
	{
		for (int i = 0; i < finalText.Length; i++)
		{
			yield return new WaitForSeconds(0.5f);
		}
	}

	protected override void SetDialogue(string dialogue, float textTime)
	{
		allCharRevealed = false;
		finalText = queue.ElementAt(0).Trim();
		string fullBlockOfText = fullBlock.ToString();
		base.SetDialogue(fullBlockOfText, textTime);
		RevealNextChar();
	}
}