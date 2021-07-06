using UnityEngine;

public class EnemyCar : Vehicle
{
    private float waypointDistance;
    private float waypointX;
    private float waypointZ;

    private bool isBackingUp = false;
    private float directionTime;
    private float currentSteeringRatio = 0f;

    [SerializeField] private GameObject carChassisNormal;
    [SerializeField] private GameObject carChassisDestroyed;

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
        Initialize();
    }

    private void NavigateToWaypoint()
    {
        float targetX = waypointX - currentPosition.x;
        float targetZ = waypointZ - currentPosition.z;
        waypointDistance = Mathf.Sqrt((targetX * targetX) + (targetZ * targetZ));
        float angleDegrees = transform.localEulerAngles.y;
        float targetAngle = MathX.AngleWithin180(Mathf.Atan2(targetX, targetZ) * Mathf.Rad2Deg - angleDegrees);

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
            // Choose another random waypoint:
            SelectRandomWaypoint();
        }

        NavigateToWaypoint();
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
                distanceFromCenter = terrainInstance.DistanceFromCenter(gameObject);

                if (currentState != VehicleState.Destroyed && distanceFromCenter > craterRadius)
                {
                    currentState = VehicleState.Eliminated;
                }

                UpdateWander();
            }
        }
    }
}
