using UnityEngine;

/// <summary>
/// Muro falso: se ve igual que la pared, pero se rompe con un botellazo y revela un cuarto secreto.
/// Usa Health (1 de vida) para recibir el golpe de la botella.
/// </summary>
[RequireComponent(typeof(Health))]
public class SecretWall : MonoBehaviour
{
    [Tooltip("Mensaje al romperlo. Vacío = sin mensaje.")]
    [SerializeField] private string message = "¡Un cuarto secreto!";

    private Health health;

    private void Awake()
    {
        health = GetComponent<Health>();
        health.OnDeath += OnBroken;
    }

    private void OnDestroy()
    {
        if (health != null) health.OnDeath -= OnBroken;
    }

    private void OnBroken()
    {
        if (HUD.Instance != null && !string.IsNullOrEmpty(message)) HUD.Instance.ShowMessage(message, 1.5f);
    }
}
