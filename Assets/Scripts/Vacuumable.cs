using UnityEngine;

public class Vacuumable : MonoBehaviour
{
    [Header("Vacuum Physics")]
    public float weight = 5f; 
    
    [Header("Health Settings")]
    public bool hasHealth = false; 
    public float health = 100f; 

    private Rigidbody rb; 

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void GetVacuumed(Vector3 pullDestination, float basePower, float distanceMultiplier)
    {

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

            if (health <= 0)
            {
                Die();
            }
        }
    }

    void Die()
    {
        Destroy(gameObject);
    }
}