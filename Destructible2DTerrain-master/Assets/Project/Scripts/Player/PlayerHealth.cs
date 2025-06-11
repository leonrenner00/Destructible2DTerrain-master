using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class PlayerHealth : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 100;
    public float knockbackForce = 10f;

    [Header("Game-Over UI")]
    public GameOverManager gameOverManager;   // assign in Inspector

    /* ───────────────── */
    public bool isDead { get; private set; }
    public float  CurrentHealth => currentHealth;

    /* ───────────────── */
    float currentHealth;
    Rigidbody2D rb;
    Animator    anim;
    bool        canTakeDamage = true;

    void Awake()
    {
        rb   = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        currentHealth = maxHealth;
    }

    /* ───────── DAMAGE ───────── */
    public void TakeDamage(float amount)
    {
        if (isDead || !canTakeDamage) return;

        currentHealth = currentHealth - (amount*1.6f);
        Debug.Log($"Player took {amount} dmg → {currentHealth}/{maxHealth}");

        if (rb)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0f);
            rb.AddForce(Vector2.up * knockbackForce, ForceMode2D.Impulse);
        }

        if (currentHealth <= 0) Die();
    }

    /* ───────── DEATH ───────── */
    void Die()
    {
        isDead = true;
        canTakeDamage = false;

        /* 1️⃣  Play death animation if the layer exists */
        if (anim)
        {
            int layer = anim.GetLayerIndex("deathLayer");
            if (layer >= 0)
            {
                anim.SetLayerWeight(layer, 1f);
                anim.Play("Death", layer, 0f);
            }
            else
            {
                Debug.LogWarning("Animator has no layer called 'deathLayer'");
            }
        }

        /* 2️⃣  Freeze physics */
        if (rb)
        {
            rb.velocity     = Vector2.zero;
            rb.constraints  = RigidbodyConstraints2D.FreezeAll;
        }

        /* 3️⃣  Show game-over UI */
        if (gameOverManager)
            gameOverManager.ShowGameOver();
        else
            Debug.LogWarning("GameOverManager reference not set!");

        /* 4️⃣  Disable the GameObject so Update() etc. stop */
        gameObject.SetActive(false);
    }
}
