using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Vida del Loquito en panalitos (5). Recibe daño, aplica empuje e invulnerabilidad,
/// y al llegar a cero reaparece en el último checkpoint con la vida completa.
/// </summary>
[RequireComponent(typeof(PlayerController))]
public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHearts = 5;
    [SerializeField] private float invulnerableTime = 1f;
    [SerializeField] private Vector2 hurtKnockback = new Vector2(6f, 8f);
    [SerializeField] private float hurtControlLock = 0.3f;
    [SerializeField] private float respawnDelay = 1.2f;
    [Tooltip("Si el jugador cae por debajo de esta altura, muere.")]
    [SerializeField] private float killHeight = -20f;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;
    [SerializeField] private string hurtState = "Herido";

    private PlayerController controller;
    private PlayerCombat combat;
    private Vector3 checkpoint;
    private float invulnerableUntil;

    public int Hearts { get; private set; }
    public int MaxHearts => maxHearts;
    public bool IsDead { get; private set; }

    public event Action OnHealthChanged;
    public event Action OnDeath;
    public event Action OnRespawn;

    private void Awake()
    {
        controller = GetComponent<PlayerController>();
        combat = GetComponent<PlayerCombat>();
        Hearts = maxHearts;
        checkpoint = transform.position;
    }

    private void Update()
    {
        if (!IsDead && transform.position.y < killHeight)
        {
            Hearts = 0;
            OnHealthChanged?.Invoke();
            StartCoroutine(DieAndRespawn());
        }
    }

    public void SetCheckpoint(Vector3 position) => checkpoint = position;

    public void Heal(int amount)
    {
        if (IsDead) return;
        Hearts = Mathf.Min(maxHearts, Hearts + amount);
        OnHealthChanged?.Invoke();
    }

    /// <summary>Devuelve true si el daño se aplicó (no estaba invulnerable ni lo bloqueó).</summary>
    public bool TakeDamage(int amount, Vector2 sourcePosition)
    {
        if (IsDead || Time.time < invulnerableUntil) return false;
        if (combat != null && combat.TryBlock(sourcePosition)) return false;

        Hearts = Mathf.Max(0, Hearts - amount);
        OnHealthChanged?.Invoke();
        if (combat != null) combat.CancelAttack();

        int away = sourcePosition.x > transform.position.x ? -1 : 1;
        controller.ApplyKnockback(new Vector2(away * hurtKnockback.x, hurtKnockback.y), hurtControlLock);
        if (animator != null) animator.Play(hurtState, 0, 0f);

        if (Hearts <= 0) StartCoroutine(DieAndRespawn());
        else
        {
            AudioManager.Instance.PlaySfx("Vida_Perdida");
            StartCoroutine(Invulnerability(invulnerableTime));
        }
        return true;
    }

    private IEnumerator Invulnerability(float duration)
    {
        invulnerableUntil = Time.time + duration;
        while (Time.time < invulnerableUntil)
        {
            if (spriteRenderer != null) spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(0.08f);
        }
        if (spriteRenderer != null) spriteRenderer.enabled = true;
    }

    private IEnumerator DieAndRespawn()
    {
        IsDead = true;
        controller.InputEnabled = false;
        if (combat != null) combat.CancelAttack();
        AudioManager.Instance.PlaySfx("Derrota");   // perdió todas las vidas: regresa al checkpoint
        OnDeath?.Invoke();

        yield return new WaitForSeconds(respawnDelay);

        controller.Teleport(checkpoint);
        Hearts = maxHearts;
        IsDead = false;
        controller.InputEnabled = true;
        OnHealthChanged?.Invoke();
        OnRespawn?.Invoke();
        StartCoroutine(Invulnerability(invulnerableTime));
    }
}
