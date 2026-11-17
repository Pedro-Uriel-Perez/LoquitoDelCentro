using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Build web para itch.io: genera Builds/BuildWeb y el ZIP listo para subir (index.html en la raíz)
/// en Builds/itch/LoquitoDelCentro_web.zip.
/// Desde Unity: menú Loquito > Build web (itch).
/// Desde la terminal (con Unity cerrado):
///   Unity.exe -batchmode -quit -projectPath . -executeMethod WebBuild.BuildBatch -logFile Builds/build_web.log
/// </summary>
public static class WebBuild
{
    private const string OutDir = "Builds/BuildWeb";
    private const string ZipPath = "Builds/itch/LoquitoDelCentro_web.zip";

    [MenuItem("Loquito/Build web (itch)")]
    public static void BuildMenu()
    {
        if (Build()) EditorUtility.RevealInFinder(ZipPath);
    }

    /// <summary>Para -executeMethod: sale con código 1 si el build falla.</summary>
    public static void BuildBatch()
    {
        if (!Build()) EditorApplication.Exit(1);
    }

    private static bool Build()
    {
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.WebGL)
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);
        // Plantilla propia (Assets/WebGLTemplates/Loquito): el juego llena la ventana en 16:9
        PlayerSettings.WebGL.template = "PROJECT:Loquito";

        if (Directory.Exists(OutDir)) Directory.Delete(OutDir, true);
        var options = new BuildPlayerOptions
        {
            scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
            locationPathName = OutDir,
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        };
        var summary = BuildPipeline.BuildPlayer(options).summary;
        Debug.Log($"[WebBuild] {summary.result} · errores {summary.totalErrors} · {summary.totalSize / (1024 * 1024)} MB · {summary.totalTime}");
        if (summary.result != BuildResult.Succeeded) return false;

        // ZIP con el contenido de la carpeta (no la carpeta), como pide itch
        Directory.CreateDirectory(Path.GetDirectoryName(ZipPath));
        if (File.Exists(ZipPath)) File.Delete(ZipPath);
        using (var zip = new ZipArchive(File.Create(ZipPath), ZipArchiveMode.Create))
        {
            foreach (var file in Directory.GetFiles(OutDir, "*", SearchOption.AllDirectories))
            {
                string entryName = Path.GetRelativePath(OutDir, file).Replace('\\', '/');
                using var entry = zip.CreateEntry(entryName, System.IO.Compression.CompressionLevel.Optimal).Open();
                using var source = File.OpenRead(file);
                source.CopyTo(entry);
            }
        }
        Debug.Log($"[WebBuild] ZIP listo: {Path.GetFullPath(ZipPath)}");
        return true;
    }
}
