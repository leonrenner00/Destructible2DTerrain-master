using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    private Animator animator;
    public int maxHealth = 100;
    public int current;
    private Rigidbody2D rb;

    public float knockbackForce = 10f; // Stärke des Sprungs / Rückstoß

    public bool isDead { get; private set; } = false; // <- öffentlich lesbar für andere Skripte

    void Awake() => current = maxHealth;
    void Start()
    {
        current = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        if (rb == null)
        {
            Debug.LogError("Kein Rigidbody2D auf dem Player!");
        }
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return; // <- Kein Schaden nach dem Tod

        current -= amount;
        Debug.Log("Player took damage! Current health: " + current);

        // Nach oben „springen“ (Knockback)
        if (rb != null)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0f); // Vertikale Geschwindigkeit resetten
            rb.AddForce(Vector2.up * knockbackForce, ForceMode2D.Impulse);
        }

        if (current <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true; // <- Tot markieren

        int deathLayerIndex = animator.GetLayerIndex("deathLayer");

        Debug.Log("Player died!");
        animator.SetLayerWeight(deathLayerIndex, 1f);
        animator.Play("Death", deathLayerIndex, 0f);

        // Optional: Physik einfrieren
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        // Hier kannst du Game Over, Respawn usw. einbauen
    }
}