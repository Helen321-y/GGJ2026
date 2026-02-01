using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int hitsToDie = 5;
    private int currentHits;

    [Header("Death")]
    [SerializeField] private float deathAnimDuration = 0.6f;
    [SerializeField] private float fadeOutDuration = 0.5f;

    public bool IsDead { get; private set; }

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Collider2D[] colliders;
    private Rigidbody2D rb;

    private static readonly string ANIM_DEATH = "BigEnemy_Death";

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        colliders = GetComponentsInChildren<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
    }

    public void TakeHit(int amount = 1)
    {
        if (IsDead) return;

        currentHits += amount;

        if (currentHits >= hitsToDie)
        {
            IsDead = true;
            StartCoroutine(DeathRoutine());
        }
    }

    private IEnumerator DeathRoutine()
    {
        // 1. Stop physics & collisions
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.simulated = false;
        }

        foreach (var col in colliders)
            col.enabled = false;

        // 2. Play death animation
        if (animator != null)
            animator.Play(ANIM_DEATH, 0, 0f);

        yield return new WaitForSeconds(deathAnimDuration);

        // 3. Fade out sprite
        if (spriteRenderer != null)
        {
            Color c = spriteRenderer.color;
            float t = 0f;

            while (t < fadeOutDuration)
            {
                t += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, t / fadeOutDuration);
                spriteRenderer.color = new Color(c.r, c.g, c.b, alpha);
                yield return null;
            }
        }

        // 4. Destroy
        Destroy(gameObject);
    }
}
