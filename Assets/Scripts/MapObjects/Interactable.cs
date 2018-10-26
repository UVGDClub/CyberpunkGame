using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class Interactable : MonoBehaviour {

	/* 
    Place this component on any GameObject in the level that has a collider.
    Then, you can connect it to any other GameObject in the scene and execute public functions of them.
    This is useful for making any buttons, damaging zones, levers, doors, etc that need to interacted with.
    The trigger unityevents will only work if the collider is set to "asdfadsf" mode.
    */

	[Tooltip("Set to true if you want the sprite to display a highlighted outline when it is hovered over.")]
	[SerializeField] private bool useHighlightShader = true;

	//Event variables
	[SerializeField] UnityEvent mouseEnterEvent;
	[SerializeField] UnityEvent mouseExitEvent;
	[SerializeField] UnityEvent mouseDownEvent;
	[SerializeField] UnityEvent enterTriggerEvent;
	[SerializeField] UnityEvent exitTriggerEvent;

	//Visual/shader variables
    [SerializeField] Material highlightMat; //Alternate material for when the object is highlighted. 
    private Material originalMat; //The material assigned to the object in the editor. 
    private SpriteRenderer mySprite;
	private int mySpriteSize;
	[SerializeField] private Color myOutlineColour = Color.white;

	void OnMouseEnter(){
		mouseEnterEvent.Invoke();
		if (useHighlightShader){
			mySprite.material = highlightMat;
			SetVisuals(mySpriteSize, myOutlineColour);
		}

	}

	void OnMouseExit(){
		mouseExitEvent.Invoke();

		if (useHighlightShader){
			mySprite.material = originalMat;
		}
	}

	void OnMouseDown(){
		mouseDownEvent.Invoke();
	}

    void OnTriggerEnter2D(Collider2D col){
		if (true){ // Replace this with code for what you want to trigger it. For example, "if(col.gameobject.tag == 'Player')"
			enterTriggerEvent.Invoke();
		}
    }

	void OnTriggerExit2D(Collider2D col){
		if (true){ // Replace this with code for what you want to trigger it. For example, "if(col.gameobject.tag == 'Player')"
			exitTriggerEvent.Invoke();
		}
	}

	// Use this for initialization
	void Start () {
		//mouseEnterEvent = new UnityEvent();
		//mouseExitEvent = new UnityEvent();
		//mouseDownEvent = new UnityEvent();
  //      enterTriggerEvent = new UnityEvent();
  //      exitTriggerEvent = new UnityEvent();

		mySprite = this.GetComponent<SpriteRenderer>();
        if (mySprite != null){
			originalMat = mySprite.material;
			mySpriteSize = (int)mySprite.sprite.rect.x;
			if (useHighlightShader)
			{
				mySprite.material = highlightMat;
				SetVisuals(mySpriteSize, myOutlineColour);
				mySprite.material = originalMat;
			}
		}
	}

	public void SetVisuals(int spriteSize, Color outlineColour){
		this.mySprite.material.SetInt("_SpriteWidth", spriteSize); 
		this.mySprite.material.SetColor("_OutlineColor", outlineColour);
	}

	void Update () {
		
	}
}
