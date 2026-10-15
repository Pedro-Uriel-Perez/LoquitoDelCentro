using UnityEngine;

/// <summary>
/// Vuelo del esbirro alado: patrulla de lado a lado flotando con un leve sube y baja.
/// Vuela a una altura fija sobre el suelo que tiene debajo para que el jugador lo alcance saltando.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class FlyingEnemy : MonoBehaviour
{
    [SerializeField] private float speed = 2f;
    [SerializeField] private float patrolDistance = 4f;
    [SerializeField] private float bobAmplitude = 0.4f;
    [SerializeField] private float bobFrequency = 0.8f;
    [Tooltip("Altura de vuelo sobre el suelo de abajo. 0 = usar la posición donde se colocó.")]
    [SerializeField] private float hoverHeight = 4.3f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D rb;
    private Health health;
    private Vector2 start;
    private int direction = 1;
    private float phase;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        health = GetComponent<Health>();
        start = rb.position;
        phase = Random.value * Mathf.PI * 2f;

        if (hoverHeight > 0f && groundLayer != 0)
        {
            var hit = Physics2D.Raycast(start, Vector2.down, 40f, groundLayer);
            if (hit.collider != null) start.y = Mathf.Min(start.y, hit.point.y + hoverHeight);
        }
        rb.position = start;
        transform.position = start;
    }

    private void FixedUpdate()
    {
        if (health != null && health.IsDead) return;

        float x = rb.position.x + direction * speed * Time.fixedDeltaTime;
        if (x > start.x + patrolDistance) direction = -1;
        else if (x < start.x - patrolDistance) direction = 1;

        float y = start.y + Mathf.Sin(Time.time * bobFrequency * Mathf.PI * 2f + phase) * bobAmplitude;
        rb.MovePosition(new Vector2(x, y));
    }
}
