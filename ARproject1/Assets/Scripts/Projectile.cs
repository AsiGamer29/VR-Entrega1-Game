using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    [SerializeField] float lifetime = 5f;
    [SerializeField] bool destroyOnHit = false;

    void Start()
    {
        //if the game objet is not destroyed on hit, destroy it after lifetime
        Destroy(gameObject, lifetime);
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"Projectile impacted with {collision.gameObject.name}");

        // Destroy the projectile on impact
        if (destroyOnHit)
            Destroy(gameObject);
    }
}
