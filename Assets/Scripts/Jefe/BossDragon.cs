using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// El Dragón Borracho (jefe de la Mazmorra 2).
/// - Movimiento estático cuando no ataca.
/// - Jugador alejado (50/50 cada cierto tiempo):
///   1) Da tragos a su copa durante 5 s (SFX Trago-Copa) y luego se contonea durante 3 s
///      mientras lanza 3 bolas de fuego lentas directo al jugador (SFX Bola-Fuego por cada una).
///   2) Lanza un ave que persigue al jugador durante 3 s (SFX Ave-Volando).
/// - Jugador cerca (50/50):
///   1) Arroja un rasguño y al terminar genera una onda expansiva (SFX Onda-Expansiva).
///   2) Hace una embestida y al terminar genera igualmente una onda expansiva (SFX Onda-Expansiva).
/// </summary>
[RequireComponent(typeof(Health))]
public class BossDragon : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite[] idleFrames;
    [SerializeField] private Sprite[] drinkFrames;
    [SerializeField] private Sprite[] swayFrames;
    [SerializeField] private Sprite[] slashFrames;
    [SerializeField] private Sprite[] chargeFrames;
    [SerializeField] private Sprite[] throwBirdFrames;
    [SerializeField] private Sprite[] defeatedFrames;
    [Tooltip("Activar si el sprite original mira a la derecha.")]
    [SerializeField] private bool spriteFacesRight = true;

    [Header("Pelea")]
    [SerializeField] private string bossName = "El Dragón Borracho";
    [Tooltip("Distancia horizontal a partir de la cual se considera que el jugador está cerca.")]
    [SerializeField] private float meleeRange = 5.2f;
    [Tooltip("Cooldown entre ataques.")]
    [SerializeField] private float pauseBetweenAttacks = 2f;
    [SerializeField] private float telegraphTime = 0.35f;
    [SerializeField] private Color telegraphColor = new Color(1f, 0.55f, 0.3f);
    [SerializeField] private LayerMask playerLayer;

    [Header("Ataques a distancia (50/50)")]
    [Tooltip("Probabilidad de bolas de fuego (el resto es el ave).")]
    [SerializeField, Range(0f, 1f)] private float fireballChance = 0.75f;
    [SerializeField] private float drinkDuration = 5f;
    [SerializeField] private float swayDuration = 3f;
    [SerializeField] private int fireballsCount = 3;
    [SerializeField] private Projectile fireballPrefab;
    [SerializeField] private HomingBird birdPrefab;
    [SerializeField] private float birdChaseDuration = 3f;
    [SerializeField] private Transform spawnPoint;

    [Header("Ataques cuerpo a cuerpo (50/50)")]
    [SerializeField] private int meleeDamage = 1;
    [SerializeField] private Vector2 slashOffset = new Vector2(2.4f, 2.2f);
    [SerializeField] private Vector2 slashSize = new Vector2(3.6f, 3.2f);
    [SerializeField] private float chargeSpeed = 9.5f;
    [SerializeField] private float chargeDuration = 0.55f;
    [SerializeField] private Vector2 chargeHitSize = new Vector2(3.0f, 3.8f);
    [SerializeField] private GroundShockwave shockwavePrefab;

    private Health health;
    private Transform player;
    private Vector3 startPosition;
    private Coroutine fight;
    private int facing = 1;
    private float idleTime;
    private float minX = float.NegativeInfinity, maxX = float.PositiveInfinity;
    private readonly List<GameObject> spawnedObjects = new List<GameObject>();

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
        if (fight != null || IsDefeated || idleFrames == null || idleFrames.Length == 0) return;
        idleTime += Time.deltaTime;
        spriteRenderer.sprite = idleFrames[(int)(idleTime * 6f) % idleFrames.Length];
    }

    // ---------- Control desde la arena ----------

    public void SetWalkBounds(float min, float max)
    {
        minX = min;
        maxX = max;
    }

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
        ClearSpawnedObjects();
        health.ResetHealth();
        ResetPose();
        transform.position = startPosition;
        if (HUD.Instance != null) HUD.Instance.HideBossBar();
    }

    // ---------- Bucle de pelea ----------

    private IEnumerator FightLoop()
    {
        yield return Idle(0.8f);
        while (!IsDefeated)
        {
            FacePlayer();

            if (DistanceToPlayer() <= meleeRange)
            {
                // Jugador cerca (50/50): Rasguño + Onda Expansiva o Embestida + Onda Expansiva
                if (Random.value < 0.5f)
                    yield return SlashWithShockwave();
                else
                    yield return ChargeWithShockwave();
            }
            else
            {
                // Jugador alejado: 75% Trago (5s) + Contoneo con 3 Bolas de Fuego (3s), 25% Lanzar un Ave (3s)
                if (Random.value < fireballChance)
                    yield return DrinkAndSwayFireballs();
                else
                    yield return ThrowBirdAttack();
            }

            yield return Idle(pauseBetweenAttacks);
        }
    }

    private float DistanceToPlayer() => player != null ? Mathf.Abs(player.position.x - transform.position.x) : 0f;

    private IEnumerator Idle(float duration, bool interruptIfClose = false)
    {
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            FacePlayer();
            if (idleFrames != null && idleFrames.Length > 0)
                spriteRenderer.sprite = idleFrames[(int)(t * 6f) % idleFrames.Length];
            if (interruptIfClose && t >= 0.35f && DistanceToPlayer() <= meleeRange)
                yield break;
            yield return null;
        }
    }

    // ---------- Ataques de lejos (50/50) ----------

    /// <summary>
    /// Da tragos a su copa durante 5 segundos con Trago-Copa, y luego se contonea durante 3 segundos
    /// mientras lanza 3 bolas de fuego lentas directo hacia el jugador (con Bola-Fuego por cada una).
    /// </summary>
    private IEnumerator DrinkAndSwayFireballs()
    {
        FacePlayer();
        AudioManager.Instance.PlaySfx("Trago-Copa");

        // 1) 5 segundos tomando de la copa
        for (float t = 0f; t < drinkDuration; t += Time.deltaTime)
        {
            FacePlayer();
            if (drinkFrames != null && drinkFrames.Length > 0)
                spriteRenderer.sprite = drinkFrames[(int)(t * 5f) % drinkFrames.Length];
            yield return null;
        }

        // 2) 3 segundos contoneándose mientras lanza 3 bolas de fuego lentas
        int fired = 0;
        for (float t = 0f; t < swayDuration; t += Time.deltaTime)
        {
            FacePlayer();
            if (swayFrames != null && swayFrames.Length > 0)
                spriteRenderer.sprite = swayFrames[(int)(t * 6f) % swayFrames.Length];

            // Dispara las 3 bolas de fuego repartidas durante los 3 segundos (p. ej. 0.45s, 1.45s, 2.45s)
            if (fired < fireballsCount)
            {
                float fireTime = (fired + 0.45f) * (swayDuration / Mathf.Max(1, fireballsCount));
                if (t >= fireTime)
                {
                    SpawnFireball();
                    fired++;
                }
            }
            yield return null;
        }

        while (fired < fireballsCount)
        {
            SpawnFireball();
            fired++;
        }
    }

    private void SpawnFireball()
    {
        if (fireballPrefab == null) return;
        FacePlayer();
        Vector3 origin = spawnPoint != null ? spawnPoint.position : transform.position + new Vector3(facing * 1.4f, 2.3f, 0f);
        Vector2 dir = new Vector2(facing, 0f);
        if (player != null)
            dir = (Vector2)(player.position + Vector3.up * 1.2f - origin);

        var fb = Instantiate(fireballPrefab, origin, Quaternion.identity);
        fb.Launch(dir);
        spawnedObjects.Add(fb.gameObject);
        AudioManager.Instance.PlaySfx("Bola-Fuego");
    }

    /// <summary>
    /// Lanza un ave que persigue al jugador durante 3 segundos con el efecto de sonido Ave-Volando.
    /// </summary>
    private IEnumerator ThrowBirdAttack()
    {
        FacePlayer();
        HomingBird activeBird = null;

        if (throwBirdFrames != null && throwBirdFrames.Length > 0)
        {
            int spawnFrame = Mathf.Min(3, throwBirdFrames.Length - 1);
            for (int i = 0; i < throwBirdFrames.Length; i++)
            {
                spriteRenderer.sprite = throwBirdFrames[i];
                if (i == spawnFrame)
                    activeBird = SpawnBird();
                yield return new WaitForSeconds(0.12f);
            }
        }
        else
        {
            activeBird = SpawnBird();
        }

        // Mientras el ave persigue al jugador (hasta 3 s), el dragón permanece en movimiento estático
        float wait = 0f;
        while (activeBird != null && wait < birdChaseDuration)
        {
            FacePlayer();
            if (idleFrames != null && idleFrames.Length > 0)
                spriteRenderer.sprite = idleFrames[(int)(wait * 6f) % idleFrames.Length];
            wait += Time.deltaTime;
            yield return null;
        }
    }

    private HomingBird SpawnBird()
    {
        if (birdPrefab == null) return null;
        Vector3 origin = spawnPoint != null ? spawnPoint.position : transform.position + new Vector3(facing * 1.4f, 2.6f, 0f);
        var bird = Instantiate(birdPrefab, origin, Quaternion.identity);
        bird.Launch(player, birdChaseDuration, facing);
        spawnedObjects.Add(bird.gameObject);
        AudioManager.Instance.PlaySfx("Ave-Volando");
        return bird;
    }

    // ---------- Ataques de cerca (50/50) ----------

    /// <summary>
    /// Arroja un rasguño y después genera una onda expansiva con el efecto Onda-Expansiva.
    /// </summary>
    private IEnumerator SlashWithShockwave()
    {
        FacePlayer();
        if (slashFrames != null && slashFrames.Length >= 6)
        {
            spriteRenderer.sprite = slashFrames[0];
            yield return new WaitForSeconds(0.08f);
            yield return Telegraph(slashFrames[1]);

            spriteRenderer.sprite = slashFrames[2];
            yield return new WaitForSeconds(0.08f);

            bool slashHit = false;
            for (int i = 3; i <= 4; i++)
            {
                spriteRenderer.sprite = slashFrames[i];
                for (float t = 0f; t < 0.12f; t += Time.deltaTime)
                {
                    if (!slashHit) slashHit = TryMeleeHit(SlashCenter(), slashSize);
                    yield return null;
                }
            }

            spriteRenderer.sprite = slashFrames[5];
            SpawnShockwave();
            yield return new WaitForSeconds(0.25f);
        }
        else
        {
            SpawnShockwave();
            yield return new WaitForSeconds(0.3f);
        }
    }

    /// <summary>
    /// Hace una embestida hacia el jugador y al terminar genera una onda expansiva con el efecto Onda-Expansiva.
    /// </summary>
    private IEnumerator ChargeWithShockwave()
    {
        FacePlayer();
        if (chargeFrames != null && chargeFrames.Length > 0)
            yield return Telegraph(chargeFrames[0]);

        bool chargeHit = false;
        for (float t = 0f; t < chargeDuration; t += Time.deltaTime)
        {
            if (chargeFrames != null && chargeFrames.Length >= 4)
            {
                int frame = 1 + ((int)(t * 12f) % 3); // cuadros 1, 2, 3 durante la carrera
                spriteRenderer.sprite = chargeFrames[frame];
            }

            float nextX = Mathf.Clamp(transform.position.x + facing * chargeSpeed * Time.deltaTime, minX, maxX);
            transform.position = new Vector3(nextX, transform.position.y, transform.position.z);

            if (!chargeHit)
            {
                Vector2 center = (Vector2)transform.position + new Vector2(facing * 0.8f, chargeHitSize.y * 0.5f);
                chargeHit = TryMeleeHit(center, chargeHitSize);
            }

            if (Mathf.Approximately(nextX, minX) || Mathf.Approximately(nextX, maxX))
                break;

            yield return null;
        }

        if (chargeFrames != null && chargeFrames.Length >= 5)
            spriteRenderer.sprite = chargeFrames[4];

        // Al terminar la embestida genera la onda expansiva hacia donde está el jugador
        if (player != null && Mathf.Abs(player.position.x - transform.position.x) > 0.5f)
            FacePlayer();
        SpawnShockwave();
        yield return new WaitForSeconds(0.3f);
    }

    private void SpawnShockwave()
    {
        if (shockwavePrefab == null) return;
        int waveDir = facing;
        if (player != null && Mathf.Abs(player.position.x - transform.position.x) > 0.2f)
            waveDir = player.position.x >= transform.position.x ? 1 : -1;

        Vector3 origin = new Vector3(transform.position.x + waveDir * 1.6f, startPosition.y, transform.position.z);
        var wave = Instantiate(shockwavePrefab, origin, Quaternion.identity);
        wave.Launch(waveDir, minX - 1f, maxX + 1f);
        spawnedObjects.Add(wave.gameObject);
        AudioManager.Instance.PlaySfx("Onda-Expansiva");
    }

    private bool TryMeleeHit(Vector2 center, Vector2 size)
    {
        Collider2D c = Physics2D.OverlapBox(center, size, 0f, playerLayer);
        var ph = c != null ? c.GetComponentInParent<PlayerHealth>() : null;
        if (ph == null) return false;
        ph.TakeDamage(meleeDamage, transform.position);
        return true;
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

    // ---------- Daño y derrota ----------

    private void OnDamaged(int amount)
    {
        if (HUD.Instance != null) HUD.Instance.SetBossHealth((float)health.Current / health.Max);
    }

    private void OnDefeated()
    {
        IsDefeated = true;
        AudioManager.Instance.StopAll();
        if (fight != null) StopCoroutine(fight);
        fight = null;
        ClearSpawnedObjects();
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
        if (defeatedFrames == null) yield break;
        foreach (var f in defeatedFrames)
        {
            spriteRenderer.sprite = f;
            yield return new WaitForSeconds(1f / 6f);
        }
    }

    // ---------- Utilidades ----------

    private void ClearSpawnedObjects()
    {
        for (int i = spawnedObjects.Count - 1; i >= 0; i--)
        {
            if (spawnedObjects[i] != null)
                Destroy(spawnedObjects[i]);
        }
        spawnedObjects.Clear();
    }

    private void ResetPose(bool keepSprite = false)
    {
        spriteRenderer.color = Color.white;
        spriteRenderer.transform.localPosition = Vector3.zero;
        spriteRenderer.transform.localScale = Vector3.one;
        if (!keepSprite && idleFrames != null && idleFrames.Length > 0)
            spriteRenderer.sprite = idleFrames[0];
    }

    private void FacePlayer()
    {
        if (player == null) return;
        facing = player.position.x >= transform.position.x ? 1 : -1;
        Vector3 s = transform.localScale;
        s.x = Mathf.Abs(s.x) * (spriteFacesRight ? facing : -facing);
        transform.localScale = s;
    }

    private Vector2 SlashCenter() => (Vector2)transform.position + new Vector2(slashOffset.x * facing, slashOffset.y);

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position + new Vector3(slashOffset.x * (Application.isPlaying ? facing : 1), slashOffset.y), slashSize);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position + Vector3.up, new Vector3(meleeRange * 2f, 2f, 0f));
    }
}
