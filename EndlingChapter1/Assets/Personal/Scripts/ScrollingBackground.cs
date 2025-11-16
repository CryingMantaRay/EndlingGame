using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollingBackground : MonoBehaviour
{
    public float startOfScrollX = -8.8888f;
    public float endOfScrollX = -26.6666f;
    public float scrollSpeed = 0.5f;
    public SpriteRenderer backgroundSpriteRenderer;

    void Update()
    {
        if (backgroundSpriteRenderer == null) return;

        float newX = backgroundSpriteRenderer.transform.position.x - (scrollSpeed * Time.deltaTime);

        if (newX <= endOfScrollX)
        {
            newX = startOfScrollX;
        }

        backgroundSpriteRenderer.transform.position = new Vector3(newX, backgroundSpriteRenderer.transform.position.y, backgroundSpriteRenderer.transform.position.z);
    }
}
