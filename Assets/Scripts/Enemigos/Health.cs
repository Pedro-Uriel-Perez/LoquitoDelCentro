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
    [Tooltip("Si está activo, el enemigo reaparece en su posición inicial cuando el jugador es derrotado.")]
    [SerializeField] private bool respawnOnPlayerRespawn = false;

    private Rigidbody2D rb;
    private RigidbodyType2D initialBodyType;
    private Vector3 initialPosition;
    private Vector3 initialScale;
    private PlayerHealth subscribedPlayer;
    private bool shouldRespawn;
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
        if (rb != null) initialBodyType = rb.bodyType;
        initialPosition = transform.position;
        initialScale = transform.localScale;
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        Current = maxHealth;
    }

    private void Start()
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        shouldRespawn = respawnOnPlayerRespawn ||
                        (destroyOnDeath && GetComponent<SecretWall>() == null &&
                         (sceneName == "Mazmorra2" || sceneName == "Mazmorra3"));

        if (shouldRespawn)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
            {
                subscribedPlayer = p.GetComponent<PlayerHealth>();
                if (subscribedPlayer != null) subscribedPlayer.OnRespawn += RespawnEnemy;
            }
        }
    }

    private void OnDestroy()
    {
        if (subscribedPlayer != null) subscribedPlayer.OnRespawn -= RespawnEnemy;
    }

    private void Update()
    {
        // Enemigos que caen por un hoyo fuera del mapa: se eliminan (o se desactivan si deben regenerarse).
        if (!IsDead && transform.position.y < -15f)
        {
            if (shouldRespawn)
            {
                IsDead = true;
                gameObject.SetActive(false);
            }
            else Destroy(gameObject);
        }
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
        stunnedUntil = 0f;
        Current = maxHealth;
        if (spriteRenderer != null) { spriteRenderer.color = Color.white; spriteRenderer.enabled = true; }
        foreach (var c in GetComponentsInChildren<Collider2D>(true)) c.enabled = true;
    }

    private void RespawnEnemy()
    {
        gameObject.SetActive(true);
        ResetHealth();
        transform.position = initialPosition;
        transform.localScale = initialScale;
        if (rb != null)
        {
            rb.bodyType = initialBodyType;
            rb.position = initialPosition;
            rb.linearVelocity = Vector2.zero;
        }
    }

    private IEnumerator DeathRoutine()
    {
        float end = Time.time + destroyDelay;
        while (Time.time < end)
        {
            if (spriteRenderer != null) spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(0.06f);
        }
        if (shouldRespawn)
        {
            if (spriteRenderer != null) spriteRenderer.enabled = true;
            gameObject.SetActive(false);
        }
        else Destroy(gameObject);
    }
}
