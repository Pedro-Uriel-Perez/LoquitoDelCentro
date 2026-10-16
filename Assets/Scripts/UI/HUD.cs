using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// HUD de las mazmorras: cara del Loquito, panalitos (vida), cartón, barra del jefe, avisos
/// y la ventana para recoger coleccionables. Se construye por código al iniciar.
/// </summary>
public class HUD : MonoBehaviour
{
    [SerializeField] private Sprite portrait;
    [SerializeField] private Sprite heartSprite;
    [SerializeField] private Sprite[] heartVanishFrames;
    [SerializeField] private Sprite dialogueBubble;

    public static HUD Instance { get; private set; }

    private PlayerHealth health;
    private PlayerCombat combat;
    private PlayerController controller;

    private Image[] hearts = new Image[0];
    private int shownHearts;
    private Text cartonText, promptText, messageText;
    private GameObject bossBar;
    private Image bossFill;
    private Text bossName;
    private GameObject popup;
    private Image popupImage;
    private Text popupTitle, popupDescription;
    private Button popupButton;
    private Action popupCallback;
    private object promptOwner;
    private Coroutine messageRoutine;
    private bool reenableAfterPopup = true;
    private string popupCollectedMessage;
    private RectTransform canvasRect;
    private RectTransform dialogue;
    private Text dialogueText;
    private Transform dialogueAnchor;

    private static readonly Color EmptyHeart = new Color(0.25f, 0.22f, 0.3f, 0.55f);

    private void Awake()
    {
        Instance = this;
        Build();
    }

