using UnityEngine;

/// <summary>
/// Cámara que sigue al objetivo de forma suave, opcionalmente limitada a los bordes del nivel.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector2 offset = new Vector2(0f, 2f);
    [SerializeField] private float smoothTime = 0.15f;

    [Header("Límites del nivel (centro de la cámara)")]
    [SerializeField] private bool useBounds = true;
    [SerializeField] private Vector2 minPosition = new Vector2(-10f, 4.5f);
    [SerializeField] private Vector2 maxPosition = new Vector2(70f, 14f);

    private Vector3 velocity;

    public void SetBounds(Vector2 min, Vector2 max)
    {
        useBounds = true;
        minPosition = min;
        maxPosition = max;
    }

    public void GetBounds(out Vector2 min, out Vector2 max)
    {
        min = minPosition;
        max = maxPosition;
    }

    /// <summary>Coloca la cámara directamente sobre el objetivo (sin suavizado), p. ej. al cargar el nivel.</summary>
    public void SnapToTarget()
    {
        if (target == null) return;
        Vector3 p = new Vector3(target.position.x + offset.x, target.position.y + offset.y, transform.position.z);
        if (useBounds)
        {
            p.x = Mathf.Clamp(p.x, minPosition.x, maxPosition.x);
            p.y = Mathf.Clamp(p.y, minPosition.y, maxPosition.y);
        }
        transform.position = p;
        velocity = Vector3.zero;
    }

    private void Start() => SnapToTarget();

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desired = new Vector3(
            target.position.x + offset.x,
            target.position.y + offset.y,
            transform.position.z);

        if (useBounds)
        {
            desired.x = Mathf.Clamp(desired.x, minPosition.x, maxPosition.x);
            desired.y = Mathf.Clamp(desired.y, minPosition.y, maxPosition.y);
        }

        transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
    }
}
