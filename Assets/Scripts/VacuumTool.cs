using UnityEngine;
using UnityEngine.InputSystem;

public class VacuumTool : MonoBehaviour
{
    [Header("Vacuum Properties")]
    public float suctionRange = 10f;
    public float suctionPower = 0.5f; // Strenght of the vacuum.

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

        RaycastHit reachedObj;
        if(Physics.Raycast(playerCamera.position, playerCamera.forward, out reachedObj, suctionRange))
        {
            Vacuumable target = reachedObj.transform.GetComponentInParent<Vacuumable>();
            if(target != null)
            {
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