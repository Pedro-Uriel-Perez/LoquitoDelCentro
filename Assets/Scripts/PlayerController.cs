using UnityEngine;

/// <summary>
/// Movimiento horizontal, salto y doble salto (Calcetines del Verde) de Loquito.
/// El doble salto solo se recarga al tocar la capa "Suelo" (ground check bajo los pies).
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(LoquitoInput))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 8f;

    [Header("Salto")]
    [SerializeField] private float jumpForce = 22f;
    [SerializeField] private float doubleJumpForce = 17f;
    [Tooltip("Multiplicador de la velocidad vertical al soltar el botón de salto (salto variable).")]
    [SerializeField, Range(0f, 1f)] private float jumpCutMultiplier = 0.5f;
    [SerializeField] private float maxFallSpeed = 25f;

    [Header("Ground check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Animación")]
    [SerializeField] private Animator animator;

    private static readonly int VelocidadXHash = Animator.StringToHash("VelocidadX");
    private static readonly int VelocidadYHash = Animator.StringToHash("VelocidadY");
    private static readonly int EnSueloHash = Animator.StringToHash("EnSuelo");

    private Rigidbody2D rb;
    private LoquitoInput input;

    private float moveInput;
    private bool jumpPressed;
    private bool jumpReleased;
    private bool isGrounded;
    private bool canDoubleJump;
    private bool facingRight = true;
    private float knockbackTimer;

    public bool IsGrounded => isGrounded;
    public int FacingDirection => facingRight ? 1 : -1;

    /// <summary>Multiplicador de velocidad horizontal (el cartón lo baja a la mitad).</summary>
    public float SpeedMultiplier { get; set; } = 1f;

    /// <summary>Si está activo, el jugador no puede moverse en el suelo, saltar ni voltearse (p. ej. al atacar).</summary>
    public bool MovementLocked { get; set; }

    /// <summary>Desactiva toda la entrada (muerte, cinemáticas).</summary>
    public bool InputEnabled { get; set; } = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        input = GetComponent<LoquitoInput>();
    }

    private void Update()
    {
        if (!InputEnabled)
        {
            moveInput = 0f;
            jumpPressed = jumpReleased = false;
            UpdateAnimator();
            return;
        }

        moveInput = input.Move.ReadValue<float>();
        if (input.Jump.WasPressedThisFrame()) jumpPressed = true;
        if (input.Jump.WasReleasedThisFrame()) jumpReleased = true;

        if (!MovementLocked && knockbackTimer <= 0f)
        {
            if (moveInput > 0.01f && !facingRight) Flip();
            else if (moveInput < -0.01f && facingRight) Flip();
        }

        UpdateAnimator();
    }

    private void FixedUpdate()
    {
        Vector2 velocity = rb.linearVelocity;

        // Solo cuenta como suelo si el círculo bajo los pies toca "Suelo" y no estamos subiendo
        // (evita que el frame justo después de saltar recargue el doble salto).
        bool touchingGround = groundCheck != null &&
            Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer) != null;
        isGrounded = touchingGround && velocity.y <= 0.01f;
        if (isGrounded) canDoubleJump = true;

        if (knockbackTimer > 0f)
        {
            knockbackTimer -= Time.fixedDeltaTime;
            jumpPressed = false;
        }
        else
        {
            bool lockedOnGround = MovementLocked && isGrounded;
            velocity.x = lockedOnGround ? 0f : moveInput * moveSpeed * SpeedMultiplier;

            if (jumpPressed && !MovementLocked)
            {
                if (isGrounded)
                {
                    velocity.y = jumpForce;
                    isGrounded = false;
                }
                else if (canDoubleJump && GameManager.Instance.TieneCalcetas)
                {
                    velocity.y = doubleJumpForce;
                    canDoubleJump = false;
                }
            }
            jumpPressed = false;
        }

        if (jumpReleased)
        {
            if (velocity.y > 0f && knockbackTimer <= 0f) velocity.y *= jumpCutMultiplier;
            jumpReleased = false;
        }

        if (velocity.y < -maxFallSpeed) velocity.y = -maxFallSpeed;
        rb.linearVelocity = velocity;
    }

    /// <summary>Empuja al jugador y le quita el control horizontal durante <paramref name="duration"/> segundos.</summary>
    public void ApplyKnockback(Vector2 velocity, float duration)
    {
        rb.linearVelocity = velocity;
        knockbackTimer = duration;
    }

    /// <summary>Coloca al jugador en una posición y lo detiene (reaparición en checkpoint).</summary>
    public void Teleport(Vector3 position)
    {
        rb.position = position;
        transform.position = position;
        rb.linearVelocity = Vector2.zero;
        knockbackTimer = 0f;
    }

    private void UpdateAnimator()
    {
        if (animator == null) return;
        Vector2 v = rb.linearVelocity;
        animator.SetFloat(VelocidadXHash, Mathf.Abs(v.x));
        animator.SetFloat(VelocidadYHash, v.y);
        animator.SetBool(EnSueloHash, isGrounded);
    }

    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1f;
        transform.localScale = scale;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
