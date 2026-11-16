using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// La Señora (jefa final de la Mazmorra 3).
/// - Al entrar a la sala suena Presentacion_LaSeñora y permanece descansando (Parado / Idle).
/// - Si el jugador está arriba de ella o salta cuando está cerca: hace una patada giratoria saltando hacia el jugador
///   (PatadaGiratoria_LaSeñora) y luego descansa 1 s.
/// - Si el jugador se acerca en el mismo nivel: 50% golpe (Golpe_LaSeñora + Onda Expansiva con Onda-Expansiva)
///   y 50% embestida (Embestida_LaSeñora), y luego descansa 1 s.
/// - Si el jugador está lejos: lanza dos aves, una a cada lado (LanzamientoAve_LaSeñora + Ave-Volando),
///   que persiguen al jugador por 3 s, y luego descansa 4 s.
/// - Requiere 20 puntos de vida; al ser derrotada suena Derrota_LaSeñora.
/// </summary>
[RequireComponent(typeof(Health))]
public class BossSenora : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite[] idleFrames;
    [SerializeField] private Sprite[] punchFrames;
    [SerializeField] private Sprite[] kickFrames;
    [SerializeField] private Sprite[] chargeFrames;
    [SerializeField] private Sprite[] throwBirdFrames;
    [SerializeField] private Sprite[] defeatedFrames;
    [Tooltip("Activar si el sprite original mira a la derecha.")]
    [SerializeField] private bool spriteFacesRight = true;

    [Header("Pelea")]
    [SerializeField] private string bossName = "La Señora";
    [Tooltip("Distancia horizontal a partir de la cual se considera que el jugador está cerca.")]
    [SerializeField] private float meleeRange = 5.5f;
    [Tooltip("Distancia horizontal dentro de la cual detecta si el jugador está arriba de ella para la patada giratoria.")]
    [SerializeField] private float jumpKickHorizontalRange = 7f;
    [Tooltip("Altura mínima del jugador sobre el suelo de la jefa para considerarlo arriba de ella.")]
    [SerializeField] private float aboveHeightThreshold = 1.6f;
    [Tooltip("Descanso después de cada ataque cercano (golpe, embestida o patada giratoria).")]
    [SerializeField] private float closeAttackRestTime = 1f;
    [Tooltip("Descanso después de cada ataque lejano (lanzar dos aves).")]
    [SerializeField] private float rangedAttackRestTime = 4f;
    [SerializeField] private float telegraphTime = 0.25f;
    [SerializeField] private Color telegraphColor = new Color(1f, 0.55f, 0.3f);
    [SerializeField] private LayerMask playerLayer;

    [Header("Golpe y Onda Expansiva")]
    [SerializeField] private int meleeDamage = 1;
    [SerializeField] private Vector2 punchOffset = new Vector2(2.2f, 2.2f);
    [SerializeField] private Vector2 punchSize = new Vector2(3.2f, 2.8f);
    [SerializeField] private GroundShockwave shockwavePrefab;

    [Header("Embestida")]
    [SerializeField] private float chargeSpeed = 10f;
    [SerializeField] private float chargeDuration = 0.6f;
    [SerializeField] private Vector2 chargeHitSize = new Vector2(2.8f, 3.8f);

    [Header("Patada Giratoria (salto hacia el jugador)")]
    [SerializeField] private float jumpKickDuration = 0.75f;
    [SerializeField] private float minJumpKickHeight = 4.5f;
    [SerializeField] private float maxJumpKickHeight = 11f;
    [SerializeField] private Vector2 kickHitSize = new Vector2(3.4f, 3.6f);

    [Header("Lanzar dos Aves (lejos)")]
    [SerializeField] private HomingBird birdPrefab;
    [SerializeField] private float birdChaseDuration = 3f;
    [SerializeField] private Vector2 birdSpawnOffset = new Vector2(1.5f, 2.6f);

    private Health health;
    private Transform player;
    private Rigidbody2D playerRb;
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
        if (p != null)
        {
            player = p.transform;
            playerRb = p.GetComponent<Rigidbody2D>();
        }
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
        // Al entrar a la sala del jefe descansa un momento mientras suena su presentación
        yield return Idle(1f);

        while (!IsDefeated)
        {
            FacePlayer();

            if (IsPlayerAboveOrJumpingNear())
            {
                // Jugador arriba de la villana o saltando cerca de ella: patada giratoria saltando hacia él
                yield return SpinningJumpKick();
                yield return Idle(closeAttackRestTime);
            }
            else if (DistanceXToPlayer() <= meleeRange)
            {
                // Jugador cerca en el mismo nivel (50/50): Golpe (+ Onda Expansiva) o Embestida
                if (Random.value < 0.5f)
                    yield return PunchWithShockwave();
                else
                    yield return ChargeAttack();

                yield return Idle(closeAttackRestTime);
            }
            else
            {
                // Jugador lejos: lanza dos aves (una a cada lado) que persiguen por 3 s, y descansa 4 s
                yield return ThrowTwoBirdsAttack();
                yield return Idle(rangedAttackRestTime);
            }
        }
    }

    private float DistanceXToPlayer() => player != null ? Mathf.Abs(player.position.x - transform.position.x) : 0f;

    private float PlayerHeightAboveBoss() => player != null ? player.position.y - startPosition.y : 0f;

    private bool IsPlayerAboveOrJumpingNear()
    {
        if (player == null) return false;
        float dx = DistanceXToPlayer();
        float dy = PlayerHeightAboveBoss();
        bool jumpingUp = playerRb != null && playerRb.linearVelocity.y > 1.2f;

        // Está arriba en una plataforma sobre la villana o saltando cerca de ella
        if (dx <= jumpKickHorizontalRange && dy >= aboveHeightThreshold) return true;
        if (dx <= meleeRange && (dy >= 0.8f || jumpingUp)) return true;
        return false;
    }

    private IEnumerator Idle(float duration)
    {
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            FacePlayer();
            if (idleFrames != null && idleFrames.Length > 0)
                spriteRenderer.sprite = idleFrames[(int)(t * 6f) % idleFrames.Length];
            yield return null;
        }
    }

    // ---------- Ataques cercanos ----------

    /// <summary>
    /// Puñetazo con Golpe_LaSeñora seguido de Onda Expansiva morada con Onda-Expansiva.
    /// </summary>
    private IEnumerator PunchWithShockwave()
    {
        FacePlayer();
        if (punchFrames != null && punchFrames.Length >= 8)
        {
            spriteRenderer.sprite = punchFrames[0];
            yield return new WaitForSeconds(0.07f);
            yield return Telegraph(punchFrames[1]);

            AudioManager.Instance.PlaySfx("Golpe_LaSeñora");

            spriteRenderer.sprite = punchFrames[2];
            yield return new WaitForSeconds(0.07f);
            spriteRenderer.sprite = punchFrames[3];
            yield return new WaitForSeconds(0.07f);

            bool hitDone = false;
            for (int i = 4; i <= 5; i++)
            {
                spriteRenderer.sprite = punchFrames[i];
                for (float t = 0f; t < 0.11f; t += Time.deltaTime)
                {
                    if (!hitDone) hitDone = TryMeleeHit(PunchCenter(), punchSize);
                    yield return null;
                }
            }

            // Se genera la onda expansiva solo cuando se lanza el golpe
            SpawnShockwave();

            spriteRenderer.sprite = punchFrames[6];
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.sprite = punchFrames[7];
            yield return new WaitForSeconds(0.12f);
        }
        else
        {
            AudioManager.Instance.PlaySfx("Golpe_LaSeñora");
            SpawnShockwave();
            yield return new WaitForSeconds(0.3f);
        }
    }

    /// <summary>
    /// Embestida rápida hacia el jugador en el mismo nivel (sin onda expansiva).
    /// </summary>
    private IEnumerator ChargeAttack()
    {
        FacePlayer();
        if (chargeFrames != null && chargeFrames.Length > 0)
            yield return Telegraph(chargeFrames[0]);

        AudioManager.Instance.PlaySfx("Embestida_LaSeñora");

        bool chargeHit = false;
        for (float t = 0f; t < chargeDuration; t += Time.deltaTime)
        {
            if (chargeFrames != null && chargeFrames.Length >= 7)
            {
                int frame = 1 + ((int)(t * 12f) % 6); // cuadros 1..6 corriendo
                spriteRenderer.sprite = chargeFrames[frame];
            }

            float nextX = Mathf.Clamp(transform.position.x + facing * chargeSpeed * Time.deltaTime, minX, maxX);
            transform.position = new Vector3(nextX, startPosition.y, transform.position.z);

            if (!chargeHit)
            {
                Vector2 center = (Vector2)transform.position + new Vector2(facing * 0.8f, chargeHitSize.y * 0.5f);
                chargeHit = TryMeleeHit(center, chargeHitSize);
            }

            if (Mathf.Approximately(nextX, minX) || Mathf.Approximately(nextX, maxX))
                break;

            yield return null;
        }

        if (chargeFrames != null && chargeFrames.Length >= 8)
        {
            spriteRenderer.sprite = chargeFrames[7];
            yield return new WaitForSeconds(0.18f);
        }
    }

    /// <summary>
    /// Patada giratoria saltando hacia la posición del jugador cuando está arriba o salta cerca de ella.
    /// </summary>
    private IEnumerator SpinningJumpKick()
    {
        FacePlayer();
        if (kickFrames != null && kickFrames.Length > 0)
            yield return Telegraph(kickFrames[0]);

        AudioManager.Instance.PlaySfx("PatadaGiratoria_LaSeñora");

        float startX = transform.position.x;
        float targetX = player != null ? Mathf.Clamp(player.position.x, minX, maxX) : startX + facing * 3f;
        float desiredHeight = player != null
            ? Mathf.Clamp(player.position.y - startPosition.y + 1.2f, minJumpKickHeight, maxJumpKickHeight)
            : minJumpKickHeight;

        bool kickHit = false;
        for (float t = 0f; t < jumpKickDuration; t += Time.deltaTime)
        {
            float u = Mathf.Clamp01(t / jumpKickDuration);
            float x = Mathf.Lerp(startX, targetX, u);
            float y = startPosition.y + 4f * desiredHeight * u * (1f - u);
            transform.position = new Vector3(x, y, transform.position.z);

            if (kickFrames != null && kickFrames.Length >= 8)
            {
                int frame = Mathf.Clamp(1 + (int)(u * 6f), 1, 6);
                spriteRenderer.sprite = kickFrames[frame];
            }

            if (!kickHit && u >= 0.15f && u <= 0.85f)
            {
                Vector2 center = (Vector2)transform.position + new Vector2(facing * 0.6f, 2.2f);
                kickHit = TryMeleeHit(center, kickHitSize);
            }

            yield return null;
        }

        transform.position = new Vector3(targetX, startPosition.y, transform.position.z);
        if (kickFrames != null && kickFrames.Length >= 8)
        {
            spriteRenderer.sprite = kickFrames[7];
            yield return new WaitForSeconds(0.12f);
        }
    }

    // ---------- Ataque lejano (dos aves, una a cada lado) ----------

    private IEnumerator ThrowTwoBirdsAttack()
    {
        FacePlayer();
        AudioManager.Instance.PlaySfx("LanzamientoAve_LaSeñora");

        if (throwBirdFrames != null && throwBirdFrames.Length >= 8)
        {
            for (int i = 0; i < throwBirdFrames.Length; i++)
            {
                spriteRenderer.sprite = throwBirdFrames[i];
                // Lanza una ave a un lado y la segunda ave al otro lado
                if (i == 3) SpawnBird(-1);
                if (i == 5) SpawnBird(1);
                yield return new WaitForSeconds(0.1f);
            }
        }
        else
        {
            SpawnBird(-1);
            SpawnBird(1);
            yield return new WaitForSeconds(0.4f);
        }
    }

    private void SpawnBird(int side)
    {
        if (birdPrefab == null) return;
        Vector3 origin = transform.position + new Vector3(side * birdSpawnOffset.x, birdSpawnOffset.y, 0f);
        var bird = Instantiate(birdPrefab, origin, Quaternion.identity);
        bird.Launch(player, birdChaseDuration, side);
        spawnedObjects.Add(bird.gameObject);
        AudioManager.Instance.PlaySfx("Ave-Volando");
    }

    private void SpawnShockwave()
    {
        if (shockwavePrefab == null) return;
        int waveDir = facing;
        if (player != null && Mathf.Abs(player.position.x - transform.position.x) > 0.2f)
            waveDir = player.position.x >= transform.position.x ? 1 : -1;

        Vector3 origin = new Vector3(transform.position.x + waveDir * 1.5f, startPosition.y, transform.position.z);
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
        AudioManager.Instance.PlaySfx("Derrota_LaSeñora");
        if (fight != null) StopCoroutine(fight);
        fight = null;
        ClearSpawnedObjects();
        transform.position = new Vector3(transform.position.x, startPosition.y, transform.position.z);
        ResetPose();
        StartCoroutine(DefeatAnimation());
        if (HUD.Instance != null)
        {
            HUD.Instance.HideBossBar();
            HUD.Instance.ShowMessage($"¡{bossName} cayó derrotada!\nDerriba la antena", 3f);
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

    private Vector2 PunchCenter() => (Vector2)transform.position + new Vector2(punchOffset.x * facing, punchOffset.y);

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position + new Vector3(punchOffset.x * (Application.isPlaying ? facing : 1), punchOffset.y), punchSize);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position + Vector3.up, new Vector3(meleeRange * 2f, 2f, 0f));
    }
}
