using System.Collections;
using UnityEngine;

/// <summary>
/// Ángel blanco (estatua): inmortal y fijo. Cuando el jugador se acerca suelta un espadazo
/// y luego espera un cooldown; durante ese tiempo el jugador puede pasar sin recibir daño.
/// </summary>
public class AngelStatue : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private string attackState = "Ataque";

    [Header("Detección")]
    [SerializeField] private float detectRange = 3.5f;
    [SerializeField] private float verticalRange = 3f;

    [Header("Ataque")]
    [SerializeField] private int damage = 1;
    [SerializeField] private float cooldown = 2f;
    [Tooltip("Tiempo desde que inicia la animación hasta que la espada hace daño.")]
    [SerializeField] private float windup = 0.25f;
    [Tooltip("Tiempo que la espada hace daño.")]
    [SerializeField] private float activeTime = 0.3f;
    [Tooltip("Resto de la animación después del golpe.")]
    [SerializeField] private float recovery = 0.2f;
    [SerializeField] private Vector2 hitOffset = new Vector2(1.6f, 1.6f);
    [SerializeField] private Vector2 hitSize = new Vector2(2.8f, 3f);
    [SerializeField] private LayerMask playerLayer;
    [Tooltip("Activar si el sprite original mira a la derecha.")]
    [SerializeField] private bool spriteFacesRight = true;

    private Transform player;
    private float readyTime;
    private bool attacking;
    private int facing = 1;

    public bool IsAttacking => attacking;
    public bool OnCooldown => Time.time < readyTime;

    private void Start()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    private void Update()
    {
        if (player == null || attacking) return;

        float dx = player.position.x - transform.position.x;
        float dy = player.position.y - transform.position.y;
        if (Mathf.Abs(dx) > detectRange * 2f) return;

        Face(dx >= 0f ? 1 : -1);

        bool inRange = Mathf.Abs(dx) <= detectRange && dy > -1f && dy < verticalRange;
        if (inRange && Time.time >= readyTime) StartCoroutine(Attack());
    }

    private IEnumerator Attack()
    {
        attacking = true;
        if (animator != null) animator.Play(attackState, 0, 0f);

        yield return new WaitForSeconds(windup);
        AudioManager.Instance.PlaySfxAt("Golpe", transform.position);   // suena aunque no alcance al jugador

        float end = Time.time + activeTime;
        bool hit = false;
        while (Time.time < end)
        {
            if (!hit)
            {
                Collider2D c = Physics2D.OverlapBox(HitCenter(), hitSize, 0f, playerLayer);
                var ph = c != null ? c.GetComponentInParent<PlayerHealth>() : null;
                if (ph != null)
                {
                    ph.TakeDamage(damage, transform.position);
                    hit = true;
                }
            }
            yield return null;
        }

        yield return new WaitForSeconds(recovery);
        attacking = false;
        readyTime = Time.time + cooldown;
    }

    private Vector2 HitCenter() => (Vector2)transform.position + new Vector2(hitOffset.x * facing, hitOffset.y);

    private void Face(int dir)
    {
        facing = dir;
        Vector3 s = transform.localScale;
        s.x = Mathf.Abs(s.x) * (spriteFacesRight ? dir : -dir);
        transform.localScale = s;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position + Vector3.up * verticalRange * 0.5f, new Vector3(detectRange * 2f, verticalRange, 0f));
        Gizmos.color = Color.red;
        Vector3 c = transform.position + new Vector3(hitOffset.x * (Application.isPlaying ? facing : 1), hitOffset.y, 0f);
        Gizmos.DrawWireCube(c, hitSize);
    }
}
