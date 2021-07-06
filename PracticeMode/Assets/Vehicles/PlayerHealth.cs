using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    protected int maxHealth = 200;
    [System.NonSerialized] public int currentHealth = 200;
    protected void StartHealth()
    {
        currentHealth = maxHealth;
    }
    protected void TakeDamage(int damage)
    {
        currentHealth -= damage;
    }
    public void HealDamage(int heal) {
        currentHealth += heal;
        if (currentHealth > maxHealth) currentHealth = maxHealth;
    }
}
