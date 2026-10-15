using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

/// <summary>
/// Utilidades para construir la interfaz por código (HUD, menú de inicio, pantallas de victoria).
/// Resolución de referencia 640x360 para que todo escale igual que el juego.
/// </summary>
public static class UIFactory
{
    public static readonly Color Gold = new Color(1f, 0.85f, 0.35f);
    public static readonly Color PanelColor = new Color(0.08f, 0.06f, 0.12f, 0.92f);

    private static Font font;
    public static Font DefaultFont => font != null ? font : (font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"));

    public static Canvas CreateCanvas(string name, int sortingOrder)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;
        var scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(640, 360);
        scaler.matchWidthOrHeight = 0.5f;
        return canvas;
    }

    public static void EnsureEventSystem()
    {
        if (Object.FindFirstObjectByType<EventSystem>() != null) return;
        new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
    }

    public static RectTransform Rect(Transform parent, string name, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        rt.anchorMin = rt.anchorMax = anchor;
        rt.pivot = pivot;
        rt.anchoredPosition = position;
        rt.sizeDelta = size;
        return rt;
    }

    public static RectTransform Stretch(Transform parent, string name)
    {
        var rt = Rect(parent, name, Vector2.zero, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        return rt;
    }

    public static Image Image(Transform parent, string name, Sprite sprite, Color color, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
    {
        var rt = Rect(parent, name, anchor, pivot, position, size);
        var img = rt.gameObject.AddComponent<Image>();
        img.sprite = sprite;
        img.color = color;
        img.preserveAspect = sprite != null;
        img.raycastTarget = false;
        return img;
    }

    public static Text Text(Transform parent, string name, string content, int fontSize, TextAnchor align, Color color,
                            Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
    {
        var rt = Rect(parent, name, anchor, pivot, position, size);
        var t = rt.gameObject.AddComponent<Text>();
        t.font = DefaultFont;
        t.text = content;
        t.fontSize = fontSize;
        t.alignment = align;
        t.color = color;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        var outline = rt.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.9f);
        outline.effectDistance = new Vector2(1.5f, -1.5f);
        return t;
    }

    public static Button Button(Transform parent, string name, string label, Vector2 anchor, Vector2 position, Vector2 size, UnityAction onClick)
    {
        var rt = Rect(parent, name, anchor, new Vector2(0.5f, 0.5f), position, size);
        var img = rt.gameObject.AddComponent<Image>();
        img.color = new Color(0.25f, 0.18f, 0.32f, 1f);
        var outline = rt.gameObject.AddComponent<Outline>();
        outline.effectColor = Gold;
        outline.effectDistance = new Vector2(1.5f, -1.5f);
        var button = rt.gameObject.AddComponent<Button>();
        var colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.6f, 1.4f, 1.1f);
        colors.selectedColor = new Color(1.6f, 1.4f, 1.1f);
        colors.pressedColor = new Color(0.8f, 0.7f, 0.6f);
        button.colors = colors;
        button.onClick.AddListener(onClick);
        Text(rt, "Texto", label, 16, TextAnchor.MiddleCenter, Color.white, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, size);
        return button;
    }
}
