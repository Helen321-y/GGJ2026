using System.Collections;
using UnityEngine;

public class BigEnemyController : MonoBehaviour
{
    private enum State
    {
        Patrol,
        Chase,
        Attack,
        Return
    }

    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform[] patrolPoints;
    private Animator animator;

    private SpriteRenderer spriteRenderer;

    [Header("Movement")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float chaseSpeed = 3.5f;
    [SerializeField] private float arriveDistX = 0.1f;

    [Header("Lose Target")]
    [SerializeField] private float loseTargetDelay = 1.5f;

    [Header("Attack")]
    [SerializeField] private float attackCooldown = 0.8f;
    [SerializeField] private int damage = 1;

    [Header("Attack Stop")]
    [SerializeField] private float attackLockTime = 0.25f;
    private float attackLockEndTime = -999f;
    private bool IsAttackLocked => Time.time < attackLockEndTime;

    [Header("Attack Windup")]
    [SerializeField] private float firstAttackWindup = 0.35f;
    private bool wasInAttackRange;

    [Header("Spore Attack")]
    [SerializeField] private float sporeIntervalMin = 3f;
    [SerializeField] private float sporeIntervalMax = 6f;
    [SerializeField] private float sporeWindup = 0.4f;
    [SerializeField] private float sporeRadius = 4f;
    [SerializeField] private int sporeDamage = 1;
    [SerializeField] private LayerMask playerLayer;

    private float nextSporeTime;
    private bool isCastingSpore;

    [Header("Patrol Wait")]
    [SerializeField] private float patrolWaitTime = 1.0f;
    private float patrolWaitEndTime = -999f;
    private bool IsPatrolWaiting => Time.time < patrolWaitEndTime;

    [Header("Hit Reaction")]
    [SerializeField] private float hitKnockbackForce = 6f;
    [SerializeField] private float hitKnockbackUp = 2f;
    [SerializeField] private float hitStunTime = 0.15f;

    private float hitStunEndTime = -999f;
    private bool IsHitStunned => Time.time < hitStunEndTime;

    [Header("Spore VFX")]
    [SerializeField] private GameObject sporeVfxPrefab;
    [SerializeField] private Vector3 sporeVfxOffset = Vector3.zero;


    // Animation state names
    private string currentAnimState;
    private const string ANIM_DEATH = "BigEnemy_Death";
    private const string ANIM_IDLE = "BigEnemy_Idle";
    private const string ANIM_WALK = "BigEnemy_Walk";
    private const string ANIM_SPORE = "BigEnemy_Spore";
    private const string ANIM_ATTACK = "BigEnemy_Attack";
    [Header("Optional")]
    [SerializeField] private bool faceMoveDirection = true;

    private State state = State.Patrol;
    private int patrolIndex;

    private Transform player;
    private bool playerDetected;
    private bool playerInAttackRange;
    private float lastSeenTime = -999f;
    private float nextAttackTime;

    private Vector2 spawnPos;

    private void Reset()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    private void Awake()
    {
        if (rb == null) 
            rb = GetComponent<Rigidbody2D>();

        if (spriteRenderer == null) 
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (animator == null)
            animator = GetComponent<Animator>();

        spawnPos = rb != null ? rb.position : (Vector2)transform.position;

        ScheduleNextSpore();
    }

    private void ScheduleNextSpore()
    {
        nextSporeTime = Time.time + Random.Range(sporeIntervalMin, sporeIntervalMax);
    }

    // Called by detection trigger
    public void SetPlayerDetected(bool detected, Transform playerTransform)
    {
        playerDetected = detected;

        if (detected)
        {
            player = playerTransform;
            lastSeenTime = Time.time;
        }
        else
        {
            // Keep lastSeenTime for lose-target delay
            lastSeenTime = Time.time;
        }
    }

    public void SetPlayerInAttackRange(bool inRange)
    {
        playerInAttackRange = inRange;
    }

    private void FixedUpdate()
    {
        var health = GetComponent<EnemyHealth>();
        if (health != null && health.IsDead)
        {
            rb.velocity = Vector2.zero;
            return;
        }


        // Target invalid 
        if (!IsTargetValid() && (playerDetected || player != null))
        {
            ClearAggro();
        }

        // Hit stun
        if (IsHitStunned)
        {
            if (rb != null) rb.velocity = new Vector2(0f, rb.velocity.y);
            if (playerDetected) lastSeenTime = Time.time;

            UpdateAnimation();
            return;
        }

        // Casting spore prep
        if (isCastingSpore)
        {
            if (rb != null) rb.velocity = Vector2.zero;
            
            UpdateAnimation();
            return;
        }

        // Attack lock
        if (IsAttackLocked)
        {
            if (rb != null) rb.velocity = new Vector2(0f, rb.velocity.y);

            UpdateAnimation();
            return;
        }

        UpdateState();

        switch (state)
        {
            case State.Patrol:
                PatrolTick();
                break;
            case State.Chase:
                ChaseTick();
                break;
            case State.Attack:
                AttackTick();
                break;
            case State.Return:
                ReturnTick();
                break;
        }

        TrySporeAttack();
        UpdateAnimation();
    }

    private void ChangeAnimationState(string newState)
{
    if (animator == null) return;
    if (currentAnimState == newState) return;

    animator.Play(newState);
    currentAnimState = newState;
}

    private void UpdateAnimation()
    {
        // If you later have EnemyHealth.IsDead, put it here:
        // if (enemyHealth != null && enemyHealth.IsDead) { ChangeAnimationState(ANIM_DEATH); return; }

        // Casting spore has highest priority (besides death)
        if (isCastingSpore)
        {
            ChangeAnimationState(ANIM_SPORE);
            return;
        }

        // If you want hitstun anim, put it here:
        if (IsHitStunned) 
        { 
            ChangeAnimationState(ANIM_IDLE); 
            return; 
        
        }

        // If you want attack anim to play when attacking/locked
        if (IsAttackLocked || state == State.Attack)
        {
            ChangeAnimationState(ANIM_ATTACK);
            return;
        }

        // Movement-based
        if (rb != null && Mathf.Abs(rb.velocity.x) > 0.01f)
            ChangeAnimationState(ANIM_WALK);
        else
        ChangeAnimationState(ANIM_IDLE);
}


    private void UpdateState()
    {
        if (playerDetected)
        {
            lastSeenTime = Time.time;

            state = playerInAttackRange ? State.Attack : State.Chase;

            if (playerInAttackRange && !wasInAttackRange)
            {
                nextAttackTime = Mathf.Max(nextAttackTime, Time.time + firstAttackWindup);
            }

            wasInAttackRange = playerInAttackRange;
            return;
        }

        // Not detected: after delay, go return
        if ((state == State.Chase || state == State.Attack) && Time.time - lastSeenTime > loseTargetDelay)
        {
            state = State.Return;
        }
    }

    private void PatrolTick()
    {

        if (rb == null || patrolPoints == null || patrolPoints.Length == 0)
        {
            if (rb != null) rb.velocity = new Vector2(0f, rb.velocity.y);
            return;
        }

        if (IsPatrolWaiting)
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
            return;
        }

        float targetX = patrolPoints[patrolIndex].position.x;
        MoveTowardX(targetX, patrolSpeed);

        if (Mathf.Abs(rb.position.x - targetX) <= arriveDistX)
        {
            rb.position = new Vector2(targetX, rb.position.y);
            rb.velocity = new Vector2(0f, rb.velocity.y);

            patrolWaitEndTime = Time.time + patrolWaitTime;
            patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
        }
    }

