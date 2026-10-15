using System.Collections;
using UnityEngine;

/// <summary>
/// El Campanero corrompido (jefe de mazmorra).
/// - Jugador lejos: lanza la chancla. Jugador cerca: golpe de garrote.
/// - Antes de cada ataque hace una pose de aviso (destello y temblor) para que el jugador reaccione.
/// - Tras varios ataques se cansa y se queda quieto unos segundos.
/// - Con los golpes suficientes cae debilitado y se queda en el suelo.
/// </summary>
[RequireComponent(typeof(Health))]
public class BossCampanero : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite[] idleFrames;
    [SerializeField] private Sprite[] clubFrames;
    [SerializeField] private Sprite[] throwFrames;
    [SerializeField] private Sprite[] defeatedFrames;
    [Tooltip("Activar si el sprite original mira a la derecha.")]
    [SerializeField] private bool spriteFacesRight = true;

    [Header("Pelea")]
    [SerializeField] private string bossName = "El Campanero";
    [Tooltip("Distancia horizontal a partir de la cual lanza la chancla en vez del garrote.")]
    [SerializeField] private float meleeRange = 4.5f;
    [SerializeField] private float pauseBetweenAttacks = 0.9f;
    [Tooltip("Duración de la pose de aviso antes de cada ataque.")]
    [SerializeField] private float telegraphTime = 0.45f;
    [SerializeField] private Color telegraphColor = new Color(1f, 0.55f, 0.3f);
    [SerializeField] private int attacksBeforeTired = 6;
    [SerializeField] private float tiredTime = 2f;

    [Header("Garrote")]
    [SerializeField] private int damage = 1;
    [SerializeField] private Vector2 clubOffset = new Vector2(2.6f, 1.2f);
    [SerializeField] private Vector2 clubSize = new Vector2(3.6f, 2.8f);
    [SerializeField] private LayerMask playerLayer;

    [Header("Chancla")]
    [SerializeField] private Projectile chanclaPrefab;
    [SerializeField] private Transform throwPoint;
    [SerializeField] private int chanclasPerThrow = 1;
    [SerializeField] private bool aimAtPlayer;

    private Health health;
    private Transform player;
    private Vector3 startPosition;
    private Coroutine fight;
    private int facing = 1;
    private bool clubHitDone;
    private float idleTime;

    public bool IsDefeated { get; private set; }
    public bool IsFighting => fight != null;
    public string BossName => bossName;

    private void Awake()
    {
        health = GetComponent<Health>();
        health.OnDamaged += OnDamaged;
        health.OnDeath += OnDefeated;
        startPosition = transform.position;
    }

    private void Start()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    private void OnDestroy()
    {
        if (health == null) return;
        health.OnDamaged -= OnDamaged;
        health.OnDeath -= OnDefeated;
    }

    private void Update()
    {
        if (fight != null || IsDefeated || idleFrames.Length == 0) return;
        idleTime += Time.deltaTime;
        spriteRenderer.sprite = idleFrames[(int)(idleTime * 6f) % idleFrames.Length];
    }

    // ---------- Control desde la arena ----------

    public void BeginFight()
    {
        if (IsDefeated || fight != null) return;
        if (HUD.Instance != null)
        {
            HUD.Instance.ShowBossBar(bossName);
            HUD.Instance.SetBossHealth((float)health.Current / health.Max);
        }
        fight = StartCoroutine(FightLoop());
    }

    public void ResetFight()
    {
        if (IsDefeated) return;
        StopAllCoroutines();
        fight = null;
        health.ResetHealth();
        ResetPose();
        transform.position = startPosition;
        if (HUD.Instance != null) HUD.Instance.HideBossBar();
    }

    // ---------- Pelea ----------

    private IEnumerator FightLoop()
    {
        int attacks = 0;
        yield return Idle(1f);
        while (!IsDefeated)
        {
            FacePlayer();
            float distance = player != null ? Mathf.Abs(player.position.x - transform.position.x) : 0f;
            if (distance <= meleeRange) yield return Club();
            else yield return Throw();

            attacks++;
            if (attacks % attacksBeforeTired == 0) yield return Tired();
            else yield return Idle(pauseBetweenAttacks);
        }
    }

    private IEnumerator Idle(float duration)
    {
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            if (idleFrames.Length > 0) spriteRenderer.sprite = idleFrames[(int)(t * 6f) % idleFrames.Length];
            yield return null;
        }
    }

    private IEnumerator Telegraph(Sprite pose)
    {
        spriteRenderer.sprite = pose;
        for (float t = 0f; t < telegraphTime; t += Time.deltaTime)
        {
            spriteRenderer.color = Color.Lerp(Color.white, telegraphColor, Mathf.PingPong(t * 10f, 1f));
            spriteRenderer.transform.localPosition = new Vector3(Random.Range(-0.06f, 0.06f), 0f, 0f);
            yield return null;
        }
        ResetPose(keepSprite: true);
    }

    private IEnumerator Club()
    {
        FacePlayer();
        clubHitDone = false;
        spriteRenderer.sprite = clubFrames[0];
        yield return new WaitForSeconds(0.1f);
        yield return Telegraph(clubFrames[1]);                  // garrote en alto: aviso

        yield return ActiveFrame(clubFrames[2], 0.15f);         // primer golpe al suelo
        spriteRenderer.sprite = clubFrames[3];
        yield return new WaitForSeconds(0.12f);
        yield return ActiveFrame(clubFrames[4], 0.15f);         // segundo golpe
        spriteRenderer.sprite = clubFrames[5];
        yield return new WaitForSeconds(0.3f);
    }

    private IEnumerator ActiveFrame(Sprite frame, float duration)
    {
        spriteRenderer.sprite = frame;
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            if (!clubHitDone)
            {
                Collider2D c = Physics2D.OverlapBox(ClubCenter(), clubSize, 0f, playerLayer);
                var ph = c != null ? c.GetComponentInParent<PlayerHealth>() : null;
                if (ph != null)
                {
                    ph.TakeDamage(damage, transform.position);
                    clubHitDone = true;
                }
            }
            yield return null;
        }
    }

    private IEnumerator Throw()
    {
        FacePlayer();
        yield return Telegraph(throwFrames[0]);                 // chancla atrás: aviso
        for (int i = 0; i < chanclasPerThrow; i++)
        {
            spriteRenderer.sprite = throwFrames[1];
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.sprite = throwFrames[2];
            SpawnChancla();
            yield return new WaitForSeconds(0.3f);
            FacePlayer();
        }
        yield return new WaitForSeconds(0.15f);
    }

    private void SpawnChancla()
    {
        if (chanclaPrefab == null) return;
        Vector3 origin = throwPoint != null ? throwPoint.position : transform.position + Vector3.up * 1.5f;
        Vector2 dir = new Vector2(facing, 0f);
        if (aimAtPlayer && player != null) dir = (Vector2)(player.position + Vector3.up * 1.3f - origin);
        var chancla = Instantiate(chanclaPrefab, origin, Quaternion.identity);
        chancla.Launch(dir);
    }

    private IEnumerator Tired()
    {
        if (HUD.Instance != null) HUD.Instance.ShowMessage("¡El Campanero se cansó!", 1.2f);
        spriteRenderer.sprite = throwFrames[throwFrames.Length - 1];
        for (float t = 0f; t < tiredTime; t += Time.deltaTime)
        {
            spriteRenderer.transform.localScale = new Vector3(1f, 1f - 0.03f * Mathf.Abs(Mathf.Sin(t * 4f)), 1f);
            yield return null;
        }
        spriteRenderer.transform.localScale = Vector3.one;
    }

    // ---------- Daño y derrota ----------

    private void OnDamaged(int amount)
    {
        if (HUD.Instance != null) HUD.Instance.SetBossHealth((float)health.Current / health.Max);
    }

    private void OnDefeated()
    {
        IsDefeated = true;
        if (fight != null) StopCoroutine(fight);
        fight = null;
        ResetPose();
        StartCoroutine(DefeatAnimation());
        if (HUD.Instance != null)
        {
            HUD.Instance.HideBossBar();
            HUD.Instance.ShowMessage($"¡{bossName} cayó debilitado!\nDerriba la antena", 3f);
        }
    }

    private IEnumerator DefeatAnimation()
    {
        foreach (var f in defeatedFrames)
        {
            spriteRenderer.sprite = f;
            yield return new WaitForSeconds(1f / 6f);
        }
    }

    // ---------- Utilidades ----------

    private void ResetPose(bool keepSprite = false)
    {
        spriteRenderer.color = Color.white;
        spriteRenderer.transform.localPosition = Vector3.zero;
        spriteRenderer.transform.localScale = Vector3.one;
        if (!keepSprite && idleFrames.Length > 0) spriteRenderer.sprite = idleFrames[0];
    }

    private void FacePlayer()
    {
        if (player == null) return;
        facing = player.position.x >= transform.position.x ? 1 : -1;
        Vector3 s = transform.localScale;
        s.x = Mathf.Abs(s.x) * (spriteFacesRight ? facing : -facing);
        transform.localScale = s;
    }

    private Vector2 ClubCenter() => (Vector2)transform.position + new Vector2(clubOffset.x * facing, clubOffset.y);

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position + new Vector3(clubOffset.x * (Application.isPlaying ? facing : 1), clubOffset.y), clubSize);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position + Vector3.up, new Vector3(meleeRange * 2f, 2f, 0f));
    }
}
