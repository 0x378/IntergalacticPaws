using UnityEngine;

public class Vehicle : MonoBehaviour
{
	[SerializeField] protected GameObject steeringAxisFL;
	[SerializeField] protected GameObject steeringAxisFR;

	[SerializeField] protected GameObject wheelFL;
	[SerializeField] protected GameObject wheelFR;
	[SerializeField] protected GameObject wheelRL;
	[SerializeField] protected GameObject wheelRR;

	[SerializeField] protected WheelCollider colliderFL;
	[SerializeField] protected WheelCollider colliderFR;
	[SerializeField] protected WheelCollider colliderRL;
	[SerializeField] protected WheelCollider colliderRR;

	[SerializeField] protected GameObject lowDamageAnimation;
	[SerializeField] protected GameObject mediumDamageAnimation;
	[SerializeField] protected GameObject highDamageAnimation;

	protected float torqueCoefficient = 1600f;
	protected float steeringCoefficient = 40f;
	protected float damageMultiplier = 0.18f;
	protected float brakingPower = 5000f;
	protected float wheelDistance = 1f;
	protected float speed = 0f;

	// Must be initialized by inheriting object prior to use:
	protected float distanceFromCenter = 0f;
	protected float craterRadius = 128f;
	protected float centerOffset = 512f;

	protected GameManager gameManager;
	protected LunarSurface terrainInstance;
	protected Rigidbody vehicleBody;

	protected Vector3 currentPosition;
	protected Vector3 velocity;

	public enum VehicleState
	{
		Wander,
		Chase,
		Flee,
		Eliminated,
		Destroyed
	}

	protected VehicleState currentState;

	private void Awake()
	{
		// Prevent the creation of this object prior to initialization:
		gameManager = GameManager.Instance;

		if (gameManager == null)
		{
			Destroy(gameObject);
		}
		else
		{
			lowDamageAnimation.SetActive(false);
			mediumDamageAnimation.SetActive(false);
			highDamageAnimation.SetActive(false);

			vehicleBody = GetComponent<Rigidbody>();
			wheelDistance = (colliderFL.suspensionDistance + colliderFL.radius);
			vehicleBody.centerOfMass += new Vector3(0f, -0.84f, 0f);
		}
	}

	protected void ApplyRearBrakes()
	{
		colliderRL.brakeTorque = brakingPower;
		colliderRR.brakeTorque = brakingPower;
	}

	protected void ReleaseRearBrakes()
	{
		colliderRL.brakeTorque = 0f;
		colliderRR.brakeTorque = 0f;
	}

	protected void UpdatePosition()
	{
		currentPosition = transform.position;

		if (gameManager.editorInterface.activeSelf)
		{
			craterRadius = terrainInstance.GetCraterRadius();
			centerOffset = terrainInstance.GetCenterOffset();
			float mapHeight = terrainInstance.GetExactHeight(currentPosition.x, currentPosition.z);

			if (currentPosition.y < mapHeight + 1f)
			{
				currentPosition.y = mapHeight + 1f;
			}

			transform.position = currentPosition;
		}

		velocity = vehicleBody.velocity;
		speed = transform.InverseTransformDirection(velocity).z;
	}

	protected void UpdateSteering(float steeringRatio)
	{
		float currentSteerAngle = steeringCoefficient * steeringRatio;

		colliderFL.steerAngle = currentSteerAngle;
		colliderFR.steerAngle = currentSteerAngle;

		Quaternion steeringRotation = Quaternion.Euler(new Vector3(0f, currentSteerAngle, 0f));
		steeringAxisFL.transform.localRotation = steeringRotation;
		steeringAxisFR.transform.localRotation = steeringRotation;

		// Flip the car over if upside-down:
		Vector3 angularV = vehicleBody.angularVelocity;
		float flipOffset = transform.up.y - 1;
		float flipRatio = 1.5f * flipOffset * flipOffset;
		angularV -= flipRatio * steeringRatio * Time.deltaTime * transform.forward;
		vehicleBody.angularVelocity = angularV;
	}

	// Align the wheel to its collider:
	private void UpdateWheelPositions(GameObject wheel, WheelCollider collider)
	{
		Vector3 center = collider.transform.TransformPoint(collider.center);
		Vector3 down = -collider.transform.up;

		if (Physics.Raycast(center, down, out RaycastHit hit, wheelDistance))
		{
			wheel.transform.position = collider.radius * collider.transform.up + hit.point;
		}
		else
		{
			wheel.transform.position = collider.suspensionDistance * down + center;
		}
	}

	protected void UpdateTorque(float ratio)
	{
		float currentTorque = torqueCoefficient * ratio;

		UpdateWheelPositions(wheelFL, colliderFL);
		UpdateWheelPositions(wheelFR, colliderFR);
		UpdateWheelPositions(wheelRL, colliderRL);
		UpdateWheelPositions(wheelRR, colliderRR);

		wheelFL.transform.Rotate(new Vector3(colliderFL.rpm * 6f * Time.deltaTime, 0f, 0f));
		wheelFR.transform.Rotate(new Vector3(colliderFR.rpm * -6f * Time.deltaTime, 0f, 0f));
		wheelRL.transform.Rotate(new Vector3(colliderRL.rpm * 6f * Time.deltaTime, 0f, 0f));
		wheelRR.transform.Rotate(new Vector3(colliderRR.rpm * -6f * Time.deltaTime, 0f, 0f));

		colliderFL.motorTorque = currentTorque;
		colliderFR.motorTorque = currentTorque;
		colliderRL.motorTorque = currentTorque;
		colliderRR.motorTorque = currentTorque;

		if ((speed > 0.125f && ratio < 0f) || (speed < -0.125f && ratio > 0f))
		{
			ApplyRearBrakes();
		}
		else
		{
			ReleaseRearBrakes();
		}
	}
}
