using UnityEngine;

public class PlayerCar : Vehicle
{
    private float minimumBound;
    private float maximumBound;

    // [SerializeField] private DebugText debugText;
    [SerializeField] private Turret turret;
    [SerializeField] private HealthBarUI healthBar;

    private new void OnCollisionEnter(Collision collision)
    {
        if (base.OnCollisionEnter(collision))
        {
            if (currentHealth > 0)
            {
                healthBar.SetHealth(currentHealth);
            }
            else
            {
                currentHealth = 0;
                healthBar.SetHealth(currentHealth);
                gameManager.Halt();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PowerUp"))
        {
            currentHealth += 40;
            healthBar.SetHealth(currentHealth);
            other.gameObject.GetComponentInParent<PowerUps>().collectPU(other);
        }
    }

    private void Initialize()
    {
        terrainInstance = GameManager.terrainInstance;

        if (terrainInstance != null)
        {
            float mapWidth = GameManager.terrainInstance.GetMapWidth();
            minimumBound = mapWidth * 0.02f;
            maximumBound = mapWidth * 0.98f;
            craterRadius = GameManager.terrainInstance.GetCraterRadius();
        }
    }

    /*
    private void UpdateDebugText()
    {
        if (gameManager.debuggingIsEnabled)
        {
            string text = "Velocity: " + velocity;
            text += "\nSpeed: " + speed.ToString("F2");
            text += "\nDistance from center: " + distanceFromCenter.ToString("F2");

            debugText.Set(text);
        }
    }
    */

    private void Start()
    {
        // debugText = Instantiate(debugText, transform.position, Quaternion.identity);
        // debugText.Initialize(gameObject);
        Initialize();
        GameManager.targetSelector.Insert(gameObject);
    }

    private void UpdateNormalOperation()
    {
        UpdateSteering(Input.GetAxis("Horizontal"));

        if (Input.GetKey(KeyCode.Space))
        {
            UpdateTorque(0f);
            ApplyRearBrakes();
        }
        else
        {
            UpdateTorque(Input.GetAxis("Vertical"));
        }

        distanceFromCenter = terrainInstance.DistanceFromCenter(gameObject);

        // Keep the vehicle within bounds:
        if (currentPosition.x < minimumBound && velocity.x < 0f)
        {
            velocity.x *= -0.2f;
            vehicleBody.velocity = velocity;
        }
        else if (currentPosition.x > maximumBound && velocity.x > 0f)
        {
            velocity.x *= -0.2f;
            vehicleBody.velocity = velocity;
        }

        if (currentPosition.z < minimumBound && velocity.z < 0f)
        {
            velocity.z *= -0.2f;
            vehicleBody.velocity = velocity;
        }
        else if (currentPosition.z > maximumBound && velocity.z > 0f)
        {
            velocity.z *= -0.2f;
            vehicleBody.velocity = velocity;
        }

        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, Mathf.Infinity))
        {
            turret.AimAtPoint(hit.point);
        }
        else
        {
            turret.AimDirection(Camera.main.transform.rotation.normalized);
        }

        if (Input.GetMouseButton(0))
        {
            turret.Fire();
        }
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

            if (Time.timeScale > 0)
            {
                if (GameManager.currentState == GameManager.GameState.End)
                {
                    UpdateTorque(0f);
                    ApplyRearBrakes();
                }
                else
                {
                    distanceFromCenter = terrainInstance.DistanceFromCenter(gameObject);

                    if (distanceFromCenter > craterRadius)
                    {
                        UpdateTorque(0f);
                        ApplyRearBrakes();

                        currentHealth = 0;
                        gameManager.Halt();
                    }
                    else
                    {
                        UpdateNormalOperation();
                    }
                }
            }

            // UpdateDebugText();
        }
    }
}
