using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LetterUIManager : MonoBehaviour
{
    public static LetterUIManager Instance;

    [Header("UI References")]
    public GameObject letterPanel;
    public TMP_Text titleText;
    public TMP_Text bodyText;

    [Header("Input")]
    public float closeInputDelay = 0.15f;

    public bool IsOpen { get; private set; }

    private ReadableObject currentReadable;
    private float closeInputTimer;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (letterPanel != null)
        {
            letterPanel.SetActive(false);
        }
    }

    public void OpenLetter(ReadableObject readable)
    {
        if (readable == null || letterPanel == null)
            return;

        currentReadable = readable;
        IsOpen = true;
        closeInputTimer = closeInputDelay;

        letterPanel.SetActive(true);

        if (titleText != null)
            titleText.text = readable.letterTitle;

        if (bodyText != null)
            bodyText.text = readable.letterBody;

        Time.timeScale = 0f;

        Debug.Log("Opened letter: " + readable.letterTitle);
    }

    public void CloseLetter()
    {
        if (letterPanel == null)
            return;

        IsOpen = false;
        currentReadable = null;

        letterPanel.SetActive(false);
        Time.timeScale = 1f;

        Debug.Log("Closed letter");
    }

    private void Update()
    {
        if (!IsOpen)
            return;

        if (closeInputTimer > 0f)
        {
            closeInputTimer -= Time.unscaledDeltaTime;
            return;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            CloseLetter();
        }
    }
}