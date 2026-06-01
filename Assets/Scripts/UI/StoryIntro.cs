using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;

public class StoryIntro : MonoBehaviour
{
    [Header("Text")]
    [Tooltip("TextAsset from Resources (e.g. 'story' loads Resources/story.txt)")]
    public string resourceName = "story";
    [Range(0.01f, 0.2f)]
    public float typewriterSpeed = 0.04f;
    public TMP_Text storyText;

    [Header("UI")]
    public GameObject storyPanel;
    public GameObject controlsPanel;

    private string fullText;
    private Coroutine typewriterRoutine;
    private bool textFullyShown = false;

    private void Start()
    {
        LoadAndStartStory();
    }

    public void LoadAndStartStory()
    {
        if (typewriterRoutine != null)
        {
            StopCoroutine(typewriterRoutine);
            typewriterRoutine = null;
        }

        var textAsset = Resources.Load<TextAsset>(resourceName);
        if (textAsset != null)
        {
            fullText = textAsset.text;
        }
        else
        {
            fullText = "No story file found at Resources/" + resourceName + ".txt";
        }

        storyPanel.SetActive(true);
        if (controlsPanel != null)
            controlsPanel.SetActive(false);

        storyText.text = "";
        textFullyShown = false;
        typewriterRoutine = StartCoroutine(TypewriterEffect());
    }

    private IEnumerator TypewriterEffect()
    {
        for (int i = 0; i < fullText.Length; i++)
        {
            storyText.text += fullText[i];
            yield return new WaitForSeconds(typewriterSpeed);
        }

        textFullyShown = true;
    }

    private void Update()
    {
        if (storyPanel.activeSelf && (Mouse.current?.leftButton.wasPressedThisFrame == true || Keyboard.current?.anyKey.wasPressedThisFrame == true))
        {
            if (!textFullyShown && typewriterRoutine != null)
            {
                StopCoroutine(typewriterRoutine);
                storyText.text = fullText;
                textFullyShown = true;
            }
            else if (textFullyShown)
            {
                ShowControls();
            }
        }
    }

    private void ShowControls()
    {
        storyPanel.SetActive(false);
        if (controlsPanel != null)
            controlsPanel.SetActive(true);
    }
}
