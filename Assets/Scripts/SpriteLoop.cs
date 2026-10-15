using UnityEngine;

/// <summary>
/// Animación ligera por cuadros para efectos simples (proyectiles, antorchas) sin Animator.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteLoop : MonoBehaviour
{
    [SerializeField] private Sprite[] frames;
    [SerializeField] private float framesPerSecond = 12f;

    private SpriteRenderer sr;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (frames == null || frames.Length == 0) return;
        int index = (int)(Time.time * framesPerSecond) % frames.Length;
        sr.sprite = frames[index];
    }
}
