using UnityEngine;

public class Health : MonoBehaviour
{
    public int maxHealth = 100;
    [HideInInspector] public int current;

    void Awake() => current = maxHealth;

    public void TakeDamage(int dmg)
    {
        current = Mathf.Max(current - dmg, 0);
        Debug.Log($"[{name}] took {dmg} dmg ➜ {current} / {maxHealth}");

        if (current == 0)
            Die();
    }

    void Die()
    {
        // spawn gore / sound / particles here
        Destroy(gameObject);                       // GameManager already handles win check
    }
}