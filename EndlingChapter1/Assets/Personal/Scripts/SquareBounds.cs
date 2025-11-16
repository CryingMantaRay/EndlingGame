using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquareBounds : MonoBehaviour
{
    public float hitRadius = 0.5f; // Square radius for hit detection
    public float hitRadiusCenterOffset = 0.5f; // Offset from player position to center of hit area

    public bool showGizmos = true;

    public bool IsInBoundsWithAnotherSquare(SquareBounds squareBounds)
    {
        Vector3 thisCenter = transform.position + (Vector3.up * hitRadiusCenterOffset);
        Vector3 otherCenter = squareBounds.transform.position + (Vector3.up * squareBounds.hitRadiusCenterOffset);

        bool overlapX = Mathf.Abs(thisCenter.x - otherCenter.x) < (hitRadius + squareBounds.hitRadius);
        bool overlapY = Mathf.Abs(thisCenter.y - otherCenter.y) < (hitRadius + squareBounds.hitRadius);

        return overlapX && overlapY;
    }

    void OnDrawGizmos()
    {
        if (!showGizmos) return;

        Gizmos.color = Color.red;
        Gizmos.DrawCube(transform.position + (Vector3.up * hitRadiusCenterOffset), new Vector3(hitRadius * 2, hitRadius * 2, 0f));
    }
}
