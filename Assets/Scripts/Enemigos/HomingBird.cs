using UnityEngine;

/// <summary>
/// Ave invocada por el Dragón: persigue al jugador durante 3 segundos aleteando.
/// Al tocar al jugador le hace daño y desaparece, o desaparece cuando termina su tiempo.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class HomingBird : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite[] flapFrames;
    [SerializeField] private float flapFps = 12f;
    [SerializeField] private float speed = 4.2f;
    [SerializeField] private float chaseDuration = 3f;
    [SerializeField] private int damage = 1;
    [SerializeField] private float radius = 0.55f;
    [SerializeField] private LayerMask playerLayer;
    [Tooltip("Muros, piso y techo: el ave desaparece al chocar con ellos. Las plataformas de un solo sentido las atraviesa.")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private bool spriteFacesRight = true;

    private Transform target;
    private float dieTime;
    private float animTime;
    private Vector2 currentDir = Vector2.right;

    private void Awake()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Launch(Transform playerTarget, float duration = 3f, int initialFacing = 1)
    {
        target = playerTarget;
        chaseDuration = duration;
        dieTime = Time.time + chaseDuration;
        currentDir = new Vector2(initialFacing >= 0 ? 1f : -1f, 0f);
        UpdateFacing(currentDir.x);
    }

    private void Start()
    {
        if (dieTime <= 0f) dieTime = Time.time + chaseDuration;
        if (target == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) target = p.transform;
        }
    }

    private void Update()
    {
        if (flapFrames != null && flapFrames.Length > 0 && spriteRenderer != null)
        {
            animTime += Time.deltaTime;
            spriteRenderer.sprite = flapFrames[(int)(animTime * flapFps) % flapFrames.Length];
        }

        if (target != null)
        {
            Vector2 desired = (Vector2)(target.position + Vector3.up * 1.1f - transform.position);
            if (desired.sqrMagnitude > 0.001f) currentDir = desired.normalized;
        }

        transform.position += (Vector3)(currentDir * speed * Time.deltaTime);
        UpdateFacing(currentDir.x);

        Collider2D hit = Physics2D.OverlapCircle(transform.position, radius, playerLayer);
        if (hit != null)
        {
            var player = hit.GetComponentInParent<PlayerHealth>();
            if (player != null) player.TakeDamage(damage, transform.position);
            Destroy(gameObject);
            return;
        }

        if (Time.time >= dieTime || Projectile.TouchesSolid(transform.position, radius * 0.5f, groundLayer, true))
            Destroy(gameObject);
    }

    private void UpdateFacing(float dirX)
    {
        if (Mathf.Abs(dirX) < 0.01f) return;
        int sign = dirX >= 0f ? 1 : -1;
        Vector3 s = transform.localScale;
        s.x = Mathf.Abs(s.x) * (spriteFacesRight ? sign : -sign);
        transform.localScale = s;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
