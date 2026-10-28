using UnityEngine;

/// <summary>
/// Hace que el enemigo salte a intervalos aleatorios mientras está en el suelo (duende).
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class RandomJumper : MonoBehaviour
{
    [SerializeField] private float minInterval = 2f;
    [SerializeField] private float maxInterval = 5f;
    [SerializeField] private float jumpForce = 13f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDistance = 0.15f;
    [SerializeField] private Animator animator;

    private static readonly int EnSueloHash = Animator.StringToHash("EnSuelo");

    private Rigidbody2D rb;
    private Health health;
    private float nextJumpTime;

    public bool IsGrounded { get; private set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();
        ScheduleNextJump();
    }

    private void FixedUpdate()
    {
        Vector2 origin = rb.position + Vector2.up * 0.1f;
        IsGrounded = rb.linearVelocity.y <= 0.01f &&
            Physics2D.Raycast(origin, Vector2.down, 0.1f + groundCheckDistance, groundLayer);

        if (animator != null) animator.SetBool(EnSueloHash, IsGrounded);
        if (health != null && (health.IsDead || health.IsStunned)) return;

        if (IsGrounded && Time.time >= nextJumpTime)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            AudioManager.Instance.PlaySfxAt("Duende", transform.position);
            ScheduleNextJump();
        }
    }

    private void ScheduleNextJump()
    {
        nextJumpTime = Time.time + Random.Range(minInterval, maxInterval);
    }
}
