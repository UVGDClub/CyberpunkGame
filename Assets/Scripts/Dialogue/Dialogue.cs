using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class Dialogue : MonoBehaviour {

    public int MaxCharacters= 120;

    public UnityEvent OnComplete;
    private Text DialogText;

    private string entireDialog = "";
    private int indexInString = 0;

    private void Start() {
        DialogText = GetComponent<Text>();
    }

    public void SetDialog(string dialog) {
        entireDialog = dialog;
        indexInString = 0;
        SetText();
    }

    public void ContinueText() {
        if(indexInString >= entireDialog.Length) {
            OnComplete.Invoke();
            return;
        }
        indexInString += MaxCharacters;
        SetText();
    }

    private void SetText() {
        DialogText.text = entireDialog.Substring(indexInString, Mathf.Min(MaxCharacters, entireDialog.Length - indexInString));
    }
}
