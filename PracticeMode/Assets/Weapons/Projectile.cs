using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Projectile : MonoBehaviour
{
    protected readonly int baseDamage;
    protected readonly int criticalDamage;
    protected readonly int criticalChance;

    protected Vector3 initialPosition;
    protected Vector3 initialDirection;

    protected GameObject sourceObject = null; // The object which fired the projectile
    protected Collider projectileCollider = null;
    protected Rigidbody projectileBody = null;

    private void Awake()
    {
        projectileCollider = GetComponent<Collider>();
        projectileCollider.enabled = false;
        projectileBody = GetComponent<Rigidbody>();
    }

    public void LinkToSource(GameObject source)
    {
        initialPosition = transform.position;
        initialDirection = transform.forward.normalized;
        sourceObject = source;
        projectileCollider.enabled = true;
    }

    public GameObject GetSource()
    {
        return sourceObject;
    }

    // Return true if the testObject is the source which fired this projectile
    public bool IsSource(GameObject testObject)
    {
        // If uninitialized, it is most likely that the object calling on this is the source which fired it.
        if (sourceObject == null)
        {
            return true;
        }

        // Otherwise, check whether the testObject is the source:
        return ReferenceEquals(testObject, sourceObject);
    }

    public Projectile(int baseDamage, int criticalDamage, int criticalChance)
    {
        this.baseDamage = baseDamage;
        this.criticalDamage = criticalDamage;
        this.criticalChance = criticalChance;
    }

    public int GetDamageAmount()
    {
        int chance = Random.Range(0, 100);

        if (chance < criticalChance)
        {
            return criticalDamage;
        }

        return baseDamage;
    }

    protected void OnCollisionEnter(Collision collision)
    {
        if (!IsSource(collision.gameObject))
        {
            Destroy(gameObject);
        }
    }
}
