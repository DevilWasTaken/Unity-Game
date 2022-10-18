using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenu;
    public KeyCode pauseKey = KeyCode.Escape;
    private bool isPaused = false;

    void Start()
    {
        pauseMenu.SetActive(false);
    }

    
    void Update()
    {
        if (Input.GetKeyDown(pauseKey) && isPaused) 
        {
            ResumeGame();
        }
        else if (Input.GetKeyDown(pauseKey))
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    public void ResumeGame()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
