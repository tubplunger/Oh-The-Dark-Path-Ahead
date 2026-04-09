using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RopeClimbable : MonoBehaviour
{
    [Header("Rope Settings")]
    public float swingForce = 12f;
    public float climbStepDelay = 0.12f;
    public float jumpHorizontalForce = 8f;
    public float jumpVerticalForce = 10f;

    [Header("Player Snap")]
    public Vector2 playerOffset = new Vector2(0f, 0f);

    [Header("Segments")]
    public List<RopeSegment> segments = new List<RopeSegment>();

    private void Awake()
    {
        AutoAssignSegments();
    }

    private void AutoAssignSegments()
    {
        segments.Clear();

        RopeSegment[] found = GetComponentsInChildren<RopeSegment>();

        for (int i = 0; i < found.Length; i++)
        {
            segments.Add(found[i]);
        }

        segments.Sort((a, b) => a.transform.position.y > b.transform.position.y ? -1 : 1);

        for (int i = 0; i < segments.Count; i++)
        {
            segments[i].rope = this;
            segments[i].index = i;
        }
    }

    public RopeSegment GetNearestSegment(Vector2 worldPosition, float maxDistance)
    {
        RopeSegment best = null;
        float bestDist = maxDistance * maxDistance;

        for (int i = 0; i < segments.Count; i++)
        {
            float dist = ((Vector2)segments[i].transform.position - worldPosition).sqrMagnitude;
            if (dist < bestDist)
            {
                bestDist = dist;
                best = segments[i];
            }
        }

        return best;
    }

    public RopeSegment GetNextSegment(RopeSegment current, int direction)
    {
        if (current == null) return null;

        int targetIndex = current.index - direction;
        // direction: +1 means up, -1 means down

        if (targetIndex < 0 || targetIndex >= segments.Count)
            return current;

        return segments[targetIndex];
    }
}
