using UnityEngine;
using UnityEngine.InputSystem;

public class LanternTool : MonoBehaviour
{
    [Header("Lantern Parts")]
    public GameObject spotlight;
    public AudioSource lanternAudio;
    
    [Header("Sounds")]
    public AudioClip turnOnSound;
    public AudioClip turnOffSound;

    private bool isLightOn = false; // Lantern is initially off.

    void Start()
    {
        if (spotlight != null) 
        {
            spotlight.SetActive(isLightOn);
        }
    }

    void Update()
    {
        // wasPressedThisFrame so that one click = one toggle, even if button held.
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            ToggleLight();
        }
    }

    void ToggleLight()
    {
        isLightOn = !isLightOn;
        
        if (spotlight != null)
        {
            spotlight.SetActive(isLightOn);
        }

        if (lanternAudio != null) // Play sound effect.
        {
            // PlayOneShot to play different sounds from one source.
            if (isLightOn && turnOnSound != null)
            {
                lanternAudio.PlayOneShot(turnOnSound);
            }
            else if (!isLightOn && turnOffSound != null)
            {
                lanternAudio.PlayOneShot(turnOffSound);
            }
        }
    }
}