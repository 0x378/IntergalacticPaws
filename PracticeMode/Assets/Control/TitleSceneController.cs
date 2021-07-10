using System.Collections;
using UnityEngine;

public class TitleSceneController : MonoBehaviour
{
    [SerializeField] GameObject planet, car, camTarget, speedometerObj;
    [SerializeField] GameObject[] wheels;
    [SerializeField] Camera sceneCam;
    [SerializeField] float rotateSpeed, carBounce;
    Vector3 carMover;
    Transform camFinal;
    public float speed = 0f;

    void Start()
    {
        carMover = car.transform.localPosition;
        carBounce = carMover.y;
    }

    void Update()
    {
        speed = Mathf.Clamp(speed + 0.01257f, 0f, 88f);
        planet.transform.Rotate(Vector3.up, -1f * rotateSpeed * Time.smoothDeltaTime);
        carMover.y = Mathf.Clamp((carMover.y + Mathf.Sin(Time.time * Random.Range(4f, 6f)) * (rotateSpeed / 100)), carBounce - 1.5f, carBounce + 1.5f);
        car.transform.localPosition = carMover;
        foreach (GameObject w in wheels)
        {
            w.transform.Rotate(Vector3.left, -30f * rotateSpeed * Time.smoothDeltaTime);
        }
        rotateSpeed += 0.02f;
        // if (!(sceneCam.transform.position.x >= -1600))
        // {
        sceneCam.transform.RotateAround(car.transform.position, Vector3.up, -2.8f * Time.smoothDeltaTime);
        // }
        // if (rotateSpeed >= 150f)
        // {
        //     camFinal = sceneCam.transform;
        //     foreach (GameObject w in wheels)
        //     {
        //         w.SetActive(false);
        //     }
        //     StartCoroutine(titleScreenFun());
        // }
    }

    IEnumerator titleScreenFun()
    {
        carMover.z -= rotateSpeed;
        car.transform.localPosition = carMover;
        yield return new WaitUntil(() => rotateSpeed >= 250f);
        StartCoroutine(titleScreenFunOver());
    }

    IEnumerator titleScreenFunOver()
    {
        sceneCam.transform.rotation = camFinal.rotation;
        sceneCam.transform.position = camFinal.position;
        yield return null;
    }
}
