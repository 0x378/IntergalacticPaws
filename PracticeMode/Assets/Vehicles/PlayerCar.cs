using UnityEngine;

public class PlayerCar : Vehicle
{
    [SerializeField] private Turret turret;

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
        UpdatePosition();

        if (Time.timeScale > 0)
        {
            UpdateNormalOperation();
        }
    }
}
