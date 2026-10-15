using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Martha la Loca (Nivel 0). Ronca mientras nadie le habla; al interactuar (E / botón Norte) se pone a hablar,
/// su diálogo sale en la nube y en ciertos momentos entrega objetos (cartón, botella, calcetas).
/// Al terminar el diálogo el Loquito puede atacar, cubrirse y hacer doble salto.
/// </summary>
public class MarthaNPC : MonoBehaviour
{
    [Serializable]
    public class Gift
    {
        [Tooltip("Índice de la línea después de la cual se entrega (0 = primera línea).")]
        public int afterLine;
        public string title;
        [TextArea] public string description;
        public Sprite sprite;
    }

    [SerializeField] private SpriteAnim anim;
    [Tooltip("Punto donde apunta la nube de diálogo (cabeza de Martha).")]
    [SerializeField] private Transform bubbleAnchor;
    [SerializeField] private float talkRange = 3.5f;
    [SerializeField, TextArea(2, 4)] private string[] lines;
    [SerializeField] private List<Gift> gifts = new List<Gift>();
    [Tooltip("Lo que dice si le hablas otra vez después de entregarte todo.")]
    [SerializeField, TextArea] private string repeatLine = "Confío en ti, Loquito. Hazlo por mí, ¿ok?";

    private Transform player;
    private LoquitoInput input;
    private PlayerController controller;
    private bool talking;
    private bool waitingGift;
    private bool repeatMode;
    private int index;
    private float advanceAllowedAt;

    private void Start()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
        {
            player = p.transform;
            input = p.GetComponent<LoquitoInput>();
            controller = p.GetComponent<PlayerController>();
        }
        // Hasta hablar con Martha solo se puede caminar y saltar.
        if (!GameManager.Instance.TalkedToMartha) GameManager.Instance.SetAbilities(false);
        if (anim != null) anim.Play("roncar");
    }

    private void Update()
    {
        if (player == null || input == null) return;
        var hud = HUD.Instance;

        if (!talking)
        {
            bool near = Mathf.Abs(player.position.x - transform.position.x) < talkRange &&
                        Mathf.Abs(player.position.y - transform.position.y) < 3f;
            if (hud != null) hud.SetPrompt(this, near ? "E / Y: Hablar con Martha" : null);
            if (near && input.Interact.WasPressedThisFrame()) Begin();
            return;
        }

        if (waitingGift) return;
        if (Time.unscaledTime >= advanceAllowedAt && input.Interact.WasPressedThisFrame()) Advance();
    }

    private void Begin()
    {
        talking = true;
        repeatMode = GameManager.Instance.TalkedToMartha;
        index = 0;
        if (HUD.Instance != null) HUD.Instance.SetPrompt(this, null);
        if (controller != null) controller.InputEnabled = false;
        if (anim != null) anim.Play("hablar");
        ShowLine();
    }

    private void ShowLine()
    {
        string text = repeatMode ? repeatLine : lines[index];
        if (HUD.Instance != null) HUD.Instance.ShowDialogue(bubbleAnchor != null ? bubbleAnchor : transform, text);
        advanceAllowedAt = Time.unscaledTime + 0.25f;
    }

    private void Advance()
    {
        if (!repeatMode)
        {
            var gift = gifts.Find(g => g.afterLine == index);
            if (gift != null)
            {
                Give(gift);
                return;
            }
        }
        NextLine();
    }

    private void Give(Gift gift)
    {
        waitingGift = true;
        var hud = HUD.Instance;
        if (hud == null) { waitingGift = false; NextLine(); return; }
        hud.HideDialogue();
        hud.ShowCollectible(gift.sprite, gift.title, gift.description, () =>
        {
            waitingGift = false;
            NextLine();
        }, reenableInput: false, collectedMessage: null);
    }

    private void NextLine()
    {
        index++;
        if (repeatMode || index >= lines.Length) End();
        else ShowLine();
    }

    private void End()
    {
        talking = false;
        var hud = HUD.Instance;
        if (hud != null) hud.HideDialogue();
        if (anim != null) anim.Play("roncar");

        if (!repeatMode)
        {
            var gm = GameManager.Instance;
            gm.TalkedToMartha = true;
            gm.SetAbilities(true);
            if (hud != null) hud.ShowMessage("¡Ya puedes atacar (J), cubrirte (K)\ny hacer doble salto!", 4f);
        }
        StartCoroutine(ReturnControl());
    }

    private IEnumerator ReturnControl()
    {
        // Un par de frames para que la misma tecla no cuente como acción del jugador.
        yield return null;
        yield return null;
        if (controller != null) controller.InputEnabled = true;
    }
}
