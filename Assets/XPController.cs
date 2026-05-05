using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class XPController : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip xpSound;
    [SerializeField] private TextMeshProUGUI LevelText;
    [SerializeField] private TextMeshProUGUI ExperienceText;
    [SerializeField] private int Level;
    public float CurrentXP;
    [SerializeField] private float TargetXP;
    [SerializeField] private Image XpProgressBar;
    public static XPController instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {

        ExperienceText.text = CurrentXP + " / " + TargetXP + " XP";

        ExperienceController();
    }

    public void AddXp(float amount)
    {
        CurrentXP += amount;

        if (audioSource != null && xpSound != null)
        {
            audioSource.PlayOneShot(xpSound);
        }
    }

    public void ExperienceController()
    {
        LevelText.text = "Level: " + Level.ToString();
        XpProgressBar.fillAmount = (CurrentXP / TargetXP);

        if(CurrentXP >= TargetXP) // Level up
        {
            CurrentXP = CurrentXP - TargetXP;
            Level++;
            TargetXP += 50;
        }
    }
}
