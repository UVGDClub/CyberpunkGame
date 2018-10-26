using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour {

    Player player;

    private void Awake()
    {
        player = FindObjectOfType<Player>();

        Initialize();        
    }

    public void Initialize()
    {
        if (!PopulateFiles())
            loadGameButton.interactable = false;
    }

    public IEnumerator DeactivateMenu()
    {
        while (player.initialized == false)
            yield return null;

        gameObject.SetActive(false);
    }

    #region New Game
    public FileDetails newFile;
    public void StartNewGame () {

        FileDetails newFile = this.newFile;
        newFile.SetPlayer(ref player);
        player.fileDetails = newFile;

        StartCoroutine(DeactivateMenu());
    }

    #endregion

    #region Load Game
    public Button loadGameButton;
    public GameObject SavedGamesPanel;
    public Button loadFileButtonPrefab;
    public float loadButtonHeightOffset = .15f;
    FileDetails[] files;
    Button[] fileButtons;

    public void ShowSavedGames () {
        SavedGamesPanel.SetActive(true);
	}

    bool PopulateFiles()
    {
        files = PlayerFile.GetFiles();

        if (files == null || files.Length == 0)
            return false;

        if (fileButtons == null || files.Length > fileButtons.Length)
        {
            fileButtons = new Button[files.Length];
            for (int i = 0; i < files.Length; i++)
            {
                fileButtons[i] = Instantiate(loadFileButtonPrefab, SavedGamesPanel.transform);

                RectTransform rt = fileButtons[i].GetComponent<RectTransform>();
                rt.anchorMax = new Vector2(rt.anchorMax.x, rt.anchorMax.y - loadButtonHeightOffset * i);
                rt.anchorMin = new Vector2(rt.anchorMin.x, rt.anchorMin.y - loadButtonHeightOffset * i);

                fileButtons[i].GetComponentInChildren<Text>().text = files[i].filename;
                fileButtons[i].onClick.AddListener(() => LoadFile(i));
            }
        }

        return true;
    }

    public void LoadFile(int i)
    {
        i--;
        files[i].SetPlayer(ref player);
        player.fileDetails = files[i];
        
        StartCoroutine(DeactivateMenu());
    }
    #endregion

    #region Options
    void ShowOptions()
    {

    }
    #endregion

    void QuitGame()
    {
        Application.Quit();
    }
}