    private void Start()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            health = player.GetComponent<PlayerHealth>();
            combat = player.GetComponent<PlayerCombat>();
            controller = player.GetComponent<PlayerController>();
        }
        if (health != null)
        {
            BuildHearts(health.MaxHearts);
            shownHearts = health.Hearts;
            for (int i = 0; i < hearts.Length; i++) SetHeart(i, i < shownHearts);
            health.OnHealthChanged += OnHealthChanged;
            health.OnDeath += OnPlayerDeath;
        }
        if (LevelInfo.Current != null)
        {
            var lvl = LevelInfo.Current;
            ShowMessage(lvl.LevelNumber == 0 ? lvl.LevelName : $"Mazmorra {lvl.LevelNumber}\n{lvl.LevelName}", 3f);
        }
    }

    private void OnDestroy()
    {
        if (health != null)
        {
            health.OnHealthChanged -= OnHealthChanged;
            health.OnDeath -= OnPlayerDeath;
        }
        if (Instance == this) Instance = null;
    }

    private void Update()
    {
        if (combat == null || cartonText == null) return;
        cartonText.text = combat.CartonRoto
            ? $"Cartón roto ({combat.RecoveryRemaining:0}s)"
            : $"Cartón {combat.Durability}/{combat.MaxDurability}";
    }

    // ---------- Construcción ----------

    private void Build()
    {
        var canvas = UIFactory.CreateCanvas("HUD_Canvas", 10);
        canvas.transform.SetParent(transform, false);
        var root = canvas.transform;
        canvasRect = (RectTransform)root;

        // Nube de diálogo (se posiciona sobre quien habla)
        dialogue = UIFactory.Rect(root, "Dialogo", new Vector2(0.5f, 0.5f), new Vector2(0.85f, 0f), Vector2.zero, new Vector2(270, 170));
        var cloud = dialogue.gameObject.AddComponent<Image>();
        cloud.sprite = dialogueBubble;
        cloud.color = dialogueBubble != null ? Color.white : new Color(1f, 1f, 1f, 0.95f);
        cloud.raycastTarget = false;
        dialogueText = UIFactory.Text(dialogue, "Texto", "", 14, TextAnchor.MiddleCenter, new Color(0.1f, 0.08f, 0.12f),
                                      new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-8, 20), new Vector2(210, 100));
        dialogueText.GetComponent<Outline>().enabled = false;
        UIFactory.Text(dialogue, "Continuar", "E / Y >>", 11, TextAnchor.MiddleRight, new Color(0.35f, 0.3f, 0.4f),
                       new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-28, 36), new Vector2(150, 16)).GetComponent<Outline>().enabled = false;
        dialogue.gameObject.SetActive(false);
        var topLeft = new Vector2(0f, 1f);

        UIFactory.Image(root, "Cara", portrait, Color.white, topLeft, topLeft, new Vector2(8, -6), new Vector2(42, 42));
        var heartsRow = UIFactory.Rect(root, "Panalitos", topLeft, topLeft, new Vector2(54, -8), new Vector2(200, 26));
        var layout = heartsRow.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 3;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        cartonText = UIFactory.Text(root, "Carton", "", 12, TextAnchor.UpperLeft, new Color(0.85f, 0.7f, 0.45f), topLeft, topLeft, new Vector2(56, -34), new Vector2(220, 18));

        promptText = UIFactory.Text(root, "Aviso", "", 14, TextAnchor.MiddleCenter, Color.white, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 34), new Vector2(500, 24));
        messageText = UIFactory.Text(root, "Mensaje", "", 22, TextAnchor.MiddleCenter, UIFactory.Gold, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 70), new Vector2(560, 80));

        // Barra del jefe
        var bar = UIFactory.Rect(root, "BarraJefe", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 8), new Vector2(300, 22));
        bossBar = bar.gameObject;
        var bg = bar.gameObject.AddComponent<Image>(); bg.color = new Color(0, 0, 0, 0.75f);
        bossFill = UIFactory.Image(bar, "Relleno", null, new Color(0.75f, 0.15f, 0.15f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(3, 0), new Vector2(294, 8));
        bossFill.sprite = UIFactory.WhiteSprite;
        bossFill.preserveAspect = false;
        bossFill.type = Image.Type.Filled;
        bossFill.fillMethod = Image.FillMethod.Horizontal;
        bossFill.rectTransform.anchoredPosition = new Vector2(3, -4);
        bossName = UIFactory.Text(bar, "Nombre", "", 11, TextAnchor.MiddleCenter, Color.white, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -1), new Vector2(300, 14));
        bossBar.SetActive(false);

        // Ventana de coleccionable
        var overlay = UIFactory.Stretch(root, "Coleccionable");
        popup = overlay.gameObject;
        overlay.gameObject.AddComponent<Image>().color = new Color(0, 0, 0, 0.6f);
        var panel = UIFactory.Rect(overlay, "Panel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(360, 250));
        panel.gameObject.AddComponent<Image>().color = UIFactory.PanelColor;
        var pOutline = panel.gameObject.AddComponent<Outline>(); pOutline.effectColor = UIFactory.Gold; pOutline.effectDistance = new Vector2(2, -2);
        popupTitle = UIFactory.Text(panel, "Titulo", "", 20, TextAnchor.MiddleCenter, UIFactory.Gold, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -8), new Vector2(340, 28));
        popupImage = UIFactory.Image(panel, "Imagen", null, Color.white, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -40), new Vector2(110, 100));
        popupDescription = UIFactory.Text(panel, "Descripcion", "", 14, TextAnchor.UpperCenter, Color.white, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -146), new Vector2(330, 50));
        popupButton = UIFactory.Button(panel, "Recoger", "Recoger", new Vector2(0.5f, 0f), new Vector2(0, 26), new Vector2(130, 30), OnCollectPressed);
        popup.SetActive(false);
        UIFactory.EnsureEventSystem();
    }

    private void BuildHearts(int count)
    {
        var row = transform.Find("HUD_Canvas/Panalitos");
        hearts = new Image[count];
        for (int i = 0; i < count; i++)
        {
            var img = UIFactory.Image(row, "Panalito" + (i + 1), heartSprite, Color.white, new Vector2(0, 1), new Vector2(0, 1), Vector2.zero, new Vector2(14, 26));
            var le = img.gameObject.AddComponent<LayoutElement>();
            le.preferredWidth = 14; le.preferredHeight = 26;
            hearts[i] = img;
        }
    }

    // ---------- Vida ----------

    private void SetHeart(int i, bool full)
    {
        hearts[i].sprite = heartSprite;
        hearts[i].color = full ? Color.white : EmptyHeart;
        hearts[i].rectTransform.localScale = Vector3.one;
    }

    private void OnHealthChanged()
    {
        int h = health.Hearts;
        for (int i = 0; i < hearts.Length; i++)
        {
            if (i < h && i >= shownHearts) StartCoroutine(Pop(i));
            else if (i >= h && i < shownHearts) StartCoroutine(Vanish(i));
        }
        shownHearts = h;
    }

    private IEnumerator Vanish(int i)
    {
        foreach (var f in heartVanishFrames)
        {
            hearts[i].sprite = f;
            hearts[i].color = Color.white;
            yield return new WaitForSecondsRealtime(0.09f);
        }
        if (i >= shownHearts) SetHeart(i, false);
    }

    private IEnumerator Pop(int i)
    {
        SetHeart(i, true);
        for (float t = 0f; t < 0.25f; t += Time.unscaledDeltaTime)
        {
            hearts[i].rectTransform.localScale = Vector3.one * (1f + 0.5f * Mathf.Sin(t / 0.25f * Mathf.PI));
            yield return null;
        }
        hearts[i].rectTransform.localScale = Vector3.one;
    }

    private void OnPlayerDeath() => ShowMessage("¡Te noquearon!\nDe vuelta a la fogata...", 1.5f);

    // ---------- Mensajes y avisos ----------

    public void ShowMessage(string text, float seconds = 2f)
    {
        if (messageRoutine != null) StopCoroutine(messageRoutine);
        messageRoutine = StartCoroutine(MessageRoutine(text, seconds));
    }

    private IEnumerator MessageRoutine(string text, float seconds)
    {
        messageText.text = text;
        messageText.color = UIFactory.Gold;
        yield return new WaitForSecondsRealtime(seconds);
        for (float t = 0f; t < 0.5f; t += Time.unscaledDeltaTime)
        {
            var c = UIFactory.Gold; c.a = 1f - t / 0.5f; messageText.color = c;
            yield return null;
        }
        messageText.text = "";
    }

    /// <summary>Muestra un aviso abajo (p. ej. "E: derribar la antena"). Pasar null lo oculta si era de ese dueño.</summary>
    public void SetPrompt(object owner, string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            if (promptOwner == owner) { promptText.text = ""; promptOwner = null; }
            return;
        }
        promptOwner = owner;
        promptText.text = text;
    }

    // ---------- Jefe ----------

    public void ShowBossBar(string name)
    {
        bossName.text = name;
        bossFill.fillAmount = 1f;
        bossBar.SetActive(true);
    }

    public void SetBossHealth(float fraction) => bossFill.fillAmount = Mathf.Clamp01(fraction);
    public void HideBossBar() => bossBar.SetActive(false);

    // ---------- Diálogos ----------

    public bool DialogueVisible => dialogue != null && dialogue.gameObject.activeSelf;

    /// <summary>Muestra la nube de diálogo apuntando a <paramref name="anchor"/> (p. ej. la cabeza del NPC).</summary>
    public void ShowDialogue(Transform anchor, string text)
    {
        if (messageRoutine != null) StopCoroutine(messageRoutine);
        messageText.text = "";
        dialogueAnchor = anchor;
        dialogueText.text = text;
        dialogue.gameObject.SetActive(true);
        PositionDialogue();
    }

    public void HideDialogue()
    {
        dialogue.gameObject.SetActive(false);
        dialogueAnchor = null;
    }

    private void LateUpdate()
    {
        if (DialogueVisible) PositionDialogue();
    }

    private void PositionDialogue()
    {
        if (dialogueAnchor == null || Camera.main == null) return;
        Vector2 screen = Camera.main.WorldToScreenPoint(dialogueAnchor.position);
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screen, null, out Vector2 local)) return;
        // Mantener la nube dentro de la pantalla
        Vector2 half = canvasRect.rect.size * 0.5f;
        Vector2 size = dialogue.sizeDelta;
        local.x = Mathf.Clamp(local.x, -half.x + size.x * 0.85f + 4, half.x - size.x * 0.15f - 4);
        local.y = Mathf.Clamp(local.y, -half.y + 4, half.y - size.y - 4);
        dialogue.anchoredPosition = local;
    }

    // ---------- Coleccionables y objetos ----------

    /// <summary>
    /// Muestra un objeto en grande con su descripción y el botón "Recoger". Pausa el juego hasta recogerlo.
    /// Con <paramref name="reenableInput"/> en false, quien lo llamó decide cuándo devolver el control (p. ej. un diálogo).
    /// </summary>
    public void ShowCollectible(Sprite sprite, string title, string description, Action onCollect, bool reenableInput = true,
                                string collectedMessage = "¡Coleccionable guardado!")
    {
        reenableAfterPopup = reenableInput;
        popupCollectedMessage = collectedMessage;
        popupImage.sprite = sprite;
        popupTitle.text = title;
        popupDescription.text = description;
        popupCallback = onCollect;
        popup.SetActive(true);
        Time.timeScale = 0f;
        if (controller != null) controller.InputEnabled = false;
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(popupButton.gameObject);
    }

    private void OnCollectPressed()
    {
        popup.SetActive(false);
        Time.timeScale = 1f;
        var callback = popupCallback;
        popupCallback = null;
        if (!string.IsNullOrEmpty(popupCollectedMessage)) ShowMessage(popupCollectedMessage, 1.5f);
        if (reenableAfterPopup) StartCoroutine(ReenableInput());
        callback?.Invoke();
    }

    private IEnumerator ReenableInput()
    {
        // Un frame de espera para que el mismo botón de "Recoger" no cuente como salto.
        yield return null;
        yield return null;
        if (controller != null) controller.InputEnabled = true;
    }
}
