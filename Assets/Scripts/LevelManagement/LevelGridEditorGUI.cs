using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(LevelGrid))]
public class LevelGridEditorGUI : Editor  {
    GUIStyle header = new GUIStyle();
    SerializedObject myTarget;
    SerializedProperty levels;
    SerializedObject _level;
    SerializedProperty dimensions;
    Vector2Int gridIndex = new Vector2Int();
    bool foldoutEnemySpawnPos = false;

    private void OnEnable()
    {
        header.fontStyle = FontStyle.Bold;
    }

    public override void OnInspectorGUI()
    {
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

        levels.arraySize = dimensions.vector2IntValue.x * dimensions.vector2IntValue.y;     
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
        EditorGUILayout.LabelField("LevelGrid State", header);
        DrawDefaultInspector();

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("LevelGrid Properties", header);

        //DrawDefaultInspector();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Grid dimensions:");
        dimensions.vector2IntValue = EditorGUILayout.Vector2IntField("", dimensions.vector2IntValue);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Grid index:");
        gridIndex = EditorGUILayout.Vector2IntField("", gridIndex);
        EditorGUILayout.EndHorizontal();

        Rect levelSelectRect = GUILayoutUtility.GetRect(50, 20);

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Level Properties", header);

        Rect sceneSelectRect = GUILayoutUtility.GetRect(50, 20);
        Rect audioClipSelectRect = GUILayoutUtility.GetRect(50, 20);

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

            level.backgroundMusic = (AudioClip)EditorGUI.ObjectField(audioClipSelectRect, "Background Music", level.backgroundMusic, typeof(AudioClip), true);

            _level = new SerializedObject(level);
            SerializedProperty enemySpawnInfo = _level.FindProperty("enemySpawnInfo");

            EditorGUILayout.BeginHorizontal();
            foldoutEnemySpawnPos = EditorGUILayout.Foldout(foldoutEnemySpawnPos, "Enemy Spawn Positions", true);
            enemySpawnInfo.arraySize = EditorGUILayout.IntField(enemySpawnInfo.arraySize);
            EditorGUILayout.EndHorizontal();

            if (foldoutEnemySpawnPos)
            {
                EditorGUIUtility.labelWidth = 55;
                for (int i = 0; i < level.enemySpawnInfo.Length; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    
                    EditorGUILayout.LabelField("Name: ", GUILayout.Width(45));
                    EditorGUILayout.TextField(level.enemySpawnInfo[i].name, GUILayout.MaxWidth(65));
                    EditorGUILayout.LabelField("Facing: ", GUILayout.Width(50));
                    EditorGUILayout.EnumPopup(level.enemySpawnInfo[i].facingDir, GUILayout.Width(70));
                    EditorGUILayout.Vector2Field("Position", level.enemySpawnInfo[i].position);

                    EditorGUILayout.EndHorizontal();
                }
                EditorGUIUtility.labelWidth = 0;
            }
                

            

            
        }
        /*else
        {
            Debug.Log("oops something went wrong, not drawing object fields");
        }*/
    }

    public void Save()
    {
        foreach (Object obj in _level.targetObjects)
        {
            EditorUtility.SetDirty(obj);
        }

        _level.ApplyModifiedProperties();

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
