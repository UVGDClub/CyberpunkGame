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
    Vector2Int Dimensions
    {
        get { return dimensions.vector2IntValue; }

        set
        {
            if (value == dimensions.vector2IntValue)
                return;

            if (value.x <= 0)
                value = new Vector2Int(1, value.y);
            else if (value.x > char.MaxValue)
                value = new Vector2Int(char.MaxValue, value.y);

            if (value.y <= 0)
                value = new Vector2Int(value.x, 1);
            else if (value.y > char.MaxValue)
                value = new Vector2Int(value.x, char.MaxValue);

            dimensions.vector2IntValue = value;

            levels.arraySize = Dimensions.x * Dimensions.y;
            RefreshLevels();
        }
    }
    SerializedProperty dimensions;
    Vector2Int gridIndex = new Vector2Int();
    bool foldoutEnemySpawnPos = false;
    bool foldoutAudioSettings = false;

    private void OnEnable()
    {
        header.fontStyle = FontStyle.Bold;
    }

    public override void OnInspectorGUI()
    {
        if (((LevelGrid)target).sceneRefPrefab == null)
        {
            EditorGUILayout.LabelField("Assign a scene ref prefab (LevelSceneRefs, Grid > Tilemap) to get started.");
            LevelSceneRefs sceneRefPrefab = (LevelSceneRefs)EditorGUI.ObjectField(GUILayoutUtility.GetRect(50, 20), "Scene Ref Prefab", ((LevelGrid)target).sceneRefPrefab, typeof(LevelSceneRefs), true);

            LevelGrid lg = (LevelGrid)target;
            myTarget = new UnityEditor.SerializedObject(lg);
            SerializedProperty spGrid = myTarget.FindProperty("sceneRefPrefab");
            spGrid.objectReferenceValue = sceneRefPrefab;
            SaveGrid();

            if (sceneRefPrefab == null)
                return;
        }

        Init();

        //GridDimensionsUpdate();
        GridIndicesUpdate();

        DrawLayout();
        
        //levels.objectReferenceValue == null 
        if (dimensions.vector2IntValue.x == 0 || dimensions.vector2IntValue.y == 0
            || gridIndex.x < 0 || gridIndex.x >= dimensions.vector2IntValue.x
            || gridIndex.y < 0 || gridIndex.y >= dimensions.vector2IntValue.y)
        {
            // Debug.Log("trying to index outside dimension bounds");
            return;
        }

        Repaint();

        Save();    
    }

    void Init()
    {
        LevelGrid lg = (LevelGrid)target;
        myTarget = new UnityEditor.SerializedObject(lg);
        levels = myTarget.FindProperty("levels");
        dimensions = myTarget.FindProperty("dimensions");
        Dimensions = dimensions.vector2IntValue;
    }

    void GridIndicesUpdate()
    {
        if (Dimensions.x == 0)
            gridIndex.x = 0;
        else if (gridIndex.x >= Dimensions.x)
            gridIndex.x = Dimensions.x - 1;
        else if (gridIndex.x < 0)
            gridIndex.x = 0;

        if (Dimensions.y == 0)
            gridIndex.y = 0;
        else if (gridIndex.y >= Dimensions.y)
            gridIndex.y = Dimensions.y - 1;
        else if (gridIndex.y < 0)
            gridIndex.y = 0;
    }

    void DrawLayout()
    {
        if (GUI.Button(GUILayoutUtility.GetRect(50, 20), "Populate Grid"))
        {
            PopulateGrid();
            return;
        }            

        EditorGUILayout.Separator();

        if (GUI.Button(GUILayoutUtility.GetRect(50, 20), "Refresh scene indices"))
            ((LevelGrid)target).RefreshLevelsceneIndices();

        EditorGUILayout.Separator();

        EditorGUILayout.LabelField("LevelGrid State", header);
        DrawDefaultInspector();

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("LevelGrid Properties", header);
      
        //DrawDefaultInspector();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Grid dimensions:");
        Dimensions = EditorGUILayout.Vector2IntField("", Dimensions);
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

        if (Dimensions.x > gridIndex.x && Dimensions.y > gridIndex.y
            && gridIndex.x >= 0 && gridIndex.y >= 0)
        {
            if (levels.GetArrayElementAtIndex(gridIndex.x + Dimensions.x * gridIndex.y).objectReferenceValue == null && GetLevelAsset(gridIndex.x, gridIndex.y) == false)
                CreateLevelAsset(gridIndex.x, gridIndex.y);

            Level level = (Level)levels.GetArrayElementAtIndex(gridIndex.x + Dimensions.x * gridIndex.y).objectReferenceValue;

            level = (Level)EditorGUI.ObjectField(levelSelectRect, "Level", level, typeof(Level), false);

            levels.GetArrayElementAtIndex(gridIndex.x + Dimensions.x * gridIndex.y).objectReferenceValue = level;

            _level = new SerializedObject(level);

            if (levels.GetArrayElementAtIndex(gridIndex.x + Dimensions.x * gridIndex.y).objectReferenceValue != null)
            {
                level.scene = (SceneAsset)EditorGUI.ObjectField(sceneSelectRect, "Scene", level.scene, typeof(SceneAsset), true);

                if(level.scene == null)
                {
                    level.scene = GetSceneAsset(gridIndex.x, gridIndex.y);

                    if (level.scene == null)
                        level.scene = CreateSceneAsset(gridIndex.x, gridIndex.y);
                }

                level.RefreshSceneIndex();
            }
            
            foldoutAudioSettings = EditorGUILayout.Foldout(foldoutAudioSettings, "Audio Settings", true);
            
            if(foldoutAudioSettings)
            {
                Rect audioClipSelectRect = GUILayoutUtility.GetRect(50, 20);
                level.audioSettings.clip = (AudioClip)EditorGUI.ObjectField(audioClipSelectRect, "Background Music", level.audioSettings.clip, typeof(AudioClip), true);
                level.audioSettings.fadeRate = EditorGUILayout.Slider("Fade Rate", level.audioSettings.fadeRate, 0, 1);
                level.audioSettings.pitch = EditorGUILayout.Slider("Pitch", level.audioSettings.pitch, -10, 10);                
            }

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
        SaveLevel();
        SaveGrid();   
    }

    public void SaveGrid()
    {
        foreach (Object obj in myTarget.targetObjects)
        {
            EditorUtility.SetDirty(obj);
        }

        myTarget.ApplyModifiedProperties();
    }

    public void SaveLevel()
    {
        foreach (Object obj in _level.targetObjects)
        {
            EditorUtility.SetDirty(obj);
        }

        _level.ApplyModifiedProperties();
    }

    bool GetLevelAsset(int x, int y)
    {
        string path = AssetDatabase.GetAssetPath(target.GetInstanceID());
        path = path.Substring(0, path.Length - target.name.Length - 6);
        path += target.name + "_levels/" + target.name + "_" + x + "_" + y + ".asset";

        Level level = (Level)AssetDatabase.LoadAssetAtPath(path, typeof(Level));
        if (level != null)
        {
            levels.GetArrayElementAtIndex(x + Dimensions.x * y).objectReferenceValue = level;
            return true;
        }
            
        return false;
    }

    void CreateLevelAsset(int x, int y)
    {
        string path = AssetDatabase.GetAssetPath(target.GetInstanceID());
        path = path.Substring(0, path.Length - target.name.Length - 6);
        if (AssetDatabase.IsValidFolder(path + target.name + "_levels") == false)
        {
            //Debug.Log("trying to make folder at: " + path + target.name + "_levels/");
            string folder = AssetDatabase.CreateFolder(path.Substring(0, path.Length - 1), target.name + "_levels");
        }

        ScriptableObject _level = ScriptableObject.CreateInstance<Level>();
        _level.name = target.name + "_" + x + "_" + y;
        AssetDatabase.CreateAsset(_level, path + target.name + "_levels/" + _level.name + ".asset");

        levels.GetArrayElementAtIndex(x + Dimensions.x * y).objectReferenceValue = _level;
    }

    //Should make a callback from modifying dimensions
    public void RefreshLevels()
    {
        string path = AssetDatabase.GetAssetPath(target.GetInstanceID());
        path = path.Substring(0, path.Length - target.name.Length - 6);
        path += target.name + "_levels/" + target.name + "_";

        //Debug.Log("trying to update level references to fit dimensions: " + Dimensions);

        string indicesPlusExtension;
        for (int x = 0; x < Dimensions.x; x++)
        {
            for (int y = 0; y < Dimensions.y; y++)
            {
                indicesPlusExtension = x + "_" + y + ".asset";

               // Debug.Log("current path = " + path + indicesPlusExtension);

                Level level = (Level)AssetDatabase.LoadAssetAtPath(path + indicesPlusExtension, typeof(Level));
                levels.GetArrayElementAtIndex(x + Dimensions.x * y).objectReferenceValue = level;                

               // Debug.Log("level updated for (x,y) " + x + ", " + y);
               // Debug.Log("new array index = " + (x + Dimensions.x * y));
            }
        }
    }

    void PopulateGrid()
    {        
        for (int x = 0; x < Dimensions.x; x++)
        {
            for(int y = 0; y < Dimensions.y; y++)
            {
                if (GetLevelAsset(x, y) == false)
                    CreateLevelAsset(x, y);

                SceneAsset scene = GetSceneAsset(x, y);
                if (scene == null)
                    scene = CreateSceneAsset(x, y);

                //race conditions where new scenes aren't created fast enough to do in one nested loop...
                //there are callbacks for scene creation, maybe that's a better solution?
            }
        }

        string path = AssetDatabase.GetAssetPath(target.GetInstanceID());
        path = path.Substring(0, path.Length - target.name.Length - 6);
        path += target.name + "_levels/" + target.name + "_";
        string indicesPlusExtension;
        for (int x = 0; x < Dimensions.x; x++)
        {
            for (int y = 0; y < Dimensions.y; y++)
            {
                indicesPlusExtension = x + "_" + y + ".asset";
                Level level = (Level)AssetDatabase.LoadAssetAtPath(path + indicesPlusExtension, typeof(Level));

                SceneAsset scene = GetSceneAsset(x, y);
                if (scene == null)
                    scene = CreateSceneAsset(x, y);

                level.scene = scene;
                level.RefreshSceneIndex();
                levels.GetArrayElementAtIndex(x + Dimensions.x * y).objectReferenceValue = level;
            }
        }
    }

    SceneAsset GetSceneAsset(int x, int y)
    {
        string path = AssetDatabase.GetAssetPath(target.GetInstanceID());
        path = path.Substring(0, path.Length - target.name.Length - 6);
        path += target.name + "_scenes";
        SceneAsset scene;
        if (AssetDatabase.IsValidFolder(path) == true)
        {
            path += "/" + target.name + "_(" + x + "," + y + ").unity";
            scene = (SceneAsset)AssetDatabase.LoadAssetAtPath(path, typeof(SceneAsset));

            return scene;
        }        

        return null;
    }

    SceneAsset CreateSceneAsset(int x, int y)
    {
        string path = AssetDatabase.GetAssetPath(target.GetInstanceID());
        path = path.Substring(0, path.Length - target.name.Length - 6);
        if (AssetDatabase.IsValidFolder(path + target.name + "_scenes") == false)
        {
            //Debug.Log("trying to make folder at: " + path + target.name + "_scenes/");
            string folder = AssetDatabase.CreateFolder(path.Substring(0, path.Length - 1), target.name + "_scenes");
        }

        path += target.name + "_scenes";

        //Debug.Log("new scene path = " + path);
       
        Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);

        Scene activeScene = EditorSceneManager.GetActiveScene();

        EditorSceneManager.SetActiveScene(newScene);

        LevelSceneRefs sceneRefs;
        if (myTarget.FindProperty("sceneRefPrefab").objectReferenceValue == null)
        {
            Debug.LogError("Couldn't find scene ref prefab; assign it in the inspector!");
            return null;
        }       
        else
            sceneRefs = Instantiate((LevelSceneRefs)myTarget.FindProperty("sceneRefPrefab").objectReferenceValue);

        sceneRefs.level = (Level)levels.GetArrayElementAtIndex(x + Dimensions.x * y).objectReferenceValue;
        sceneRefs.grid = sceneRefs.GetComponent<Grid>();
        sceneRefs.tilemap = sceneRefs.transform.GetChild(0).GetComponent<UnityEngine.Tilemaps.Tilemap>();

        sceneRefs.name = "Grid_" + x + "_" + y;

        sceneRefs.transform.GetChild(0).name += "_" + x + "_" + y;

        EditorSceneManager.SaveScene(newScene, path + "/" + target.name + "_(" + x + "," + y + ").unity");

        EditorSceneManager.SetActiveScene(activeScene);
        EditorSceneManager.CloseScene(newScene, true);

        return (SceneAsset)AssetDatabase.LoadAssetAtPath(newScene.path, typeof(SceneAsset));
    }

}
