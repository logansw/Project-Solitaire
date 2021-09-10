using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    // Global Variables
    public GameStatus gameStatus;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (gameStatus == GameStatus.paused)
                ResumeGame();
            else if (gameStatus == GameStatus.live)
                PauseGame();
        }
    }

    public void PauseGame()
    {
        gameStatus = GameStatus.paused;
        UIMan.Instance.pausePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        gameStatus = GameStatus.live;
        UIMan.Instance.pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void ExitGame()
    {
        LoadLevelSingle("Menu");
    }

    public void RestartGame()
    {
        LoadLevelSingle("Solitaire");
    }

    public void LoadLevelAdditive(string levelName)
    {
        SceneManager.LoadSceneAsync(levelName, LoadSceneMode.Additive);
    }

    public void LoadLevelSingle(string levelName)
    {
        SceneManager.LoadSceneAsync(levelName, LoadSceneMode.Single);
    }

    public void CheckGameOver()
    {
        if (BoardManager.Instance.clearPile.Count == 52)
        {
            UIMan.Instance.victoryPanel.SetActive(true);
        }
    }
}
