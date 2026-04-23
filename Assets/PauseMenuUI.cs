using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;

public partial class PauseMenuUI : MonoBehaviour
{
    private PauseManager _pauseManager;

    private void Start()
    {
        _pauseManager = Object.FindAnyObjectByType<PauseManager>();

        if (_pauseManager == null) 
        {
            Debug.LogError("PauseManager not found in the scene. Please ensure there is a PauseManager component in the scene.");
        }
    }

    public void OnResumeClick() 
    {
        if (_pauseManager != null)
        {
            _pauseManager.Resume();
        }
    }

    public void OnQuitClick()
    { 
        Time.timeScale = 1f;

        if (SceneManager.GetSceneByName("PauseMenuScene").isLoaded) 
        {
            SceneManager.UnloadSceneAsync("PauseMenuScene");
        }

        SceneManager.LoadScene("MainMenuScene");
    }
}
