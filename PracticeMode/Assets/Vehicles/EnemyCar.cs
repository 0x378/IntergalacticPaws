using UnityEngine;

public class EnemyCar : Vehicle
{
    private Vehicle currentTarget;

    private float startTime;
    private float waypointDistance;
    private float waypointX;
    private float waypointZ;

    private bool isBackingUp = false;
    private float directionTime;
    private float currentSteeringRatio = 0f;

    [SerializeField] private GameObject carChassisNormal;
    [SerializeField] private GameObject carChassisDestroyed;
    // [SerializeField] private DebugText debugText;
    [SerializeField] private Turret turret;

    private readonly float aimingAccuracy = 9f; // Maximum degree range of deviation

    private void AttemptNewTarget(GameObject potentialCandidate)
    {
        if (ReferenceEquals(potentialCandidate, gameObject))
        {
            return;
        }

        if (potentialCandidate.CompareTag("PlayerVehicle") || potentialCandidate.CompareTag("EnemyVehicle"))
        {
            currentTarget = potentialCandidate.GetComponent<Vehicle>();

            if (currentState == VehicleState.Wander)
            {
                currentState = VehicleState.Chase;
            }
        }
    }

    private new void OnCollisionEnter(Collision collision)
    {
        if (base.OnCollisionEnter(collision))
        {
            if (currentHealth > 0)
            {
                if (collision.gameObject.CompareTag("Projectile"))
                {
                    // Attempt to change the target to the projectile's firing source:
                    AttemptNewTarget(collision.gameObject.GetComponent<Projectile>().GetSource());
                }
                else
                {
                    // Attempt to change the targt to the collision source:
                    AttemptNewTarget(collision.gameObject);

                    float distanceAlongFront = transform.InverseTransformDirection(collision.transform.position - transform.position).z;
                    directionTime = Time.time;

                    if (distanceAlongFront > 0f)
                    {
                        isBackingUp = true;
                    }
                    else
                    {
                        isBackingUp = false;
                    }
                }
            }
            else
            {
                carChassisNormal.SetActive(false);
                carChassisDestroyed.SetActive(true);
                GameManager.targetSelector.RemoveBySearch(gameObject);
            }
        }
    }

    private Vector3 RandomAccuracyDeviation()
    {
        // float deviationX = Random.Range(-aimingAccuracy, aimingAccuracy);
        // float deviationY = Random.Range(-aimingAccuracy, aimingAccuracy);

        float seed = Time.time * 3f;

        float deviationX = (Mathf.PerlinNoise(seed, 0f) - 0.5f) * aimingAccuracy;
        float deviationY = (Mathf.PerlinNoise(0f, seed) - 0.5f) * aimingAccuracy;

        return new Vector3(deviationX, deviationY, 0f);
    }

    private void FireLaser()
    {
        turret.AimAtPoint(currentTarget.transform.position);
        turret.transform.Rotate(RandomAccuracyDeviation());
        turret.Fire();
    }

    private bool SelectNewTarget()
    {
        currentTarget = null;
        GameObject potentialTarget = GameManager.targetSelector.GetRandomTargetExcept(gameObject);

        if (potentialTarget == null)
        {
            return false;
        }

        currentTarget = potentialTarget.GetComponent<Vehicle>();

        if (currentTarget == null)
        {
            return false;
        }

        waypointX = currentTarget.transform.position.x;
        waypointZ = currentTarget.transform.position.z;

        return true;
    }

    private void SelectRandomWaypoint()
    {
        // Select random waypoint:
        float centerOffset = terrainInstance.GetCenterOffset();
        float craterRadius = terrainInstance.GetCraterRadius();
        float targetRadius = Random.Range(0.2f * craterRadius, 0.7f * craterRadius);
        float targetAngle = Random.Range(0f, 2f * Mathf.PI);

        waypointX = targetRadius * Mathf.Cos(targetAngle) + centerOffset;
        waypointZ = targetRadius * Mathf.Sin(targetAngle) + centerOffset;

        float targetX = waypointX - currentPosition.x;
        float targetZ = waypointZ - currentPosition.z;
        waypointDistance = Mathf.Sqrt((targetX * targetX) + (targetZ * targetZ));
    }

