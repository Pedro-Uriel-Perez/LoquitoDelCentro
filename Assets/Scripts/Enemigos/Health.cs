using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Vida de enemigos y jefes. Recibe daño con empuje, parpadea y muere.
/// </summary>
public class Health : MonoBehaviour
{
    [SerializeField] private int maxHealth = 3;
    [Tooltip("Tiempo que el enemigo queda aturdido (sin moverse por su cuenta) después de recibir un golpe.")]
    [SerializeField] private float hitStunTime = 0.3f;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color hitColor = new Color(1f, 0.4f, 0.4f);
    [SerializeField] private float destroyDelay = 0.6f;
    [Tooltip("Desactivar para objetos que deben quedarse al morir (p. ej. el jefe derrotado en el suelo).")]
    [SerializeField] private bool destroyOnDeath = true;

    private Rigidbody2D rb;
    private float stunnedUntil;

    public int Current { get; private set; }
    public int Max => maxHealth;
    public bool IsDead { get; private set; }
    public bool IsStunned => Time.time < stunnedUntil;

    public event Action<int> OnDamaged;
    public event Action OnDeath;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        Current = maxHealth;
    }

    private void Update()
    {
        // Enemigos que caen por un hoyo fuera del mapa: se eliminan.
        if (!IsDead && transform.position.y < -15f) Destroy(gameObject);
    }

    public void TakeDamage(int amount, Vector2 knockback)
    {
        if (IsDead) return;

        Current -= amount;
        stunnedUntil = Time.time + hitStunTime;
        if (rb != null && rb.bodyType == RigidbodyType2D.Dynamic) rb.linearVelocity = knockback;
        OnDamaged?.Invoke(amount);

        if (Current <= 0) Die();
        else StartCoroutine(Flash());
    }

    private IEnumerator Flash()
    {
        if (spriteRenderer == null) yield break;
        spriteRenderer.color = hitColor;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = Color.white;
    }

    private void Die()
    {
        IsDead = true;
        Current = 0;
        if (spriteRenderer != null) spriteRenderer.color = Color.white;
        OnDeath?.Invoke();
        foreach (var c in GetComponentsInChildren<Collider2D>()) c.enabled = false;
        if (rb != null) rb.bodyType = RigidbodyType2D.Kinematic;
        if (rb != null) rb.linearVelocity = Vector2.zero;
        if (destroyOnDeath) StartCoroutine(DeathRoutine());
    }

    /// <summary>Restaura la vida completa (p. ej. al reiniciar la pelea contra el jefe).</summary>
    public void ResetHealth()
    {
        StopAllCoroutines();
        IsDead = false;
        Current = maxHealth;
        if (spriteRenderer != null) { spriteRenderer.color = Color.white; spriteRenderer.enabled = true; }
        foreach (var c in GetComponentsInChildren<Collider2D>(true)) c.enabled = true;
    }

    private IEnumerator DeathRoutine()
    {
        float end = Time.time + destroyDelay;
        while (Time.time < end)
        {
            if (spriteRenderer != null) spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(0.06f);
        }
        Destroy(gameObject);
    }
}
