using UnityEngine;

public class BigCat : Vehicle
{
    private Vehicle currentTarget;

    private float waypointDistance;
    private float waypointX;
    private float waypointZ;

    private bool isBackingUp = false;
    private float directionTime;
    private float currentSteeringRatio = 0f;

    [SerializeField] private Laser laserProjectile;

    private readonly float aimingAccuracy = 2f;         // Maximum degrees of deviation
    private readonly float laserCooldownTime = 1.5f / 7f; // Seconds
    private float previousLaserTime;

    private Vector3 turretOffset = new Vector3(3f, 8f, 10f); // x (right), y (up), z (forward)

    private new void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Terrain"))
        {
            return;
        }

        int damage;

        if (collision.gameObject.CompareTag("Projectile"))
        {
            Projectile hit = collision.gameObject.GetComponent<Projectile>();

            if (hit.IsSource(gameObject))
            {
                damage = 0;
            }
            else
            {
                damage = collision.gameObject.GetComponent<Projectile>().GetDamageAmount();
            }
        }
        else
        {
            damage = (int)(collision.relativeVelocity.magnitude * damageMultiplier);
        }

        TakeDamage(damage);

        if (currentHealth <= 0)
        {
            highDamageAnimation.SetActive(true);
            currentState = VehicleState.Destroyed;
            GameManager.targetSelector.FlagBossAsDestroyed();
        }
        else if (currentHealth <= 60)
        {
            mediumDamageAnimation.SetActive(true);
        }
        else if (currentHealth <= 120)
        {
            lowDamageAnimation.SetActive(true);
        }
    }

    private bool SelectNewTarget()
    {
        currentTarget = null;

        if (GameManager.targetSelector == null)
        {
            return false;
        }

        PlayerCar potentialTarget = GameManager.targetSelector.GetPlayer();

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

    private Vector3 GetTurretLocation()
    {
        Vector3 turretPosition = currentPosition;

        turretPosition += transform.right * turretOffset.x;
        turretPosition += transform.up * turretOffset.y;
        turretPosition += transform.forward * turretOffset.z;

        turretOffset.x = -turretOffset.x; // Switch sides for the next laser shot

        return turretPosition;
    }

    private Vector3 RandomAccuracyDeviation()
    {
        float deviationX = Random.Range(-aimingAccuracy, aimingAccuracy);
        float deviationY = Random.Range(-aimingAccuracy, aimingAccuracy);

        return new Vector3(deviationX, deviationY, 0f);
    }

    private void FireLaser()
    {
        if (Time.time - previousLaserTime > laserCooldownTime)
        {
            Laser firedLaser = Instantiate(laserProjectile, GetTurretLocation(), Quaternion.identity);
            firedLaser.transform.LookAt(currentTarget.transform);
            firedLaser.transform.Rotate(RandomAccuracyDeviation());
            firedLaser.LinkToSource(gameObject);
            previousLaserTime = Time.time;
        }
    }

    private void SelectRandomWaypoint()
    {
        // Select random waypoint:
        float centerOffset = terrainInstance.GetCenterOffset();
        float craterRadius = terrainInstance.GetCraterRadius();
        float targetRadius = Random.Range(0.0625f * craterRadius, 0.25f * craterRadius);
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
        Initialize();

        // Override the original Vehicle settings:
        maxHealth = 220;
        currentHealth = 240;
        torqueCoefficient = 2005000f;
        steeringCoefficient = 40f;
        vehicleBody.centerOfMass += new Vector3(0f, -8.5f, 0f);

        previousLaserTime = Time.time;
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
            currentSteeringRatio -= Time.deltaTime;

            if (currentSteeringRatio < -1f)
            {
                currentSteeringRatio = -1f;
            }
        }
        else
        {
            currentSteeringRatio += Time.deltaTime;

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
            if (Time.time - directionTime > 3f)
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
                if (Time.time - directionTime > 3f)
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
        if (waypointDistance < 25f) // If arrived at waypoint...
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
        if (currentTarget != null)
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

    private void UpdateDestroyed()
    {
        UpdateTorque(0);
        ApplyRearBrakes();
    }

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
                    default: // Destroyed state:
                        {
                            UpdateDestroyed();
                            break;
                        }
                }
            }
        }
    }
}
