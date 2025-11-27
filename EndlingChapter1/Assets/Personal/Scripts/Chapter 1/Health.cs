using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    public int maxHealth = 6; // Every health unit represents half a heart
    public int currentHealth;
    public SpriteRenderer tiledHeartSprite; // We change size width (draw mode is tiled) based on max health

    public UnityEvent<int> OnTakeDamage;
    public UnityEvent OnDeath;

    float startingFullHeartWidth; // Width of a full heart in the tiled sprite

    void Start()
    {
        currentHealth = maxHealth;
        startingFullHeartWidth = tiledHeartSprite.size.x;
    }

    public void ChangeHealth(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (amount < 0)
        {
            OnTakeDamage.Invoke(currentHealth);
        }

        if (currentHealth == 0)
        {
            OnDeath.Invoke();
        }

        UpdateHealthVisuals();
    }

    void UpdateHealthVisuals()
    {
        // Update the health UI visuals based on currentHealth
        float healthRatio = (float)currentHealth / (float)maxHealth;
        tiledHeartSprite.size = new Vector2(startingFullHeartWidth * healthRatio, tiledHeartSprite.size.y);
    }

}
