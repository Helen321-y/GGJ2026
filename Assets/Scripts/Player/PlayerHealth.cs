using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHp = 3;
    [SerializeField] private int hp = 3;

    [Header("Respawn")]
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private float deathAnimWait = 0.6f;
    [SerializeField] private float fadeOutTime = 0.25f;
    [SerializeField] private float fadeInTime = 0.25f;
    [SerializeField] private float blackScreenHoldTime = 2f;

    [Header("Respawn Invincibility")]
    [SerializeField] private float respawnInvincibleTime = 0.5f;
    private float invincibleEndTime;

    public bool isDead;

    private Coroutine deathRoutine;
    private PlayerController playerController;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        hp = Mathf.Clamp(hp, 0, maxHp);
    }

    private void OnValidate()
    {
        if (maxHp < 1) maxHp = 1;
        hp = Mathf.Clamp(hp, 0, maxHp);

        if (deathAnimWait < 0f) deathAnimWait = 0f;
        if (fadeOutTime < 0f) fadeOutTime = 0f;
        if (fadeInTime < 0f) fadeInTime = 0f;
        if (blackScreenHoldTime < 0f) blackScreenHoldTime = 0f;
        if (respawnInvincibleTime < 0f) respawnInvincibleTime = 0f;
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0) return;

        // Ignore damage while dead or invincible
        if (isDead) return;
        if (Time.time < invincibleEndTime) return;

        hp = Mathf.Max(0, hp - amount);

        if (hp == 0)
        {
            // Lock immediately to prevent multiple death routines in the same frame
            isDead = true;

            if (deathRoutine == null)
                deathRoutine = StartCoroutine(DeathRoutine());
        }
    }

    private IEnumerator DeathRoutine()
    {

        playerController?.OnDeathBegin();

        if (deathAnimWait > 0f)
            yield return new WaitForSeconds(deathAnimWait);

        var fade = ScreenFade.Instance;

        // Fade out (to black)
        if (fade != null && fadeOutTime > 0f)
            yield return fade.FadeOut(fadeOutTime);

        // Reset HP while hidden
        hp = maxHp;

        // Teleport + set player visuals (idle) while still locked
        Vector3 respawnPos = respawnPoint != null ? respawnPoint.position : transform.position;
        playerController?.BeginRespawnVisual(respawnPos);

    
        if (blackScreenHoldTime > 0f)
            yield return new WaitForSeconds(blackScreenHoldTime);

        // Fade in
        if (fade != null && fadeInTime > 0f)
            yield return fade.FadeIn(fadeInTime);

        // Enable control 
        playerController?.EndRespawnControl();

        invincibleEndTime = Time.time + respawnInvincibleTime;

        isDead = false;
        deathRoutine = null;
    }

    public int CurrentHp => hp;
    public int MaxHp => maxHp;
}
