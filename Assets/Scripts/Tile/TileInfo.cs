using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

public class TileInfo : MonoBehaviour {

    Player player;
    Vector3Int playerCellPosition;
    public LevelGrid levelGrid;

    Vector3 mousePos;
    Vector3Int tilePos;

    SecretTile secretTile;
    bool activeSecret = false;
    int secretLevelIndex;
    Vector3Int secretTilePos;
    List<Vector3Int> visited = new List<Vector3Int>();

    private void Start()
    {
        player = FindObjectOfType<Player>();
        StartCoroutine(HandleTileEvents());
    }

    IEnumerator HandleTileEvents()
    {
        while (levelGrid.levels[levelGrid.curIndex] == null || levelGrid.levels[levelGrid.curIndex].grid == null)
            yield return null;

        while(true)
        {
            playerCellPosition = levelGrid.levels[levelGrid.curIndex].grid.WorldToCell(player.rigidbody2d.position);
            if (!activeSecret)
            {
                if (levelGrid.levels[levelGrid.curIndex].tilemap.GetTile<SecretTile>(playerCellPosition))
                {
                    visited.Clear();
                    secretTile = (SecretTile)levelGrid.levels[levelGrid.curIndex].tilemap.GetTile(playerCellPosition);
                    secretTile.ToggleVisibility(playerCellPosition, levelGrid.levels[levelGrid.curIndex].tilemap, true, visited);
                    activeSecret = true;
                    secretLevelIndex = levelGrid.curIndex;
                    secretTilePos = playerCellPosition;
                }
            }
            else if (secretLevelIndex >= 0)
            {
                if (activeSecret && !levelGrid.levels[secretLevelIndex].tilemap.GetTile<SecretTile>(playerCellPosition))
                {
                    visited.Clear();
                    secretTile.ToggleVisibility(secretTilePos, levelGrid.levels[secretLevelIndex].tilemap, false, visited);
                    activeSecret = false;
                }
            }


            yield return null;
        }
    }

    //FOR DEBUGGING
    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            if (levelGrid == null || levelGrid.levels[levelGrid.curIndex].grid == null)
                return;

            //Debug.Log(Input.mousePosition);
            mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos = new Vector3(mousePos.x, mousePos.y, 1);
            //Debug.Log("mouse x,y at tilemap z pos = " + mousePos);

            tilePos = levelGrid.levels[levelGrid.curIndex].grid.WorldToCell(mousePos);

            TileBase tile = levelGrid.levels[levelGrid.curIndex].tilemap.GetTile(tilePos);

            if (tile == null)
                return;

           // Debug.Log(tile.name + " " + tilePos);            
        }
    }

    private void OnDrawGizmos()
    {
        if (levelGrid == null || levelGrid.levels[levelGrid.curIndex].grid == null)
            return;

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(levelGrid.levels[levelGrid.curIndex].grid.CellToWorld(tilePos), levelGrid.levels[levelGrid.curIndex].grid.cellSize);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(mousePos, levelGrid.levels[levelGrid.curIndex].grid.cellSize);
    }


    /*
     * SwapTile for prisoner breakout -- swaps all tiles of <tilebase>A on a tilemap with <tilebase>B
     * 
     * SetTiles for breakable walls -- swap tile <tilebase>A at position with <tilebase>B
     * 
     * could use TilemapCollider.getContacts to find out where what's touching and where
     */ 
}
