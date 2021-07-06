using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    private float mouseSensitivity = 6f;
    private float acceleration = 120f;
    private float minimumFollowDistance = 3.8f;
    private float maximumFollowDistance = 45f;
    private float targetFollowDistance = 16f;
    //private float bumpUp = 0f;

    [SerializeField] private GameObject player;
    [SerializeField] private GameObject canvasUI;
    [SerializeField] private GameObject inGameInterface;

    private Vector2 currentRotation;
    private Vector3 position;
    private Vector3 velocity;

    private GameManager.GameState previousState;
    private LunarSurface terrainInstance;

    private float currentMapHeight;
    private float mapOffset = 1.6f;
    private float minimumBound;
    private float maximumBound;

    private void Awake()
    {
        // Prevent the creation of this object prior to initialization:
        if (GameManager.Instance == null)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        currentRotation = new Vector3(0f, 0f, 0f);
        velocity = new Vector3(0f, 0f, 0f);
        previousState = GameManager.currentState;
    }

    private void InitializeTerrain()
    {
        currentMapHeight = 0f;
        //bumpUp = 0f;
        terrainInstance = GameManager.terrainInstance;

        if (terrainInstance != null)
        {
            float mapWidth = terrainInstance.GetMapWidth();
            minimumBound = mapWidth * 0.02f;
            maximumBound = mapWidth * 0.98f;

            canvasUI.SetActive(true);
            GameManager.Instance.activeInterface = inGameInterface;

        }
    }

    private void UpdateControl()
    {
        float deltaAcceleration = acceleration * Time.unscaledDeltaTime;
        velocity *= Mathf.Exp(-2f * Time.unscaledDeltaTime);
        Vector3 direction = new Vector3(transform.forward.x, 0f, transform.forward.z);
        direction = deltaAcceleration * direction.normalized;

        // Move the camera in the direction it is facing:
        if (Input.GetKey(KeyCode.W))
        {
            velocity.x += direction.x;
            velocity.z += direction.z;
        }

        // Move the camera in the opposite direction:
        if (Input.GetKey(KeyCode.S))
        {
            velocity.x -= direction.x;
            velocity.z -= direction.z;
        }

        // Move the camera to the left, relative to its current direction:
        if (Input.GetKey(KeyCode.A))
        {
            velocity.x -= direction.z;
            velocity.z += direction.x;
        }

        // Move the camera to the right of its current direction:
        if (Input.GetKey(KeyCode.D))
        {
            velocity.x += direction.z;
            velocity.z -= direction.x;
        }

        // Ascend or descend in elevation:
        if (Input.GetKey(KeyCode.Space))
        {
            velocity.y += deltaAcceleration;
        }

        if (Input.GetKey(KeyCode.LeftShift))
        {
            velocity.y -= deltaAcceleration;
        }
    }

    private void UpdateDirection()
    {
        currentRotation.x += Input.GetAxis("Mouse X") * mouseSensitivity;
        currentRotation.y -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        currentRotation.x = MathX.AngleWithin180(currentRotation.x);
        currentRotation.y = Mathf.Clamp(currentRotation.y, -40f, 89.875f);
        transform.rotation = Quaternion.Euler(currentRotation.y, currentRotation.x, 0f);
    }

    private void UpdatePosition()
    {
        position += velocity * Time.unscaledDeltaTime;

        if (terrainInstance == null)
        {
            InitializeTerrain();
        }
        else
        {
            currentMapHeight = GameManager.terrainInstance.GetSmoothHeight(position.x, position.z) + mapOffset;

            MathX.Clamp(ref position.x, minimumBound, maximumBound, ref velocity.x);
            MathX.Clamp(ref position.y, currentMapHeight, maximumBound, ref velocity.y);
            MathX.Clamp(ref position.z, minimumBound, maximumBound, ref velocity.z);
        }
    }

    private void FollowPlayer()
    {
        // Use the scroll wheel to control the camera's distance from player:
        targetFollowDistance -= 0.3f * Input.mouseScrollDelta.y;
        targetFollowDistance = Mathf.Clamp(targetFollowDistance, minimumFollowDistance, maximumFollowDistance);

        UpdateDirection();

        //float followDistance = (targetFollowDistance - minimumFollowDistance) * (currentRotation.y + 90f) * 0.0056f + minimumFollowDistance;
        float followDistance = (targetFollowDistance - minimumFollowDistance) * (1f - Mathf.Exp(-0.01f * (currentRotation.y + 90f))) + minimumFollowDistance;

        position = player.transform.position - (followDistance * transform.forward);
        position.y += 3.2f; // Vertical offset;

        if (terrainInstance == null)
        {
            InitializeTerrain();
        }
        else
        {
            //currentMapHeight = GameManager.terrainInstance.GetSmoothHeight(position.x, position.z) + mapOffset;
            currentMapHeight = GameManager.terrainInstance.GetExactHeight(position.x, position.z) + mapOffset;
        }

        //KeepAboveTerrain();

        if (position.y < currentMapHeight)
        {
            position.y = currentMapHeight;
        }
    }

    /*
    // Prevent the camera from clipping through terrain:
    private void KeepAboveTerrain()
    {
        float heightDifference = currentMapHeight - position.y;

        if (heightDifference > 0f)
        {
            bumpUp += 16f * heightDifference * Time.unscaledDeltaTime;
        }

        bumpUp *= Mathf.Exp(-15f * Time.unscaledDeltaTime);

        position.y += bumpUp;
    }*/

    private void OnStateChange()
    {
        // Zoom out upon pausing or leaving the gameplay, by
        // ascending at a 45 degree angle from the horizon:
        if (previousState == GameManager.GameState.Play)
        {

            velocity = new Vector3(transform.forward.x, 0f, transform.forward.z);
            velocity = velocity.normalized * -20f;
            velocity.y = 20f;
        }

        if (GameManager.currentState == GameManager.GameState.Play)
        {
            canvasUI.SetActive(true);
            GameManager.Instance.activeInterface = inGameInterface;
            velocity = new Vector3(0f, 0f, 0f);
        }
        else
        {
            canvasUI.SetActive(false);
        }

        previousState = GameManager.currentState;
    }

    private void Update()
    {
        // Upon a change of the current state:
        if (GameManager.currentState != previousState)
        {
            OnStateChange();
        }

        // Follow the player's vehicle during normal gameplay:
        if (GameManager.currentState == GameManager.GameState.Play)
        {
            FollowPlayer();
        }
        else // Pause or edit modes; spectate rather than following player:
        {
            position = transform.position;

            UpdateControl();
            UpdatePosition();

            if (GameManager.currentState == GameManager.GameState.Spectate)
            {
                UpdateDirection(); // Mouse-controlled camera angle
            }
        }

        transform.position = position;
    }
}
