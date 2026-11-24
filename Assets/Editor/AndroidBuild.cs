using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Build de Android: genera Builds/Android/LoquitoDelCentro.apk (se instala directo en el celular).
/// Requiere el módulo "Android Build Support" (con OpenJDK y Android SDK & NDK Tools) en Unity Hub.
/// Desde Unity: menú Loquito > Build Android (APK).
/// Desde la terminal (con Unity cerrado):
///   Unity.exe -batchmode -quit -projectPath . -executeMethod AndroidBuild.BuildBatch -logFile Builds/build_android.log
/// </summary>
public static class AndroidBuild
{
    private const string ApkPath = "Builds/Android/LoquitoDelCentro.apk";

    [MenuItem("Loquito/Build Android (APK)")]
    public static void BuildMenu()
    {
        if (Build()) EditorUtility.RevealInFinder(ApkPath);
    }

    /// <summary>Para -executeMethod: sale con código 1 si el build falla.</summary>
    public static void BuildBatch()
    {
        if (!Build()) EditorApplication.Exit(1);
    }

    private static bool Build()
    {
        if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android, BuildTarget.Android))
        {
            Debug.LogError("[AndroidBuild] Falta el módulo Android Build Support (Unity Hub > Installs > Add modules).");
            return false;
        }
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);

        var android = NamedBuildTarget.Android;
        PlayerSettings.SetApplicationIdentifier(android, "com.utng.loquitodelcentro");
        PlayerSettings.bundleVersion = "1.0";
        PlayerSettings.Android.bundleVersionCode = 1;
        // Juego horizontal: solo gira entre las dos posiciones acostadas
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
        PlayerSettings.allowedAutorotateToLandscapeLeft = true;
        PlayerSettings.allowedAutorotateToLandscapeRight = true;
        PlayerSettings.allowedAutorotateToPortrait = false;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
        // IL2CPP con ARM64: los celulares nuevos ya solo aceptan apps de 64 bits
        PlayerSettings.SetScriptingBackend(android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;
        EditorUserBuildSettings.buildAppBundle = false;   // .apk, no .aab

        Directory.CreateDirectory(Path.GetDirectoryName(ApkPath));
        var options = new BuildPlayerOptions
        {
            scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
            locationPathName = ApkPath,
            target = BuildTarget.Android,
            options = BuildOptions.None
        };
        var summary = BuildPipeline.BuildPlayer(options).summary;
        Debug.Log($"[AndroidBuild] {summary.result} · errores {summary.totalErrors} · {summary.totalSize / (1024 * 1024)} MB · {summary.totalTime}");
        if (summary.result != BuildResult.Succeeded) return false;
        Debug.Log($"[AndroidBuild] APK listo: {Path.GetFullPath(ApkPath)}");
        return true;
    }
}
