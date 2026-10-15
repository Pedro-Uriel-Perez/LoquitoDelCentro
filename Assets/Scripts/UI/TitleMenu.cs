using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Pantalla de inicio: título, retrato del Loquito y botones Jugar / Salir.
/// </summary>
public class TitleMenu : MonoBehaviour
{
    [SerializeField] private Sprite background;
    [SerializeField] private Sprite portrait;
    [SerializeField] private string subtitle = "San Felipe, Guanajuato";

    private void Start()
    {
        Time.timeScale = 1f;
        var canvas = UIFactory.CreateCanvas("Inicio_Canvas", 0);
        var root = canvas.transform;

        // Sin imagen de fondo se ve la escena del juego que hay detrás (decorado del templo).
        var shade = UIFactory.Stretch(root, "Sombra").gameObject.AddComponent<Image>();
        shade.sprite = background;
        shade.color = background != null ? Color.white : new Color(0f, 0f, 0f, 0.35f);
        shade.raycastTarget = false;

        UIFactory.Image(root, "Retrato", portrait, Color.white, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(24, 0), new Vector2(170, 340));

        UIFactory.Text(root, "Titulo", "LOQUITO\nDEL CENTRO", 48, TextAnchor.MiddleCenter, UIFactory.Gold,
                       new Vector2(0.62f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -30), new Vector2(400, 130));
        UIFactory.Text(root, "Subtitulo", subtitle, 16, TextAnchor.MiddleCenter, new Color(0.85f, 0.85f, 0.95f),
                       new Vector2(0.62f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -160), new Vector2(400, 24));

        var jugar = UIFactory.Button(root, "Jugar", "Jugar", new Vector2(0.62f, 0f), new Vector2(0, 130), new Vector2(160, 34),
                                     () => GameManager.Instance.NewGame());
        UIFactory.Button(root, "Salir", "Salir", new Vector2(0.62f, 0f), new Vector2(0, 85), new Vector2(160, 34), Quit);
        UIFactory.Text(root, "Controles", "Mover: A/D · Saltar: Espacio · Botella: J · Cartón: K · Interactuar: E",
                       11, TextAnchor.MiddleCenter, new Color(0.8f, 0.8f, 0.9f), new Vector2(0.62f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 20), new Vector2(420, 34));

        UIFactory.EnsureEventSystem();
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(jugar.gameObject);
    }

    private static void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
