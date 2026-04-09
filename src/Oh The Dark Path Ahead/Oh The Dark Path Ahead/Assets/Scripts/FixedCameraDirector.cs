using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixedCameraDirector : MonoBehaviour
{
    public static FixedCameraDirector Instance;

    [Header("Switching")]
    public float switchCooldown = 0.15f;
    public bool instantSnap = true;
    public float smoothSpeed = 8f;

    [Header("Defaults")]
    public Transform defaultAnchor;

    private readonly List<CameraZone> activeZones = new List<CameraZone>();

    private CameraZone currentZone;
    private Transform targetAnchor;
    private Camera cam;

    private float switchCooldownTimer;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        cam = GetComponent<Camera>();
    }

    private void Start()
    {
        if (defaultAnchor != null)
        {
            ApplyAnchorInstant(defaultAnchor);
            targetAnchor = defaultAnchor;
        }
    }

    private void Update()
    {
        if (switchCooldownTimer > 0f)
        {
            switchCooldownTimer -= Time.deltaTime;
        }

        if (!instantSnap && targetAnchor != null)
        {
            transform.position = Vector3.Lerp(
                transform.position,
                targetAnchor.position,
                smoothSpeed * Time.deltaTime
            );

            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                targetAnchor.rotation,
                smoothSpeed * Time.deltaTime
            );
        }
    }

    public void EnterZone(CameraZone zone)
    {
        if (zone == null || activeZones.Contains(zone))
            return;

        activeZones.Add(zone);
        EvaluateBestZone();
    }

    public void ExitZone(CameraZone zone)
    {
        if (zone == null)
            return;

        activeZones.Remove(zone);

        if (currentZone == zone)
        {
            currentZone = null;
        }

        EvaluateBestZone();
    }

    private void EvaluateBestZone()
    {
        CameraZone bestZone = GetBestActiveZone();

        if (bestZone == null)
        {
            if (defaultAnchor != null)
            {
                targetAnchor = defaultAnchor;
                if (instantSnap)
                    ApplyAnchorInstant(defaultAnchor);
            }

            return;
        }

        if (bestZone == currentZone)
            return;

        if (switchCooldownTimer > 0f)
            return;

        currentZone = bestZone;
        targetAnchor = bestZone.cameraAnchor;
        switchCooldownTimer = switchCooldown;

        if (instantSnap)
        {
            ApplyAnchorInstant(bestZone.cameraAnchor);
        }
    }

    private CameraZone GetBestActiveZone()
    {
        if (activeZones.Count == 0)
            return null;

        CameraZone best = null;

        for (int i = 0; i < activeZones.Count; i++)
        {
            CameraZone zone = activeZones[i];

            if (zone == null || zone.cameraAnchor == null)
                continue;

            if (best == null)
            {
                best = zone;
                continue;
            }

            if (zone.priority > best.priority)
            {
                best = zone;
            }
        }

        return best;
    }

    private void ApplyAnchorInstant(Transform anchor)
    {
        transform.position = anchor.position;
        transform.rotation = anchor.rotation;

        CameraAnchorSettings settings = anchor.GetComponent<CameraAnchorSettings>();
        if (settings != null && cam != null)
        {
            cam.orthographicSize = settings.orthographicSize;
        }
    }
}
