using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelSceneRefs : MonoBehaviour {

    public Level level;
    public Grid grid;
    public Tilemap tilemap;

    private void Awake()
    {
        level.grid = grid;
        level.tilemap = tilemap;
    }
}
