using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Movimiento horizontal, salto y doble salto de Loquito.
/// El doble salto solo se recarga al tocar la capa "Suelo" (ground check bajo los pies).
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 6f;

    [Header("Salto")]
    [SerializeField] private float jumpForce = 13f;
    [SerializeField] private float doubleJumpForce = 11f;
    [Tooltip("Multiplicador de la velocidad vertical al soltar el botón de salto (salto variable).")]
    [SerializeField, Range(0f, 1f)] private float jumpCutMultiplier = 0.5f;

    [Header("Ground check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D rb;
    private InputAction moveAction;
    private InputAction jumpAction;

    private float moveInput;
    private bool jumpPressed;
    private bool jumpReleased;
    private bool isGrounded;
    private bool canDoubleJump;
    private bool facingRight = true;

    public bool IsGrounded => isGrounded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        CreateInputActions();
    }

    private void CreateInputActions()
    {
        moveAction = new InputAction("Move", InputActionType.Value, expectedControlType: "Axis");
        moveAction.AddCompositeBinding("1DAxis")
            .With("Negative", "<Keyboard>/a")
            .With("Positive", "<Keyboard>/d");
        moveAction.AddCompositeBinding("1DAxis")
            .With("Negative", "<Keyboard>/leftArrow")
            .With("Positive", "<Keyboard>/rightArrow");
        moveAction.AddCompositeBinding("1DAxis")
            .With("Negative", "<Gamepad>/dpad/left")
            .With("Positive", "<Gamepad>/dpad/right");
        moveAction.AddBinding("<Gamepad>/leftStick/x").WithProcessor("axisDeadzone(min=0.2)");

        jumpAction = new InputAction("Jump", InputActionType.Button);
        jumpAction.AddBinding("<Keyboard>/space");
        jumpAction.AddBinding("<Gamepad>/buttonSouth");
    }

    private void OnEnable()
    {
        moveAction.Enable();
        jumpAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
        jumpAction.Disable();
    }

    private void OnDestroy()
    {
        moveAction.Dispose();
        jumpAction.Dispose();
    }

    private void Update()
    {
        moveInput = moveAction.ReadValue<float>();
        if (jumpAction.WasPressedThisFrame()) jumpPressed = true;
        if (jumpAction.WasReleasedThisFrame()) jumpReleased = true;

        if (moveInput > 0.01f && !facingRight) Flip();
        else if (moveInput < -0.01f && facingRight) Flip();
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

        velocity.x = moveInput * moveSpeed;

        if (jumpPressed)
        {
            if (isGrounded)
            {
                velocity.y = jumpForce;
                isGrounded = false;
            }
            else if (canDoubleJump)
            {
                velocity.y = doubleJumpForce;
                canDoubleJump = false;
            }
            jumpPressed = false;
        }

        if (jumpReleased)
        {
            if (velocity.y > 0f) velocity.y *= jumpCutMultiplier;
            jumpReleased = false;
        }

        rb.linearVelocity = velocity;
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