    private void Initialize()
    {
        terrainInstance = GameManager.terrainInstance;
        waypointDistance = 0f;
        currentState = VehicleState.Wander;
        startTime = Time.time;
        directionTime = Time.time;

        if (terrainInstance != null)
        {
            craterRadius = terrainInstance.GetCraterRadius();
            centerOffset = terrainInstance.GetCenterOffset();
            SelectRandomWaypoint();
        }
    }

    private void Start()
    {
        carChassisDestroyed.SetActive(false);
        // debugText = Instantiate(debugText, transform.position, Quaternion.identity);
        // debugText.Initialize(gameObject);
        Initialize();
        GameManager.targetSelector.Insert(gameObject);
    }

    private void NavigateToWaypoint()
    {
        float targetX = waypointX - currentPosition.x;
        float targetZ = waypointZ - currentPosition.z;
        waypointDistance = Mathf.Sqrt((targetX * targetX) + (targetZ * targetZ));
        float angleDegrees = transform.localEulerAngles.y;
        float targetAngle = MathX.AngleWithin180(Mathf.Atan2(targetX, targetZ) * Mathf.Rad2Deg - angleDegrees);
        //float waypointAngleRatio = Mathf.Clamp(targetAngle, -steeringCoefficient, steeringCoefficient) / steeringCoefficient;





        // +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // Experimental: +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        if (speed < 0f) // If traveling in reverse:
        {
            targetAngle = -targetAngle;
        }

        if (targetAngle < colliderFL.steerAngle)
        {
            currentSteeringRatio -= 2f * Time.deltaTime;

            if (currentSteeringRatio < -1f)
            {
                currentSteeringRatio = -1f;
            }
        }
        else
        {
            currentSteeringRatio += 2f * Time.deltaTime;

            if (currentSteeringRatio > 1f)
            {
                currentSteeringRatio = 1f;
            }
        }

        UpdateSteering(currentSteeringRatio);

        // +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++







        // -------------------------------------------------------------------------------------------------------------
        // Previous version: -------------------------------------------------------------------------------------------
        /*
        float waypointAngleRatio = Mathf.Clamp(targetAngle, -steeringCoefficient, steeringCoefficient) / steeringCoefficient;

        // Turn towards the waypoint:
        if (speed < 0f)
        {
            UpdateSteering(-waypointAngleRatio);
        }
        else
        {
            UpdateSteering(waypointAngleRatio);
        }*/
        // -------------------------------------------------------------------------------------------------------------
        // -------------------------------------------------------------------------------------------------------------







        float targetSpeed = 30f / (speed * speed / 25f + 1f) + 4f;

        if (isBackingUp)
        {
            if (Time.time - directionTime > 2f)
            {
                isBackingUp = false;
                directionTime = Time.time;
            }
            else
            {
                targetSpeed = -targetSpeed;
            }
        }
        else
        {
            if (speed > 4f)
            {
                directionTime = Time.time;
            }
            else
            {
                // If currently stuck:
                if (Time.time - directionTime > 2f)
                {
                    isBackingUp = true;
                    directionTime = Time.time;
                    targetSpeed = -targetSpeed;
                }
            }
        }

        if (speed < targetSpeed)
        {
            UpdateTorque(1f);
        }
        else
        {
            UpdateTorque(-1f);
        }
    }

    private void UpdateWander()
    {
        if (waypointDistance < 8f) // If arrived at waypoint...
        {
            // Attempt to select a target:
            if (SelectNewTarget())
            {
                currentState = VehicleState.Chase; // If successful.
            }
            else // Choose another random waypoint if unsuccessful:
            {
                SelectRandomWaypoint();
            }
        }

        NavigateToWaypoint();
    }

