using UnityEngine;
using UnityEngine.UI;
using StealthGame;

public class ScaredMeter : MonoBehaviour
{
    [Header("Fill Rates (per second)")]
    public float passiveRate      = 4f;
    public float ghostVisibleRate = 8f;
    public float vacuumDrainRate  = 7f;

    [Header("Instant Events")]
    public float ectoplasmHit      = 150f;
    public float physicalCatchFill = 1000f;

    [Header("References")]
    public Image     scaredBarImage;
    public Transform ghostTransform;
    public Camera    playerCamera;
    public LayerMask environmentMask;
    public GameEnding gameEnding;

    private float currentFear = 100f; // Initial fear defined here (avoids using a different variable)
    private const float maxFear = 1300f;
    private bool gameover = false;

    void Update()
    {
        if (gameover) return;

        if (CanPlayerSeeGhost())
            this.AddFear(ghostVisibleRate * Time.deltaTime);
        else
            this.AddFear(passiveRate * Time.deltaTime);

        currentFear = Mathf.Clamp(currentFear, 0f, maxFear);

        UpdateBar();

        if (currentFear >= maxFear)
        {
            gameover = true;
            gameEnding.CaughtPlayer();
        }
    }

    // Called by EctoplasmProjectile on hit, and by GhostAI on physical catch
    public void AddFear(float amount)
    {
        if (gameover) return;
        currentFear = Mathf.Clamp(currentFear + amount, 0f, maxFear);
        UpdateBar();
    }

    // Called by VacuumTool each frame while actively vacuuming the ghost
    public void ReduceFear(float amount)
    {
        currentFear = Mathf.Max(0f, currentFear-amount);
        UpdateBar();
    }

    // Is the ghost currently visible from the player's camera?
    private bool CanPlayerSeeGhost()
    {
        if (ghostTransform == null || playerCamera == null) return false;

        Vector3 toGhost = ghostTransform.position - playerCamera.transform.position;
        float dist = toGhost.magnitude;

        if (dist > 20f) return false;

        // Is the ghost within the camera's field of view cone?
        if (Vector3.Angle(playerCamera.transform.forward, toGhost) > playerCamera.fieldOfView * 0.5f)
            return false;

        // Is there a wall between the camera and the ghost?
        return !Physics.Raycast(playerCamera.transform.position, toGhost.normalized,
                                dist - 0.5f, environmentMask);
    }

    private void UpdateBar()
    {
        if (scaredBarImage != null)
            scaredBarImage.fillAmount = currentFear / maxFear;
    }
}
