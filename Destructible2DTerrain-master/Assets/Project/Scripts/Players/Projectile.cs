using UnityEngine;

public class Projectile : MonoBehaviour
{
    public int damage = 10;
    [HideInInspector] public GameObject owner;

    public GameObject explosionPrefab; // <-- Explosion Prefab mit Animator

    private Collider2D ownerCollider;
    private Collider2D thisCollider;

    private bool canDamageOwner = false;
    private float ignoreOwnerTime = 0.5f; // Zeit in Sekunden, wie lange der Spieler ignoriert wird
    private float timer = 0f;

    private void Start()
    {
        if (owner != null)
        {
            ownerCollider = owner.GetComponent<Collider2D>();
            thisCollider = GetComponent<Collider2D>();

            if (ownerCollider != null && thisCollider != null)
            {
                Physics2D.IgnoreCollision(thisCollider, ownerCollider, true);
            }
        }
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (!canDamageOwner && timer > ignoreOwnerTime)
        {
            if (ownerCollider != null && thisCollider != null)
            {
                Physics2D.IgnoreCollision(thisCollider, ownerCollider, false);
                canDamageOwner = true;
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject == owner && !canDamageOwner)
            return;

        PlayerHealth player = collision.collider.GetComponent<PlayerHealth>();

        if (player != null)
        {
            player.TakeDamage(damage);
        }

        // Explosion erzeugen
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}