using UnityEngine;

/// <summary>
/// Proyectil enemigo en línea recta. Daña al jugador (o se bloquea con el cartón) y se destruye al chocar con el suelo.
/// </summary>
public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 7f;
    [SerializeField] private float lifetime = 4f;
    [SerializeField] private int damage = 1;
    [SerializeField] private float radius = 0.3f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask groundLayer;
    [Tooltip("Giro del sprite en grados por segundo (p. ej. la chancla).")]
    [SerializeField] private float spinSpeed = 0f;

    private Vector2 direction = Vector2.right;
    private float dieTime;

    public void Launch(Vector2 dir)
    {
        direction = dir.normalized;
        Vector3 s = transform.localScale;
        s.x = Mathf.Abs(s.x) * (direction.x < 0f ? -1f : 1f);
        transform.localScale = s;
    }

    private void Start()
    {
        dieTime = Time.time + lifetime;
    }

    private void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
        if (spinSpeed != 0f) transform.Rotate(0f, 0f, -spinSpeed * Mathf.Sign(direction.x == 0f ? 1f : direction.x) * Time.deltaTime);

        Collider2D hit = Physics2D.OverlapCircle(transform.position, radius, playerLayer);
        if (hit != null)
        {
            var player = hit.GetComponentInParent<PlayerHealth>();
            if (player != null) player.TakeDamage(damage, transform.position);
            Destroy(gameObject);
            return;
        }

        if (Physics2D.OverlapCircle(transform.position, radius * 0.5f, groundLayer) != null || Time.time >= dieTime)
            Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
