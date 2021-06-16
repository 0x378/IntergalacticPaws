using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightFixture : MonoBehaviour
{
    Vector3 initPos;
    [SerializeField] Vector3 com;
    Quaternion initRot;
    Rigidbody fixtureBody;

    private bool repositionIsScheduled = true;

    void Start()
    {
        fixtureBody = GetComponent<Rigidbody>();
        fixtureBody.centerOfMass = com;
        initPos = transform.position;
        initRot = transform.localRotation;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("BossVehicle"))
        {
            fixtureBody.isKinematic = false;
        }
    }

    void FixedUpdate()
    {
        // If terrainInstance exists AND the fixture is NOT already at the correct Y position,
        // do so and make the fixture face the center of the arena
        if (repositionIsScheduled && GameManager.terrainInstance)
        {
            if (!(transform.position.y == GameManager.terrainInstance.GetExactHeight(transform.position.x, transform.position.z)))
            {
                placeFixture(GameManager.terrainInstance.GetExactHeight(initPos.x, initPos.z));
                repositionIsScheduled = false;
            }
        }
    }

    public void placeFixture(float y)
    {
        initPos.y = y - 0.4f;
        transform.position = initPos;
        transform.LookAt(GameManager.terrainInstance.getCraterCenter()); // point at center of crater
        initRot = transform.localRotation;
        initRot.x = 0f;
        transform.localRotation = initRot;
    }
    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position + transform.rotation * com, 1f);
    }
}
