using UnityEngine;

/// <summary>
/// Patrulla simple: camina de un lado a otro y se da la vuelta en el límite, ante una pared o un borde.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyPatrol : MonoBehaviour
{
    [SerializeField] private float speed = 2.5f;
    [Tooltip("Distancia máxima desde el punto inicial hacia cada lado.")]
    [SerializeField] private float patrolDistance = 4f;
    [SerializeField] private LayerMask groundLayer;
    [Tooltip("Desde dónde se revisan paredes y bordes (relativo al pivote en los pies).")]
    [SerializeField] private Vector2 probeOffset = new Vector2(0.7f, 0.5f);
    [Tooltip("Activar si el sprite original mira a la derecha.")]
    [SerializeField] private bool spriteFacesRight = false;

    private Rigidbody2D rb;
    private Health health;
    private float startX;
    private int direction = 1;
    private float nextTurnTime;
    private float stuckTimer;
    private float lastX;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();
        startX = transform.position.x;
        ApplyFacing();
    }

    private void FixedUpdate()
    {
        if (health != null && (health.IsDead || health.IsStunned)) return;

        // Primero los obstáculos (pared o borde) y después el límite de patrulla, que solo voltea si
        // el enemigo se sigue alejando. Antes ambas reglas podían contradecirse cada frame
        // (p. ej. tras un salto que lo dejaba fuera de su zona junto a un borde) y se quedaba quieto.
        float x = transform.position.x;
        bool blocked = WallAhead() || (IsGrounded() && !GroundAhead());
        if (blocked)
        {
            if (Time.time >= nextTurnTime) SetDirection(-direction);
        }
        else if (x > startX + patrolDistance && direction > 0) SetDirection(-1);
        else if (x < startX - patrolDistance && direction < 0) SetDirection(1);

        // Seguro: si intenta avanzar en el suelo pero no se mueve (algo lo detiene que los rayos no ven), se da la vuelta.
        if (IsGrounded() && Mathf.Abs(x - lastX) < speed * Time.fixedDeltaTime * 0.2f)
        {
            stuckTimer += Time.fixedDeltaTime;
            if (stuckTimer > 0.4f)
            {
                stuckTimer = 0f;
                nextTurnTime = 0f;
                SetDirection(-direction);
            }
        }
        else stuckTimer = 0f;
        lastX = x;

        rb.linearVelocity = new Vector2(direction * speed, rb.linearVelocity.y);
    }

    private bool WallAhead()
    {
        Vector2 origin = (Vector2)transform.position + new Vector2(0f, probeOffset.y);
        return Physics2D.Raycast(origin, Vector2.right * direction, probeOffset.x, groundLayer);
    }

    // En el aire (p. ej. al saltar) no hay suelo adelante; solo se revisan bordes al estar apoyado.
    private bool IsGrounded()
    {
        return Physics2D.Raycast((Vector2)transform.position + Vector2.up * 0.1f, Vector2.down, 0.3f, groundLayer);
    }

    private bool GroundAhead()
    {
        Vector2 origin = (Vector2)transform.position + new Vector2(direction * probeOffset.x, probeOffset.y);
        return Physics2D.Raycast(origin, Vector2.down, probeOffset.y + 0.5f, groundLayer);
    }

    private void SetDirection(int dir)
    {
        if (dir == direction) return;
        direction = dir;
        nextTurnTime = Time.time + 0.25f;
        ApplyFacing();
    }

    private void ApplyFacing()
    {
        Vector3 s = transform.localScale;
        int visualDir = spriteFacesRight ? direction : -direction;
        s.x = Mathf.Abs(s.x) * visualDir;
        transform.localScale = s;
    }
}
