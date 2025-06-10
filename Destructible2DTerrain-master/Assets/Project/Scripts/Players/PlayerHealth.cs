using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    private Animator animator;
    public int maxHealth = 100;
    public int current;
    private Rigidbody2D rb;

    public float knockbackForce = 10f;
    public bool isDead { get; private set; } = false;

    public int CurrentHealth => currentHealth;

    public GameOverManager gameOverManager; // Referenz im Inspector setzen

    private bool canTakeDamage = true; // Steuerung, ob Schaden möglich ist

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
        if (isDead || !canTakeDamage) return; // Schaden ignorieren wenn tot oder deaktiviert

        current -= amount;
        Debug.Log("Player took damage! Current health: " + current);

        if (rb != null)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0f);
            rb.AddForce(Vector2.up * knockbackForce, ForceMode2D.Impulse);
        }

        if (current <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        canTakeDamage = false; // Schaden deaktivieren

        int deathLayerIndex = animator.GetLayerIndex("deathLayer");

        Debug.Log("Player died!");
        animator.SetLayerWeight(deathLayerIndex, 1f);
        animator.Play("Death", deathLayerIndex, 0f);
        gameOverManager.ShowGameOver();
        gameObject.SetActive(true);

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        if (gameOverManager != null)
        {
            gameOverManager.ShowGameOver();
        }
        else
        {
            Debug.LogWarning("GameOverManager ist nicht gesetzt!");
        }
    }
}