using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Interactor : MonoBehaviour
{
    public KeyCode interactKey = KeyCode.E;

    Interactable currentTarget;

    void Update()
    {
        if (!currentTarget)
            return;

        if (!currentTarget.CanInteract)
            return;

        if (Input.GetKeyDown(interactKey))
            currentTarget.Interact(this);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var interactable = other.GetComponent<Interactable>();
        if (!interactable)
            return;

        currentTarget = interactable;
        currentTarget.OnEnterRange(this);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        var interactable = other.GetComponent<Interactable>();
        if (!interactable)
            return;

        if (currentTarget == interactable)
        {
            currentTarget.OnExitRange(this);
            currentTarget = null;
        }
    }
}
