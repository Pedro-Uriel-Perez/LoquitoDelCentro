using UnityEngine;

/// <summary>
/// Daña al jugador cuando su cuerpo toca al enemigo.
/// </summary>
public class ContactDamage : MonoBehaviour
{
    [SerializeField] private int damage = 1;
    [SerializeField] private Collider2D body;
    [SerializeField] private LayerMask playerLayer;

    private Health health;

    private void Awake()
    {
        health = GetComponent<Health>();
        if (body == null) body = GetComponent<Collider2D>();
    }

    private void FixedUpdate()
    {
        if (health != null && health.IsDead) return;
        if (body == null || !body.enabled) return;

        Bounds b = body.bounds;
        Collider2D hit = Physics2D.OverlapBox(b.center, b.size, 0f, playerLayer);
        if (hit == null) return;

        var player = hit.GetComponentInParent<PlayerHealth>();
        if (player != null) player.TakeDamage(damage, transform.position);
    }
}
