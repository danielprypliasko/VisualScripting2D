using UnityEngine;

// Creates script
public class BackgroundMusic_Script : MonoBehaviour
{
    public static BackgroundMusic_Script instance;

    private AudioSource audioSource;
    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        // Keep object when switching scenes
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();

        // If audio exists
        if (audioSource != null)
        {
            audioSource.loop = true;

            // If the music is not playing
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }
    }
}