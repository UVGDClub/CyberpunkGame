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
    public Vector2Int position = Vector2Int.zero;

    //need to flatten array for serialization; can use x*y; index with [x + dimension.x*y]
    [HideInInspector][SerializeField] Vector2Int dimensions = Vector2Int.zero;
    public int currentScene = -1;
    public Level[] levels = new Level[25];    
    Dictionary<int, Vector2Int> activeScenes = new Dictionary<int, Vector2Int>();

    public void ModifyGridSize(int x, int y)
    {
        Level[] temp = new Level[x * y];
        for(int i = 0; i < x && i < dimensions.x; i++)
        {
            for(int k = 0; k < y && k < dimensions.y; k++)
            {
                temp[i * k] = levels[i * k];
            }
        }

        levels = temp;
    }

    //@todo paramaterize initial position once saving/loading is implemented
    public void InitializeActiveGrid(Vector2Int center)
    {
        //once saving is implemented, set position based on save data
        //when starting a new game, remember to set the position accordingly

        position = center;
        activeScenes = new Dictionary<int, Vector2Int>();

        //Quick and dirty test -- should have a base scene to load from, and load additively so that 
        //all 'levels' have no essential gameobjects in the scene at load.
        
        currentScene = levels[position.x + dimensions.x * position.y].sceneIndex;
        SceneManager.LoadScene(currentScene, LoadSceneMode.Additive);
        activeScenes.Add(currentScene, position);

        Debug.Log("levels length: " + levels.Length);
        Debug.Log("EditorBuildSettingsSceneLength: " + EditorBuildSettings.scenes.Length);

        for (int x = position.x - activeRadius; x <= position.x + activeRadius; x++)
        {
            if (x < 0 || x >= dimensions.x)
            {
                Debug.Log("x: " + x + " outside bounds");
                continue;
            }

            for (int y = position.y - activeRadius; y <= position.y + activeRadius; y++)
            {
                if (y < 0 || y >= dimensions.y)
                {
                    Debug.Log("y: " + y + " outside bounds");
                    continue;
                }

                if (x == position.x && y == position.y)
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

                //AsyncOperation ao = SceneManager.LoadSceneAsync(levels[x, y].sceneIndex, LoadSceneMode.Additive);
                //ao.allowSceneActivation = true;
                SceneManager.LoadScene(levels[x + dimensions.x * y].sceneIndex, LoadSceneMode.Additive);
            }
        }
    }

    public void CompareScenePositions(int otherIndex)
    {
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
                Debug.LogWarning("Couldn't resolve position in grid from scene index {" + otherIndex + "} in build order!");
                return;
            }
                
        }

        UpdateActiveGrid(newPosition - position);
        currentScene = otherIndex;

    }

    /// <summary>
    /// Loads and unloads level chunks based on the radius around the position in the level grid
    /// where the player is currently.
    /// 
    /// Idea:
    /// Consider updating player position based on the ground check in the player's main loop.
    /// See which scene the tileset collider belongs to, and then set the position accordingly.
    /// </summary>
    /// <param name="offset"></param>    
    public void UpdateActiveGrid(Vector2Int offset)
    {
        Debug.Log("offset " + offset);

        //testing
        position += offset;

        switch(offset.x)
        {
            case -1:
                switch (offset.y)
                {
                    case -1:
                        //unload position.x + 2 for position.y to position.y + 2
                        //unload position.x to position.x + 1 for position.y + 2
                        UnloadLevelRange(new Vector2Int(2, 2), new Vector2Int(0, 2));
                        UnloadLevelRange(new Vector2Int(0, 1), new Vector2Int(2, 2));

                        //load position.x - 1 for position.y-1 to position.y+1
                        //load position.x to position.x + 1 for position.y-1 to position.y+1
                        LoadLevelRange(new Vector2Int(-1, -1), new Vector2Int(-1, 1));
                        LoadLevelRange(new Vector2Int(0, 1), new Vector2Int(-1, 1));
                        break;

                    case 0:
                        //unload position.x + 1 for position.y - 1 to position.y + 1
                        UnloadLevelRange(new Vector2Int(-2,-2), new Vector2Int(-1, 1));
                        LoadLevelRange(new Vector2Int(-1, -1), new Vector2Int(-1, 1));
                        break;

                    case 1:
                        //unload position.x + 2 for position.y - 2 to position.y
                        //unload position.x to position.x + 1 for position.y - 2
                        UnloadLevelRange(new Vector2Int(2, 2), new Vector2Int(-2, 0));
                        UnloadLevelRange(new Vector2Int(0, 1), new Vector2Int(-2, -2));

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
                        //unload position.x-1 to position.x+1 for position.y + 2
                        UnloadLevelRange(new Vector2Int(-1, 1), new Vector2Int(2, 2));
                        LoadLevelRange(new Vector2Int(-1, 1), new Vector2Int(-1, -1));

                        break;

                    case 0:
                        //not sure why this would be running? do nothing
                        break;

                    case 1:
                        //unload position.x-1 to position.x+1 for position.y - 2
                        UnloadLevelRange(new Vector2Int(-1, 1), new Vector2Int(-2, -2));
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
                        //unload position.x-2 for position.y to position.y + 2
                        //unload position.x-1 to position.x for position.y + 2
                        UnloadLevelRange(new Vector2Int(-2, -2), new Vector2Int(0, 2));
                        UnloadLevelRange(new Vector2Int(-1, 0), new Vector2Int(2, 2));

                        LoadLevelRange(new Vector2Int(1, 1), new Vector2Int(-1, 1));
                        LoadLevelRange(new Vector2Int(-1, 0), new Vector2Int(-1, -1));
                        break;

                    case 0:
                        //unload position.x-2 for position.y - 1 to position.y + 1
                        UnloadLevelRange(new Vector2Int(-2, -2), new Vector2Int(-1, 1));
                        LoadLevelRange(new Vector2Int(1, 1), new Vector2Int(-1, 1));
                        break;

                    case 1:
                        //unload position.x-2 for position.y to position.y - 2
                        //unload position.x-2 to position.x for position.y - 2
                        UnloadLevelRange(new Vector2Int(-2, -2), new Vector2Int(0, -2));
                        UnloadLevelRange(new Vector2Int(-1, 1), new Vector2Int(-2, -2));
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

        for(int x = position.x + xRange.x; x <= position.x + xRange.y; x++)
        {
            if (x < 0 || x >= dimensions.x)
                continue; 

            for (int y = position.y + yRange.x; y <= position.y + yRange.y; y++)
            {
                if (y < 0 || y >= dimensions.y
                    || levels[x + dimensions.x * y] == null
                    || levels[x + dimensions.x * y].sceneIndex < 0 
                    || levels[x + dimensions.x * y].sceneIndex >= EditorBuildSettings.scenes.Length
                    || activeScenes.ContainsKey(levels[x + dimensions.x * y].sceneIndex) == false)
                    continue;

                //Debug.Log("unloading scene: " + levels[x + dimensions.x * y].name);

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

        for (int x = position.x + xRange.x; x <= position.x + xRange.y; x++)
        {
            if (x < 0 || x >= dimensions.x)
                continue;

            for (int y = position.y + yRange.x; y <= position.y + yRange.y; y++)
            {
                if (y < 0 || y >= dimensions.y
                    || levels[x + dimensions.x * y] == null
                    || levels[x + dimensions.x * y].scene == null
                    || levels[x + dimensions.x * y].sceneIndex < 0
                    || levels[x + dimensions.x * y].sceneIndex >= EditorBuildSettings.scenes.Length
                    || activeScenes.ContainsKey(levels[x + dimensions.x * y].sceneIndex) == true)
                    continue;

                activeScenes.Add(levels[x + dimensions.x * y].sceneIndex, new Vector2Int(x,y));
                SceneManager.LoadScene(levels[x + dimensions.x * y].sceneIndex, LoadSceneMode.Additive);
            }
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

                    Debug.Log(levels[x + dimensions.x * y].name + " sceneName: " + levels[x + dimensions.x * y].scene.name +  " sceneIndex: " + levels[x + dimensions.x * y].sceneIndex);
                }
            }
        }
    }

    /*
    IEnumerator LoadLevelAsync(int index)
    {
        AsyncOperation ao = SceneManager.LoadSceneAsync(index, LoadSceneMode.Additive);
        while(!ao.isDone)
        {
            yield return null;
        }
    }*/

}
