using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ProjectileShooter : MonoBehaviour
{
    public GameObject projectilePrefab;
    public Transform shootPoint;
    public float maxChargeTime = 2f;
    public float maxShootForce = 20f;
    public Image chargeBarImage;

    private float chargeTime = 0f;
    private float lastChargeTime = 0f;
    private bool isCharging = false;

    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        if (chargeBarImage != null)
            chargeBarImage.enabled = false;
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            isCharging = true;
            chargeTime += Time.deltaTime;
            chargeTime = Mathf.Clamp(chargeTime, 0f, maxChargeTime);

            if (animator != null)
                animator.SetFloat("chargeBlend", chargeTime / maxChargeTime);

            if (chargeBarImage != null)
            {
                chargeBarImage.enabled = true;
                float t = chargeTime / maxChargeTime;
                chargeBarImage.fillAmount = t;
                chargeBarImage.color = t < 0.5f
                    ? Color.Lerp(Color.green, Color.red, t * 2)
                    : Color.Lerp(Color.red, Color.blue, (t - 0.5f) * 2);
            }
        }

        if (Input.GetMouseButtonUp(0) && isCharging)
        {
            if (animator != null)
            {
                animator.SetTrigger("throw"); // Trigger für ChargeEndThrow
                StartCoroutine(ResetChargeBlend());
            }

            lastChargeTime = chargeTime;  // Speichern vor Reset

            chargeTime = 0f;
            isCharging = false;

            if (chargeBarImage != null)
            {
                chargeBarImage.fillAmount = 0f;
                chargeBarImage.enabled = false;
            }
        }
    }

    public void ShootProjectile()
    {
        Debug.Log("ShootProjectile wurde durch Animation ausgelöst");

        GameObject projectile = Instantiate(projectilePrefab, shootPoint.position, Quaternion.identity);

        // Setze den Owner, damit das Projektil weiß, wer es abgeschossen hat
        Projectile projScript = projectile.GetComponent<Projectile>();
        if (projScript != null)
        {
            projScript.owner = this.gameObject;
        }

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = shootPoint.position.z;

        Vector2 shootDirection = (mousePos - shootPoint.position).normalized;
        float force = (lastChargeTime / maxChargeTime) * maxShootForce;

        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.AddForce(shootDirection * force, ForceMode2D.Impulse);
        }
    }

    IEnumerator ResetChargeBlend()
    {
        yield return new WaitForSeconds(0.3f); // Warte auf Animation (optional)
        if (animator != null)
            animator.SetFloat("chargeBlend", 0f);
    }
}
