using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Manager : MonoBehaviour {

    public static UI_Manager instance;

    public GameObject optionsPanel;
    public GameObject audioPanel;
    public GameObject videoPanel;
    public RectTransform saveHUD;

    private void Awake()
    {
        if (instance != null)
            Destroy(this);

        instance = this;
    }

    public void MenuTransition()
    {
        if(audioPanel.activeSelf)
        {
            ToggleAudioPanel();
            return;
        }

        if(videoPanel.activeSelf)
        {
            ToggleVideoPanel();
            return;
        }

        ToggleOptionMenu();
    }

    public void ToggleOptionMenu()
    {
        optionsPanel.SetActive(!optionsPanel.activeSelf);
    }

    public void ToggleAudioPanel()
    {
        audioPanel.SetActive(!audioPanel.activeSelf);
    }

    public void ToggleVideoPanel()
    {
        videoPanel.SetActive(!videoPanel.activeSelf);
    }

    public void ToggleSaveHUD()
    {
        saveHUD.gameObject.SetActive(!saveHUD.gameObject.activeSelf);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
    
}
