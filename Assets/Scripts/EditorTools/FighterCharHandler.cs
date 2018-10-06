using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;

[CustomEditor(typeof(FighterChar), true)]
public class FighterCharHandler : Editor {

	private FighterChar myFighterChar;

	void Awake() 
	{
		myFighterChar = (FighterChar)target;
	}

	public override void OnInspectorGUI() 
	{
		DrawDefaultInspector();
		if(GUILayout.Button("Set default movement variables."))
		{
			Undo.RecordObject(myFighterChar, "Set default movement vars" );
			myFighterChar.m.SetDefaults();
		}

		if(GUILayout.Button("Set default audiovisual variables."))
		{
			Undo.RecordObject(myFighterChar, "Set default audiovisual vars" );
			myFighterChar.v.SetDefaults();
		}
	}

//	public void DefaultMovementVars()
//	{
//		myFighterChar.m = new MovementVars();
//	}
//

}
