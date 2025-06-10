using System.Collections;
                using System.Collections.Generic;
                using UnityEngine;
                
                public class PlayerMovement : MonoBehaviour
                {
                    public float moveSpeed = 5f;
                    public float jumpForce = 12f;
                    public Transform groundCheck;
                    public LayerMask groundLayer;
                    public float groundCheckRadius = 0.2f;
                
                    [Header("Jump Sound Settings")]
                    public AudioClip[] jumpSounds;
                    public float jumpVolume = 1f;
                    [Range(0.5f, 2f)]
                    public float jumpPitch = 1f;
                
                    private Rigidbody2D rb;
                    private Animator animator;
                    private SpriteRenderer spriteRenderer;
                    private bool isGrounded;
                    private PlayerHealth playerHealth;
                    private AudioSource audioSource;
                    private bool jumpRequested = false;
                
                    void Start()
                    {
                        rb = GetComponent<Rigidbody2D>();
                        animator = GetComponent<Animator>();
                        spriteRenderer = GetComponent<SpriteRenderer>();
                        playerHealth = GetComponent<PlayerHealth>();
                
                        audioSource = GetComponent<AudioSource>();
                        if (audioSource == null)
                            audioSource = gameObject.AddComponent<AudioSource>();
                    }
                
                    void Update()
                    {
                        if (playerHealth != null && playerHealth.isDead)
                        {
                            rb.velocity = Vector2.zero;
                            animator.SetFloat("xVelocity", 0f);
                            animator.SetFloat("yVelocity", 0f);
                            animator.SetBool("isJumping", false);
                            return;
                        }
                
                        float moveInput = Input.GetAxisRaw("Horizontal");
                        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
                
                        if (moveInput > 0)
                            spriteRenderer.flipX = false;
                        else if (moveInput < 0)
                            spriteRenderer.flipX = true;
                
                        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
                
                        // Nur springen, wenn am Boden und kein Sprung angefordert wurde
                        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !jumpRequested)
                        {
                            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                            jumpRequested = true;
                        }
                
                        // Rücksetzen, wenn wieder am Boden
                        if (isGrounded)
                            jumpRequested = false;
                
                        animator.SetFloat("xVelocity", Mathf.Abs(rb.velocity.x));
                        animator.SetFloat("yVelocity", rb.velocity.y);
                        animator.SetBool("isJumping", !isGrounded);
                    }
                
                    // Diese Methode im Animation Event aufrufen
                    public void PlayJumpSound()
                    {
                        if (jumpSounds.Length == 0) return;
                        int index = Random.Range(0, jumpSounds.Length);
                        audioSource.pitch = jumpPitch;
                        audioSource.PlayOneShot(jumpSounds[index], jumpVolume);
                    }
                }