using System.Collections;
using UnityEngine;

/// <summary>
/// Panalito tirado en el mapa: gira flotando y al tocarlo se recoge y devuelve una vida (hasta el máximo).
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class PanalitoPickup : MonoBehaviour
{
    [SerializeField] private SpriteAnim anim;
    [SerializeField] private int heal = 1;
    [SerializeField] private float bobAmplitude = 0.15f;
    [SerializeField] private float bobSpeed = 2f;

    private Vector3 start;
    private bool taken;

    private void Start()
    {
        start = transform.position;
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void Update()
    {
        if (!taken) transform.position = start + Vector3.up * Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (taken) return;
        var player = other.GetComponentInParent<PlayerHealth>();
        if (player == null || player.IsDead) return;

        taken = true;
        player.Heal(heal);
        AudioManager.Instance.PlaySfx("Panalito_Agarrado");
        StartCoroutine(PickUp());
    }

    private IEnumerator PickUp()
    {
        if (anim != null) anim.Play("recoger");
        yield return new WaitForSeconds(0.35f);
        if (anim != null) anim.Play("desaparecer");
        yield return new WaitForSeconds(0.3f);
        Destroy(gameObject);
    }
}
