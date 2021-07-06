using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugText : MonoBehaviour
{
    private GameObject target;
    private TextMesh text3D;
    private Vector3 position;

    public void Initialize(GameObject newTarget)
    {
        target = newTarget;

        text3D = GetComponent<TextMesh>();

        int selectPrimaryColor = Random.Range(0, 3);
        int selectSecondaryColor = Random.Range(0, 2);

        float secondary = Random.Range(0.8f, 1f);
        float terciary = Random.Range(0.4f, 0.8f);

        if (selectPrimaryColor == 0) // Red primary
        {
            if (selectSecondaryColor == 0)
            {
                text3D.color = new Color(1f, secondary, terciary);
            }
            else
            {
                text3D.color = new Color(1f, terciary, secondary);
            }
        }
        else if (selectPrimaryColor == 0) // Green primary
        {
            if (selectSecondaryColor == 0)
            {
                text3D.color = new Color(secondary, 1f, terciary);
            }
            else
            {
                text3D.color = new Color(terciary, 1f, secondary);
            }
        }
        else // Blue primary
        {
            if (selectSecondaryColor == 0)
            {
                text3D.color = new Color(secondary, terciary, 1f);
            }
            else
            {
                text3D.color = new Color(terciary, secondary, 1f);
            }
        }

        Set(""); // Start with no text
    }

    public void Set(string text)
    {
        if (text3D != null)
        {
            text3D.text = text;
        }
    }

    public void SetRed(string text)
    {
        if (text3D != null)
        {
            text3D.text = text;
            text3D.color = Color.red;
        }
    }

    void Update()
    {
        if (target != null)
        {
            position = target.transform.position;
            position.y += 2.5f;
            transform.position = position;
        }

        /*
        float newScale = Vector3.Distance(Camera.main.transform.position, transform.position) * 0.025f;
        transform.localScale = new Vector3(-newScale, newScale, newScale);
        */

        transform.LookAt(Camera.main.transform);
    }
}
