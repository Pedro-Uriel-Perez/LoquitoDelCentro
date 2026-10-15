using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Colisionador de la botella. Se activa solo durante el golpe y daña una vez a cada enemigo por golpe.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class AttackHitbox : MonoBehaviour
{
    private Collider2D col;
    private int damage;
    private Vector2 knockback;
    private readonly HashSet<Health> alreadyHit = new HashSet<Health>();

    private void Awake()
    {
        col = GetComponent<Collider2D>();
        col.isTrigger = true;
        col.enabled = false;
    }

    /// <summary>Cambia la posición local y el tamaño del área de golpe (p. ej. más grande en el aire).</summary>
    public void SetShape(Vector2 localPosition, Vector2 size)
    {
        transform.localPosition = localPosition;
        if (col is BoxCollider2D box) box.size = size;
    }

    public void Activate(int hitDamage, Vector2 hitKnockback, int direction)
    {
        damage = hitDamage;
        knockback = new Vector2(hitKnockback.x * direction, hitKnockback.y);
        alreadyHit.Clear();
        col.enabled = true;
    }

    public void Deactivate()
    {
        col.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var health = other.GetComponentInParent<Health>();
        if (health == null || health.IsDead || !alreadyHit.Add(health)) return;
        health.TakeDamage(damage, knockback);
    }
}