    private void ChaseTick()
    {

        if (rb == null)
            return;

        if (player == null)
        {
            state = State.Return;
            return;
        }

        MoveTowardX(player.position.x, chaseSpeed);
    }

    private void AttackTick()
    {

        if (rb == null)
            return;

        rb.velocity = new Vector2(0f, rb.velocity.y);

        if (!playerInAttackRange)
        {
            state = playerDetected ? State.Chase : State.Return;
            return;
        }

        if (Time.time < nextAttackTime)
            return;

        nextAttackTime = Time.time + attackCooldown;
        attackLockEndTime = Time.time + attackLockTime;

        ChangeAnimationState(ANIM_ATTACK);

        if (player == null)
            return;

        var health = player.GetComponent<PlayerHealth>();
        if (health != null) health.TakeDamage(damage);

        var playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            int direction = (transform.position.x > player.position.x) ? 1 : -1;
            playerController.Knockback(direction);
        }
    }

    private void ReturnTick()
    {

        if (rb == null)
            return;

        float targetX;
        bool hasPatrol = patrolPoints != null && patrolPoints.Length > 0;

        if (hasPatrol)
        {
            targetX = patrolPoints[patrolIndex].position.x;
        }
        else
        {
            targetX = spawnPos.x;
        }

        MoveTowardX(targetX, patrolSpeed);

        if (Mathf.Abs(rb.position.x - targetX) <= arriveDistX)
        {
            rb.position = new Vector2(targetX, rb.position.y);
            rb.velocity = new Vector2(0f, rb.velocity.y);

            patrolWaitEndTime = Time.time + patrolWaitTime;
            state = State.Patrol;
        }
    }

    private void MoveTowardX(float targetX, float speed)
    {
        if (rb == null) return;

        float delta = targetX - rb.position.x;
        float dir = Mathf.Sign(delta);

        float vx = Mathf.Abs(delta) < 0.01f ? 0f : dir * speed;
        rb.velocity = new Vector2(vx, rb.velocity.y);

        if (faceMoveDirection && Mathf.Abs(vx) > 0.01f)
        {
            Vector3 s = transform.localScale;
            s.x = Mathf.Sign(vx) * Mathf.Abs(s.x);
            transform.localScale = s;
        }
    }

    public void TakeKnockbackFromPlayer(Vector2 fromPos)
    {
        // Cancel actions immediately
        isCastingSpore = false;
        attackLockEndTime = -999f;

        hitStunEndTime = Time.time + hitStunTime;

        if (rb == null) return;

        float dir = Mathf.Sign(rb.position.x - fromPos.x);
        if (Mathf.Abs(dir) < 0.001f) dir = 1f;

        rb.velocity = new Vector2(dir * hitKnockbackForce, hitKnockbackUp);
    }

    private void TrySporeAttack()
    {
        if (isCastingSpore) return;
        if (IsAttackLocked) return;
        if (state == State.Attack) return;
        if (Time.time < nextSporeTime) return;

        if (!HasAggro())
        {
            ScheduleNextSpore();
            return;
        }

        // random skip
        if (Random.value > 0.8f)
        {
            ScheduleNextSpore();
            return;
        }

        StartCoroutine(SporeRoutine());
    }

    private IEnumerator SporeRoutine()
    {
        isCastingSpore = true;

        if (rb != null) rb.velocity = Vector2.zero;

        yield return new WaitForSeconds(sporeWindup);
        SpawnSporeVfx();

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, sporeRadius, playerLayer);
        foreach (var hit in hits)
        {
            var health = hit.GetComponent<PlayerHealth>();
            if (health != null) health.TakeDamage(sporeDamage);

            var playerController = hit.GetComponent<PlayerController>();
            if (playerController != null)
            {
                int dir = (transform.position.x > hit.transform.position.x) ? 1 : -1;
                playerController.Knockback(dir);
            }
        }

        ScheduleNextSpore();
        isCastingSpore = false;
    }

    private bool HasAggro()
    {
        return player != null && (playerDetected || Time.time - lastSeenTime <= loseTargetDelay);
    }

    private bool IsTargetValid()
    {
        if (player == null) return false;

        var ph = player.GetComponent<PlayerHealth>();
        if (ph != null && ph.isDead) return false;

        if (player.gameObject.layer == LayerMask.NameToLayer("PlayerDead")) return false;

        return true;
    }

    private void ClearAggro()
    {
        wasInAttackRange = false;

        playerDetected = false;
        playerInAttackRange = false;
        player = null;

        isCastingSpore = false;
        attackLockEndTime = -999f;

        state = State.Return;

        ScheduleNextSpore();
    }

    private void SpawnSporeVfx()
{
    if (sporeVfxPrefab == null) return;

    var go = Instantiate(sporeVfxPrefab, transform.position + sporeVfxOffset, Quaternion.identity);
    var auto = go.GetComponent<AutoDestroyParticles>();
    if (auto != null) auto.Play();
    else
    {
        // fallback: if prefab forgot the script, still try play particles
        foreach (var ps in go.GetComponentsInChildren<ParticleSystem>(true))
            ps.Play(true);
        Destroy(go, 2f);
    }
}


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, sporeRadius);
    }
}
