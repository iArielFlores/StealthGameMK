using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f; // Speed of the player
    [SerializeField] private float rotateSpeed = 700f; // Speed of rotation
    [SerializeField] private bool isSneaking = false; // Flag for sneaking state
    [SerializeField] private AudioSource footstepAudioSource; // Reference to the AudioSource for footstep sounds
    [SerializeField] private AudioClip footstepSound; // Footstep sound clip
    [SerializeField] private float footstepDelay = 0.5f; // Delay between footsteps

    private CharacterController characterController;
    private float footstepTimer;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    void Update()
    {
        MovePlayer();

        // Check for toggle sneaking input (press "C" to toggle sneaking)
        if (Input.GetKeyDown(KeyCode.C))
        {
            ToggleSneak();
        }
    }

    private void MovePlayer()
    {
        // Get input
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Create a movement vector
        Vector3 move = new Vector3(horizontal, 0, vertical).normalized;

        if (move.magnitude > 0)
        {
            // Calculate the target rotation
            Quaternion targetRotation = Quaternion.LookRotation(move);
            // Rotate towards the target rotation smoothly
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);

            // Play footstep sound if not sneaking
            if (!isSneaking)
            {
                footstepTimer += Time.deltaTime;
                if (footstepTimer >= footstepDelay)
                {
                    PlayFootstepSound();
                    footstepTimer = 0f; // Reset timer
                }
            }
        }

        // Apply movement based on sneaking state
        float currentSpeed = isSneaking ? moveSpeed * 0.5f : moveSpeed; // Reduce speed if sneaking
        characterController.Move(move * currentSpeed * Time.deltaTime);
    }

    private void PlayFootstepSound()
    {
        if (footstepAudioSource != null && footstepSound != null)
        {
            footstepAudioSource.PlayOneShot(footstepSound);
        }
    }

    // Call this method to toggle sneaking
    public void ToggleSneak()
    {
        isSneaking = !isSneaking;
        Debug.Log("Sneaking: " + isSneaking); // Optional: Log the sneaking state
    }

    // Check if the player is making noise
    public bool IsMakingNoise()
    {
        return !isSneaking; // Consider player making noise if not sneaking
    }

    
    public bool IsSneaking => isSneaking;
}
