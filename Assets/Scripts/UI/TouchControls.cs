using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Controles táctiles para celular (Android y navegador móvil). Solo aparecen dentro de los niveles.
/// ◀ ▶ = moverse, Saltar, Atacar, Cartón (se mantiene presionado) e Interactuar. La configuración se abre con el engrane.
/// Los botones guardan su estado aquí y LoquitoInput lo suma al teclado y al control (sin control virtual
/// de por medio, así los menús no reaccionan a los botones de juego).
/// </summary>
[DefaultExecutionOrder(-200)]   // antes que el Loquito, para que lea el estado ya actualizado en el mismo cuadro
public class TouchControls : MonoBehaviour
{
    private static TouchControls instance;
    private GameObject root;

    public static readonly Color ButtonColor = new Color(1f, 1f, 1f, 0.28f);
    public static readonly Color PressedColor = new Color(1f, 1f, 1f, 0.55f);
    private static readonly Color IconColor = new Color(1f, 1f, 1f, 0.85f);

    /// <summary>Estado de los botones táctiles (cero / null si no hay controles táctiles).</summary>
    public static float Horizontal => instance != null && instance.root.activeInHierarchy
        ? (instance.right.Held ? 1f : 0f) - (instance.left.Held ? 1f : 0f) : 0f;
    public static TouchButton Jump => instance != null ? instance.jump : null;
    public static TouchButton Attack => instance != null ? instance.attack : null;
    public static TouchButton Block => instance != null ? instance.block : null;
    public static TouchButton Interact => instance != null ? instance.interact : null;

    private TouchButton left, right, jump, attack, block, interact;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => instance = null;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        // En el editor se ven con el Device Simulator (Window > General > Device Simulator)
        if (instance != null || !Application.isMobilePlatform) return;
        var go = new GameObject("ControlesTactiles");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<TouchControls>();
    }

    private static bool InLevel
    {
        get { string n = SceneManager.GetActiveScene().name; return n == "Jardin" || n.StartsWith("Mazmorra"); }
    }

    private void Awake()
    {
        Build();
        SceneManager.sceneLoaded += OnSceneLoaded;
        root.SetActive(InLevel);
    }

    private void OnDestroy() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene s, LoadSceneMode m)
    {
        root.SetActive(InLevel);
        UIFactory.EnsureEventSystem();
    }

    // Los toques llegan durante el cuadro; aquí se fijan al inicio del siguiente para que todos los scripts los vean igual
    private void Update()
    {
        left.Latch(); right.Latch(); jump.Latch(); attack.Latch(); block.Latch(); interact.Latch();
    }

    private void Build()
    {
        var canvas = UIFactory.CreateCanvas("Tactil_Canvas", 50);   // debajo del menú de configuración (100)
        canvas.transform.SetParent(transform, false);
        root = canvas.gameObject;
        var t = canvas.transform;
        Sprite circle = CircleSprite();
        Sprite arrow = ArrowSprite();

        // Izquierda: moverse
        var l = new Vector2(0f, 0f);
        left = Button(t, "Izquierda", l, new Vector2(46, 46), 64, circle, arrow, null, 180f);
        right = Button(t, "Derecha", l, new Vector2(124, 46), 64, circle, arrow, null, 0f);

        // Derecha: acciones
        var r = new Vector2(1f, 0f);
        jump = Button(t, "Saltar", r, new Vector2(-50, 50), 70, circle, arrow, null, 90f);
        attack = Button(t, "Atacar", r, new Vector2(-128, 40), 56, circle, Icon("Menu/icono_botella"), "J", 0f);
        block = Button(t, "Carton", r, new Vector2(-122, 108), 50, circle, Icon("Menu/icono_carton"), "K", 0f);
        interact = Button(t, "Interactuar", r, new Vector2(-52, 128), 46, circle, null, "E", 0f);
    }

    /// <summary>Botón redondo semitransparente con un ícono (o letra si no hay ícono).</summary>
    private static TouchButton Button(Transform parent, string name, Vector2 anchor, Vector2 pos, float size,
                                      Sprite circle, Sprite icon, string fallbackText, float iconRotation)
    {
        var bg = UIFactory.Image(parent, name, circle, ButtonColor, anchor, new Vector2(0.5f, 0.5f), pos, new Vector2(size, size));
        bg.raycastTarget = true;   // UIFactory crea las imágenes sin detectar toques; el botón sí los necesita
        var go = bg.gameObject;

        if (icon != null)
        {
            var img = UIFactory.Image(go.transform, "Icono", icon, IconColor, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero,
                                      new Vector2(size * 0.55f, size * 0.55f));
            img.preserveAspect = true;
            img.raycastTarget = false;
            img.rectTransform.localRotation = Quaternion.Euler(0f, 0f, iconRotation);
        }
        else if (fallbackText != null)
        {
            var txt = UIFactory.Text(go.transform, "Letra", fallbackText, Mathf.RoundToInt(size * 0.45f), TextAnchor.MiddleCenter, IconColor,
                                     new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(size, size));
            txt.raycastTarget = false;
        }
        return go.AddComponent<TouchButton>();
    }

    /// <summary>Ícono desde Resources como textura (no depende de cómo se importe la imagen).</summary>
    private static Sprite Icon(string path)
    {
        var tex = Resources.Load<Texture2D>(path);
        return tex != null ? Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f) : null;
    }

    private static Sprite CircleSprite()
    {
        const int n = 64;
        var tex = new Texture2D(n, n, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
        var px = new Color32[n * n];
        float r = n * 0.5f - 1f;
        for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(n * 0.5f, n * 0.5f));
                float a = Mathf.Clamp01(r - d + 0.5f);                       // borde suave
                float ring = d > r - 3f ? 1f : 0.55f;                         // aro más marcado
                px[y * n + x] = new Color32(255, 255, 255, (byte)(a * ring * 255));
            }
        tex.SetPixels32(px);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), 100f);
    }

    /// <summary>Triángulo que apunta a la derecha (se rota para las demás direcciones).</summary>
    private static Sprite ArrowSprite()
    {
        const int n = 64;
        var tex = new Texture2D(n, n, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
        var px = new Color32[n * n];
        for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float u = (x + 0.5f) / n, v = Mathf.Abs((y + 0.5f) / n - 0.5f) * 2f;   // v: 0 al centro, 1 en los bordes
                bool inside = u > 0.15f && u < 0.9f && v < (0.9f - u) / 0.75f;
                px[y * n + x] = inside ? new Color32(255, 255, 255, 255) : new Color32(255, 255, 255, 0);
            }
        tex.SetPixels32(px);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), 100f);
    }
}
