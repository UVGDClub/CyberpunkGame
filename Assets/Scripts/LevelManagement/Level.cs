using Audio;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;

/*
 * Should know about the tilemap in the scene
 * should know where to spawn enemies
 * should know where to spawn special items, if they are still available
 * should know what music to play
 */

//this should perhaps be a monobehaviour instead, for scene references.

[CreateAssetMenu(menuName = "Level")]
public class Level : ScriptableObject {
    
    [HideInInspector] public int sceneIndex = -1;
    public SceneAsset scene;
    public LoadSceneMode loadSceneMode = LoadSceneMode.Additive;
    public LevelAudioSettings audioSettings;
    public EnemySpawnInfo[] enemySpawnInfo = new EnemySpawnInfo[1];
    [HideInInspector] public Grid grid;
    [HideInInspector] public Tilemap tilemap;

    [ContextMenu("Refresh Scene Index")]
    public void RefreshSceneIndex()
    {
        if (scene == null)
        {
            sceneIndex = -1;
            return;
        }
            
        sceneIndex = IsSceneInBuildSettings();
        if (sceneIndex == -1)
            AddSceneToBuildSettings();
    }

  #region Build Settings Scene Injection
    public int IsSceneInBuildSettings()
    {
        if (scene == null)
            return -1;

        EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
        for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
        {
            if (scenes[i].path == AssetDatabase.GetAssetPath(scene.GetInstanceID()))
                return i;
        }

        return -1;
    }

    public void AddSceneToBuildSettings()
    {
        if (IsSceneInBuildSettings() >= 0)
            return;

        EditorBuildSettingsScene[] scenes = new EditorBuildSettingsScene[EditorBuildSettings.scenes.Length + 1];


        sceneIndex = EditorBuildSettings.scenes.Length;

        for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
        {
            scenes[i] = EditorBuildSettings.scenes[i];           
        }

        scenes[EditorBuildSettings.scenes.Length] = new EditorBuildSettingsScene(AssetDatabase.GetAssetPath(scene), true);

        EditorBuildSettings.scenes = scenes;
    }

    public void RemoveSceneFromBuildSettings()
    {
        if (IsSceneInBuildSettings() == -1)
            return;

        EditorBuildSettingsScene[] scenes = new EditorBuildSettingsScene[EditorBuildSettings.scenes.Length - 1];

        int i = -1;
        int count = 0;
        while(count < scenes.Length - 1 && i < EditorBuildSettings.scenes.Length)
        {
            i++;

            if (i == sceneIndex)
                continue;

            scenes[i] = EditorBuildSettings.scenes[i];
            count++;            
        }

        EditorBuildSettings.scenes = scenes;
    }

    public void ReplaceSceneInBuildSettings()
    {
        EditorBuildSettingsScene[] scenes = new EditorBuildSettingsScene[EditorBuildSettings.scenes.Length + 1];

        for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
        {
            if(i == sceneIndex)
            {
                scenes[i] = new EditorBuildSettingsScene(AssetDatabase.GetAssetPath(scene), true);
                continue;
            }

            scenes[i] = EditorBuildSettings.scenes[i];
        }

        EditorBuildSettings.scenes = scenes;
    }
  #endregion
}
