using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

public class TileInfo : MonoBehaviour {

    public LevelGrid levelGrid;

    Vector3 mousePos;
    Vector3Int tilePos;
    

    //@TODO Implement checking only if tilemap contains special tile of type using tilemap.ContainsTile
    private void Update()
    {
        if(Input.GetMouseButton(0))
        {
            //Debug.Log(Input.mousePosition);
            mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos = new Vector3(mousePos.x, mousePos.y, 1);
            //Debug.Log("mouse x,y at tilemap z pos = " + mousePos);

            tilePos = levelGrid.levels[levelGrid.curIndex].grid.WorldToCell(mousePos);

            TileBase tile = levelGrid.levels[levelGrid.curIndex].tilemap.GetTile(tilePos);

            if (tile == null)
                return;

            Debug.Log(tile.name + " " + tilePos);            
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
