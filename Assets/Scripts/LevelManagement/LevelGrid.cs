using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

[CreateAssetMenu(menuName = "LevelGrid")]
public class LevelGrid : ScriptableObject {
    [Range(1,char.MaxValue)]
    public int activeRadius = 1;
    public Vector2Int position = Vector2Int.one;
    public Vector2Int Position
    {
        get { return position; }

        set
        {
            if (Position == value)
                return;

            position = value;
            if(CrossfadeBGM != null && levels[Position.x + dimensions.x * Position.y].backgroundMusic != null )
            {
                CrossfadeBGM(levels[Position.x + dimensions.x * Position.y].backgroundMusic, levels[Position.x + dimensions.x * Position.y].fadeRate);
            }
        }
    }
    public delegate void CrossfadeBGM_Delegate(AudioClip clip, float fadeRate);
    public event CrossfadeBGM_Delegate CrossfadeBGM;

    [HideInInspector][SerializeField] Vector2Int dimensions = Vector2Int.one;
    public int currentScene = -1;
    [HideInInspector][SerializeField] Level[] levels = new Level[25];    
    Dictionary<int, Vector2Int> activeScenes = new Dictionary<int, Vector2Int>();

    public void InitializeActiveGrid(Vector2Int center)
    {
        //once saving is implemented, set Position based on save data
        //when starting a new game, remember to set the Position accordingly
        Position = center;
        activeScenes = new Dictionary<int, Vector2Int>();
        
        currentScene = levels[Position.x + dimensions.x * Position.y].sceneIndex;
        LoadLevel(Position.x + dimensions.x * Position.y);
        activeScenes.Add(currentScene, Position);

        Debug.Log("levels length: " + levels.Length);
        Debug.Log("EditorBuildSettingsSceneLength: " + EditorBuildSettings.scenes.Length);

        for (int x = Position.x - activeRadius; x <= Position.x + activeRadius; x++)
        {
            if (x < 0 || x >= dimensions.x)
            {
                Debug.Log("x: " + x + " outside bounds");
                continue;
            }

            for (int y = Position.y - activeRadius; y <= Position.y + activeRadius; y++)
            {
                if (y < 0 || y >= dimensions.y)
                {
                    Debug.Log("y: " + y + " outside bounds");
                    continue;
                }

                if (x == Position.x && y == Position.y)
                    continue;

                if (levels[x + dimensions.x*y] == null)
                {
                    Debug.Log("level at x,y: " + x + ", " + y + " is null");
                    continue;
                }
                if (levels[x + dimensions.x * y].scene == null)
                {
                    Debug.Log("level at x, y: " + x + ", " + y + " scene is null");
                    continue;
                }
                if (levels[x + dimensions.x * y].sceneIndex < 0)
                {
                    Debug.Log("scene index at x,y: " + x + ", " + y + " is " + levels[x + dimensions.x * y].sceneIndex);
                    continue;
                }

                if (levels[x + dimensions.x * y].sceneIndex >= EditorBuildSettings.scenes.Length)
                {
                    Debug.Log("scene index at x,y " + x + ", " + y + " is " + levels[x + dimensions.x * y].sceneIndex + " >= build order length" + EditorBuildSettings.scenes.Length);
                    continue;
                }
                Debug.Log("loading " + levels[x + dimensions.x * y].scene.name + " at buildIndex: " + levels[x + dimensions.x * y].sceneIndex);

                activeScenes.Add(levels[x + dimensions.x * y].sceneIndex, new Vector2Int(x, y));
                LoadLevel(x + dimensions.x * y);
            }
        }
    }

    public void CompareScenePositions(int otherIndex)
    {
        Debug.Log("otherIndex: " + otherIndex);

        if (otherIndex == currentScene)
            return;

        Vector2Int newPosition;
        if (!activeScenes.TryGetValue(otherIndex, out newPosition))
        {
            for (int x = 0; x < dimensions.x; x++)
            {
                for (int y = 0; y < dimensions.y; y++)
                {
                    if (levels[x + dimensions.x * y] == null)
                        continue;

                    if (levels[x + dimensions.x * y].sceneIndex == otherIndex)
                        newPosition = new Vector2Int(x, y);
                }
            }
            if (newPosition == null)
            {
                Debug.LogWarning("Couldn't resolve Position in grid from scene index {" + otherIndex + "} in build order!");
                return;
            }
                
        }

        Debug.Log(newPosition + " - " + Position + " = " + (newPosition - Position));

        UpdateActiveGrid(newPosition - Position);
        currentScene = otherIndex;

    }

