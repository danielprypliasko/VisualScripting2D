using UnityEngine;

public class MagnetScript : MonoBehaviour
{
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
    }
}