    private void UpdateChase()
    {
        // If the target is still valid:
        if (currentTarget != null && currentTarget.currentHealth > 0 && terrainInstance.IsWithinCrater(currentTarget.gameObject))
        {
            waypointX = currentTarget.transform.position.x;
            waypointZ = currentTarget.transform.position.z;
            FireLaser();
        }
        else // Otherwise, return to wandering state:
        {
            SelectNewTarget();

            if (currentTarget == null)
            {
                SelectRandomWaypoint();
                currentState = VehicleState.Wander;
            }
        }

        NavigateToWaypoint();
    }

    /*
    private void UpdateFlee()
    {
        if (currentTarget != null && currentTarget.currentHealth > 0 && terrainInstance.IsWithinCrater(currentTarget.gameObject))
        {
            // Select a trajectory in the direction of this vehicle from the target
            Vector3 trajectory = currentPosition - currentTarget.transform.position;
            trajectory.y = 0f;
            trajectory = trajectory.normalized * 8f; // Place waypoint at a close distance away

            waypointX = currentPosition.x + trajectory.x;
            waypointZ = currentPosition.z + trajectory.z;
        }
        else // Otherwise, drive around randomly:
        {
            SelectRandomWaypoint();
        }

        NavigateToWaypoint();
    }*/

    private void UpdateEliminated()
    {
        if (distanceFromCenter < craterRadius + 16f) // Drive away from the crater:
        {
            Vector3 trajectory = currentPosition;
            trajectory.x -= centerOffset;
            trajectory.y = 0f;
            trajectory.z -= centerOffset;
            trajectory = trajectory.normalized * 8f; // Place waypoint at a close distance away

            waypointX = currentPosition.x + trajectory.x;
            waypointZ = currentPosition.z + trajectory.z;

            NavigateToWaypoint();
            startTime = Time.time;
        }
        else
        {
            UpdateTorque(0);
            ApplyRearBrakes();

            // Despawn after 8 seconds:
            if (Time.time - startTime > 8f)
            {
                // Destroy(debugText.gameObject);
                Destroy(gameObject);
            }
        }
    }

    private void UpdateDestroyed()
    {
        UpdateTorque(0);
        ApplyRearBrakes();

        // Despawn after 8 seconds:
        if (Time.time - startTime > 8f)
        {
            // Destroy(debugText.gameObject);
            Destroy(gameObject);
        }
    }

    /*
    private void UpdateDebugText()
    {
        if (gameManager.debuggingIsEnabled)
        {
            string text = "State: " + currentState;

            if (currentTarget != null)
            {
                text += "\nTarget: " + gameObject.name;
            }

            text += "\nSpeed: " + speed.ToString("F2");
            text += "\nDistance from center: " + distanceFromCenter.ToString("F2");

            debugText.Set(text);
        }
    }
    */

    private void Update()
    {
        if (terrainInstance == null)
        {
            Initialize();
        }
        else
        {
            UpdatePosition();

            if (GameManager.currentState == GameManager.GameState.End)
            {
                UpdateTorque(0f);
                ApplyRearBrakes();
            }
            else
            {
                distanceFromCenter = terrainInstance.DistanceFromCenter(gameObject);

                if (currentState != VehicleState.Destroyed && distanceFromCenter > craterRadius)
                {
                    GameManager.targetSelector.RemoveBySearch(gameObject);
                    currentState = VehicleState.Eliminated;
                    // debugText.SetRed("Eliminated!");
                }

                switch (currentState)
                {
                    case VehicleState.Wander:
                        {
                            UpdateWander();
                            break;
                        }
                    case VehicleState.Chase:
                        {
                            UpdateChase();
                            break;
                        }
                    case VehicleState.Eliminated:
                        {
                            UpdateEliminated();
                            break;
                        }
                    default: // Destroyed state:
                        {
                            UpdateDestroyed();
                            break;
                        }
                }

                /*
                if (currentHealth > 0)
                {
                    UpdateDebugText();
                }
                else */

                if (currentHealth <= 0 && currentState != VehicleState.Destroyed)
                {
                    GameManager.targetSelector.RemoveBySearch(gameObject);
                    currentState = VehicleState.Destroyed;
                    // debugText.SetRed("Destroyed!");
                    startTime = Time.time;
                }
            }
        }
    }
}
