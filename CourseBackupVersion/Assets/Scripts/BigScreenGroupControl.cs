using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigScreenGroupControl : MonoBehaviour
{
    [SerializeField] [Range(0.0f, 0.1f)] float rotationSpeed;
    [SerializeField] [Range(0.00f, 1.00f)] float insetOffset;
    void Start()
    {
        // foreach (Transform c in transform)
        // {
        //     Vector3 initPos = c.transform.position;
        //     initPos.x = Mathf.Cos(GameManager.terrainInstance.GetCraterRadius() * insetOffset);
        // }
    }
    void Update()
    {
        transform.Rotate(0f, 2 * (Mathf.Sin(Time.time) * (rotationSpeed + Time.smoothDeltaTime)), 0f);
    }
}