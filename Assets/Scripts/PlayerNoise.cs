using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerNoise : MonoBehaviour
{
    [SerializeField] private AudioClip[] footstepClips;
    [SerializeField] private AudioSource footstepSource;
    [SerializeField] private float stepInterval  = 0.45f;
    [SerializeField] private float moveThreshold = 0.1f;

    public bool isEmittingNoise { get; private set; }

    private CharacterController _cc;
    private float _stepTimer;

    private void Awake()
    {
        _cc = GetComponent<CharacterController>();
    }

    private void Update()
    {
        Vector3 hVel = new Vector3(_cc.velocity.x, 0f, _cc.velocity.z);
        isEmittingNoise = hVel.magnitude > moveThreshold;

        if (isEmittingNoise)
        {
            _stepTimer += Time.deltaTime;
            if (_stepTimer >= stepInterval)
            {
                _stepTimer = 0f;
                PlayFootstep();
            }
        }
        else
        {
            _stepTimer = 0f;
        }
    }

    private void PlayFootstep()
    {
        if (footstepSource == null || footstepClips == null || footstepClips.Length == 0) return;
        footstepSource.PlayOneShot(footstepClips[Random.Range(0, footstepClips.Length)]);
    }
}
