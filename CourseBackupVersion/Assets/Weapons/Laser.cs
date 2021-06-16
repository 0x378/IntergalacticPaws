using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : Projectile
{
    private readonly float speed = 90f;
    private readonly float maxDistance = 512f;

    private Vector3 targetVelocity;

    [SerializeField] private GameObject smokeParticles;
    [SerializeField] private GameObject explosionParticles;

    public Laser() : base(1, 3, 20) { }

    public new void LinkToSource(GameObject source)
    {
        base.LinkToSource(source);

        targetVelocity = initialDirection * speed;
    }

    private void Update()
    {
        Vector3 position = transform.position;

        if (Vector3.Distance(position, initialPosition) > maxDistance)
        {
            Destroy(gameObject);
        }
        else
        {
            projectileBody.velocity = targetVelocity;
            projectileBody.transform.forward = initialDirection;
            //position += speed * Time.deltaTime * initialDirection;
            //transform.position = position;
        }
    }

    private new void OnCollisionEnter(Collision collision)
    {
        if (!IsSource(collision.gameObject))
        {
            GameObject animation;

            if (collision.gameObject.CompareTag("Terrain"))
            {
                animation = Instantiate(smokeParticles, transform.position, transform.rotation);
            }
            else
            {
                animation = Instantiate(explosionParticles, transform.position, transform.rotation);
            }

            animation.SetActive(true);
            animation.transform.localScale = new Vector3(0.01f, 0.01f, 0.06f);
            Destroy(gameObject);
        }
    }
}
