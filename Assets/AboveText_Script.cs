using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem;

public class DialogueTypewriter : MonoBehaviour
{
    public TextMeshProUGUI dialogueText;

    public string[] dialogueLines; // Store dialogue lines
    public float typingSpeed = 0.04f; // Speed of typing effect

    public AudioSource audioSource;
    public AudioClip nextLineSound;

    public RectTransform dialogueBox;
    public float popUpSpeed = 8f;

    private int currentLine = 0;
    private bool isTyping = false; // Checks if text is typing
    private bool isTransitioning = false; // Stops clicks while the box is popping up

    private Vector2 shownPosition;
    private Vector2 hiddenPosition;

    private Coroutine typingCoroutine;

    private void Start()
    {
        shownPosition = dialogueBox.anchoredPosition;
        hiddenPosition = shownPosition + new Vector2(0, 150);

        dialogueBox.anchoredPosition = hiddenPosition;

        StartCoroutine(StartDialogue());
    }

    private IEnumerator StartDialogue()
    {
        isTransitioning = true;

        dialogueText.text = "";
        yield return StartCoroutine(PopUp());

        PlayDialogueSound();
        typingCoroutine = StartCoroutine(TypeLine());

        isTransitioning = false;
    }

    private void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) // Left mouse click
        {
            if (isTransitioning)
            {
                return;
            }

            if (isTyping)
            {
                if (typingCoroutine != null)
                {
                    StopCoroutine(typingCoroutine);
                }

                dialogueText.text = dialogueLines[currentLine];
                isTyping = false;
            }
            else
            {
                NextLine();
            }
        }
    }

    private IEnumerator PopUp()
    {
        while (Vector2.Distance(dialogueBox.anchoredPosition, shownPosition) > 1f)
        {
            dialogueBox.anchoredPosition = Vector2.Lerp(
                dialogueBox.anchoredPosition,
                shownPosition,
                Time.deltaTime * popUpSpeed
            );

            yield return null;
        }

        dialogueBox.anchoredPosition = shownPosition;
    }

    private IEnumerator TypeLine()
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char letter in dialogueLines[currentLine])
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    private void NextLine()
    {
        currentLine++;

        if (currentLine < dialogueLines.Length)
        {
            StartCoroutine(PopUpThenType());
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private IEnumerator PopUpThenType()
    {
        isTransitioning = true;

        dialogueText.text = "";
        dialogueBox.anchoredPosition = hiddenPosition;

        yield return StartCoroutine(PopUp());

        PlayDialogueSound();
        typingCoroutine = StartCoroutine(TypeLine());

        isTransitioning = false;
    }

    private void PlayDialogueSound()
    {
        if (audioSource != null && nextLineSound != null)
        {
            audioSource.PlayOneShot(nextLineSound);
        }
    }

    public void StartDialogueWithLines(string[] newLines)
    {
        gameObject.SetActive(true);

        StopAllCoroutines();

        dialogueLines = newLines;
        currentLine = 0;
        isTyping = false;
        isTransitioning = false;

        dialogueText.text = "";
        dialogueBox.anchoredPosition = hiddenPosition;

        StartCoroutine(StartDialogue());
    }
}