using UnityEngine;

public class Turret : MonoBehaviour
{
    [SerializeField] private GameObject source;    // The parent object of this turret
    [SerializeField] private Transform spawnPoint; // Keeps track of the firing location
    [SerializeField] private Laser projectile;     // The projectile prefab to be fired

    private readonly float cooldownTime = 3f / 7f; // Seconds
    private float previousTime;

    private void Start()
    {
        previousTime = Time.time;
    }

    public void AimAtPoint(Vector3 point)
    {
        transform.rotation = Quaternion.LookRotation(point - transform.position, source.transform.up);
    }

    public void AimDirection(Quaternion direction)
    {
        transform.rotation = Quaternion.LookRotation(direction * Vector3.forward, source.transform.up);
    }

    public void Fire()
    {
        if (Time.time - previousTime > cooldownTime)
        {
            Instantiate(projectile, spawnPoint.position, transform.rotation).LinkToSource(source);
            previousTime = Time.time;
        }
    }
}
