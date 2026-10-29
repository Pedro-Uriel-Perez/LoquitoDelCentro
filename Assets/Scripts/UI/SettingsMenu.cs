using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Botón de configuración (esquina superior derecha) y su menú, presentes en todas las escenas.
/// Silencio, volumen de música y de efectos, Jugar, Créditos y Salir.
/// Dentro de un nivel el juego se pausa: Jugar reanuda y Salir regresa al menú de inicio.
/// En el menú de inicio: Jugar empieza la partida y Salir cierra el juego.
/// Se abre y cierra también con Esc o con Start del control.
/// </summary>
public class SettingsMenu : MonoBehaviour
{
    private static SettingsMenu instance;
    public static SettingsMenu Instance => instance;

    /// <summary>True mientras el menú o los créditos están abiertos (otros scripts ignoran la entrada).</summary>
    public static bool IsOpen => instance != null && (instance.panel.activeSelf || instance.credits.activeSelf);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => instance = null;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        if (instance != null) return;
        var go = new GameObject("MenuConfiguracion");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<SettingsMenu>();
    }

    private GameObject panel, credits;
    private Button playButton, creditsBack;
    private Image muteIcon, muteLine;
    private Image[] musicBar, sfxBar;
    private bool paused;
    private float savedTimeScale = 1f;
    private PlayerController pausedController;
    private bool savedInput;

    private static readonly Color SegmentOn = new Color(0.86f, 0.32f, 0.5f);
    private static readonly Color SegmentOff = new Color(0.12f, 0.11f, 0.18f);
    private static readonly Color Stone = new Color(0.35f, 0.42f, 0.58f);

    private static bool InTitle => SceneManager.GetActiveScene().name == GameManager.TitleScene;
    private static bool InLevel
    {
        get { string n = SceneManager.GetActiveScene().name; return n == "Jardin" || n.StartsWith("Mazmorra"); }
    }

    private void Awake()
    {
        Build();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene s, LoadSceneMode m)
    {
        panel.SetActive(false);
        credits.SetActive(false);
        paused = false;
        UIFactory.EnsureEventSystem();
    }

    private void Update()
    {
        bool toggle = (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) ||
                      (Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame);
        if (!toggle) return;
        if (credits.activeSelf) CloseCredits();
        else if (panel.activeSelf) Resume();
        else Open();
    }

    // ---------- Construcción ----------

    private void Build()
    {
        var canvas = UIFactory.CreateCanvas("Config_Canvas", 100);
        canvas.transform.SetParent(transform, false);
        var root = canvas.transform;
        var topRight = new Vector2(1f, 1f);

        ImageButton(root, "Engrane", "config", topRight, topRight, new Vector2(-6, -6), new Vector2(34, 33), Open);

        // Panel de configuración
        var overlay = UIFactory.Stretch(root, "Configuracion");
        panel = overlay.gameObject;
        overlay.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.65f);   // bloquea clics al juego
        var box = Panel(overlay, new Vector2(360, 250));
        UIFactory.Text(box, "Titulo", "Configuración", 20, TextAnchor.MiddleCenter, UIFactory.Gold,
                       new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -10), new Vector2(340, 28));

        Label(box, "Silencio", new Vector2(-70, -60));
        var mute = ImageButton(box, "Volumen", "volumen", new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f), new Vector2(40, -60), new Vector2(40, 38), ToggleMute);
        muteIcon = mute.GetComponent<Image>();
        muteLine = UIFactory.Image(mute.transform, "Tachado", UIFactory.WhiteSprite, new Color(0.9f, 0.15f, 0.2f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(44, 4));
        muteLine.rectTransform.localRotation = Quaternion.Euler(0, 0, 40);

        musicBar = VolumeRow(box, "Música", -104, d => { AudioManager.Instance.ChangeMusic(d); Refresh(); });
        sfxBar = VolumeRow(box, "Efectos", -146, d => { AudioManager.Instance.ChangeSfx(d); Refresh(); });

        var bottom = new Vector2(0.5f, 0f);
        playButton = ImageButton(box, "Jugar", "jugar", bottom, new Vector2(0.5f, 0f), new Vector2(-112, 14), new Vector2(104, 44), OnPlay);
        ImageButton(box, "Creditos", "creditos", bottom, new Vector2(0.5f, 0f), new Vector2(0, 14), new Vector2(104, 44), ShowCredits);
        ImageButton(box, "Salir", "salir", bottom, new Vector2(0.5f, 0f), new Vector2(112, 14), new Vector2(104, 44), OnExit);
        panel.SetActive(false);

        // Créditos
        var cOverlay = UIFactory.Stretch(root, "Creditos");
        credits = cOverlay.gameObject;
        cOverlay.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.75f);
        var cBox = Panel(cOverlay, new Vector2(440, 320));
        UIFactory.Text(cBox, "Titulo", "Créditos", 22, TextAnchor.MiddleCenter, UIFactory.Gold,
                       new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -10), new Vector2(420, 30));
        UIFactory.Text(cBox, "Texto", CreditsText, 12, TextAnchor.UpperCenter, Color.white,
                       new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -44), new Vector2(420, 230));
        creditsBack = UIFactory.Button(cBox, "Volver", "Volver", new Vector2(0.5f, 0f), new Vector2(0, 24), new Vector2(120, 28), CloseCredits);
        credits.SetActive(false);

        Refresh();
    }

    private const string CreditsText =
        "LOQUITO DEL CENTRO\n\n" +
        "Pedro Uriel Pérez Monzón\n" +
        "Miguel Ángel Álvarez Ibarra\n\n" +
        "Universidad Tecnológica del Norte de Guanajuato\n" +
        "Ingeniería en Desarrollo y Gestión de Software\n" +
        "Creación de Videojuegos · Profesor: Gabriel Barrón Rodríguez\n\n" +
        "Recursos externos\n" +
        "GothicVania Church Pack · Luis Zuno (ansimuz)\n" +
        "Hecho con Unity 6";

    private static RectTransform Panel(Transform parent, Vector2 size)
    {
        var box = UIFactory.Rect(parent, "Panel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, size);
        box.gameObject.AddComponent<Image>().color = UIFactory.PanelColor;
        var o = box.gameObject.AddComponent<Outline>(); o.effectColor = Stone; o.effectDistance = new Vector2(2, -2);
        return box;
    }

    private static void Label(Transform parent, string text, Vector2 pos) =>
        UIFactory.Text(parent, "Etiqueta_" + text, text, 15, TextAnchor.MiddleLeft, Color.white,
                       new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f), pos, new Vector2(90, 24));

    private Image[] VolumeRow(Transform parent, string label, float y, System.Action<int> change)
    {
        Label(parent, label, new Vector2(-122, y));
        ImageButton(parent, "Menos_" + label, "menos", new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f), new Vector2(-50, y), new Vector2(28, 28), () => change(-1));
        ImageButton(parent, "Mas_" + label, "mas", new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f), new Vector2(140, y), new Vector2(28, 28), () => change(1));

        // Barra dibujada: marco de "piedra" y 10 segmentos
        var bar = UIFactory.Rect(parent, "Barra_" + label, new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f), new Vector2(45, y), new Vector2(146, 18));
        bar.gameObject.AddComponent<Image>().color = new Color(0.05f, 0.05f, 0.09f);
        var o = bar.gameObject.AddComponent<Outline>(); o.effectColor = Stone; o.effectDistance = new Vector2(2, -2);
        var segs = new Image[AudioManager.MaxSteps];
        for (int i = 0; i < segs.Length; i++)
            segs[i] = UIFactory.Image(bar, "Seg" + i, UIFactory.WhiteSprite, SegmentOff, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                                      new Vector2(4 + i * 14, 0), new Vector2(11, 10));
        return segs;
    }

    private static Button ImageButton(Transform parent, string name, string sprite, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size, UnityEngine.Events.UnityAction onClick)
    {
        var normal = Resources.Load<Sprite>("Menu/boton_" + sprite + "_normal");
        var red = Resources.Load<Sprite>("Menu/boton_" + sprite + "_rojo");
        var img = UIFactory.Image(parent, name, normal, Color.white, anchor, pivot, pos, size);
        img.raycastTarget = true;
        var b = img.gameObject.AddComponent<Button>();
        b.transition = Selectable.Transition.SpriteSwap;
        b.spriteState = new SpriteState { highlightedSprite = red, selectedSprite = red, pressedSprite = red };
        b.targetGraphic = img;
        b.onClick.AddListener(onClick);
        return b;
    }

    // ---------- Comportamiento ----------

    private void Refresh()
    {
        var a = AudioManager.Instance;
        for (int i = 0; i < musicBar.Length; i++) musicBar[i].color = i < a.MusicSteps ? SegmentOn : SegmentOff;
        for (int i = 0; i < sfxBar.Length; i++) sfxBar[i].color = i < a.SfxSteps ? SegmentOn : SegmentOff;
        muteLine.enabled = a.Muted;
        muteIcon.color = a.Muted ? new Color(0.55f, 0.55f, 0.6f) : Color.white;
    }

    private void ToggleMute()
    {
        AudioManager.Instance.ToggleMute();
        Refresh();
    }

    public void Open()
    {
        if (panel.activeSelf) return;
        Refresh();
        panel.SetActive(true);
        if (InLevel && !paused)
        {
            paused = true;
            savedTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            var p = GameObject.FindGameObjectWithTag("Player");
            pausedController = p != null ? p.GetComponent<PlayerController>() : null;
            if (pausedController != null) { savedInput = pausedController.InputEnabled; pausedController.InputEnabled = false; }
        }
        Select(playButton);
    }

    /// <summary>Cierra el menú y, si estaba en un nivel, reanuda donde se quedó.</summary>
    public void Resume()
    {
        panel.SetActive(false);
        credits.SetActive(false);
        if (!paused) return;
        paused = false;
        Time.timeScale = savedTimeScale;
        if (pausedController != null) StartCoroutine(RestoreInput(pausedController, savedInput));
    }

    private IEnumerator RestoreInput(PlayerController c, bool value)
    {
        // Un par de frames para que el botón con que se cerró el menú no cuente como salto o ataque.
        yield return null;
        yield return null;
        if (c != null) c.InputEnabled = value;
    }

    private void OnPlay()
    {
        if (InTitle) { Resume(); GameManager.Instance.NewGame(); }
        else Resume();
    }

    private void OnExit()
    {
        if (InTitle)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
            return;
        }
        panel.SetActive(false);
        credits.SetActive(false);
        paused = false;
        Time.timeScale = 1f;
        GameManager.Instance.GoToTitle();
    }

    /// <summary>Muestra los créditos (desde este menú o desde el menú de inicio).</summary>
    public void ShowCredits()
    {
        credits.SetActive(true);
        Select(creditsBack);
    }

    private void CloseCredits()
    {
        credits.SetActive(false);
        if (panel.activeSelf) Select(playButton);
    }

    private static void Select(Selectable s)
    {
        if (EventSystem.current != null && s != null) EventSystem.current.SetSelectedGameObject(s.gameObject);
    }
}
