using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReadableObject : MonoBehaviour
{
    [Header("Letter Content")]
    public string letterTitle;

    [TextArea(6, 20)]
    public string letterBody;

    [Header("Prompt")]
    public GameObject interactIcon;

    private bool playerInRange;

    private void Start()
    {
        if (interactIcon != null)
        {
            interactIcon.SetActive(false);
        }
    }

    private void Update()
    {
        if (!playerInRange)
            return;

        if (LetterUIManager.Instance == null)
            return;

        if (LetterUIManager.Instance.IsOpen)
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            LetterUIManager.Instance.OpenLetter(this);

            if (interactIcon != null)
            {
                interactIcon.SetActive(false);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsPlayerCollider(other))
            return;

        playerInRange = true;

        if (interactIcon != null && LetterUIManager.Instance != null && !LetterUIManager.Instance.IsOpen)
        {
            interactIcon.SetActive(true);
        }

        Debug.Log("Player entered readable trigger: " + gameObject.name);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!IsPlayerCollider(other))
            return;

        playerInRange = false;

        if (interactIcon != null)
        {
            interactIcon.SetActive(false);
        }

        Debug.Log("Player exited readable trigger: " + gameObject.name);
    }

    private bool IsPlayerCollider(Collider2D other)
    {
        if (other == null)
            return false;

        if (other.attachedRigidbody != null && other.attachedRigidbody.CompareTag("Player"))
            return true;

        if (other.CompareTag("Player"))
            return true;

        if (other.transform.root.CompareTag("Player"))
            return true;

        return false;
    }
}