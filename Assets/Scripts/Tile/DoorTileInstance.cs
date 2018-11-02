using Audio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class DoorTileInstance : MonoBehaviour {

    public int cells = 2;
    public DoorTile tileRef;
    public SpriteRenderer spriteRenderer;
    PolygonCollider2D polygonCollider;

    bool animating = false;

    private void Awake()
    {
        spriteRenderer.sprite = tileRef.sprites[0];
        polygonCollider = GetComponentInChildren<PolygonCollider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        animating = false;
        StartCoroutine(OpenDoor());
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        animating = false;
        StartCoroutine(CloseDoor());
    }

    public IEnumerator OpenDoor()
    {
        animating = true;
        while(animating)
        {
            if (tileRef.openSFX != null)
                AudioManager.instance.PlaySFX(tileRef.openSFX);

            int index = 1;
            while (index < tileRef.sprites.Length)
            {
                UpdateSprite(index);

                yield return tileRef.framewait;

                index++;
            }

            animating = false;
        }       
    }

    public IEnumerator CloseDoor()
    {
        animating = true;
        while (animating)
        {
            if (tileRef.openSFX != null)
                AudioManager.instance.PlaySFX(tileRef.openSFX);

            int index = tileRef.sprites.Length - 2;
            while (index >= 0)
            {
                UpdateSprite(index);

                yield return tileRef.framewait;

                index--;
            }

            animating = false;
        }
    }

    public void UpdateSprite(int index)
    {
        if (index < 0 || index > tileRef.sprites.Length)
            return;

        //Debug.Log("Updating sprite, index = " + index);

        spriteRenderer.sprite = tileRef.sprites[index];
    }


}
