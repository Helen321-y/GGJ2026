using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Components")]
    private Rigidbody2D rb;
    private Animator animator;
    private CapsuleCollider2D bodyCollider;

    [Header("Ground Check")]
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private float groundCheckDistance = 0.2f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 7f;
    private float moveInput;
    private bool facingRight = true;
    private bool canMove = true;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 32f;
    private bool canDoubleJump;
    private bool hasDoubleJumpAbility;

    [Header("Slow Fall")]
    [SerializeField] private float slowFallSpeed = -2f; // usually a small negative value
    private bool hasSlowFallAbility;
    private bool slowFallEquipped;

    [Header("Knockback")]
    [SerializeField] private float knockbackDuration = 0.2f;
    [SerializeField] private Vector2 knockbackVelocity = new Vector2(8f, 10f);
    private bool isKnockbackActive;
    private float knockbackEndTime;

    [Header("Attack")]
    [SerializeField] private KeyCode attackKey = KeyCode.J;
    [SerializeField] private Collider2D attackCollider;
    [SerializeField] private AttackHitbox attackHitbox;
    [SerializeField] private float attackCooldown = 0.25f;
    [SerializeField] private Animator slashAnimator;
    [SerializeField] private float hitboxDelay = 0.05f;
    [SerializeField] private float hitboxActiveTime = 0.12f;
    private float nextAttackTime = -999f;
    private bool isAttacking;

    [Header("Hit Flash")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float hitFlashTime = 0.08f;

    private Coroutine hitFlashRoutine;
    private Color defaultColor = Color.white;

    [Header("State")]
    private bool isGrounded;
    private bool isDead;
    private bool isRespawning;
    private int defaultLayer;

    // Animation state names
    private string currentAnimState;
    private const string ANIM_DEATH = "Player_Death";
    private const string ANIM_IDLE = "Player_Idle";
    private const string ANIM_WALK = "Player_Walk";
    private const string ANIM_JUMP = "Player_Jump";
    private const string ANIM_FALL = "Player_Fall";
    private const string ANIM_KNOCKBACK = "Player_Knockback";
    private const string ANIM_ATTACK = "Player_Attack";
    private const string SLASH_ANIM = "Slash";

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        bodyCollider = GetComponent<CapsuleCollider2D>();
        defaultLayer = gameObject.layer;

        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null) defaultColor = spriteRenderer.color;

        if (attackCollider != null)
            attackCollider.enabled = false;
    }

    private void Update()
    {
        if (isDead) return;

        UpdateKnockbackState();
        if (isKnockbackActive)
        {
            ApplySlowFall();
            return;
        }

        ReadInput();
        UpdateFacingDirection();

        if (!isRespawning && Input.GetKeyDown(attackKey))
            TryAttack();

        UpdateLocomotionAnimation();

        if (canMove)
            ApplyHorizontalMovement();

        // Reset jump availability on ground
        if (isGrounded)
            canDoubleJump = hasDoubleJumpAbility;

        ApplySlowFall();
    }

    private void FixedUpdate()
    {
        UpdateGrounded();
    }

    private void ReadInput()
    {
        if (isRespawning) return;

        moveInput = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump"))
            TryJump();
    }

    private void UpdateFacingDirection()
    {
        if (!canMove) return;

        if (facingRight && moveInput < 0f)
            Flip();
        else if (!facingRight && moveInput > 0f)
            Flip();
    }

    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1f;
        transform.localScale = scale;
    }

    private void ApplyHorizontalMovement()
    {
        float vx = moveSpeed * moveInput;
        rb.velocity = new Vector2(vx, rb.velocity.y);
    }

    private void TryJump()
    {
        if (isKnockbackActive) return;

        if (isGrounded)
        {
            PerformJump();
        }
        else if (hasDoubleJumpAbility && canDoubleJump)
        {
            canDoubleJump = false;
            PerformJump();

            // If you want to disable horizontal control during the double jump impulse:
            // canMove = false;
        }
    }

    private void PerformJump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        isGrounded = false;
    }

    private void ApplySlowFall()
    {
        if (!hasSlowFallAbility || !slowFallEquipped) return;
        if (isGrounded) return;

        // Clamp falling speed 
        if (rb.velocity.y < slowFallSpeed)
            rb.velocity = new Vector2(rb.velocity.x, slowFallSpeed);
    }

    private void UpdateLocomotionAnimation()
    {
        if (isAttacking) return;
        if (isKnockbackActive) return;

        if (!isGrounded)
        {
            PlayAnim(rb.velocity.y > 0.01f ? ANIM_JUMP : ANIM_FALL);
            return;
        }

        PlayAnim(Mathf.Abs(rb.velocity.x) > 0.01f ? ANIM_WALK : ANIM_IDLE);
    }

    private void PlayAnim(string stateName)
    {
        if (currentAnimState == stateName) return;
        animator.Play(stateName);
        currentAnimState = stateName;
    }

    public void Knockback(int direction)
    {
        if (isDead) return;
        
        FlashHit(); 

        // Cancel attack if needed 
        isAttacking = false;
        if (attackCollider != null) attackCollider.enabled = false;

        isKnockbackActive = true;
        knockbackEndTime = Time.time + knockbackDuration;

        canMove = false;
        canDoubleJump = false;

        rb.velocity = new Vector2(knockbackVelocity.x * -direction, knockbackVelocity.y);

        PlayAnim(ANIM_KNOCKBACK);
        CameraShake.Instance?.Shake(5f, 0.15f, 2f);
    }

    private void UpdateKnockbackState()
    {
        if (!isKnockbackActive) return;

        // Force knockback animation while active
        PlayAnim(ANIM_KNOCKBACK);

        if (Time.time < knockbackEndTime) return;

        isKnockbackActive = false;

        // Clear a bit of horizontal velocity on recovery (optional)
        rb.velocity = new Vector2(0f, rb.velocity.y);
        canMove = true;

        // Snap to a reasonable animation on exit
        if (!isGrounded)
            PlayAnim(rb.velocity.y > 0.01f ? ANIM_JUMP : ANIM_FALL);
        else
            PlayAnim(Mathf.Abs(rb.velocity.x) > 0.01f ? ANIM_WALK : ANIM_IDLE);
    }

    private void FlashHit()
    {
        if (spriteRenderer == null) return;

        if (hitFlashRoutine != null)
            StopCoroutine(hitFlashRoutine);

        hitFlashRoutine = StartCoroutine(HitFlashCoroutine());
    }

    private IEnumerator HitFlashCoroutine()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(hitFlashTime);
        spriteRenderer.color = defaultColor;
        hitFlashRoutine = null;
    }


    private void TryAttack()
    {
        if (isAttacking) return;
        if (isKnockbackActive) return;
        if (Time.time < nextAttackTime) return;

        nextAttackTime = Time.time + attackCooldown;
        StartCoroutine(AttackRoutine());
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;

        // Lock movement during attack
        canMove = false;
        moveInput = 0f;
        rb.velocity = new Vector2(0f, rb.velocity.y);

        PlayAnim(ANIM_ATTACK);

        if (slashAnimator != null)
            slashAnimator.Play(SLASH_ANIM, 0, 0f);

        yield return new WaitForSeconds(hitboxDelay);

        // Do not open hitbox if death/knockback/respawn
        if (isDead || isKnockbackActive || isRespawning)
        {
            if (attackCollider != null) attackCollider.enabled = false;
            isAttacking = false;
            yield break;
        }

        if (attackHitbox != null) attackHitbox.BeginSwing();
        if (attackCollider != null) attackCollider.enabled = true;

        yield return new WaitForSeconds(hitboxActiveTime);

        if (attackCollider != null) attackCollider.enabled = false;

        // Only restore movement if not interrupted 
        if (!isDead && !isKnockbackActive && !isRespawning)
            canMove = true;

        isAttacking = false;
    }

    // ===== Abilities =====
    public void EnableDoubleJump()
    {
        hasDoubleJumpAbility = true;
        canDoubleJump = true;
    }

    public void UnlockSlowFall()
    {
        hasSlowFallAbility = true;
        slowFallEquipped = true;
    }

    // ===== Death / Respawn =====
    public void OnDeathBegin()
    {
        isDead = true;
        isRespawning = false;
        isKnockbackActive = false;
        isAttacking = false;

        gameObject.layer = LayerMask.NameToLayer("PlayerDead");

        canMove = false;
        moveInput = 0f;

        if (attackCollider != null) attackCollider.enabled = false;

        rb.velocity = Vector2.zero;
        rb.simulated = false;
        bodyCollider.enabled = false;

        PlayAnim(ANIM_DEATH);
    }

    public void RespawnTeleportOnly(Vector3 pos)
    {
        transform.position = pos;
        rb.velocity = Vector2.zero;

        if (hasDoubleJumpAbility)
            canDoubleJump = true;
    }

    //full respawn in one call 
    public void FinishRespawnEnableControl()
    {
        gameObject.layer = defaultLayer;

        rb.simulated = true;
        bodyCollider.enabled = true;

        canMove = true;
        isDead = false;
        isRespawning = false;

        PlayAnim(ANIM_IDLE);
    }

    // respawn visuals first (during fade out)
    public void BeginRespawnVisual(Vector3 pos)
    {
        isDead = false;
        isRespawning = true;
        isKnockbackActive = false;
        isAttacking = false;

        transform.position = pos;
        rb.velocity = Vector2.zero;

        gameObject.layer = defaultLayer;
        rb.simulated = true;
        bodyCollider.enabled = true;

        canMove = false;
        moveInput = 0f;

        animator.Play(ANIM_IDLE, 0, 0f);
        currentAnimState = ANIM_IDLE;

        if (attackCollider != null) attackCollider.enabled = false;

        // Refill double jump if the ability is owned
        if (hasDoubleJumpAbility)
            canDoubleJump = true;
    }

    //enable control after fade-in completes
    public void EndRespawnControl()
    {
        isRespawning = false;
        canMove = true;
    }

    private void UpdateGrounded()
    {
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, whatIsGround);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position,
            new Vector3(transform.position.x, transform.position.y - groundCheckDistance));
    }
}
