using UnityEngine;

/// <summary>
/// Puerta de salida del nivel: al acercarse e interactuar (E / botón Norte) se pasa al siguiente nivel.
/// </summary>
public class LevelExit : MonoBehaviour
{
    [SerializeField] private float range = 1.8f;
    [SerializeField] private string prompt = "E / Y: Entrar al Templo del Reloj";

    private Transform player;
    private LoquitoInput input;
    private bool used;

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
        if (used || player == null || SettingsMenu.IsOpen) return;
        bool near = Mathf.Abs(player.position.x - transform.position.x) < range &&
                    Mathf.Abs(player.position.y - transform.position.y) < 2.5f;
        if (HUD.Instance != null) HUD.Instance.SetPrompt(this, near ? prompt : null);

        if (near && input != null && input.Interact.WasPressedThisFrame())
        {
            used = true;
            int level = LevelInfo.Current != null ? LevelInfo.Current.LevelNumber : 0;
            GameManager.Instance.CompleteLevel(level);
        }
    }
}
