using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class SavePoint : MonoBehaviour {

    bool canSave = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") == false)
            return;

        canSave = true;
        StartCoroutine(SaveAvailable(collision.GetComponent<Player>()));
        UI_Manager.instance.ToggleSaveHUD();
    }

    IEnumerator SaveAvailable(Player player)
    {
        while(canSave)
        {
            if (Input.GetKey(KeyCode.Return))
                Save(player);                

            yield return null;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") == false)
            return;

        canSave = false;
        UI_Manager.instance.ToggleSaveHUD();
    }

    void Save(Player player)
    {
        canSave = false;
        player.fileDetails.SetDetails(player);
        PlayerFile.Save(player.fileDetails);
    }
}
