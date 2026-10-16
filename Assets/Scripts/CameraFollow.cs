using UnityEngine;

/// <summary>
/// Cámara que sigue al objetivo de forma suave sin mostrar nada fuera del área del nivel.
/// El área es un rectángulo en el mundo; los límites se calculan con el tamaño real de la vista
/// (la Pixel Perfect Camera cambia el tamaño visible según la resolución y la proporción de la pantalla).
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector2 offset = new Vector2(0f, 2f);
    [SerializeField] private float smoothTime = 0.15f;

    [Header("Área visible (rectángulo del mundo)")]
    [SerializeField] private bool useBounds = true;
    [SerializeField] private Vector2 areaMin = new Vector2(0f, 0f);
    [SerializeField] private Vector2 areaMax = new Vector2(100f, 40f);

    private Camera cam;
    private Vector3 velocity;

    private void Awake() => cam = GetComponent<Camera>();

    /// <summary>Cambia el área visible (p. ej. la arena del jefe).</summary>
    public void SetBounds(Vector2 min, Vector2 max)
    {
        useBounds = true;
        areaMin = min;
        areaMax = max;
    }

    public void GetBounds(out Vector2 min, out Vector2 max)
    {
        min = areaMin;
        max = areaMax;
    }

    /// <summary>Coloca la cámara directamente sobre el objetivo (sin suavizado), p. ej. al cargar el nivel.</summary>
    public void SnapToTarget()
    {
        if (target == null) return;
        transform.position = Clamp(Desired());
        velocity = Vector3.zero;
    }

    private void Start() => SnapToTarget();

    private void LateUpdate()
    {
        if (target == null) return;
        Vector3 next = Vector3.SmoothDamp(transform.position, Clamp(Desired()), ref velocity, smoothTime);
        transform.position = Clamp(next);   // nunca se asoma fuera del área, ni durante el suavizado
    }

    private Vector3 Desired() =>
        new Vector3(target.position.x + offset.x, target.position.y + offset.y, transform.position.z);

    private Vector3 Clamp(Vector3 p)
    {
        if (!useBounds || cam == null) return p;
        float halfH = cam.orthographicSize, halfW = halfH * cam.aspect;
        p.x = ClampAxis(p.x, areaMin.x + halfW, areaMax.x - halfW);
        p.y = ClampAxis(p.y, areaMin.y + halfH, areaMax.y - halfH);
        return p;
    }

    // Si el área es más chica que la vista en ese eje, se centra.
    private static float ClampAxis(float v, float min, float max) => min > max ? (min + max) * 0.5f : Mathf.Clamp(v, min, max);
}
