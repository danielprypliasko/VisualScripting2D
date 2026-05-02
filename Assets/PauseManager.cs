using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    private bool isPaused = false;
    private string pauseSceneName = "PauseMenuScene";

    private void Update()
    {
        if (SceneManager.GetActiveScene().name == "MainMenuScene") return;


        if (Keyboard.current.escapeKey.wasPressedThisFrame) 
        {
            if (isPaused)
            {
                Resume();
            }
            else 
            {
                Pause();
            }
        }
    }

    public void Resume() 
    {
        isPaused = false;
        Time.timeScale = 1f;
        SceneManager.UnloadSceneAsync(pauseSceneName);
    }

    public void Pause() 
    {
        isPaused = true;
        Time.timeScale = 0f;
        SceneManager.LoadScene(pauseSceneName, LoadSceneMode.Additive);
    }

    public void LoadMenu() 
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenuScene");
    }

}
