using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Toma capturas de los niveles para el README (docs/img/capturas) con la cámara del juego.
/// Desde Unity: menú Loquito > Capturas para el README.
/// Desde la terminal (con Unity cerrado):
///   Unity.exe -batchmode -quit -projectPath . -executeMethod ReadmeCaptures.CaptureBatch -logFile Builds/capturas.log
/// La interfaz (HUD, menús) se crea al jugar, así que no aparece: solo el mundo del nivel.
/// </summary>
public static class ReadmeCaptures
{
    private const string OutDir = "docs/img/capturas";
    private const int Width = 1152, Height = 648;   // 384x216 (vista del juego) x3
    private const float HalfH = 6.75f, HalfW = 12f;

    private enum Target { PlayerStart, Martha, BossArena }

    private static readonly (string scene, Target target, string file)[] Shots =
    {
        ("Jardin", Target.PlayerStart, "jardin_inicio"),
        ("Jardin", Target.Martha, "jardin_martha"),
        ("Mazmorra1", Target.PlayerStart, "mazmorra1"),
        ("Mazmorra1", Target.BossArena, "jefe_campanero"),
        ("Mazmorra2", Target.PlayerStart, "mazmorra2"),
        ("Mazmorra2", Target.BossArena, "jefe_dragon"),
        ("Mazmorra3", Target.PlayerStart, "mazmorra3"),
        ("Mazmorra3", Target.BossArena, "jefe_senora"),
    };

    [MenuItem("Loquito/Capturas para el README")]
    public static void CaptureMenu()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        string previous = EditorSceneManager.GetActiveScene().path;
        int done = CaptureAll();
        if (!string.IsNullOrEmpty(previous)) EditorSceneManager.OpenScene(previous);
        EditorUtility.DisplayDialog("Capturas", $"{done} capturas guardadas en {OutDir}", "OK");
    }

    public static void CaptureBatch()
    {
        if (CaptureAll() == 0) EditorApplication.Exit(1);
    }

    private static int CaptureAll()
    {
        Directory.CreateDirectory(OutDir);
        int done = 0;
        foreach (var shot in Shots)
        {
            EditorSceneManager.OpenScene($"Assets/Scenes/{shot.scene}.unity", OpenSceneMode.Single);
            if (Capture(shot.target, Path.Combine(OutDir, shot.file + ".png"))) done++;
            else Debug.LogWarning($"[Capturas] No se pudo capturar {shot.file}");
        }
        Debug.Log($"[Capturas] {done}/{Shots.Length} listas en {Path.GetFullPath(OutDir)}");
        return done;
    }

    private static bool Capture(Target target, string path)
    {
        var cam = Camera.main;
        if (cam == null || !FindFocus(target, cam, out Vector3 focus)) return false;

        // La Pixel Perfect Camera cambia el tamaño en el editor: se apaga mientras se captura
        var ppc = cam.GetComponent("PixelPerfectCamera") as Behaviour;
        bool ppcWasEnabled = ppc != null && ppc.enabled;
        if (ppc != null) ppc.enabled = false;

        Vector3 camStart = cam.transform.position;
        float sizeStart = cam.orthographicSize;
        Vector3 newPos = new Vector3(focus.x, focus.y, camStart.z);

        // Los fondos con parallax se mueven una fracción de lo que se mueve la cámara (como en el juego)
        var layers = Object.FindObjectsByType<Parallax>(FindObjectsSortMode.None);
        var layerStart = new Vector3[layers.Length];
        Vector3 delta = newPos - camStart;
        for (int i = 0; i < layers.Length; i++)
        {
            layerStart[i] = layers[i].transform.position;
            Vector2 f = new SerializedObject(layers[i]).FindProperty("factor").vector2Value;
            layers[i].transform.position = layerStart[i] + new Vector3(delta.x * f.x, delta.y * f.y, 0f);
        }

        cam.transform.position = newPos;
        cam.orthographicSize = HalfH;
        var rt = new RenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB) { filterMode = FilterMode.Point };
        cam.targetTexture = rt;
        cam.aspect = (float)Width / Height;
        cam.Render();

        var previousActive = RenderTexture.active;
        RenderTexture.active = rt;
        var tex = new Texture2D(Width, Height, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
        tex.Apply();
        RenderTexture.active = previousActive;
        File.WriteAllBytes(path, tex.EncodeToPNG());

        // Deja la escena como estaba
        cam.targetTexture = null;
        cam.ResetAspect();
        cam.transform.position = camStart;
        cam.orthographicSize = sizeStart;
        if (ppc != null) ppc.enabled = ppcWasEnabled;
        for (int i = 0; i < layers.Length; i++) layers[i].transform.position = layerStart[i];
        Object.DestroyImmediate(tex);
        rt.Release();
        Object.DestroyImmediate(rt);
        return true;
    }

    /// <summary>Punto al que mira la cámara, dentro del área visible del nivel.</summary>
    private static bool FindFocus(Target target, Camera cam, out Vector3 focus)
    {
        focus = Vector3.zero;
        Vector2 min, max;
        switch (target)
        {
            case Target.BossArena:
                var arena = Object.FindFirstObjectByType<BossArena>();
                if (arena == null) return false;
                var so = new SerializedObject(arena);
                min = so.FindProperty("cameraMin").vector2Value;
                max = so.FindProperty("cameraMax").vector2Value;
                focus = (min + max) * 0.5f;
                return true;

            case Target.Martha:
                var martha = Object.FindFirstObjectByType<MarthaNPC>();
                if (martha == null) return false;
                focus = martha.transform.position + new Vector3(-3f, 2f, 0f);
                break;

            default:
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player == null) return false;
                focus = player.transform.position + new Vector3(4f, 2f, 0f);
                break;
        }

        var follow = cam.GetComponent<CameraFollow>();
        if (follow != null)
        {
            follow.GetBounds(out min, out max);
            focus.x = max.x - min.x > HalfW * 2f ? Mathf.Clamp(focus.x, min.x + HalfW, max.x - HalfW) : (min.x + max.x) * 0.5f;
            focus.y = max.y - min.y > HalfH * 2f ? Mathf.Clamp(focus.y, min.y + HalfH, max.y - HalfH) : (min.y + max.y) * 0.5f;
        }
        return true;
    }
}
