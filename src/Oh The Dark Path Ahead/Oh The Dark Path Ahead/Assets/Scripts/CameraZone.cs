using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CameraZone : MonoBehaviour
{
    [Header("Camera")]
    public Transform cameraAnchor;
    public int priority = 0;

    private void Reset()
    {
        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (FixedCameraDirector.Instance != null)
        {
            FixedCameraDirector.Instance.EnterZone(this);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (FixedCameraDirector.Instance != null)
        {
            FixedCameraDirector.Instance.ExitZone(this);
        }
    }
}
