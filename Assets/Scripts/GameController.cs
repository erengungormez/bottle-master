using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public static int CurGameLevel;
    [SerializeField] private Canvas menuCanvas;

    // Start is called before the first frame update
    void Start()
    {

        if (SaveSystem.LoadGame() == null)
        {
            CurGameLevel = LevelCreatorScript.CurGameLevel; // Ýlk defa oyun baþlatýldýðýnda baþlangýç seviyesi
            LevelCreatorScript.instance.CreateTutorialLevel();
        }
        else
        {
            Data save = SaveSystem.LoadGame();
            CurGameLevel = save.curGameLevel;
            LevelCreatorScript.instance.CreateNewLevel();
        }
    }

    public void CreateMenu()
    {
        menuCanvas.gameObject.SetActive(true);
    }

    public void QuitButton()
    {
        Application.Quit();
    }
}

[System.Serializable]
public struct TouchData
{
    public string device;
    public Vector3 mousePosition;
}