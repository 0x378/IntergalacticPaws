using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bigScreenMovement : MonoBehaviour
{
    [SerializeField] float delta, speed, yVariance;
    Vector3 initPos;
    void Start()
    {
        float r = Random.Range(-yVariance, yVariance);
        initPos = transform.localPosition;
    }
    void Update()
    {
        Vector3 move = initPos;
        move.y += (delta + yVariance) * Mathf.Sin(Time.time * speed);
        transform.localPosition = move;
    }
}