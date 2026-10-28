using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Música y efectos de sonido de todo el juego. Sobrevive entre escenas y elige la música de cada una.
/// Los audios se cargan por nombre desde Assets/Resources/Audio/Musica y Assets/Resources/Audio/SFX.
/// Volúmenes (0-10), silencio, canción del menú y final secreto se guardan aunque se cierre el juego.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public const int MaxSteps = 10;

    // Música
    public const string MusicaInicioFin = "Musica_Inicio_Fin";
    public const string MusicaFinalSecreto = "Musica_Final_Secreto";

    const string KeyMusic = "vol_musica", KeySfx = "vol_efectos", KeyMute = "silencio";
    const string KeySecret = "final_secreto_desbloqueado", KeyMenuSong = "musica_menu_secreta";

    private static AudioManager instance;

    public static AudioManager Instance
    {
        get
        {
            if (instance == null)
            {
                var go = new GameObject("AudioManager");
                instance = go.AddComponent<AudioManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => instance = null;

    // Se crea al arrancar el juego (en cualquier escena) para que suene la música desde el principio.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        Instance.PlaySceneMusic(SceneManager.GetActiveScene().name);
    }

    private AudioSource music, sfx, voice;
    private readonly Dictionary<string, AudioClip> cache = new Dictionary<string, AudioClip>();
    private string currentMusic;

    public int MusicSteps { get; private set; }
    public int SfxSteps { get; private set; }
    public bool Muted { get; private set; }
    public bool SecretUnlocked => PlayerPrefs.GetInt(KeySecret, 0) == 1;
    public bool MenuUsesSecretSong => SecretUnlocked && PlayerPrefs.GetInt(KeyMenuSong, 0) == 1;

    private void Awake()
    {
        music = gameObject.AddComponent<AudioSource>();
        music.loop = true;
        music.playOnAwake = false;
        sfx = gameObject.AddComponent<AudioSource>();
        sfx.playOnAwake = false;
        voice = gameObject.AddComponent<AudioSource>();
        voice.playOnAwake = false;

        MusicSteps = PlayerPrefs.GetInt(KeyMusic, 7);
        SfxSteps = PlayerPrefs.GetInt(KeySfx, 8);
        Muted = PlayerPrefs.GetInt(KeyMute, 0) == 1;
        ApplyVolumes();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => PlaySceneMusic(scene.name);

    // ---------- Música ----------

    /// <summary>Música de cada escena. La de Victoria la elige VictoryScreen (normal o secreta).</summary>
    public void PlaySceneMusic(string sceneName)
    {
        switch (sceneName)
        {
            case GameManager.TitleScene: PlayMusic(MenuUsesSecretSong ? MusicaFinalSecreto : MusicaInicioFin); break;
            case "Jardin": PlayMusic("Musica_Parque"); break;
            case "Mazmorra1": PlayMusic("Musica_Mazmorra1"); break;
            case "Mazmorra2": PlayMusic("Musica_Mazmorra2"); break;
            case "Mazmorra3": PlayMusic("Musica_Mazmorra3"); break;
        }
    }

    public void PlayMusic(string name)
    {
        if (currentMusic == name && music.isPlaying) return;
        var clip = Load("Musica/" + name);
        if (clip == null) return;
        currentMusic = name;
        music.clip = clip;
        music.Play();
    }

    public void StopMusic()
    {
        music.Stop();
        currentMusic = null;
    }

    /// <summary>Corta música, efectos y voces (silencio total).</summary>
    public void StopAll()
    {
        StopMusic();
        sfx.Stop();
        voice.Stop();
    }

    /// <summary>Música del jefe del nivel actual (Mazmorra N → Musica_JefeN).</summary>
    public void PlayBossMusic()
    {
        string scene = SceneManager.GetActiveScene().name;
        if (scene.StartsWith("Mazmorra")) PlayMusic("Musica_Jefe" + scene.Substring("Mazmorra".Length));
    }

    // ---------- Efectos ----------

    public void PlaySfx(string name, float volume = 1f)
    {
        var clip = Load("SFX/" + name);
        if (clip != null) sfx.PlayOneShot(clip, volume);
    }

    /// <summary>Efecto de algo que está en el mundo: solo suena si está cerca de la cámara y baja con la distancia.</summary>
    public void PlaySfxAt(string name, Vector3 position, float maxDistance = 16f)
    {
        var cam = Camera.main;
        if (cam == null) { PlaySfx(name); return; }
        float d = Vector2.Distance(cam.transform.position, position);
        if (d > maxDistance) return;
        PlaySfx(name, Mathf.Lerp(1f, 0.35f, d / maxDistance));
    }

    /// <summary>Voz de diálogo: corta la anterior para que no se encimen al avanzar rápido.</summary>
    public void PlayVoice(string name)
    {
        var clip = Load("SFX/" + name);
        if (clip == null) return;
        voice.Stop();
        voice.clip = clip;
        voice.Play();
    }

    // ---------- Volumen y opciones ----------

    public void ChangeMusic(int delta) { MusicSteps = Mathf.Clamp(MusicSteps + delta, 0, MaxSteps); Save(); }
    public void ChangeSfx(int delta) { SfxSteps = Mathf.Clamp(SfxSteps + delta, 0, MaxSteps); Save(); }
    public void ToggleMute() { Muted = !Muted; Save(); }

    public void UnlockSecretEnding()
    {
        PlayerPrefs.SetInt(KeySecret, 1);
        PlayerPrefs.Save();
    }

    /// <summary>Elige la canción del menú de inicio (solo si ya se vio el final secreto).</summary>
    public void SetMenuSong(bool secret)
    {
        PlayerPrefs.SetInt(KeyMenuSong, secret && SecretUnlocked ? 1 : 0);
        PlayerPrefs.Save();
        if (SceneManager.GetActiveScene().name == GameManager.TitleScene) PlaySceneMusic(GameManager.TitleScene);
    }

    private void Save()
    {
        PlayerPrefs.SetInt(KeyMusic, MusicSteps);
        PlayerPrefs.SetInt(KeySfx, SfxSteps);
        PlayerPrefs.SetInt(KeyMute, Muted ? 1 : 0);
        PlayerPrefs.Save();
        ApplyVolumes();
    }

    private void ApplyVolumes()
    {
        float m = Muted ? 0f : MusicSteps / (float)MaxSteps;
        float s = Muted ? 0f : SfxSteps / (float)MaxSteps;
        music.volume = m;
        sfx.volume = s;
        voice.volume = s;
    }

    private AudioClip Load(string path)
    {
        if (cache.TryGetValue(path, out var clip)) return clip;
        clip = Resources.Load<AudioClip>("Audio/" + path);
        if (clip == null) Debug.LogWarning("[Audio] No se encontró Audio/" + path);
        cache[path] = clip;
        return clip;
    }
}
