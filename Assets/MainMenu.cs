using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{

    public void PlayGame()
    {
        // Loads the next scene in the build index. The main menu scene is set to index 0
        // and the main game scene is set to index 1.
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }


    public void QuitGame()
    {
        // Will close the game when it is an actual application but when just in the Unity editor
        // it will print a message to the console.
        Debug.Log("Quit button has been pressed.");
        Application.Quit();
    }
}
