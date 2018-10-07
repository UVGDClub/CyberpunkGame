using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(LevelGrid))]
public class LevelGridEditorGUI : Editor  {

    SerializedObject myTarget;
    SerializedProperty levels;
    SerializedProperty dimensions;
    Vector2Int gridIndex = new Vector2Int();
    
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        Init();
        
        GridDimensionsUpdate();
        GridIndicesUpdate();

        DrawLayout();
        
        //levels.objectReferenceValue == null 
        if (dimensions.vector2IntValue.x == 0 || dimensions.vector2IntValue.y == 0
            || gridIndex.x < 0 || gridIndex.x >= dimensions.vector2IntValue.x
            || gridIndex.y < 0 || gridIndex.y >= dimensions.vector2IntValue.y)
        {
            Debug.Log("trying to index outside dimension bounds");
            return;
        }
            

        if (levels.GetArrayElementAtIndex(gridIndex.x + dimensions.vector2IntValue.x * gridIndex.y).objectReferenceValue == null && GetLevelAsset() == false)
            CreateLevelAsset();
            
        Repaint();

        Save();    
    }

    void Init()
    {
        LevelGrid lg = (LevelGrid)target;
        myTarget = new UnityEditor.SerializedObject(lg);
        levels = myTarget.FindProperty("levels");
        dimensions = myTarget.FindProperty("dimensions");
    }

    void GridDimensionsUpdate()
    {
        if (dimensions.vector2IntValue.x > char.MaxValue)
            dimensions.vector2IntValue = new Vector2Int(char.MaxValue, dimensions.vector2IntValue.y);
        else if(dimensions.vector2IntValue.x < 0)
            dimensions.vector2IntValue = new Vector2Int(0, dimensions.vector2IntValue.y);

        if (dimensions.vector2IntValue.y > char.MaxValue)
            dimensions.vector2IntValue = new Vector2Int(dimensions.vector2IntValue.x, char.MaxValue);
        else if (dimensions.vector2IntValue.y < 0)
            dimensions.vector2IntValue = new Vector2Int(dimensions.vector2IntValue.x, 0);
        
    }

    void GridIndicesUpdate()
    {
        if (dimensions.vector2IntValue.x == 0)
            gridIndex.x = 0;
        else if (gridIndex.x >= dimensions.vector2IntValue.x)
            gridIndex.x = dimensions.vector2IntValue.x - 1;
        else if (gridIndex.x < 0)
            gridIndex.x = 0;

        if (dimensions.vector2IntValue.y == 0)
            gridIndex.y = 0;
        else if (gridIndex.y >= dimensions.vector2IntValue.y)
            gridIndex.y = dimensions.vector2IntValue.y - 1;
        else if (gridIndex.y < 0)
            gridIndex.y = 0;
    }

    void DrawLayout()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Grid dimensions:");
        dimensions.vector2IntValue = EditorGUILayout.Vector2IntField("", dimensions.vector2IntValue);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Grid index:");
        gridIndex = EditorGUILayout.Vector2IntField("", gridIndex);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        Rect levelSelectRect = GUILayoutUtility.GetRect(50, 20);
        Rect sceneSelectRect = GUILayoutUtility.GetRect(50, 20);

        if (dimensions.vector2IntValue.x > gridIndex.x && dimensions.vector2IntValue.y > gridIndex.y
            && gridIndex.x >= 0 && gridIndex.y >= 0)
        {
            Level level = (Level)levels.GetArrayElementAtIndex(gridIndex.x + dimensions.vector2IntValue.x * gridIndex.y).objectReferenceValue;
            level = (Level)EditorGUI.ObjectField(levelSelectRect, "Level", level, typeof(Level), false);

            levels.GetArrayElementAtIndex(gridIndex.x + dimensions.vector2IntValue.x * gridIndex.y).objectReferenceValue = level;

            if (levels.GetArrayElementAtIndex(gridIndex.x + dimensions.vector2IntValue.x * gridIndex.y).objectReferenceValue != null)
            {
                level.scene = (SceneAsset)EditorGUI.ObjectField(sceneSelectRect, "Scene", level.scene, typeof(SceneAsset), true);
                level.RefreshSceneIndex();
            }
                
        }
        else
        {
            Debug.Log("oops something went wrong, not drawing object fields");
        }
    }

    public void Save()
    {
        foreach (Object obj in myTarget.targetObjects)
        {
            EditorUtility.SetDirty(obj);
        }

        myTarget.ApplyModifiedProperties();
    }

    bool GetLevelAsset()
    {
        string path = AssetDatabase.GetAssetPath(target.GetInstanceID());
        path = path.Substring(0, path.Length - target.name.Length - 6);
        path += target.name + "_levels/" + target.name + "_" + gridIndex.x + "_" + gridIndex.y + ".asset";

        Level level = (Level)AssetDatabase.LoadAssetAtPath(path, typeof(Level));
        if (level != null)
        {
            SerializedProperty levelAtIndex = levels.GetArrayElementAtIndex(gridIndex.x + dimensions.vector2IntValue.x * gridIndex.y);
            levelAtIndex.objectReferenceValue = level;
            return true;
        }
            
        return false;
    }

    void CreateLevelAsset()
    {
        string path = AssetDatabase.GetAssetPath(target.GetInstanceID());
        path = path.Substring(0, path.Length - target.name.Length - 6);
        if (AssetDatabase.IsValidFolder(path + target.name + "_levels") == false)
        {
            Debug.Log("trying to make folder at: " + path + target.name + "_levels/");
            string folder = AssetDatabase.CreateFolder(path.Substring(0, path.Length - 1), target.name + "_levels");
        }

        ScriptableObject _level = ScriptableObject.CreateInstance<Level>();
        _level.name = target.name + "_" + gridIndex.x + "_" + gridIndex.y;
        AssetDatabase.CreateAsset(_level, path + target.name + "_levels/" + _level.name + ".asset");

        levels.GetArrayElementAtIndex(gridIndex.x + dimensions.vector2IntValue.x * gridIndex.y).objectReferenceValue = _level;
    }

}
