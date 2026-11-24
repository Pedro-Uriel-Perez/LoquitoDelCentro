using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Botón táctil: presionado mientras al menos un dedo esté encima (varios dedos a la vez funcionan).
/// Si el dedo se desliza de un botón a otro, se suelta el primero y se presiona el segundo.
/// </summary>
public class TouchButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    private readonly HashSet<int> pointers = new HashSet<int>();
    private bool pendingPress, pendingRelease;
    private Image image;

    public bool Held => pointers.Count > 0;
    public bool PressedThisFrame { get; private set; }
    public bool ReleasedThisFrame { get; private set; }

    private void Awake() => image = GetComponent<Image>();

    public void OnPointerDown(PointerEventData e) => Press(e.pointerId);
    public void OnPointerUp(PointerEventData e) => Release(e.pointerId);
    public void OnPointerExit(PointerEventData e) => Release(e.pointerId);
    public void OnPointerEnter(PointerEventData e) { if (e.eligibleForClick || e.dragging) Press(e.pointerId); }

    private void Press(int id)
    {
        if (pointers.Add(id) && pointers.Count == 1) pendingPress = true;
        image.color = TouchControls.PressedColor;
    }

    private void Release(int id)
    {
        if (pointers.Remove(id) && pointers.Count == 0) pendingRelease = true;
        if (pointers.Count == 0) image.color = TouchControls.ButtonColor;
    }

    private void OnDisable()
    {
        pointers.Clear();
        pendingPress = pendingRelease = false;
        if (image != null) image.color = TouchControls.ButtonColor;
    }

    public void Latch()
    {
        PressedThisFrame = pendingPress;
        ReleasedThisFrame = pendingRelease;
        pendingPress = pendingRelease = false;
    }
}
