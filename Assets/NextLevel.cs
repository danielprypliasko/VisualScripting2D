using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NextLevel : MonoBehaviour
{
    public string targetSceneName;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.name == "LevelTransitionTrigger")
        {
            SceneManager.LoadScene(targetSceneName);
        }
    }
}
