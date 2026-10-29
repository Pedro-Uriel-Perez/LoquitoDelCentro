using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Pantalla de inicio: ilustración de fondo, título y botones Jugar / Créditos / Salir.
/// Después de ver el final secreto aparece una opción para elegir la canción del menú.
/// La configuración (engrane de la esquina) la pone SettingsMenu.
/// </summary>
public class TitleMenu : MonoBehaviour
{
    private Text songLabel;

    private void Start()
    {
        Time.timeScale = 1f;
        var canvas = UIFactory.CreateCanvas("Inicio_Canvas", 0);
        var root = canvas.transform;

        // Fondo: cubre toda la pantalla sin deformarse (recorta lo que sobre)
        var bgRect = UIFactory.Stretch(root, "Fondo");
        var bg = bgRect.gameObject.AddComponent<Image>();
        bg.sprite = Resources.Load<Sprite>("Menu/menu_fondo");
        bg.raycastTarget = false;
        var fitter = bgRect.gameObject.AddComponent<AspectRatioFitter>();
        fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
        fitter.aspectRatio = bg.sprite != null ? bg.sprite.rect.width / bg.sprite.rect.height : 4f / 3f;

        // Logo: ocupa el espacio entre el borde de arriba y el botón Jugar, sin deformarse
        var logo = UIFactory.Image(root, "Logo", Resources.Load<Sprite>("Menu/menu_logo"), Color.white,
                                   new Vector2(0.24f, 1f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        logo.preserveAspect = true;
        logo.raycastTarget = false;
        var logoRect = logo.rectTransform;
        logoRect.anchorMin = new Vector2(0.24f, 0.5f);
        logoRect.anchorMax = new Vector2(0.24f, 1f);
        logoRect.offsetMin = new Vector2(-135, 40);
        logoRect.offsetMax = new Vector2(135, -6);

        var anchor = new Vector2(0.24f, 0.5f);
        var jugar = MenuButton(root, "Jugar", "jugar", anchor, new Vector2(0, 0), () => GameManager.Instance.NewGame());
        MenuButton(root, "Creditos", "creditos", anchor, new Vector2(0, -58), () => SettingsMenu.Instance.ShowCredits());
        MenuButton(root, "Salir", "salir", anchor, new Vector2(0, -116), Quit);

        if (AudioManager.Instance.SecretUnlocked)
        {
            var row = UIFactory.Rect(root, "MusicaMenu", new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-10, 30), new Vector2(240, 26));
            row.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.55f);
            songLabel = UIFactory.Text(row, "Texto", "", 12, TextAnchor.MiddleCenter, Color.white,
                                       new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(170, 24));
            UIFactory.Button(row, "Anterior", "<", new Vector2(0f, 0.5f), new Vector2(14, 0), new Vector2(24, 22), ToggleSong);
            UIFactory.Button(row, "Siguiente", ">", new Vector2(1f, 0.5f), new Vector2(-14, 0), new Vector2(24, 22), ToggleSong);
            RefreshSong();
        }

        UIFactory.Text(root, "Controles", "Mover: A/D · Saltar: Espacio · Botella: J · Cartón: K · Interactuar: E · Configuración: Esc",
                       11, TextAnchor.MiddleCenter, new Color(0.85f, 0.85f, 0.95f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 6), new Vector2(620, 20));

        UIFactory.EnsureEventSystem();
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(jugar.gameObject);
    }

    private static Button MenuButton(Transform parent, string name, string sprite, Vector2 anchor, Vector2 pos, UnityEngine.Events.UnityAction onClick)
    {
        var img = UIFactory.Image(parent, name, Resources.Load<Sprite>("Menu/boton_" + sprite + "_normal"), Color.white,
                                  anchor, new Vector2(0.5f, 0.5f), pos, new Vector2(150, 64));
        img.raycastTarget = true;
        var b = img.gameObject.AddComponent<Button>();
        var red = Resources.Load<Sprite>("Menu/boton_" + sprite + "_rojo");
        b.transition = Selectable.Transition.SpriteSwap;
        b.spriteState = new SpriteState { highlightedSprite = red, selectedSprite = red, pressedSprite = red };
        b.targetGraphic = img;
        b.onClick.AddListener(onClick);
        return b;
    }

    private void ToggleSong()
    {
        AudioManager.Instance.SetMenuSong(!AudioManager.Instance.MenuUsesSecretSong);
        RefreshSong();
    }

    private void RefreshSong()
    {
        if (songLabel != null)
            songLabel.text = "Música del menú: " + (AudioManager.Instance.MenuUsesSecretSong ? "Final secreto" : "Inicio");
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
