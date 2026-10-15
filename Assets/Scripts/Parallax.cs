using UnityEngine;

/// <summary>
/// Capa de fondo con parallax: sigue una fracción del movimiento de la cámara.
/// factor 0 = se queda fija en el mundo; factor 1 = se mueve igual que la cámara (infinitamente lejos).
/// </summary>
[DefaultExecutionOrder(1000)] // después de CameraFollow
public class Parallax : MonoBehaviour
{
    [SerializeField] private Vector2 factor = new Vector2(0.5f, 0.5f);

    private Transform cam;
    private Vector3 startPosition;
    private Vector3 cameraStart;

    private void Start()
    {
        cam = Camera.main != null ? Camera.main.transform : null;
        startPosition = transform.position;
        if (cam != null) cameraStart = cam.position;
    }

    private void LateUpdate()
    {
        if (cam == null) return;
        Vector3 delta = cam.position - cameraStart;
        transform.position = startPosition + new Vector3(delta.x * factor.x, delta.y * factor.y, 0f);
    }
}
