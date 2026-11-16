using System.Collections;
using UnityEngine;

/// <summary>
/// Sala del jefe: cuando el jugador cruza la entrada, la compuerta baja y no lo deja escapar.
/// La cámara se limita a la arena. Si el jugador muere, la compuerta se abre y la pelea se reinicia.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class BossArena : MonoBehaviour
{
    [SerializeField] private BossCampanero boss;
    [SerializeField] private BossDragon dragonBoss;
    [SerializeField] private BossSenora senoraBoss;
    [SerializeField] private string introSfx = "Presentacion_Jefe1";
    [SerializeField] private Transform gate;
    [Tooltip("Cuánto sube la compuerta cuando está abierta (queda escondida en el muro).")]
    [SerializeField] private float gateOpenHeight = 5f;
    [SerializeField] private float gateMoveTime = 0.35f;
    [SerializeField] private Vector2 cameraMin;
    [SerializeField] private Vector2 cameraMax;

    private bool closed;
    private Vector3 gateClosedPosition;
    private PlayerHealth player;
    private CameraFollow cameraFollow;
    private Vector2 savedMin, savedMax;

    private bool HasBoss => boss != null || dragonBoss != null || senoraBoss != null;
    private bool IsBossDefeated => (boss != null && boss.IsDefeated) ||
                                   (dragonBoss != null && dragonBoss.IsDefeated) ||
                                   (senoraBoss != null && senoraBoss.IsDefeated);

    private void Start()
    {
        var area = GetComponent<BoxCollider2D>();
        area.isTrigger = true;
        if (boss != null) boss.SetWalkBounds(area.bounds.min.x + 1.5f, area.bounds.max.x - 1.5f);
        if (dragonBoss != null) dragonBoss.SetWalkBounds(area.bounds.min.x + 1.5f, area.bounds.max.x - 1.5f);
        if (senoraBoss != null) senoraBoss.SetWalkBounds(area.bounds.min.x + 1.5f, area.bounds.max.x - 1.5f);
        gateClosedPosition = gate.position;
        gate.position = gateClosedPosition + Vector3.up * gateOpenHeight;

        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
        {
            player = p.GetComponent<PlayerHealth>();
            player.OnRespawn += OnPlayerRespawn;
        }
        if (Camera.main != null) cameraFollow = Camera.main.GetComponent<CameraFollow>();
    }

    private void OnDestroy()
    {
        if (player != null) player.OnRespawn -= OnPlayerRespawn;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (closed || !HasBoss || IsBossDefeated) return;
        if (other.GetComponentInParent<PlayerHealth>() == null) return;
        Close();
    }

    private void Close()
    {
        closed = true;
        StopAllCoroutines();
        StartCoroutine(MoveGate(gateClosedPosition));
        if (cameraFollow != null)
        {
            cameraFollow.GetBounds(out savedMin, out savedMax);
            cameraFollow.SetBounds(cameraMin, cameraMax);
        }
        if (HUD.Instance != null) HUD.Instance.ShowMessage("¡No hay escapatoria!", 1.5f);
        // La presentación y la música del jefe empiezan al mismo tiempo
        string sfxToPlay = senoraBoss != null ? "Presentacion_LaSeñora"
                         : dragonBoss != null ? "Presentacion-dragon"
                         : introSfx;
        AudioManager.Instance.PlaySfx(sfxToPlay);
        AudioManager.Instance.PlayBossMusic();
        if (boss != null) boss.BeginFight();
        if (dragonBoss != null) dragonBoss.BeginFight();
        if (senoraBoss != null) senoraBoss.BeginFight();
    }

    private void OnPlayerRespawn()
    {
        if (!closed || IsBossDefeated) return;
        closed = false;
        StopAllCoroutines();
        StartCoroutine(MoveGate(gateClosedPosition + Vector3.up * gateOpenHeight));
        if (cameraFollow != null) cameraFollow.SetBounds(savedMin, savedMax);
        if (boss != null) boss.ResetFight();
        if (dragonBoss != null) dragonBoss.ResetFight();
        if (senoraBoss != null) senoraBoss.ResetFight();
        // Reaparece en el checkpoint antes de la arena: vuelve la música de la mazmorra
        AudioManager.Instance.PlaySceneMusic(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    private IEnumerator MoveGate(Vector3 target)
    {
        Vector3 from = gate.position;
        for (float t = 0f; t < gateMoveTime; t += Time.deltaTime)
        {
            gate.position = Vector3.Lerp(from, target, t / gateMoveTime);
            yield return null;
        }
        gate.position = target;
    }
}
