using System.Reflection;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Сборка плеера BeeSwarm из batchmode.
/// Сцена в репозитории пустая: её содержимое создаёт BeeSwarmSceneBuilder
/// (Tools > BeeSwarm > Build Main Scene), поэтому перед билдом сцену собираем,
/// сохраняем и только потом запускаем BuildPipeline.
/// </summary>
public static class BeeSwarmBuilder
{
    private const string ScenePath = "Assets/Scenes/MainScene.unity";

    [MenuItem("Tools/BeeSwarm/Build Windows Player")]
    public static void BuildWindows()
    {
        Build(BuildTarget.StandaloneWindows64, "Builds/Windows/BeeSwarm.exe", "BeeSwarm.exe");
    }

    public static void BuildLinux()
    {
        Build(BuildTarget.StandaloneLinux64, "Builds/Linux/BeeSwarm", "BeeSwarm");
    }

    private static void Build(BuildTarget target, string outputPath, string executableName)
    {
        EnsureScene();

        string dir = System.IO.Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(dir)) System.IO.Directory.CreateDirectory(dir);

        var options = new BuildPlayerOptions
        {
            scenes = new[] { ScenePath },
            locationPathName = outputPath,
            target = target,
            targetGroup = BuildTargetGroup.Standalone,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = report.summary;

        Debug.Log($"[BUILD] target={target} result={summary.result} size={summary.totalSize} errors={summary.totalErrors} time={summary.totalTime}");

        if (summary.result != BuildResult.Succeeded)
        {
            Debug.LogError($"[BUILD] FAILED: {summary.result}, errors={summary.totalErrors}");
            EditorApplication.Exit(1);
            return;
        }

        Debug.Log($"[BUILD] OK: {outputPath} ({executableName})");
        EditorApplication.Exit(0);
    }

    /// <summary>
    /// Собирает сцену штатным билдером проекта (его метод — private static),
    /// затем сохраняет результат на диск, иначе BuildPipeline возьмёт пустую сцену.
    /// </summary>
    private static void EnsureScene()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        MethodInfo build = typeof(BeeSwarmSceneBuilder).GetMethod(
            "BuildMainScene", BindingFlags.NonPublic | BindingFlags.Static);

        if (build == null)
        {
            Debug.LogError("[BUILD] BeeSwarmSceneBuilder.BuildMainScene не найден");
            EditorApplication.Exit(1);
            return;
        }

        build.Invoke(null, null);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();
        Debug.Log("[BUILD] Сцена собрана и сохранена");
    }
}
