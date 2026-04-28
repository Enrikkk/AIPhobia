using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Raycast")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float interactDistance = 2f;
    [SerializeField] private float sphereRadius = 0.3f;
    [SerializeField] private LayerMask interactMask = ~0; // default: everything

    [Header("UI (optional)")]
    [SerializeField] private GameObject promptRoot; // parent to show/hide
    [SerializeField] private TMP_Text promptLabel;  // text inside the prompt

    private IInteractable currentTarget;

    void Update()
    {
        UpdateAim();

        if (currentTarget != null && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            currentTarget.Interact(gameObject);
    }

    private void UpdateAim()
    {
        IInteractable hitInteractable = null;

        if (playerCamera != null)
        {
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            if (Physics.SphereCast(ray, sphereRadius, out RaycastHit hit, interactDistance, interactMask, QueryTriggerInteraction.Collide))
            {
                hitInteractable = hit.collider.GetComponentInParent<IInteractable>();
            }
        }

        if (hitInteractable != currentTarget)
        {
            currentTarget = hitInteractable;
            RefreshPrompt();
        }
        else if (currentTarget != null)
        {
            // Same target — refresh the label in case its state changed (e.g. door became unlocked)
            RefreshPrompt();
        }
    }

    private void RefreshPrompt()
    {
        if (promptRoot == null) return;

        if (currentTarget == null)
        {
            promptRoot.SetActive(false);
            return;
        }

        promptRoot.SetActive(true);
        if (promptLabel != null)
            promptLabel.text = currentTarget.GetPrompt(gameObject);
    }
}
