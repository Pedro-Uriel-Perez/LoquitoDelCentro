using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Acciones de entrada del Loquito (New Input System), teclado y gamepad.
/// Teclado: A/D o flechas = mover, Espacio = saltar, J = atacar (botella), K = bloquear (cartón), E = interactuar.
/// Gamepad: stick/cruceta = mover, Sur (A/Cruz) = saltar, Oeste (X/Cuadrado) = atacar, gatillo derecho = bloquear,
/// Norte (Y/Triángulo) = interactuar.
/// </summary>
[DefaultExecutionOrder(-100)]
public class LoquitoInput : MonoBehaviour
{
    public InputAction Move { get; private set; }
    public InputAction Jump { get; private set; }
    public InputAction Attack { get; private set; }
    public InputAction Block { get; private set; }
    public InputAction Interact { get; private set; }

    private void Awake()
    {
        Move = new InputAction("Move", InputActionType.Value, expectedControlType: "Axis");
        Move.AddCompositeBinding("1DAxis")
            .With("Negative", "<Keyboard>/a")
            .With("Positive", "<Keyboard>/d");
        Move.AddCompositeBinding("1DAxis")
            .With("Negative", "<Keyboard>/leftArrow")
            .With("Positive", "<Keyboard>/rightArrow");
        Move.AddCompositeBinding("1DAxis")
            .With("Negative", "<Gamepad>/dpad/left")
            .With("Positive", "<Gamepad>/dpad/right");
        Move.AddBinding("<Gamepad>/leftStick/x").WithProcessor("axisDeadzone(min=0.2)");

        Jump = new InputAction("Jump", InputActionType.Button);
        Jump.AddBinding("<Keyboard>/space");
        Jump.AddBinding("<Gamepad>/buttonSouth");

        Attack = new InputAction("Attack", InputActionType.Button);
        Attack.AddBinding("<Keyboard>/j");
        Attack.AddBinding("<Gamepad>/buttonWest");

        Block = new InputAction("Block", InputActionType.Button);
        Block.AddBinding("<Keyboard>/k");
        Block.AddBinding("<Gamepad>/rightTrigger");

        Interact = new InputAction("Interact", InputActionType.Button);
        Interact.AddBinding("<Keyboard>/e");
        Interact.AddBinding("<Gamepad>/buttonNorth");
    }

    private void OnEnable()
    {
        Move.Enable();
        Jump.Enable();
        Attack.Enable();
        Block.Enable();
        Interact.Enable();
    }

    private void OnDisable()
    {
        Move.Disable();
        Jump.Disable();
        Attack.Disable();
        Block.Disable();
        Interact.Disable();
    }

    private void OnDestroy()
    {
        Move.Dispose();
        Jump.Dispose();
        Attack.Dispose();
        Block.Dispose();
        Interact.Dispose();
    }
}
