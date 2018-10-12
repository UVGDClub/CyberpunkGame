using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileInfo : MonoBehaviour {

    public LevelGrid levelGrid;

    Vector3 mousePos;
    Vector3Int tilePos;


    private void Update()
    {
        if(Input.GetMouseButton(0))
        {
            Debug.Log(Input.mousePosition);
            mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos = new Vector3(mousePos.x, mousePos.y, 1);
            Debug.Log("mouse x,y at tilemap z pos = " + mousePos);

            tilePos = levelGrid.levels[levelGrid.curIndex].grid.WorldToCell(mousePos);
            //grid.CellToWorld(tilePos);
            TileBase tb = levelGrid.levels[levelGrid.curIndex].tilemap.GetTile(tilePos);

            if (tb != null)
                Debug.Log(tilePos + " " + tb.name);
            else
                Debug.Log("can't get tilebase at " + tilePos);
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

}
