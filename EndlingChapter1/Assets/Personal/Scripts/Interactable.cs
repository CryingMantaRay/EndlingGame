using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider2D))]
public class Interactable : MonoBehaviour
{
    public GameObject indicator;
    public bool interactOnlyOnce = true;
    public UnityEvent onInteract;

    BoxCollider2D boundingBox;
    bool hasInteracted;

    public bool CanInteract => !interactOnlyOnce || !hasInteracted;

    void Awake()
    {
        boundingBox = GetComponent<BoxCollider2D>();
        boundingBox.isTrigger = true;

        if (indicator)
            indicator.SetActive(false);
    }

    public void OnEnterRange(Interactor interactor)
    {
        if (indicator && CanInteract)
            indicator.SetActive(true);
    }

    public void OnExitRange(Interactor interactor)
    {
        if (indicator)
            indicator.SetActive(false);
    }

    public void Interact(Interactor interactor)
    {
        if (!CanInteract)
            return;

        hasInteracted = true;

        if (indicator)
            indicator.SetActive(false);

        onInteract?.Invoke();
    }
}
