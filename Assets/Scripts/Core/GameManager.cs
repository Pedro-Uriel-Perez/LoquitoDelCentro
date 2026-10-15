using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Estado global de la partida que sobrevive entre escenas: orden de niveles,
/// coleccionables obtenidos y habilidades del Loquito.
/// </summary>
public class GameManager : MonoBehaviour
{
    public const string TitleScene = "Inicio";
    public const string VictoryScene = "Victoria";
    public const int TotalCollectibles = 4;

    /// <summary>Orden de juego: índice = número de nivel (0 = El centro, 1..4 = mazmorras).</summary>
    public static readonly string[] LevelScenes = { "Nivel0", "Mazmorra1", "Mazmorra2", "Mazmorra3", "Mazmorra4" };

    private static GameManager instance;

    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                var go = new GameObject("GameManager");
                instance = go.AddComponent<GameManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    // Con "Enter Play Mode" sin recarga de dominio los estáticos sobreviven entre sesiones de Play.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => instance = null;

    private readonly HashSet<int> collectibles = new HashSet<int>();

    // Martha la Loca entrega estos objetos en el Nivel 0. Empiezan desbloqueados para poder probar cualquier
    // mazmorra directamente desde el editor; una partida nueva (o entrar al Nivel 0) los bloquea.
    public bool TieneCalcetas { get; set; } = true;
    public bool TieneCarton { get; set; } = true;
    public bool TieneBotella { get; set; } = true;
    public bool TalkedToMartha { get; set; }

    public void SetAbilities(bool unlocked)
    {
        TieneCalcetas = unlocked;
        TieneCarton = unlocked;
        TieneBotella = unlocked;
    }

    public int CollectibleCount => collectibles.Count;
    public bool AllCollectibles => collectibles.Count >= TotalCollectibles;
    public bool HasCollectible(int id) => collectibles.Contains(id);
    public void AddCollectible(int id) => collectibles.Add(id);

    public void NewGame()
    {
        collectibles.Clear();
        TalkedToMartha = false;
        SetAbilities(false);
        LoadScene(LevelScenes[0]);
    }

    /// <summary>Pasa al nivel siguiente al número indicado (0 = El centro, 1 = Mazmorra 1...), o a la victoria tras el último.</summary>
    public void CompleteLevel(int levelNumber)
    {
        int next = levelNumber + 1;
        if (next >= 0 && next < LevelScenes.Length) LoadScene(LevelScenes[next]);
        else LoadScene(VictoryScene);
    }

    public void GoToTitle() => LoadScene(TitleScene);

    public static void LoadScene(string sceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }
}
