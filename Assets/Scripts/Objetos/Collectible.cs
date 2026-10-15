using UnityEngine;

/// <summary>
/// Coleccionable escondido en el cuarto secreto de cada mazmorra. Al tocarlo se muestra
/// en grande con su descripción y un botón para guardarlo.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Collectible : MonoBehaviour
{
    [SerializeField] private int id = 1;
    [SerializeField] private string displayName = "Bolsa de vagabundo";
    [SerializeField, TextArea] private string description = "Como la del Chavo del 8.";
    [SerializeField] private Sprite bigSprite;
    [SerializeField] private float bobAmplitude = 0.12f;

    private Vector3 start;

    private void Start()
    {
        if (GameManager.Instance.HasCollectible(id))
        {
            gameObject.SetActive(false);
            return;
        }
        start = transform.position;
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void Update()
    {
        transform.position = start + Vector3.up * Mathf.Sin(Time.time * 2.5f) * bobAmplitude;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponentInParent<PlayerHealth>() == null) return;
        gameObject.SetActive(false);

        if (HUD.Instance != null) HUD.Instance.ShowCollectible(bigSprite, displayName, description, () => GameManager.Instance.AddCollectible(id));
        else GameManager.Instance.AddCollectible(id);
    }
}
