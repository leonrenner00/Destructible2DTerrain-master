using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(SpriteRenderer))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 12f;
    public float maxMoveDistance = 28f;

    [Header("Ground Probe")]
    public Transform groundCheck;
    public LayerMask groundLayer;
    public float groundCheckRadius = 0.25f;

    [Header("Fall-Damage")]
    public float safeFallDistance = 5f;
    public float dmgPerUnit = 10f;

    [Header("Projectile Shooter")]
    public ProjectileShooter shooter;

    Rigidbody2D rb;
    Animator anim;
    SpriteRenderer sr;
    PlayerHealth health;

    bool lockMove, hasShot, jumpRequested;

    bool  isGrounded, wasGrounded;
    float fallStartY, distanceUsed, prevX;

    void Awake()
    {
        rb     = GetComponent<Rigidbody2D>();
        anim   = GetComponent<Animator>();
        sr     = GetComponent<SpriteRenderer>();
        health = GetComponent<PlayerHealth>();
    }

    public void BeginTurn()
    {
        rb.velocity   = Vector2.zero;
        lockMove      = false;
        hasShot       = false;
        distanceUsed  = 0f;

        wasGrounded = isGrounded = true;
        fallStartY  = rb.position.y;
        prevX       = rb.position.x;
    }

    void Update()
    {
        
        if (!enabled || (health && health.isDead)) return;

        isGrounded = Physics2D.OverlapCircle(groundCheck.position,
                                             groundCheckRadius, groundLayer);

        float hIn = Input.GetAxisRaw("Horizontal");
        bool anyInput = hIn != 0 ||
                        Input.GetKeyDown(KeyCode.Space) ||
                        Input.GetMouseButtonDown(0);

        if (anyInput && TurnManager.Instance)            
            TurnManager.Instance.HideBannerIfCurrent(this);
        
        if (hIn > 0) sr.flipX = false;
        if (hIn < 0) sr.flipX = true;

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            jumpRequested = true;

        if (!hasShot && shooter)
        {
            if (Input.GetMouseButtonDown(0)) shooter.BeginCharge();
            if (Input.GetMouseButton(0))     shooter.ContinueCharge(Time.deltaTime);
            if (Input.GetMouseButtonUp(0))
            {
                Vector2 aim = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                if (shooter.ReleaseAndShoot(aim)) hasShot = true;
            }
        }

        anim.SetFloat("xVelocity", Mathf.Abs(rb.velocity.x));
        anim.SetFloat("yVelocity", rb.velocity.y);
        anim.SetBool ("isJumping", !isGrounded);

        AttemptEndTurn();
    }

    void FixedUpdate()
    {
        if (!enabled || (health && health.isDead)) return;

        float hIn = lockMove ? 0f : Input.GetAxisRaw("Horizontal");

        Vector2 v = rb.velocity;
        v.x = hIn * moveSpeed;

        if (jumpRequested && isGrounded)
        {
            v.y          = jumpForce;
            jumpRequested = false;
        }
        rb.velocity = v;

        float step = Mathf.Abs(rb.position.x - prevX);
        prevX = rb.position.x;

        if (!lockMove)
        {
            distanceUsed += step;
            if (distanceUsed >= maxMoveDistance)
            {
                lockMove = true;
                rb.velocity = new Vector2(0f, rb.velocity.y);
            }
        }

        if (!wasGrounded && isGrounded)
        {
            float drop = fallStartY - rb.position.y;
            if (drop > safeFallDistance)
            {
                int dmg = Mathf.Max(1,
                          Mathf.CeilToInt((drop - safeFallDistance) * dmgPerUnit));
                GetComponent<PlayerHealth>()?.TakeDamage(dmg);
            }
        }
        if (wasGrounded && !isGrounded)
            fallStartY = rb.position.y;

        wasGrounded = isGrounded;
    }

    void AttemptEndTurn()
    {
        if (Input.GetButtonDown("Submit"))
        {
            FindObjectOfType<TurnManager>()?.EndTurn();
            return;
        }
        if (lockMove && hasShot)
            FindObjectOfType<TurnManager>()?.EndTurn();
    }

    public float RemainingMoveRatio =>
        Mathf.Clamp01(1f - distanceUsed / maxMoveDistance);
}
