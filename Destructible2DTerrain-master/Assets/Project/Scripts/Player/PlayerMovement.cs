using System.Collections;
                using System.Collections.Generic;
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
                
                    [Header("Jump Sound Settings")]
                    public AudioClip[] jumpSounds;
                    public float jumpVolume = 1f;
                    [Range(0.5f, 2f)]
                    public float jumpPitch = 1f;
                
                    [Header("Fall-Damage")]
                    public float safeFallDistance = 5f;
                    public float dmgPerUnit = 10f;
                    
                    [Header("Projectile Shooter")]
                    public ProjectileShooter shooter;
                    
                    public Rigidbody2D rb;
                    public Animator anim;
                    private SpriteRenderer sr;
                    
                    public PlayerHealth health;
                    public AudioSource audioSource;
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
                    void Start()
                    {
                        audioSource = GetComponent<AudioSource>();
                        if (audioSource == null)
                            audioSource = gameObject.AddComponent<AudioSource>();
                    }
                
                    void Update()
                    {
                        Debug.Log(isGrounded);
                        if (health != null && health.isDead)
                        {
                            rb.velocity = Vector2.zero;
                            anim.SetFloat("xVelocity", 0f);
                            anim.SetFloat("yVelocity", 0f);
                            anim.SetBool("isJumping", false);
                            return;
                        }
                        
                        isGrounded = Physics2D.OverlapCircle(groundCheck.position,
                            groundCheckRadius, groundLayer);
                        Debug.Log(isGrounded);
                        float hIn = Input.GetAxisRaw("Horizontal");
                        bool anyInput = hIn != 0 ||
                                        Input.GetKeyDown(KeyCode.Space) ||
                                        Input.GetMouseButtonDown(0); 
                        if (anyInput && TurnManager.Instance)            
                            TurnManager.Instance.HideBannerIfCurrent(this);
                
                        if (hIn > 0) sr.flipX = false;
                        if (hIn < 0) sr.flipX = true;
                        
                        if (Input.GetKeyDown(KeyCode.Space) && isGrounded){
                            
                            jumpRequested = true;
                            anim.SetFloat("xVelocity", Mathf.Abs(rb.velocity.x));
                            anim.SetFloat("yVelocity", rb.velocity.y);
                            anim.SetBool("isJumping", !isGrounded);
                        }
                        
                        
                        if (!hasShot && shooter)                 // <— only reference check
                        {
                            if (Input.GetMouseButtonDown(0)) shooter.BeginCharge();
                            if (Input.GetMouseButton(0))     shooter.ContinueCharge(Time.deltaTime);
                            if (Input.GetMouseButtonUp(0))
                            {
                                Vector2 aim = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                                if (shooter.ReleaseAndShoot(aim)) hasShot = true;
                            }
                        }
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
                            PlayJumpSound();
                            jumpRequested = false;
                        }
                        Debug.Log(v.y);
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
                
                    // Diese Methode im Animation Event aufrufen
                    public void PlayJumpSound()
                    {
                        if (jumpSounds.Length == 0) return;
                        int index = Random.Range(0, jumpSounds.Length);
                        audioSource.pitch = jumpPitch;
                        audioSource.PlayOneShot(jumpSounds[index], jumpVolume);
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
                    
#if UNITY_EDITOR
                    void OnDrawGizmos()
                    {
                        Gizmos.color = Color.yellow;
                        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
                    }
#endif
                }

                