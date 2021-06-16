using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PowerUps : MonoBehaviour
{
    [SerializeField] GameObject[] powerUps;
    [SerializeField] Vector3[] puLocations;
    [SerializeField] GameObject timerObj;
    private Timer timer;
    private float nextActivate;
    private Vector3 scalar;
    [SerializeField] float chanceToActivate;
    [SerializeField] float activationDelay;
    void Start()
    {
        scalar = Vector3.one;
        nextActivate = 0f;
        timer = timerObj.GetComponent<Timer>();
        for (int i = 0; i < puLocations.Length; i++)
        {
            puLocations[i].y = GameManager.terrainInstance.GetExactHeight(puLocations[i].x, puLocations[i].z) + 1f;
        }
        int j = 0;
        foreach (GameObject p in powerUps)
        {
            p.transform.position = puLocations[j];
            p.SetActive(false);
            j++;
        }
    }
    void Update()
    {
        if (timer.isTimerActive() && timer.returnTimeRemainingPercent() <= 0.8f)
        {
            if (!(powerUps.Any(p => p.activeSelf == true)) && Time.time >= nextActivate)
            {
                activatePowerUp(Random.Range(0, puLocations.Length));
            }
        }
        foreach (GameObject p in powerUps)
        {
            if (p.activeSelf)
            {
                p.transform.Rotate(Vector3.up, 50f * Time.smoothDeltaTime);
                scalar.x = scalar.y = scalar.z = Mathf.Clamp(Mathf.Sin(Time.time), 0.5f, 1f) * 5;
                p.transform.localScale = scalar;
            }
        }
    }
    private void activatePowerUp(int pu)
    {
        powerUps[pu].SetActive(true);
    }
    public void collectPU(Collider pu) {
        nextActivate = Time.time + activationDelay;
        pu.gameObject.SetActive(false);
    }
}