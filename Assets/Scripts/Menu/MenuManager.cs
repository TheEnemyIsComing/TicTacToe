using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public Canvas[] pages;

    private void Start()
    {
        PageSwitch(pages[0]);
    }

    public void PageSwitch(Canvas page)
    {
        foreach (Canvas c in pages)
        {
            if (c == page)
                c.gameObject.SetActive(true);
            else
                c.gameObject.SetActive(false);
        }
    } 

    public void StartGame(int difficulty)
    {
        GameSettings.difficulty = difficulty;
        SceneManager.LoadScene("game");
    }

    public void Exit()
    {
        Debug.Log("Exit");
        Application.Quit();
    }
}