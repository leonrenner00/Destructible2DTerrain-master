using UnityEngine;

public class ExplosionDamage : MonoBehaviour
{
    [Header("Explosion Settings")]
    [Tooltip("Der maximale Radius der Explosion")]
    [SerializeField] private float explosionRadius = 2f;

    [Tooltip("Der maximale Schaden im Zentrum der Explosion")]
    [SerializeField] private int maxDamage = 20;

    void Start()
    {
        // Finde alle Objekte im Explosionsradius
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

        foreach (Collider2D hit in hits)
        {
            PlayerHealth player = hit.GetComponent<PlayerHealth>();

            if (player != null)
            {
                float distance = Vector2.Distance(transform.position, player.transform.position);
                float damageMultiplier = Mathf.Clamp01(1 - (distance / explosionRadius));
                int finalDamage = Mathf.RoundToInt(maxDamage * damageMultiplier);

                player.TakeDamage(finalDamage);
            }
        }
    }

    // Optional: Zeigt den Explosionsradius im Editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}