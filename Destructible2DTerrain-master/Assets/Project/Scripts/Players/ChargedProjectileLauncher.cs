using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ProjectileShooter : MonoBehaviour
{
    [Header("Setup")]
    public GameObject projectilePrefab;
    public Transform  shootPoint;
    public Image      chargeBarImage;

    [Header("Tuning")]
    public float maxChargeTime  = 2f;
    public float maxShootForce  = 20f;

    /* ───────── private ───────── */
    float chargeTime;           // grows while holding fire
    bool  isCharging;
    Animator anim;

    void Awake()
    {
        anim = GetComponent<Animator>();
        if (chargeBarImage) chargeBarImage.enabled = false;
    }

    /* ─────────────────────────────────────────────
     *  Call these three methods from PlayerController
     *───────────────────────────────────────────── */

    public void BeginCharge()
    {
        if (isCharging) return;             // already charging
        isCharging = true;
        chargeTime = 0f;

        if (chargeBarImage) chargeBarImage.enabled = true;
    }

    public void ContinueCharge(float dt)
    {
        if (!isCharging) return;

        chargeTime  = Mathf.Clamp(chargeTime + dt, 0f, maxChargeTime);

        // Animator blend
        if (anim) anim.SetFloat("chargeBlend", chargeTime / maxChargeTime);

        // UI bar
        if (chargeBarImage)
        {
            float t                = chargeTime / maxChargeTime;
            chargeBarImage.fillAmount = t;
            chargeBarImage.color      = t < 0.5f
                ? Color.Lerp(Color.green, Color.red,  t * 2f)
                : Color.Lerp(Color.red,   Color.blue, (t - 0.5f) * 2f);
        }
    }

    /// <summary>
    ///  Releases the charge, spawns projectile, returns true if shot.
    /// </summary>
    public bool ReleaseAndShoot(Vector2 aimWorldPos)
    {
        if (!isCharging) return false;      // nothing to release
        isCharging = false;

        // play throw anim
        if (anim)
        {
            anim.SetTrigger("throw");
            StartCoroutine(ResetChargeBlend());
        }

        // Spawn projectile
        if (projectilePrefab && shootPoint)
        {
            GameObject proj = Instantiate(projectilePrefab, shootPoint.position, Quaternion.identity);

            // If your projectile has a script that tracks owner:
            if (proj.TryGetComponent<Projectile>(out var pScript))
                pScript.owner = gameObject;

            Vector2 dir   = (aimWorldPos - (Vector2)shootPoint.position).normalized;
            float   force = (chargeTime / maxChargeTime) * maxShootForce;

            if (proj.TryGetComponent<Rigidbody2D>(out var rb))
            {
                rb.velocity = Vector2.zero;
                rb.AddForce(dir * force, ForceMode2D.Impulse);
            }
        }

        // reset UI
        chargeTime = 0f;
        if (chargeBarImage)
        {
            chargeBarImage.enabled    = false;
            chargeBarImage.fillAmount = 0f;
        }

        return true;                        // a shot was fired
    }

    /* ───────── helper ───────── */
    IEnumerator ResetChargeBlend()
    {
        yield return new WaitForSeconds(0.3f);   // matches your anim
        if (anim) anim.SetFloat("chargeBlend", 0f);
    }
}
