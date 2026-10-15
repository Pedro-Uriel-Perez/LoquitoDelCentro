using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Animador ligero por cuadros con varios clips con nombre. Sirve para SpriteRenderer o Image (UI).
/// </summary>
public class SpriteAnim : MonoBehaviour
{
    [Serializable]
    public class Clip
    {
        public string name;
        public Sprite[] frames;
        public float fps = 10f;
        public bool loop = true;
    }

    [SerializeField] private SpriteRenderer spriteTarget;
    [SerializeField] private Image imageTarget;
    [SerializeField] private List<Clip> clips = new List<Clip>();
    [SerializeField] private string playOnStart;
    [Tooltip("Usar tiempo sin escala (sigue animando con el juego en pausa).")]
    [SerializeField] private bool unscaledTime;

    private Clip current;
    private float time;

    public string CurrentClip => current?.name;
    public bool IsFinished => current != null && !current.loop && time * current.fps >= current.frames.Length;

    private void Awake()
    {
        if (spriteTarget == null) spriteTarget = GetComponent<SpriteRenderer>();
        if (imageTarget == null) imageTarget = GetComponent<Image>();
        if (!string.IsNullOrEmpty(playOnStart)) Play(playOnStart);
    }

    public void SetClips(List<Clip> newClips, string startClip)
    {
        clips = newClips;
        playOnStart = startClip;
    }

    public float GetLength(string clipName)
    {
        var c = clips.Find(x => x.name == clipName);
        return c == null || c.fps <= 0f ? 0f : c.frames.Length / c.fps;
    }

    public void Play(string clipName, bool restart = true)
    {
        var c = clips.Find(x => x.name == clipName);
        if (c == null || c.frames == null || c.frames.Length == 0) return;
        if (!restart && current == c) return;
        current = c;
        time = 0f;
        Apply(0);
    }

    private void Update()
    {
        if (current == null) return;
        time += unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        int frame = (int)(time * current.fps);
        frame = current.loop ? frame % current.frames.Length : Mathf.Min(frame, current.frames.Length - 1);
        Apply(frame);
    }

    private void Apply(int frame)
    {
        Sprite s = current.frames[frame];
        if (spriteTarget != null) spriteTarget.sprite = s;
        if (imageTarget != null) imageTarget.sprite = s;
    }
}
