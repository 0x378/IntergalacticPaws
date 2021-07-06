using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneBehavior : MonoBehaviour
{
    [SerializeField] float offsetX, offsetY, offsetZ, speed;
    [SerializeField] private GameObject currTar;
    float targetTime;
    bool activated = false, deactivated = false;
    Vector3 pos, moveTo, activateTargetPosition, startingPosition;
    public GameObject oCam;
    public Camera cCam;
    void Start()
    {
        pos = transform.position;
        targetTime = Time.time + Random.Range(12f, 18f);
        startingPosition = activateTargetPosition = transform.position;
        activateTargetPosition.y += 10f;
        StartCoroutine(droneActivate());
    }
    void Update()
    {
        if (!activated && !deactivated)
        {
            StartCoroutine(droneActivate());
        }
        else if (deactivated)
        {
            // do nothing
        }
        else
        {
            if (targetTime <= Time.time || currTar == null)
            {
                currTar = GameManager.targetSelector.GetRandomTarget();
                targetTime = Time.time + Random.Range(15f, 18f);
            }
            else // I added this "else" here, because it can still be null if no targets remain.
                 // Otherwise, it can crash upon pressing Quit from the pause menu.
            {
                moveTo.x = currTar.transform.localPosition.x + offsetX + (Mathf.Sin(Time.time) * 3f);
                moveTo.y = currTar.transform.localPosition.y + offsetY + (Mathf.Sin(Time.time) * 3f);
                moveTo.z = currTar.transform.localPosition.z + offsetZ + (Mathf.Sin(Time.time) * 3f);
                transform.position = Vector3.MoveTowards(transform.position, moveTo, speed * Time.smoothDeltaTime);
                transform.RotateAround(currTar.transform.position, Vector3.up, Time.smoothDeltaTime * 10f);
                oCam.transform.LookAt(currTar.transform);
            }
        }
    }
    IEnumerator droneActivate()
    {
        transform.position = Vector3.MoveTowards(transform.position, activateTargetPosition, (speed * 0.1f) * Time.smoothDeltaTime);
        yield return new WaitUntil(() => (transform.position == activateTargetPosition));
        activated = true;
        currTar = GameManager.targetSelector.GetRandomTarget();
    }
    IEnumerator droneDeactivate()
    {
        transform.position = Vector3.MoveTowards(transform.position, activateTargetPosition, speed * Time.smoothDeltaTime);
        yield return new WaitUntil(() => (transform.position == activateTargetPosition));
        transform.LookAt(GameManager.Instance.GetComponent<LunarSurface>().getCraterCenter(), Vector3.up);
        transform.position = Vector3.MoveTowards(transform.position, startingPosition, (speed * 0.1f) * Time.smoothDeltaTime);
        yield return new WaitUntil(() => (transform.position == startingPosition));
        deactivated = true;
    }
    public GameObject returnCurrTar()
    {
        return currTar;
    }
}