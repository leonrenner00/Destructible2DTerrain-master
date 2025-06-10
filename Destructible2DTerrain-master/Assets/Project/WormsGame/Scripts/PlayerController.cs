using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    /* ────────  INSPECTOR  ──────── */
    [Header("Movement")]
    public float moveSpeed       = 5f;
    public float jumpForce       = 12f;
    public float maxMoveDistance = 28f;

    [Header("Fall-Damage")]
    public float safeFallDistance = 5f;
    public float dmgPerUnit       = 10f;

    [Header("Projectile")]
    public GameObject projectilePrefab;
    public Transform  shootPoint;               // a child transform
    public float      maxChargeTime = 2f;
    public float      maxShootForce = 20f;
    public Image      chargeBarImage;           // UI fill-image

    [Header("Ground Probe")]
    public Transform groundProbe;               // child just below feet
    public LayerMask groundMask;                // tick only “Ground”
    public float     probeRadius = 0.05f;

    /* ────────  PRIVATE  ──────── */
    Rigidbody2D    rb;
    Animator       animator;
    SpriteRenderer spriteRenderer;

    bool   jumpPressed;
    bool   hasShot;
    bool   lockMovement;
    float  distanceMoved;
    float prevX;
                            
    bool   wasGrounded;
    float  fallStartY;

    // charge-shot state
    float chargeTime;
    bool  isCharging;

    /* ─────────── INITIALISE ─────────── */
    void Awake()
    {
        rb             = GetComponent<Rigidbody2D>();
        animator       = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (chargeBarImage) chargeBarImage.enabled = false;
    }

    /* ─────────── TURN ENTRY ─────────── */
    public void BeginTurn()
    {
        rb.velocity   = Vector2.zero;
        distanceMoved = 0f;
        lockMovement  = false;
        hasShot       = false;
        prevX = rb.position.x;
        fallStartY   = rb.position.y;
        wasGrounded  = true;

        chargeTime   = 0f;
        isCharging   = false;
        if (chargeBarImage) chargeBarImage.enabled = false;
    }

    /* ─────────── INPUT (frame) ─────────── */
    void Update()
    {
        if (!enabled) return;

        /* ───────── CHARGE-TO-SHOOT ───────── */
        if (!hasShot)                        // <──- NEW guard
        {
            if (Input.GetMouseButton(0))
            {
                // start / continue charging
                isCharging  = true;
                chargeTime += Time.deltaTime;
                chargeTime  = Mathf.Clamp(chargeTime, 0f, maxChargeTime);

                if (animator)
                    animator.SetFloat("chargeBlend", chargeTime / maxChargeTime);

                if (chargeBarImage)
                {
                    chargeBarImage.enabled    = true;
                    float t                  = chargeTime / maxChargeTime;
                    chargeBarImage.fillAmount = t;
                    chargeBarImage.color      = t < 0.5f
                        ? Color.Lerp(Color.green, Color.red,  t * 2f)
                        : Color.Lerp(Color.red,   Color.blue, (t - 0.5f) * 2f);
                }
            }

            if (Input.GetMouseButtonUp(0) && isCharging)
            {
                if (animator)
                {
                    animator.SetTrigger("throw");
                    StartCoroutine(ResetChargeBlend());
                }

                FireChargedShot();           // sets hasShot = true
            }
        }
        /* ─────────────────────────────────── */

        /* cache jump key, turn-end check, animator updates … */
        jumpPressed |= Input.GetKeyDown(KeyCode.Space);
        AttemptEndTurn();

        if (animator)
        {
            animator.SetFloat("xVelocity", Mathf.Abs(rb.velocity.x));
            animator.SetFloat("yVelocity", rb.velocity.y);
            animator.SetBool ("isJumping", !wasGrounded);
        }
    }

    /* ───────────  PHYSICS  ─────────── */
    void FixedUpdate()
    {
        if (!enabled) return;

        bool isGrounded = Physics2D.OverlapCircle(
                              groundProbe.position, probeRadius, groundMask);

        HandleMovement(isGrounded);
        HandleJump(isGrounded);
        HandleFallDamage(isGrounded);

        wasGrounded = isGrounded;
    }

    /* ─── horizontal move & distance cap ─── */
    void HandleMovement(bool isGrounded)
    {
        float h = Input.GetAxisRaw("Horizontal");
        if (lockMovement) h = 0f;

        rb.velocity = new Vector2(h * moveSpeed, rb.velocity.y);

        /* 1️⃣  after velocity applied, measure how far we *really* moved */
        float stepX = Mathf.Abs(rb.position.x - prevX);
        prevX = rb.position.x;

        /* 2️⃣  accumulate only while on the ground (optional; comment out the test
               if you *do* want air-borne distance to count) */
        if (isGrounded && !Mathf.Approximately(stepX, 0f))
            distanceMoved += stepX;

        /* 3️⃣  lock when the real total reaches the cap */
        if (distanceMoved >= maxMoveDistance && !lockMovement)
        {
            lockMovement    = true;
            rb.velocity     = new Vector2(0f, rb.velocity.y);
        }

        /* Sprite facing */
        if (spriteRenderer && h != 0f)
            spriteRenderer.flipX = h < 0f;
    }

    /* ─── jump ─── */
    void HandleJump(bool isGrounded)
    {
        if (jumpPressed && isGrounded)
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);

        jumpPressed = false;
    }

    /* ─── fall damage ─── */
    void HandleFallDamage(bool isGrounded)
    {
        // landed
        if (!wasGrounded && isGrounded)
        {
            float fall = fallStartY - rb.position.y;
            if (fall > safeFallDistance)
            {
                int dmg = Mathf.Max(
                    1, Mathf.CeilToInt((fall - safeFallDistance) * dmgPerUnit));

                GetComponent<Health>()?.TakeDamage(dmg);
                Debug.Log($"[{name}] fell {fall:F2}u → {dmg} dmg");
            }
        }

        // left ground
        if (wasGrounded && !isGrounded)
            fallStartY = rb.position.y;
    }

    /* ─── shoot after charging ─── */
    void FireChargedShot()
    {
        if (!projectilePrefab || !shootPoint) return;

        Vector3 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouse.z       = shootPoint.position.z;

        Vector2 dir   = (mouse - shootPoint.position).normalized;
        float   force = (chargeTime / maxChargeTime) * maxShootForce;

        GameObject proj = Instantiate(projectilePrefab,
                                      shootPoint.position, Quaternion.identity);

        if (proj.TryGetComponent<Rigidbody2D>(out var prb))
        {
            prb.velocity = Vector2.zero;
            prb.AddForce(dir * force, ForceMode2D.Impulse);
        }
        
        // reset UI & state
        chargeTime = 0f;
        isCharging = false;
        if (chargeBarImage)
        {
            chargeBarImage.enabled   = false;
            chargeBarImage.fillAmount = 0f;
        }

        hasShot = true;                 // for turn-end logic
        isCharging = false;
        chargeTime = 0f;
        if (chargeBarImage)
        {
            chargeBarImage.enabled    = false;
            chargeBarImage.fillAmount = 0f;
        }
    }

    /* ─── turn-end rules ─── */
    void AttemptEndTurn()
    {
        if (Input.GetButtonDown("Submit"))
        {
            FindObjectOfType<TurnManager>()?.EndTurn();
            return;
        }
        if (lockMovement && hasShot)
            FindObjectOfType<TurnManager>()?.EndTurn();
    }

    IEnumerator ResetChargeBlend()
    {
        yield return new WaitForSeconds(0.3f);        // matches ChargeEndThrow anim
        if (animator) animator.SetFloat("chargeBlend", 0f);
    }
   
    /// 1  = full allowance left, 0  = none.
    public float RemainingMoveRatio
    {
        get
        {
            return Mathf.Clamp01(1f - distanceMoved / maxMoveDistance);
        }
    }
    
    

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (groundProbe)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundProbe.position, probeRadius);
        }
    }
#endif
}