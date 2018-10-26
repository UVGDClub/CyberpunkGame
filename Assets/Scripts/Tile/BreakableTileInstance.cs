using Audio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(PolygonCollider2D))]
public class BreakableTileInstance : MonoBehaviour {

    public BreakableTile tileRef;
    public SpriteRenderer spriteRenderer;
    PolygonCollider2D polygonCollider;

    private void Awake()
    {
        spriteRenderer.sprite = tileRef.sprites[0];
    }

    public IEnumerator BreakTile()
    {
        if (tileRef.breakSFX != null)
            AudioManager.instance.PlaySFX(tileRef.breakSFX);

        int index = 1;
        while (index < tileRef.sprites.Length)
        {
            UpdateSprite(index);
            //levelGrid.levels[levelIndex].tilemap.RefreshTile(position);

            yield return tileRef.framewait;

            index++;
        }

        if(tileRef.nullWhenBroken == true)
        {
            gameObject.SetActive(false);
            //use a pool?
        }
    }

    public void UpdateSprite(int index)
    {
        if (index < 0 || index > tileRef.sprites.Length)
            return;

        //Debug.Log("Updating sprite, index = " + index);

        spriteRenderer.sprite = tileRef.sprites[index];

        if (tileRef.lerpColour == true && index != tileRef.sprites.Length - 1)
            spriteRenderer.color = Color.Lerp(tileRef.colourTransition[0], tileRef.colourTransition[1], (float)index / (tileRef.sprites.Length - 1));
        else if (index == tileRef.sprites.Length - 1)
            spriteRenderer.color = tileRef.colourTransition[2];
    }
}
