using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(Level))]
public class LevelEditorGUI : Editor  {

    //Rect levelLabelRect;

    public override void OnInspectorGUI()
    {
        Level myTarget = (Level)target;
        EditorGUILayout.LabelField("Scene Index: " + myTarget.sceneIndex);
        if (GUI.Button(GUILayoutUtility.GetRect(50, 20), "Refresh scene index"))
            myTarget.RefreshSceneIndex();

        DrawDefaultInspector(); //rather than this, make the slider go below the scene name
        
        //Level myTarget = (Level)target;

        //EditorGUI.DrawPreviewTexture(GUILayoutUtility.GetRect(128, 128), myTarget.);

        /*
        if (myTarget.sceneIndex >= SceneManager.sceneCountInBuildSettings)
            myTarget.sceneIndex = SceneManager.sceneCountInBuildSettings - 1;

        string levelName = SceneManager.GetSceneAt(myTarget.sceneIndex).name;

        levelLabelRect = GUILayoutUtility.GetRect(50, 20);
        EditorGUI.LabelField(levelLabelRect, "Scene Name: " + levelName);

        this.Repaint();
        */
    }
}
