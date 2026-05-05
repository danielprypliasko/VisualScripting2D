using UnityEngine;

public class MagnetScript : MonoBehaviour
{
    [SerializeField] private DialogueTypewriter dialogue;

    public ParticleSystem sparkParticles;
    public AudioSource sparkSound;

    public void TriggerSpark() 
    {
        if (sparkParticles != null)
            {
            sparkParticles.Play();
        }

        if (sparkSound != null)
        {
            sparkSound.Play();
        }

        if (dialogue != null)
        {
            string[] lines = new string[]
            {
                "[+25xp] Objective completed...",
                "Character: It worked!!",
                "Robot: Thank you for saving me!!"
            };

            dialogue.StartDialogueWithLines(lines);
        }
    }
}
