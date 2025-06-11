using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[System.Serializable]
public class ChargeEndSoundData
{
    public AudioClip[] sounds;
    public float animationDuration = 0.7f;
}

public class ProjectileShooter : MonoBehaviour
{
    [Header("Projectile")]
    public GameObject projectilePrefab;
    public Transform  shootPoint;
    public float      maxChargeTime = 2f;
    public float      maxShootForce = 20f;

    [Header("UI")]
    public Image chargeBarImage;

    [Header("Sounds")]
    public ChargeEndSoundData chargeEndSoundData;   // one-shot at release
    public float chargeEndVolume = 1f;

    public AudioClip  chargeLoopSound;              // looping while held
    [Range(0f,1f)] public float chargeLoopVolume = 1f;

    /* ─── private ─── */
    float chargeTime;
    bool  isCharging;

    AudioSource oneShotSource;      // for release SFX
    AudioSource loopSource;         // for charging loop
    Animator    anim;

    void Awake()
    {
        anim = GetComponent<Animator>();

        oneShotSource = gameObject.AddComponent<AudioSource>();
        oneShotSource.spatialBlend = 0f;

        loopSource = gameObject.AddComponent<AudioSource>();
        loopSource.loop         = true;
        loopSource.playOnAwake  = false;
        loopSource.spatialBlend = 0f;
        loopSource.volume       = chargeLoopVolume;

        if (chargeBarImage) chargeBarImage.enabled = false;
    }

    /* ── called by PlayerMovement on mouse DOWN ── */
    public void BeginCharge()
    {
        if (isCharging) return;

        isCharging = true;
        chargeTime = 0f;

        if (chargeLoopSound)
        {
            loopSource.clip = chargeLoopSound;
            loopSource.Play();
        }

        if (chargeBarImage) chargeBarImage.enabled = true;
    }

    /* ── called every frame while mouse HELD ── */
    public void ContinueCharge(float dt)
    {
        if (!isCharging) return;

        chargeTime  = Mathf.Clamp(chargeTime + dt, 0f, maxChargeTime);

        float t = chargeTime / maxChargeTime;

        // pitch & bar update
        loopSource.pitch = Mathf.Lerp(1f, 2f, t);   // tweak to taste
        if (chargeBarImage)
        {
            chargeBarImage.fillAmount = t;
            chargeBarImage.color      = t < 0.5f
                ? Color.Lerp(Color.green, Color.red,  t * 2f)
                : Color.Lerp(Color.red,   Color.blue, (t - 0.5f) * 2f);
        }
        if (anim) anim.SetFloat("chargeBlend", t);
    }

    /* ── called once on mouse UP ── */
    public bool ReleaseAndShoot(Vector2 aimWorldPos)
    {
        Debug.Log($"Shoot! chargeTime={chargeTime} / {maxChargeTime}");
        if (!isCharging) return false;
        isCharging = false;

        loopSource.Stop();                    // stop loop

        if (anim)
        {
            anim.SetTrigger("throw");
            StartCoroutine(ResetChargeBlend());
        }

        if (chargeEndSoundData != null &&
            chargeEndSoundData.sounds.Length > 0)
        {
            int i = Random.Range(0, chargeEndSoundData.sounds.Length);
            AudioClip clip = chargeEndSoundData.sounds[i];
            oneShotSource.pitch = clip.length > 0
                ? clip.length / chargeEndSoundData.animationDuration
                : 1f;
            oneShotSource.PlayOneShot(clip, chargeEndVolume);
            StartCoroutine(ResetPitch());
        }

        /* ---- spawn projectile ---- */
        if (projectilePrefab && shootPoint)
        {
            var proj = Instantiate(projectilePrefab, shootPoint.position, Quaternion.identity);
            var token = new GameObject("CamToken").transform;
            token.SetParent(proj.transform, false);     // follow projectile exactly
            token.gameObject.AddComponent<CameraReturnToken>();
            CameraFollowTarget.I.Follow(token);   
            CameraFollowTarget.I.Follow(proj.transform);
            proj.AddComponent<ProjectileCameraReturn>();
            if (proj.TryGetComponent<Projectile>(out var p))
                p.owner = gameObject;

            float force = (chargeTime / maxChargeTime) * maxShootForce;
            Vector2 dir = (aimWorldPos - (Vector2)shootPoint.position).normalized;

            if (proj.TryGetComponent<Rigidbody2D>(out var rb))
            {
                rb.velocity = Vector2.zero;
                rb.AddForce(dir * force, ForceMode2D.Impulse);
            }
        }

        /* ---- reset UI ---- */
        chargeTime = 0f;
        if (chargeBarImage)
        {
            chargeBarImage.enabled = false;
            chargeBarImage.fillAmount = 0f;
        }

        return true;
    }

    IEnumerator ResetChargeBlend()
    {
        yield return new WaitForSeconds(0.3f);   // match your anim
        if (anim) anim.SetFloat("chargeBlend", 0f);
    }

    IEnumerator ResetPitch()
    {
        yield return null;
        oneShotSource.pitch = 1f;
    }
}