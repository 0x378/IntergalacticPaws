using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisasterController : MonoBehaviour
{
    Timer matchTimer;
    [SerializeField] GameObject meteor;
    [SerializeField] float meteorInitSpeed;
    void Start()
    {
        matchTimer = GameObject.Find("MatchTimer").GetComponent<Timer>();
    }
    void Update()
    {
        if (matchTimer.pastHalfTime())
        {
            StartCoroutine(MeteorGo());
        }
    }
    IEnumerator MeteorGo()
    {
        meteorInitSpeed *= 1.01f;
        meteor.transform.position = Vector3.MoveTowards(meteor.transform.position, GameManager.terrainInstance.getCraterCenter(), meteorInitSpeed);
        yield return new WaitUntil(() => meteor.transform.position.y <= GameManager.terrainInstance.GetExactHeight(meteor.transform.position.x, meteor.transform.position.z));
        StartCoroutine(MeteorExplode());
    }
    IEnumerator MeteorExplode()
    {
        yield return null;
    }
}
