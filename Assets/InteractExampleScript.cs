using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractExampleScript : MonoBehaviour {

	public Color[] colourList;
	private SpriteRenderer sp;
	private int iterator;

	public void ChangeColours(){
		sp.color = colourList[iterator];
		iterator++;
		if (iterator >= colourList.Length){Destroy(this.gameObject);}
	}

	// Use this for initialization
	void Start () {
		sp = this.GetComponent<SpriteRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
