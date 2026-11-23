using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(AudioSource))]
public class FootstepController : MonoBehaviour
{
    [Header("Настройки звука")]
    [SerializeField] private AudioClip[] footstepClips; 
    [SerializeField] private float stepInterval = 0.5f; 
    [SerializeField] private float minPitch = 0.9f;     
    [SerializeField] private float maxPitch = 1.1f;     

    [Header("Проверка земли")]
    [SerializeField] private LayerMask groundLayer;     
    [SerializeField] private float rayDistance = 1.2f;  

    private AudioSource audioSource;
    private float stepTimer;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        bool isMoving = CheckMovementInput();

        bool isGrounded = Physics.Raycast(transform.position, Vector3.down, rayDistance, groundLayer);

        if (isMoving && isGrounded)
        {
            stepTimer -= Time.deltaTime;

            if (stepTimer <= 0f)
            {
                PlayRandomFootstep();
                stepTimer = stepInterval; // Сбрасываем таймер до следующего шага
            }
        }
        else
        {
            stepTimer = 0.1f; 
        }
    }

    private bool CheckMovementInput()
    {
        if (Keyboard.current == null) return false;

        return Keyboard.current.wKey.isPressed ||
               Keyboard.current.aKey.isPressed ||
               Keyboard.current.sKey.isPressed ||
               Keyboard.current.dKey.isPressed ||
               Keyboard.current.upArrowKey.isPressed ||
               Keyboard.current.downArrowKey.isPressed ||
               Keyboard.current.leftArrowKey.isPressed ||
               Keyboard.current.rightArrowKey.isPressed;
    }

    private void PlayRandomFootstep()
    {
        if (footstepClips.Length == 0) return;
        audioSource.clip = footstepClips[0];
        audioSource.pitch = Random.Range(minPitch, maxPitch);

        audioSource.time = 0.5f; 
        audioSource.Play();

        CancelInvoke(nameof(StopFootstep));
        Invoke(nameof(StopFootstep), 0.4f);

        int randomIndex = Random.Range(0, footstepClips.Length);
        AudioClip clip = footstepClips[randomIndex];

        audioSource.pitch = Random.Range(minPitch, maxPitch);
        
        audioSource.PlayOneShot(clip);
    }
    private void StopFootstep()
    {
        audioSource.Stop();
    }
}