using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SpawnGizmos : MonoBehaviour {

    public LevelGrid levelGrid;
    public Level level;
    public Color colour = Color.cyan;
    public float scale = 0.1f;

    private void OnValidate()
    {
        level = levelGrid.GetLevelFromBuildIndex(gameObject.scene.buildIndex);
        
    }

    void OnDrawGizmos()
    {
        if (level == null)
            return;

        Gizmos.color = colour;

        for (int i = 0; i < level.enemySpawnInfo.Length; i++)
        {
            Gizmos.DrawWireCube(level.enemySpawnInfo[i].position, Vector2.one * scale);
            Handles.Label(level.enemySpawnInfo[i].position, i.ToString());
        }
    }
}
