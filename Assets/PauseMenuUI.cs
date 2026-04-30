using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public partial class PauseMenuUI : MonoBehaviour
{
    private PauseManager _pauseManager;
    public AudioSource clickSound;

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
            StartCoroutine(PlaySoundAndLoad());
        }
    }

    public void OnQuitClick()
    { 
        
        Time.timeScale = 1f;

        StartCoroutine(PlaySoundAndUnload());
    }

    IEnumerator PlaySoundAndLoad() 
    {
        clickSound.Play();

        yield return new WaitForSecondsRealtime(clickSound.clip.length);

        _pauseManager.Resume();
    }

    IEnumerator PlaySoundAndUnload()
    {
        clickSound.Play();

        yield return new WaitForSecondsRealtime(clickSound.clip.length);

        SceneManager.LoadScene("MainMenuScene");
    }
}
