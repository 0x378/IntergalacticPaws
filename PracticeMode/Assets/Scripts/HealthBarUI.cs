using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private GameObject barObject;

    private int maxValue = 200;

    public void SetMaxHealth(int health)
    {
        maxValue = health;
    }

    public void SetHealth(int health)
    {
        Vector3 newScale = barObject.transform.localScale;
        newScale.x = (float)health / (float)maxValue;
        barObject.transform.localScale = newScale;
    }
}
