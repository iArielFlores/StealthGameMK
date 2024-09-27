using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TurretStates
{
    IDLE,
    SHOOT
}

public class LineOfSight : MonoBehaviour
{
    [SerializeField] Transform target; // Reference to the player
    public TurretStates state = TurretStates.IDLE;
    float timeTilShoot = 1.5f;

    // Update method
    void Update()
    {
        if (target == null) return; // Ensure target is assigned

        Vector3 directionToTarget = (target.position - transform.position).normalized;
        Vector3 forwardDirection = transform.forward;

        float dot = Vector3.Dot(forwardDirection, directionToTarget);

        // Check if the target is in line of sight
        if (dot > 0.5f)
        {
            state = TurretStates.SHOOT;
        }
        else
        {
            state = TurretStates.IDLE;
        }

        switch (state)
        {
            case TurretStates.IDLE:
                UpdateIdle();
                break;
            case TurretStates.SHOOT:
                UpdateShoot();
                break;
        }
    }

    void UpdateIdle()
    {
        // Optional: you can implement additional idle behavior here
    }

    void UpdateShoot()
    {
        timeTilShoot -= Time.deltaTime;

        if (timeTilShoot <= 0)
        {
            Debug.Log("BANG");
            timeTilShoot = 2.0f; // Reset shoot timer
        }
    }
}
