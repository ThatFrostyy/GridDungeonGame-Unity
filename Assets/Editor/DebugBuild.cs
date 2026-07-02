using System;
using System.IO;
using System.Linq;
using UnityEditor;

/// <summary>
/// Batchmode build entry points for local development builds.
/// </summary>
public static class DebugBuild
{
    private const string DefaultBuildPath = "Builds/WindowsDebug/GridDungeonGame.exe";

    public static void BuildWindowsDebug()
    {
        string[] scenes = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();

        if (scenes.Length == 0)
        {
            throw new InvalidOperationException("No enabled scenes are configured in EditorBuildSettings.");
        }

        string outputPath = GetCommandLineValue("-buildOutput") ?? DefaultBuildPath;
        string outputDirectory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        BuildPlayerOptions options = new()
        {
            scenes = scenes,
            locationPathName = outputPath,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.Development | BuildOptions.AllowDebugging,
        };

        var report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            throw new InvalidOperationException($"Debug build failed: {report.summary.result}");
        }
    }

    private static string GetCommandLineValue(string argumentName)
    {
        string[] args = Environment.GetCommandLineArgs();
        int index = Array.IndexOf(args, argumentName);

        if (index < 0 || index + 1 >= args.Length)
        {
            return null;
        }

        return args[index + 1];
    }
}
