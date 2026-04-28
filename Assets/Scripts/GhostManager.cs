using UnityEngine;
using StealthGame;  // For GameEnding scene.
using TMPro;        // TMP Class.

public class GhostManager : MonoBehaviour
{
    [Header("Ghost Manager Attributes")]
    [SerializeField] private GameEnding gameEnding;
    [SerializeField] private TextMeshProUGUI text;

    private int numGhosts = 0; // Until ghosts don't get added there are 0 of them.
    
    // Public for getters but private for setters.
    // statis to use the singleton pattern (one copy shared accross everyone).
    public static GhostManager Instance { get; private set; }

    void Awake()
    {
        // The double guard pattern is used to avoid the (common) error of creating 
        // two different GhostManagers that overwrite each other silently.
        if(Instance != null) // A GhostManager already exists.
        {
            Destroy(gameObject); // Destroy duplicate
            return; // stop!
        }

        Instance = this;
    }

    public void AddGhost()
    {
        this.numGhosts++;
        this.UpdateUI();
    }

    public void RemoveGhost()
    {
        this.numGhosts = Mathf.Max(0, numGhosts - 1); // Don't go below zero.
        this.UpdateUI();
        
        if(this.numGhosts <= 0)
        {
            this.gameEnding.WinGame();
        }
    }

    public void UpdateUI()
    {
        this.text.SetText("Ghosts Left: {0}", this.numGhosts);
    }
    
}
