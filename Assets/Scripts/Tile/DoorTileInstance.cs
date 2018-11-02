using Audio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class DoorTileInstance : MonoBehaviour {

    int index = 0;
    public int cells = 2;
    public DoorTile tileRef;
    public SpriteRenderer spriteRenderer;
    PolygonCollider2D polygonCollider;

    bool animating = false;

    private void Awake()
    {
        spriteRenderer.sprite = tileRef.sprites[0];
        polygonCollider = GetComponentInChildren<PolygonCollider2D>();
        List<Vector2> shape = new List<Vector2>();
        int point_count = spriteRenderer.sprite.GetPhysicsShape(0, shape);
        polygonCollider.points = shape.ToArray();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        animating = false;
        StopAllCoroutines();
        StartCoroutine(OpenDoor());
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        animating = false;
        StopAllCoroutines();
        StartCoroutine(CloseDoor());
    }

    public IEnumerator OpenDoor()
    {
        polygonCollider.gameObject.SetActive(false);
        animating = true;
        while(animating)
        {
            if (tileRef.openSFX != null)
                AudioManager.instance.PlaySFX(tileRef.openSFX);

            index++;
            while (index < tileRef.sprites.Length)
            {
                //UpdateSprite(index);
                spriteRenderer.sprite = tileRef.sprites[index];

                yield return tileRef.framewait;

                index++;
            }

            animating = false;
        }       
    }

    public IEnumerator CloseDoor()
    {
        polygonCollider.gameObject.SetActive(true);
        animating = true;
        while (animating)
        {
            if (tileRef.openSFX != null)
                AudioManager.instance.PlaySFX(tileRef.openSFX);

            index--;
            while (index >= 0)
            {
                //UpdateSprite(index);
                spriteRenderer.sprite = tileRef.sprites[index];

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
        /*polygonCollider.gameObject.SetActive(spriteRenderer.sprite != null);

        List<Vector2> shape = new List<Vector2>();
        int point_count = spriteRenderer.sprite.GetPhysicsShape(0, shape);
        polygonCollider.points = shape.ToArray();*/

    }


}
