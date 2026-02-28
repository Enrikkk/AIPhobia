using UnityEngine;
using UnityEngine.InputSystem;

public class VacuumTool : MonoBehaviour
{
    [Header("Vacuum Properties")]
    public float suctionRange = 10f;
    public float suctionPower = 2f; // Strenght of the vacuum.

    [Header("Connections")]
    public Transform playerCamera; 
    public ParticleSystem suctionParticles;
    public Transform vacuumModel;

    [Header("Sound")]
    public AudioSource vacuumAudio;
    public AudioClip motorSound;

    [Header("Animation")]
    public float maxShakeIntensity = 0.015f; 
    public float motorRampSpeed = 3f; 
    
    private Vector3 originalPosition; 
    private float currentShakeWeight = 0f; 

    void Start()
    {
        if (vacuumModel != null)
        {
            originalPosition = vacuumModel.localPosition;
        }
    }

    void Update()
    {
        if(Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            suckVacuum();
        }
        else
        {
            stopSuckVacuum();
        }

        if (vacuumModel != null)
        {
            if (currentShakeWeight > 0f)
            {
                Vector3 randomShake = Random.insideUnitSphere * maxShakeIntensity * currentShakeWeight;
                vacuumModel.localPosition = originalPosition + randomShake;
            }
            else
            {
                vacuumModel.localPosition = originalPosition;
            }
        }
    }

    void suckVacuum()
    {
        currentShakeWeight = Mathf.MoveTowards(currentShakeWeight, 1f, Time.deltaTime * motorRampSpeed);

        if(suctionParticles != null && !suctionParticles.isPlaying)
        {
            suctionParticles.Play();
        }

        if(vacuumAudio != null)
        {
            vacuumAudio.pitch = currentShakeWeight;
            vacuumAudio.volume = currentShakeWeight;

            if(motorSound != null && !vacuumAudio.isPlaying)
            {
                vacuumAudio.clip = motorSound;
                vacuumAudio.loop = true;
                vacuumAudio.Play();
            }
        }

        Debug.DrawRay(playerCamera.position, playerCamera.forward * suctionRange, Color.red);

        RaycastHit reachedObj;
        if(Physics.Raycast(playerCamera.position, playerCamera.forward, out reachedObj, suctionRange))
        {
            Debug.Log("Vacuum is sucking: " + reachedObj.transform.name); // See which object we are vacuuming.

            Vacuumable target = reachedObj.transform.GetComponent<Vacuumable>();
            if(target != null) 
            {
                Debug.Log("Pulling object: " + reachedObj.transform.name);
                // Vacuum the object (if vacuumable) to actual position, using actual suction power and calculating the distance multiplier as the distance 
                // from the player to the object, normalized by the suction range.
                target.GetVacuumed(vacuumModel.position, suctionPower, 1f - (Vector3.Distance(playerCamera.position, reachedObj.point) / suctionRange));
            }
        }
    }

    void stopSuckVacuum()
    {
        currentShakeWeight = Mathf.MoveTowards(currentShakeWeight, 0f, Time.deltaTime * motorRampSpeed);

        if(suctionParticles != null && suctionParticles.isPlaying)
        {
            suctionParticles.Stop(); 
        }

        if (vacuumAudio != null)
        {
            vacuumAudio.pitch = currentShakeWeight;
            vacuumAudio.volume = currentShakeWeight;

            if(currentShakeWeight <= 0 && vacuumAudio.isPlaying)
            {
                vacuumAudio.Stop();
            } 
        }
    }

    void OnDisable()
    {
        currentShakeWeight = 0f; 
        if (vacuumModel != null) vacuumModel.localPosition = originalPosition;
        
        if(suctionParticles != null && suctionParticles.isPlaying) suctionParticles.Stop();
        
        if (vacuumAudio != null && vacuumAudio.isPlaying) vacuumAudio.Stop();
    }
}