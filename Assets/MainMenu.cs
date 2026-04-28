using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    public AudioSource clickSound;

    public void PlayGame()
    {
        // Loads the next scene in the build index. The main menu scene is set to index 0
        // and the main game scene is set to index 1.
        StartCoroutine(PlaySoundAndLoad());
    }

    // Needed to allow the sound effect to play before the scene changes.
    IEnumerator PlaySoundAndLoad() 
    {
        clickSound.Play();

        yield return new WaitForSecondsRealtime(clickSound.clip.length);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);

    }

    public void QuitGame()
    {
        // Will close the game when it is an actual application but when just in the Unity editor
        // it will print a message to the console.
        clickSound.Play();
        Debug.Log("Quit button has been pressed.");
        Application.Quit();
    }


}
