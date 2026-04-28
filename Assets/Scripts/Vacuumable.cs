using UnityEngine;
using UnityEngine.UI;

public class Vacuumable : MonoBehaviour
{
    [Header("Vacuum Physics")]
    public float weight = 5f; 
    
    [Header("Health Settings")]
    public bool hasHealth = false;
    public float health;
    public float maxHealth { get; private set; }
    public Image healthBar;
    private float healthBarMaxLength;

    [Header("On-Kill Reward")]
    [Tooltip("Fear amount removed from the player's ScaredMeter when this object is killed (only fires if hasHealth is true).")]
    public float killFearReduction = 50f;

    private Rigidbody rb; 

    void Start()
    {
        this.rb = GetComponent<Rigidbody>();
        this.maxHealth = health; // Max health is the same as the initial one.
        this.healthBarMaxLength = -1; // Initial Value -> Used to know if we have a value for the health bar length.
        
        if(this.hasHealth)
            GhostManager.Instance?.AddGhost(); // Add ghost to count.
    }

    public void GetVacuumed(Vector3 pullDestination, float basePower, float distanceMultiplier)
    {
        GetComponent<GhostAI>()?.NotifyVacuumed();

        if (rb != null)
        {
            Vector3 pullDirection = (pullDestination - transform.position).normalized;
            rb.AddForce(pullDirection * basePower * distanceMultiplier * 20f); // Multiply by 50 so it works better since we are applying forces now.
        }
        else
        {
            // Basic move towards method, affect non-rigit-body game objects, like maybe the ghost, which is supposed to not be affected by gravity.
            float actualPullSpeed = (basePower / weight) * distanceMultiplier;
            transform.position = Vector3.MoveTowards(transform.position, pullDestination, actualPullSpeed * Time.deltaTime);
        }

        if (hasHealth)
        {
            float actualDamage = basePower * distanceMultiplier * Time.deltaTime;
            health -= actualDamage;
            if (this.healthBar != null)
            {
                this.updateHealthBarUI();
            }

            if (health <= 0)
            {
                Debug.Log("Ghost vacuumed!");
                Die();
            }
        }
    }

    // Function to update the health bar's UI.
    void updateHealthBarUI()
    {
        // We get the health's bar maxWidth (it's initial width) if we did never update it.
        if(this.healthBarMaxLength == -1)
            this.healthBarMaxLength = this.healthBar.rectTransform.sizeDelta.x;
        
        // Update width of the health bar based on the actual hp.
        this.healthBar.rectTransform.sizeDelta = new Vector2(this.mapRange(this.health, 0.0f, this.maxHealth, 0.0f, this.healthBarMaxLength), this.healthBar.rectTransform.sizeDelta.y);
    }

    // Auxiliar function to map values from one to another range.
    private float mapRange(double value, double originalMin, double originalMax, double newMin, double newMax)
    {
        return (float) ((value - originalMin) / (originalMax - originalMin) * (newMax - newMin) + newMin);
    }

    void Die()
    {
        // Reward the player for slaying a vacuum-killable target (e.g. the ghost).
        ScaredMeter sm = Object.FindAnyObjectByType<ScaredMeter>();
        if (sm != null)
        {
            sm.ReduceFear(killFearReduction);
            Debug.Log($"[Vacuumable] '{name}' died — reduced fear by {killFearReduction}.");
        }

        GhostManager.Instance?.RemoveGhost(); // Remove ghost from count.
        Destroy(gameObject);
    }
}