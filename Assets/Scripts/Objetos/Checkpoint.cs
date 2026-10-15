using UnityEngine;

/// <summary>
/// Checkpoint (cama de cartón + fogata). Al tocarlo se guarda como punto de reaparición.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Checkpoint : MonoBehaviour
{
    [SerializeField] private Vector2 spawnOffset = new Vector2(0f, 0.1f);

    private bool activated;

    private void Start() => GetComponent<Collider2D>().isTrigger = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        var player = other.GetComponentInParent<PlayerHealth>();
        if (player == null || player.IsDead) return;

        player.SetCheckpoint(transform.position + (Vector3)spawnOffset);
        if (!activated)
        {
            activated = true;
            if (HUD.Instance != null) HUD.Instance.ShowMessage("Checkpoint", 1.5f);
        }
    }
}
