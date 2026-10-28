using System.Collections;
using UnityEngine;

/// <summary>
/// Enemigo estático que mira al jugador y le dispara proyectiles cada cierto tiempo cuando está en rango.
/// </summary>
public class EnemyShooter : MonoBehaviour
{
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private Animator animator;
    [SerializeField] private string fireState = "Disparar";
    [Tooltip("Retraso entre iniciar la animación y soltar el proyectil.")]
    [SerializeField] private float fireDelay = 0.65f;
    [SerializeField] private float interval = 2.5f;
    [SerializeField] private float range = 14f;
    [Tooltip("Activar si el sprite original mira a la derecha.")]
    [SerializeField] private bool spriteFacesRight = false;
    [Tooltip("Apuntar el proyectil hacia el jugador en vez de dispararlo en horizontal.")]
    [SerializeField] private bool aimAtPlayer = false;
    [Tooltip("Altura sobre los pies del jugador a la que se apunta.")]
    [SerializeField] private float aimHeight = 1.4f;

    private Transform player;
    private Health health;
    private float nextShotTime;
    private int facing = 1;

    private void Awake()
    {
        health = GetComponent<Health>();
        nextShotTime = Time.time + interval;
    }

    private void Start()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    private void Update()
    {
        if (player == null || (health != null && health.IsDead)) return;

        float dx = player.position.x - transform.position.x;
        if (Mathf.Abs(dx) > range) return;

        facing = dx >= 0f ? 1 : -1;
        Vector3 s = transform.localScale;
        s.x = Mathf.Abs(s.x) * (spriteFacesRight ? facing : -facing);
        transform.localScale = s;

        if (Time.time >= nextShotTime)
        {
            nextShotTime = Time.time + interval;
            StartCoroutine(Shoot());
        }
    }

    private IEnumerator Shoot()
    {
        if (animator != null) animator.Play(fireState, 0, 0f);
        yield return new WaitForSeconds(fireDelay);
        if (health != null && health.IsDead) yield break;

        Vector3 origin = firePoint != null ? firePoint.position : transform.position;
        Vector2 direction = new Vector2(facing, 0f);
        if (aimAtPlayer && player != null)
            direction = (Vector2)(player.position + Vector3.up * aimHeight - origin);

        var projectile = Instantiate(projectilePrefab, origin, Quaternion.identity);
        projectile.Launch(direction);
        AudioManager.Instance.PlaySfxAt("Lanzamiento_Esbirros", origin);
    }
}
