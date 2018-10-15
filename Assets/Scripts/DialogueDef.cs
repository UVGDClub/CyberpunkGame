using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue Def")]
public class DialogueDef : ScriptableObject {

    [TextArea(2,5)]
    public string Text;
}
