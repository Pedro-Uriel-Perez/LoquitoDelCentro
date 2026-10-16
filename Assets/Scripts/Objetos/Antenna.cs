using System.Collections;
using UnityEngine;

/// <summary>
/// Antena 5G de la Señora. Tras vencer al jefe, el jugador interactúa con ella (E / botón Norte)
/// para destruirla; con eso se completa el nivel.
/// </summary>
public class Antenna : MonoBehaviour
{
    [SerializeField] private SpriteAnim anim;
    [SerializeField] private BossCampanero requiredBoss;
    [SerializeField] private float interactRange = 2.5f;
    [SerializeField] private float levelCompleteDelay = 3.5f;
    [SerializeField] private string prompt = "E / Y: Derribar la antena";
    [SerializeField] private string destroyedMessage = "¡Antena derribada!";

    private Transform player;
    private LoquitoInput input;
    private bool destroyed;

    public bool IsDestroyed => destroyed;

    private void Start()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
        {
            player = p.transform;
            input = p.GetComponent<LoquitoInput>();
        }
    }

    private void Update()
    {
        if (destroyed || player == null) return;

        bool near = Mathf.Abs(player.position.x - transform.position.x) < interactRange &&
                    Mathf.Abs(player.position.y - transform.position.y) < 3f;
        bool bossAlive = requiredBoss != null && !requiredBoss.IsDefeated;

        if (HUD.Instance != null)
        {
            string text = !near ? null : bossAlive ? "La antena está protegida. ¡Derrota al Campanero!" : prompt;
            HUD.Instance.SetPrompt(this, text);
        }

        if (near && !bossAlive && input != null && input.Interact.WasPressedThisFrame())
            StartCoroutine(DestroyAntenna());
    }

    private IEnumerator DestroyAntenna()
    {
        destroyed = true;
        if (HUD.Instance != null)
        {
            HUD.Instance.SetPrompt(this, null);
            HUD.Instance.ShowMessage(destroyedMessage, levelCompleteDelay);
        }
        if (anim != null) anim.Play("rota");

        var controller = player.GetComponent<PlayerController>();
        if (controller != null) controller.InputEnabled = false;
        var playerAnim = player.GetComponentInChildren<Animator>();
        if (playerAnim != null) playerAnim.Play("Victoria", 0, 0f);

        yield return new WaitForSeconds(levelCompleteDelay);
        int level = LevelInfo.Current != null ? LevelInfo.Current.LevelNumber : 1;
        GameManager.Instance.CompleteLevel(level);
    }
}
