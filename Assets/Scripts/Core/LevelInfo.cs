using UnityEngine;

/// <summary>
/// Datos del nivel actual (uno por escena de mazmorra).
/// </summary>
public class LevelInfo : MonoBehaviour
{
    [SerializeField] private int levelNumber = 1;
    [SerializeField] private string levelName = "Templo del Reloj";

    public int LevelNumber => levelNumber;
    public string LevelName => levelName;

    public static LevelInfo Current { get; private set; }

    private void Awake() => Current = this;

    private void OnDestroy()
    {
        if (Current == this) Current = null;
    }
}
