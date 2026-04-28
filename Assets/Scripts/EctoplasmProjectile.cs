using UnityEngine;

public class EctoplasmProjectile : MonoBehaviour
{
    public float arcHeight     = 2.5f;   // peak height above the straight line
    public float flightDuration = 1.5f;  // seconds to reach target
    public ScaredMeter targetMeter;

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float   flightTime;
    private bool    launched;

    // Target is the world position to land on (player's position at throw time)
    public void Launch(Vector3 target)
    {
        startPosition  = transform.position;
        targetPosition = target;
        flightTime     = 0f;
        launched       = true;
    }

    void Update()
    {
        if (!launched) return;

        flightTime += Time.deltaTime;
        float t = Mathf.Clamp01(flightTime / flightDuration);

        // Straight-line interpolation between start and target
        Vector3 linearPos = Vector3.Lerp(startPosition, targetPosition, t);

        // Add parabolic height: sin(t*PI) = 0 at launch, peaks at mid-flight, 0 at landing
        float height = Mathf.Sin(t * Mathf.PI) * arcHeight;
        transform.position = new Vector3(linearPos.x, linearPos.y + height, linearPos.z);

        if (t >= 1f)
            Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!launched) return;

        if (other.CompareTag("Player"))
        {
            targetMeter?.AddFear(targetMeter.ectoplasmHit);
            Destroy(gameObject);
        }
        else if (!other.isTrigger)
        {
            Destroy(gameObject);
        }
    }
}
