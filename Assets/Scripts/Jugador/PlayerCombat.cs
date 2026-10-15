using System.Collections;
using UnityEngine;

/// <summary>
/// Botella de chela rota: combo de tres golpes (el tercero empuja) y patada aérea.
/// Cartón: bloqueo frontal mientras se mantiene, mitad de velocidad, durabilidad y recuperación de 8 s.
/// </summary>
[RequireComponent(typeof(PlayerController), typeof(LoquitoInput))]
public class PlayerCombat : MonoBehaviour
{
    [Header("Botella (ataque)")]
    [SerializeField] private AttackHitbox hitbox;
    [SerializeField] private Animator animator;
    [Tooltip("Estados del Animator para cada golpe del combo en el suelo.")]
    [SerializeField] private string[] comboStates = { "Golpe1", "Golpe2", "Golpe3" };
    [SerializeField] private float[] comboDurations = { 0.3f, 0.3f, 0.36f };
    [SerializeField] private int[] comboDamage = { 1, 1, 2 };
    [SerializeField] private string airAttackState = "PatadaAerea";
    [SerializeField] private float airAttackDuration = 0.3f;
    [SerializeField] private int airAttackDamage = 1;
    [Tooltip("Espera antes de activar el colisionador de la botella (anticipación del golpe).")]
    [SerializeField] private float hitboxDelay = 0.08f;
    [Tooltip("Tiempo que el colisionador de la botella permanece activo.")]
    [SerializeField] private float hitboxDuration = 0.2f;
    [Tooltip("Tiempo tras terminar un golpe en el que todavía se puede encadenar el siguiente.")]
    [SerializeField] private float comboWindow = 0.35f;
    [SerializeField] private Vector2 hitKnockback = new Vector2(2f, 1f);
    [SerializeField] private Vector2 finisherKnockback = new Vector2(10f, 5f);
    [Tooltip("Área de golpe en el suelo (posición local y tamaño).")]
    [SerializeField] private Vector2 groundHitboxOffset = new Vector2(1.3f, 1.4f);
    [SerializeField] private Vector2 groundHitboxSize = new Vector2(1.6f, 1.2f);
    [Tooltip("Área de golpe en el aire: más grande y hacia arriba para alcanzar enemigos voladores.")]
    [SerializeField] private Vector2 airHitboxOffset = new Vector2(1.1f, 2.0f);
    [SerializeField] private Vector2 airHitboxSize = new Vector2(2.4f, 2.8f);

    [Header("Cartón (bloqueo)")]
    [SerializeField] private GameObject cartonVisual;
    [SerializeField] private int maxDurability = 3;
    [SerializeField] private float recoveryTime = 8f;
    [SerializeField, Range(0f, 1f)] private float blockSpeedMultiplier = 0.5f;
    [SerializeField] private Vector2 blockPushback = new Vector2(3f, 0f);

    private static readonly int BloqueandoHash = Animator.StringToHash("Bloqueando");

    private PlayerController controller;
    private LoquitoInput input;
    private Coroutine attackRoutine;
    private bool attackBuffered;
    private int nextComboStep;
    private float comboResetTime;
    private float recoveryTimer;

    public bool IsAttacking { get; private set; }
    public bool IsBlocking { get; private set; }
    public bool CartonRoto { get; private set; }
    public int Durability { get; private set; }
    public int MaxDurability => maxDurability;
    public float RecoveryRemaining => CartonRoto ? recoveryTimer : 0f;

    private void Awake()
    {
        controller = GetComponent<PlayerController>();
        input = GetComponent<LoquitoInput>();
        Durability = maxDurability;
    }

    private void Update()
    {
        UpdateCartonRecovery();

        if (!controller.InputEnabled)
        {
            SetBlocking(false);
            return;
        }

        var gm = GameManager.Instance;
        SetBlocking(gm.TieneCarton && input.Block.IsPressed() && !CartonRoto && !IsAttacking);

        if (gm.TieneBotella && input.Attack.WasPressedThisFrame() && !IsBlocking)
        {
            if (IsAttacking) attackBuffered = true;
            else StartAttack();
        }
    }

    private void StartAttack()
    {
        if (Time.time > comboResetTime) nextComboStep = 0;
        attackRoutine = StartCoroutine(AttackRoutine());
    }

    private IEnumerator AttackRoutine()
    {
        IsAttacking = true;
        attackBuffered = false;

        bool inAir = !controller.IsGrounded;
        int step = nextComboStep;
        string state = inAir ? airAttackState : comboStates[step];
        float duration = inAir ? airAttackDuration : comboDurations[step];
        int damage = inAir ? airAttackDamage : comboDamage[step];
        bool finisher = !inAir && step == comboStates.Length - 1;
        Vector2 knockback = finisher ? finisherKnockback : hitKnockback;

        controller.MovementLocked = !inAir;
        if (animator != null) animator.Play(state, 0, 0f);

        yield return new WaitForSeconds(hitboxDelay);
        if (hitbox != null)
        {
            if (inAir) hitbox.SetShape(airHitboxOffset, airHitboxSize);
            else hitbox.SetShape(groundHitboxOffset, groundHitboxSize);
            hitbox.Activate(damage, knockback, controller.FacingDirection);
        }
        yield return new WaitForSeconds(hitboxDuration);
        if (hitbox != null) hitbox.Deactivate();

        float remaining = duration - hitboxDelay - hitboxDuration;
        if (remaining > 0f) yield return new WaitForSeconds(remaining);

        controller.MovementLocked = false;
        IsAttacking = false;
        attackRoutine = null;

        nextComboStep = inAir ? 0 : (step + 1) % comboStates.Length;
        comboResetTime = Time.time + comboWindow;

        if (attackBuffered && !inAir && nextComboStep != 0) StartAttack();
        attackBuffered = false;
    }

    /// <summary>Interrumpe el ataque en curso (por ejemplo al recibir daño).</summary>
    public void CancelAttack()
    {
        if (attackRoutine != null) StopCoroutine(attackRoutine);
        attackRoutine = null;
        if (hitbox != null) hitbox.Deactivate();
        controller.MovementLocked = false;
        IsAttacking = false;
        attackBuffered = false;
        nextComboStep = 0;
    }

    /// <summary>
    /// Intenta bloquear un impacto que viene desde <paramref name="sourcePosition"/>.
    /// Cualquier impacto frontal bloqueado (cuerpo a cuerpo o a distancia) resta un punto de durabilidad.
    /// </summary>
    public bool TryBlock(Vector2 sourcePosition)
    {
        if (!IsBlocking) return false;

        int sourceSide = sourcePosition.x >= transform.position.x ? 1 : -1;
        if (sourceSide != controller.FacingDirection) return false;

        Durability--;
        controller.ApplyKnockback(new Vector2(-sourceSide * blockPushback.x, blockPushback.y), 0.1f);

        if (Durability <= 0)
        {
            Durability = 0;
            CartonRoto = true;
            recoveryTimer = recoveryTime;
            SetBlocking(false);
        }
        return true;
    }

    private void UpdateCartonRecovery()
    {
        if (!CartonRoto) return;
        recoveryTimer -= Time.deltaTime;
        if (recoveryTimer <= 0f)
        {
            CartonRoto = false;
            Durability = maxDurability;
        }
    }

    private void SetBlocking(bool blocking)
    {
        IsBlocking = blocking;
        controller.SpeedMultiplier = blocking ? blockSpeedMultiplier : 1f;
        if (cartonVisual != null && cartonVisual.activeSelf != blocking) cartonVisual.SetActive(blocking);
        if (animator != null) animator.SetBool(BloqueandoHash, blocking);
    }
}