    /// <summary>
    /// Loads and unloads level chunks based on the radius around the Position in the level grid
    /// where the player is currently.
    ///
    /// Seems that unloading levels is a bit slow, and since asnc operations can't be cancelled, sometimes levels
    /// are being unloaded when we don't want them to be.
    /// Given a recent profile, having 3x3 relatively small levels active at once doesn't seem to be a performance burden.
    /// 
    /// </summary>
    /// <param name="offset"></param>    
    public void UpdateActiveGrid(Vector2Int offset)
    {
        Debug.Log("offset: " + offset);

        Debug.Log("\nbefore applying offset, pos = " + Position);
        Position += offset;
        Debug.Log("after applying offset, pos = " + Position);

        switch (offset.x)
        {
            case -1:
                switch (offset.y)
                {
                    case -1:
                        //UnloadLevelRange(new Vector2Int(2, 2), new Vector2Int(0, 2));
                        //UnloadLevelRange(new Vector2Int(0, 1), new Vector2Int(2, 2));

                        LoadLevelRange(new Vector2Int(-1, -1), new Vector2Int(-1, 1));
                        LoadLevelRange(new Vector2Int(0, 1), new Vector2Int(-1, 1));
                        break;

                    case 0:
                        //UnloadLevelRange(new Vector2Int(2, 2), new Vector2Int(-1, 1));
                        LoadLevelRange(new Vector2Int(-1, -1), new Vector2Int(-1, 1));
                        break;

                    case 1:
                        //UnloadLevelRange(new Vector2Int(2, 2), new Vector2Int(-2, 0));
                        //UnloadLevelRange(new Vector2Int(0, 1), new Vector2Int(-2, -2));

                        LoadLevelRange(new Vector2Int(-1, -1), new Vector2Int(-1, 1));
                        LoadLevelRange(new Vector2Int(0, 1), new Vector2Int(-1, -1));
                        break;

                    default:
                        //teleport??
                        break;
                }
                break;

            case 0:
                switch (offset.y)
                {
                    case -1:
                        //UnloadLevelRange(new Vector2Int(-1, 1), new Vector2Int(2, 2));
                        LoadLevelRange(new Vector2Int(-1, 1), new Vector2Int(-1, -1));

                        break;

                    case 0:
                        //not sure why this would be running? do nothing
                        break;

                    case 1:
                        //UnloadLevelRange(new Vector2Int(-1, 1), new Vector2Int(-2, -2));
                        LoadLevelRange(new Vector2Int(-1, 1), new Vector2Int(1, 1));
                        break;

                    default:
                        //teleport??
                        break;
                }
                break;

            case 1:
                switch (offset.y)
                {
                    case -1:
                        //UnloadLevelRange(new Vector2Int(-2, -2), new Vector2Int(0, 2));
                        //UnloadLevelRange(new Vector2Int(-1, 0), new Vector2Int(2, 2));

                        LoadLevelRange(new Vector2Int(1, 1), new Vector2Int(-1, 1));
                        LoadLevelRange(new Vector2Int(-1, 0), new Vector2Int(-1, -1));
                        break;

                    case 0:
                        //UnloadLevelRange(new Vector2Int(-2, -2), new Vector2Int(-1, 1));
                        LoadLevelRange(new Vector2Int(1, 1), new Vector2Int(-1, 1));
                        break;

                    case 1:
                        //UnloadLevelRange(new Vector2Int(-2, -2), new Vector2Int(0, -2));
                        //UnloadLevelRange(new Vector2Int(-1, 1), new Vector2Int(-2, -2));
                        LoadLevelRange(new Vector2Int(1, 1), new Vector2Int(-1, 1));
                        LoadLevelRange(new Vector2Int(-1, 0), new Vector2Int(1, 1));
                        break;

                    default:
                        //teleport??
                        break;
                }
                break;

            default:
                //teleport??
                break;
        }
    }
    
