using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class RopeSegment : MonoBehaviour
{
    public RopeClimbable rope;
    public int index;

    private Rigidbody2D rb;

    public Rigidbody2D Body => rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
}
