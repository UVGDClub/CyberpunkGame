using Audio;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

[CreateAssetMenu(menuName = "LevelGrid")]
public class LevelGrid : ScriptableObject {
    public LevelSceneRefs sceneRefPrefab;
    [Range(1,char.MaxValue)]
    public int activeRadius = 1;
    
    //The current index within levels[] = position.x + dimensions.x * position.y
    [HideInInspector] public int curIndex;
    public Vector2Int position = Vector2Int.one;
    public Vector2Int Position
    {
        get { return position; }

        set
        {
            if (Position == value)
                return;

            position = value;
            if(CrossfadeBGM != null && levels[Position.x + dimensions.x * Position.y].audioSettings.clip != null)
            {
                CrossfadeBGM(levels[Position.x + dimensions.x * Position.y].audioSettings);
            }
        }
    }
    public delegate void CrossfadeBGM_Delegate(LevelAudioSettings las);
    public event CrossfadeBGM_Delegate CrossfadeBGM;

    [HideInInspector]public Vector2Int dimensions = Vector2Int.one;
    public int currentScene = -1;
    [HideInInspector][SerializeField] public Level[] levels = new Level[25];    
    Dictionary<int, Vector2Int> activeScenes = new Dictionary<int, Vector2Int>();

    [HideInInspector] public Grid grid;
    [HideInInspector] public UnityEngine.Tilemaps.Tilemap tilemap;
    [HideInInspector] public TileInfo tileInfo;

    public void InitializeActiveGrid(PlayerSpawnInfo spawn)
    {
        //once saving is implemented, set Position based on save data
        //when starting a new game, remember to set the Position accordingly
        Position = spawn.gridPosition;
        activeScenes = new Dictionary<int, Vector2Int>();

        curIndex = Position.x + dimensions.x * Position.y;
        currentScene = levels[curIndex].sceneIndex;
        LoadLevel(curIndex);
        activeScenes.Add(currentScene, Position);        

        Debug.Log("levels length: " + levels.Length);
        Debug.Log("EditorBuildSettingsSceneLength: " + EditorBuildSettings.scenes.Length);

        int index;
        for (int x = Position.x - activeRadius; x <= Position.x + activeRadius; x++)
        {
            if (x < 0 || x >= dimensions.x)
            {
                //Debug.Log("x: " + x + " outside bounds");
                continue;
            }

            for (int y = Position.y - activeRadius; y <= Position.y + activeRadius; y++)
            {
                if (y < 0 || y >= dimensions.y)
                {
                    //Debug.Log("y: " + y + " outside bounds");
                    continue;
                }

                index = x + dimensions.x * y;
                if (x == Position.x && y == Position.y)
                    continue;

                if (levels[index] == null)
                {
                    //Debug.Log("level at x,y: " + x + ", " + y + " is null");
                    continue;
                }
                if (levels[index].scene == null)
                {
                    //Debug.Log("level at x, y: " + x + ", " + y + " scene is null");
                    continue;
                }
                if (levels[index].sceneIndex < 0)
                {
                    //Debug.Log("scene index at x,y: " + x + ", " + y + " is " + levels[index].sceneIndex);
                    continue;
                }

                if (levels[index].sceneIndex >= EditorBuildSettings.scenes.Length)
                {
                    //Debug.Log("scene index at x,y " + x + ", " + y + " is " + levels[index].sceneIndex + " >= build order length" + EditorBuildSettings.scenes.Length);
                    continue;
                }
                //Debug.Log("loading " + levels[index].scene.name + " at buildIndex: " + levels[index].sceneIndex);

                activeScenes.Add(levels[index].sceneIndex, new Vector2Int(x, y));
                LoadLevel(index);
            }
        }
    }

    public void CompareScenePositions(int otherIndex)
    {
        //Debug.Log("otherIndex: " + otherIndex);

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

        //Debug.Log(newPosition + " - " + Position + " = " + (newPosition - Position));

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
        //Debug.Log("offset: " + offset);

        //Debug.Log("\nbefore applying offset, pos = " + Position);
        Position += offset;
        curIndex = position.x + dimensions.x * position.y;
        //Debug.Log("after applying offset, pos = " + Position);

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

        int index;
        for (int x = Position.x + xRange.x; x <= Position.x + xRange.y; x++)
        {
            if (x < 0 || x >= dimensions.x)
                continue; 

            for (int y = Position.y + yRange.x; y <= Position.y + yRange.y; y++)
            {
                index = x + dimensions.x * y;
                if (y < 0 || y >= dimensions.y
                    || levels[index] == null
                    || levels[index].sceneIndex < 0 
                    || levels[index].sceneIndex >= EditorBuildSettings.scenes.Length
                    || activeScenes.ContainsKey(levels[index].sceneIndex) == false)
                    continue;

                Debug.Log("unloading scene: " + levels[index].name);

                activeScenes.Remove(levels[index].sceneIndex);
                SceneManager.UnloadSceneAsync(levels[index].sceneIndex);
            }
        }
    }

    public void LoadLevelRange(Vector2Int xRange, Vector2Int yRange)
    {
        //Debug.Log("loading levels...");
        xRange *= activeRadius;
        yRange *= activeRadius;

        int index;
        for (int x = Position.x + xRange.x; x <= Position.x + xRange.y; x++)
        {
            if (x < 0 || x >= dimensions.x)
                continue;

            for (int y = Position.y + yRange.x; y <= Position.y + yRange.y; y++)
            {
                index = x + dimensions.x * y;
                if (y < 0 || y >= dimensions.y
                    || levels[index] == null
                    || levels[index].scene == null
                    || levels[index].sceneIndex < 0
                    || levels[index].sceneIndex >= EditorBuildSettings.scenes.Length
                    || activeScenes.ContainsKey(levels[index].sceneIndex) == true)
                    continue;

                activeScenes.Add(levels[index].sceneIndex, new Vector2Int(x,y));
                LoadLevel(index);
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
