using UnityEngine;

public class InteractableDoor : MonoBehaviour, IInteractable
{
    [SerializeField] private string requiredKey = "key";
    [SerializeField] private string lockedPrompt = "[E] Locked";
    [SerializeField] private string unlockedPrompt = "[E] Open";

    public string GetPrompt(GameObject interactor)
    {
        PlayerKeys keys = interactor.GetComponentInParent<PlayerKeys>();
        if (keys == null) keys = interactor.GetComponentInChildren<PlayerKeys>();
        bool hasKey = keys != null && keys.OwnKey(requiredKey);
        return hasKey ? unlockedPrompt : lockedPrompt;
    }

    public void Interact(GameObject interactor)
    {
        PlayerKeys keys = interactor.GetComponentInParent<PlayerKeys>();
        if (keys == null) keys = interactor.GetComponentInChildren<PlayerKeys>();
        
        if (keys == null || !keys.OwnKey(requiredKey))
        {
            Debug.Log($"[Door] Locked — need '{requiredKey}'.");
            return;
        }

        Debug.Log($"[Door] Opened with '{requiredKey}'.");
        Destroy(gameObject);
    }
}