    public void UnloadLevelRange(Vector2Int xRange, Vector2Int yRange)
    {
        //Debug.Log("unloading levels...");

        xRange *= activeRadius;
        yRange *= activeRadius;

        for(int x = Position.x + xRange.x; x <= Position.x + xRange.y; x++)
        {
            if (x < 0 || x >= dimensions.x)
                continue; 

            for (int y = Position.y + yRange.x; y <= Position.y + yRange.y; y++)
            {
                if (y < 0 || y >= dimensions.y
                    || levels[x + dimensions.x * y] == null
                    || levels[x + dimensions.x * y].sceneIndex < 0 
                    || levels[x + dimensions.x * y].sceneIndex >= EditorBuildSettings.scenes.Length
                    || activeScenes.ContainsKey(levels[x + dimensions.x * y].sceneIndex) == false)
                    continue;

                Debug.Log("unloading scene: " + levels[x + dimensions.x * y].name);

                activeScenes.Remove(levels[x + dimensions.x * y].sceneIndex);
                SceneManager.UnloadSceneAsync(levels[x + dimensions.x * y].sceneIndex);
            }
        }
    }

    public void LoadLevelRange(Vector2Int xRange, Vector2Int yRange)
    {
        //Debug.Log("loading levels...");
        xRange *= activeRadius;
        yRange *= activeRadius;

        for (int x = Position.x + xRange.x; x <= Position.x + xRange.y; x++)
        {
            if (x < 0 || x >= dimensions.x)
                continue;

            for (int y = Position.y + yRange.x; y <= Position.y + yRange.y; y++)
            {
                if (y < 0 || y >= dimensions.y
                    || levels[x + dimensions.x * y] == null
                    || levels[x + dimensions.x * y].scene == null
                    || levels[x + dimensions.x * y].sceneIndex < 0
                    || levels[x + dimensions.x * y].sceneIndex >= EditorBuildSettings.scenes.Length
                    || activeScenes.ContainsKey(levels[x + dimensions.x * y].sceneIndex) == true)
                    continue;

                activeScenes.Add(levels[x + dimensions.x * y].sceneIndex, new Vector2Int(x,y));
                LoadLevel(x + dimensions.x * y);
                //SceneManager.LoadScene(levels[x + dimensions.x * y].sceneIndex, LoadSceneMode.Additive);
            }
        }
    }

    public void LoadLevel(int gridIndex)
    {
        SceneManager.LoadScene(levels[gridIndex].sceneIndex, levels[gridIndex].loadSceneMode);
        
        for(int i = 0; i < levels[gridIndex].enemySpawnInfo.Length; i++)
        {
            if (levels[gridIndex].enemySpawnInfo[i].name == "" || levels[gridIndex].enemySpawnInfo[i].spawnMode != EnemySpawnMode.OnLoad)
                continue;

            Debug.Log("Trying to spawn " + levels[gridIndex].enemySpawnInfo[i].name);
            EnemyManager.SpawnEnemy(levels[gridIndex].enemySpawnInfo[i].name,
                                    levels[gridIndex].enemySpawnInfo[i].position,
                                    levels[gridIndex].enemySpawnInfo[i].facingDir);
        }
    }

    [ContextMenu("Refresh Level sceneIndices")]
    public void RefreshLevelsceneIndices()
    {
        for(int x = 0; x < dimensions.x; x++)
        {
            for(int y = 0; y < dimensions.y; y++)
            {
                if(levels[x + dimensions.x * y] != null)
                {
                    levels[x + dimensions.x * y].RefreshSceneIndex();

                    if(levels[x + dimensions.x * y] != null && levels[x + dimensions.x * y].scene != null)
                        Debug.Log(levels[x + dimensions.x * y].name + " sceneName: " + levels[x + dimensions.x * y].scene.name +  " sceneIndex: " + levels[x + dimensions.x * y].sceneIndex);
                }
            }
        }
    }

    public Level GetLevelFromBuildIndex(int index)
    {
        foreach(Level l in levels)
        {
            if (l == null)
                continue;

            if (l.sceneIndex == index)
                return l;
        }

        return null;
    }
}
