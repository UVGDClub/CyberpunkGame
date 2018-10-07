using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class LevelTransition : MonoBehaviour {

    public Vector2Int curLevelIndex;
    public Vector2Int offset = Vector2Int.zero;
    public LevelGrid levelGrid;

    private void Awake()
    {
        if(levelGrid == null)
        {
            Debug.LogWarning(name + " is missing a reference to the LevelGrid");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        return;

        if (curLevelIndex == levelGrid.position)
            return;

        Debug.Log("player entered transition");

        levelGrid.position = curLevelIndex;
        levelGrid.UpdateActiveGrid(offset);
    }
}
