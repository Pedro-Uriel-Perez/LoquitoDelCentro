using UnityEngine;

/// <summary>
/// HUD provisional para pruebas: panalitos y durabilidad del cartón.
/// Se reemplaza por el HUD definitivo en el sprint 3.
/// </summary>
public class DebugHUD : MonoBehaviour
{
    [SerializeField] private PlayerHealth health;
    [SerializeField] private PlayerCombat combat;

    private GUIStyle style;

    private void OnGUI()
    {
        if (health == null) return;
        style ??= new GUIStyle(GUI.skin.label) { fontSize = 18, fontStyle = FontStyle.Bold };

        string hearts = new string('♥', health.Hearts) + new string('·', health.MaxHearts - health.Hearts);
        string carton = combat == null ? "" :
            combat.CartonRoto ? $"Cartón ROTO ({combat.RecoveryRemaining:0.0}s)" :
            $"Cartón {combat.Durability}/{combat.MaxDurability}";

        GUI.color = Color.black;
        GUI.Label(new Rect(12, 10, 400, 30), $"Panalitos {hearts}", style);
        GUI.Label(new Rect(12, 34, 400, 30), carton, style);
        GUI.color = new Color(1f, 0.85f, 0.4f);
        GUI.Label(new Rect(10, 8, 400, 30), $"Panalitos {hearts}", style);
        GUI.Label(new Rect(10, 32, 400, 30), carton, style);
    }
}
