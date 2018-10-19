using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dialogue_Typed : Dialogue
{
	// Use this for initialization
	void Start()
	{
		base.Start();
	}

	// Update is called once per frame
	void Update()
	{

	}

	protected override void SetDialogue(string dialogue, float textTime)
	{
		string fullBlock = "\02588"; //Replace this is better char


		base.SetDialogue(dialogue + fullBlock, textTime);
	}
}