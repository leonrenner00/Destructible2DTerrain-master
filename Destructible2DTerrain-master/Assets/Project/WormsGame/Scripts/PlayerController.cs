using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Fall-Damage")]
    public float safeFallDistance = 5f;   // no damage if fall shorter than this
    public float   dmgPerUnit      = 10f;      // 10 HP per extra unit fallen
    float fallStartY;
    bool  wasGroundedLastFrame;
    
     [Header("Movement")]
    public float moveSpeed        = 4f;   // walk speed
    public float jumpForce        = 8f;   // impulse for Jump
    public float maxMoveDistance  = 28f;   // world-units allowed per turn

    [Header("Shooting")]
    public GameObject projectilePrefab;
    public Transform  firePoint;
    public float      launchForce = 12f;
    bool  hasShot;  

    // ────────────────────────────────────────────────────────────────
    private Rigidbody2D rb;
    private bool   isGrounded;
    private Vector2 turnStartPos;          // position when the turn begins
    private float   distanceMovedThisTurn; // accumulator
    private bool    lockMovement;          // TRUE after limit reached
    
    void Awake() => rb = GetComponent<Rigidbody2D>();

    // ── ①  CALLED FROM GameManager at the very start of this soldier’s turn
    public void BeginTurn()
    {
        turnStartPos          = rb.position;   // reference point
        distanceMovedThisTurn = 0f;            // reset counter
        lockMovement          = false;         // re-enable walking
        hasShot        = false;
        // Kill residual motion so the soldier starts “still”
        rb.velocity           = Vector2.zero;

        // OPTIONAL: Visual / UI feedback
        // TurnCircle.SetActive(true);
        // MovementBar.SetValue(1f);
    }

    void Update()
    {
        // Skip everything if we’re not enabled by GameManager
        if (!enabled) return;

        HandleMovement();
        HandleJump();
        //HandleShoot();
        AttemptEndTurn();
       
    }

    void FixedUpdate()
    {
        /* ─── FALL-DISTANCE DAMAGE ─── */
        bool nowGrounded = isGrounded;          // value was set in collision callbacks

        if (!wasGroundedLastFrame && nowGrounded)
        {
            float fall = fallStartY - rb.position.y;  // positive value if we dropped
            if (fall > safeFallDistance)
            {
                int dmg = Mathf.RoundToInt((fall - safeFallDistance) * dmgPerUnit);
                GetComponent<Health>().TakeDamage(dmg);
                Debug.Log($"Fell {fall} units, took {dmg} damage.");
            }
            Debug.Log($"Fell {fall} units");
        }

        if (wasGroundedLastFrame && !nowGrounded)
        {
            // just left the ground – remember where we started to fall
            fallStartY = rb.position.y;
        }

        wasGroundedLastFrame = nowGrounded;
    }

    // ── MOVEMENT with per-turn distance cap
    private void HandleMovement()
    {
        if (lockMovement) return;

        float h = Input.GetAxisRaw("Horizontal");
        if (Mathf.Approximately(h, 0f)) return;

        // compute intended step this frame
        Vector2 step = new Vector2(h * moveSpeed * Time.deltaTime, 0f);
        float   nextTotal = distanceMovedThisTurn + Mathf.Abs(step.x);

        if (nextTotal <= maxMoveDistance)
        {
            rb.MovePosition(rb.position + step);
            distanceMovedThisTurn = nextTotal;
        }
        else
        {
            // distance limit hit – clamp the last bit, then lock
            float remaining = maxMoveDistance - distanceMovedThisTurn;
            rb.MovePosition(rb.position + new Vector2(Mathf.Sign(h) * remaining, 0f));
            distanceMovedThisTurn = maxMoveDistance;
            lockMovement = true;
        }

        // flip sprite to face direction
        if (h != 0) transform.localScale = new Vector3(Mathf.Sign(h), 1f, 1f);
        AttemptEndTurn();
    }

    private void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

   /* private void HandleShoot()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            var proj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
            proj.GetComponent<Rigidbody2D>()
                .AddForce(firePoint.right * launchForce, ForceMode2D.Impulse);

            // tell GameManager we’re done
            FindObjectOfType<TurnManager>().EndTurn();
        }
    }
    */

    // ── Ground checks
    void OnCollisionEnter2D(Collision2D c)
    {
        if (c.collider.CompareTag("Ground")) isGrounded = true;
    }
    void OnCollisionExit2D(Collision2D c)
    {
        if (c.collider.CompareTag("Ground")) isGrounded = false;
    }
    
    void AttemptEndTurn()
    {
        // explicit key (default: Return / “Submit”) ends immediately
        if (Input.GetButtonDown("Submit"))
        {
            FindObjectOfType<TurnManager>().EndTurn();
            return;
        }

        // auto-end: walked the whole distance *and* already shot
        if (lockMovement && hasShot)
        {
            FindObjectOfType<TurnManager>().EndTurn();
        }
    }

}