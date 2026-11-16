using UnityEngine;

/// <summary>
/// Onda expansiva terrestre generada por el Dragón tras un rasguño o una embestida.
/// Avanza por el suelo en dirección al jugador y le hace daño al contacto.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class GroundShockwave : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite[] waveFrames;
    [SerializeField] private float speed = 7f;
    [SerializeField] private float lifetime = 1.6f;
    [SerializeField] private int damage = 1;
    [SerializeField] private Vector2 hitboxSize = new Vector2(2.4f, 1.8f);
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask groundLayer;

    private int direction = 1;
    private float elapsed;
    private float minX = float.NegativeInfinity;
    private float maxX = float.PositiveInfinity;
    private bool hitPlayer;

    private void Awake()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Launch(int dir, float arenaMinX = float.NegativeInfinity, float arenaMaxX = float.PositiveInfinity)
    {
        direction = dir >= 0 ? 1 : -1;
        minX = arenaMinX;
        maxX = arenaMaxX;
        Vector3 s = transform.localScale;
        s.x = Mathf.Abs(s.x) * direction;
        transform.localScale = s;
    }

    private void Update()
    {
        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / lifetime);

        if (waveFrames != null && waveFrames.Length > 0 && spriteRenderer != null)
        {
            // Crece al inicio (0..2), se mantiene activa avanzando (3..6) y se disipa al final (6..7)
            int frame;
            if (t < 0.2f)
                frame = Mathf.Clamp((int)(t / 0.2f * 3f), 0, Mathf.Min(2, waveFrames.Length - 1));
            else if (t < 0.85f && waveFrames.Length >= 7)
                frame = 3 + ((int)((elapsed - 0.2f * lifetime) * 10f) % 4);
            else
                frame = Mathf.Clamp(waveFrames.Length - (t < 0.93f ? 2 : 1), 0, waveFrames.Length - 1);

            spriteRenderer.sprite = waveFrames[frame];
        }

        transform.position += Vector3.right * (direction * speed * Time.deltaTime);

        if (!hitPlayer)
        {
            Vector2 center = (Vector2)transform.position + new Vector2(0f, hitboxSize.y * 0.5f);
            Collider2D hit = Physics2D.OverlapBox(center, hitboxSize, 0f, playerLayer);
            if (hit != null)
            {
                var player = hit.GetComponentInParent<PlayerHealth>();
                if (player != null)
                {
                    player.TakeDamage(damage, transform.position);
                    hitPlayer = true;
                }
            }
        }

        bool outOfBounds = transform.position.x < minX || transform.position.x > maxX;
        bool hitWall = groundLayer != 0 && Projectile.TouchesSolid((Vector2)transform.position + new Vector2(direction * 0.6f, 0.8f), 0.2f, groundLayer, true);
        if (elapsed >= lifetime || outOfBounds || hitWall)
            Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawWireCube(transform.position + new Vector3(0f, hitboxSize.y * 0.5f, 0f), hitboxSize);
    }
}
