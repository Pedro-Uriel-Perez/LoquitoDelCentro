using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Pantalla final. Con los 4 coleccionables muestra el final secreto (el Loquito en su cajita
/// con sus frijoles y su coca fría); si no, la pantalla de victoria básica.
/// </summary>
public class VictoryScreen : MonoBehaviour
{
    [Header("Victoria básica")]
    [SerializeField] private Sprite[] victoryFrames;

    [Header("Final secreto")]
    [SerializeField] private Sprite sittingSprite;
    [SerializeField] private Sprite beansSprite;
    [SerializeField] private Sprite colaSprite;

    private Image animated;

    private void Start()
    {
        Time.timeScale = 1f;
        var gm = GameManager.Instance;
        var canvas = UIFactory.CreateCanvas("Victoria_Canvas", 0);
        var root = canvas.transform;
        UIFactory.Stretch(root, "Fondo").gameObject.AddComponent<Image>().color = new Color(0.09f, 0.08f, 0.14f);

        if (gm.AllCollectibles)
        {
            BuildSecretEnding(root);
            AudioManager.Instance.UnlockSecretEnding();   // desde ahora el menú permite elegir esta canción
            AudioManager.Instance.PlayMusic(AudioManager.MusicaFinalSecreto);
        }
        else
        {
            BuildBasicVictory(root, gm.CollectibleCount);
            AudioManager.Instance.PlayMusic(AudioManager.MusicaInicioFin);
        }

        var back = UIFactory.Button(root, "Volver", "Volver al inicio", new Vector2(0.5f, 0f), new Vector2(0, 30), new Vector2(180, 32),
                                    () => GameManager.Instance.GoToTitle());
        UIFactory.EnsureEventSystem();
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(back.gameObject);
    }

    private void BuildBasicVictory(Transform root, int collected)
    {
        UIFactory.Text(root, "Titulo", "¡VICTORIA!", 44, TextAnchor.MiddleCenter, UIFactory.Gold,
                       new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -20), new Vector2(500, 60));
        UIFactory.Text(root, "Texto", "Las antenas cayeron y San Felipe es libre de la señal.", 16, TextAnchor.MiddleCenter, Color.white,
                       new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -80), new Vector2(560, 30));
        animated = UIFactory.Image(root, "Loquito", victoryFrames.Length > 0 ? victoryFrames[0] : null, Color.white,
                                   new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -10), new Vector2(140, 150));
        UIFactory.Text(root, "Coleccionables", $"Coleccionables: {collected}/{GameManager.TotalCollectibles}\n(Encuéntralos todos para ver el final secreto)",
                       13, TextAnchor.MiddleCenter, new Color(0.8f, 0.8f, 0.9f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 62), new Vector2(500, 40));
        if (victoryFrames.Length > 1) StartCoroutine(Animate());
    }

    private void BuildSecretEnding(Transform root)
    {
        UIFactory.Text(root, "Titulo", "FINAL SECRETO", 40, TextAnchor.MiddleCenter, UIFactory.Gold,
                       new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -16), new Vector2(500, 56));
        UIFactory.Text(root, "Texto", "Con la señal apagada, el Loquito por fin descansa en su trono:\nsus frijoles, su coca fría y su cajita de madera.",
                       15, TextAnchor.MiddleCenter, Color.white, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -74), new Vector2(580, 44));
        UIFactory.Image(root, "LoquitoSentado", sittingSprite, Color.white, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -20), new Vector2(150, 190));
        UIFactory.Image(root, "Frijoles", beansSprite, Color.white, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-120, -70), new Vector2(40, 54));
        UIFactory.Image(root, "Coca", colaSprite, Color.white, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(115, -75), new Vector2(46, 44));
    }

    private IEnumerator Animate()
    {
        int i = 0;
        while (true)
        {
            animated.sprite = victoryFrames[i];
            i = (i + 1) % victoryFrames.Length;
            yield return new WaitForSeconds(i == 0 ? 1.2f : 0.14f);
        }
    }
}
