using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CocClear.Editor
{
    public static class VisualNovelSceneCreator
    {
        private const string ScenePath = "Assets/CocClear/Scenes/VisualNovel.unity";

        [MenuItem("CocClear/Create Visual Novel Scene")]
        public static void CreateScene()
        {
            Directory.CreateDirectory("Assets/CocClear/Scenes");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
        }

        public static void BuildWindows()
        {
            PlayerSettings.resizableWindow = true;

            if (!File.Exists(ScenePath))
            {
                CreateScene();
            }

            var outputDirectory = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "_scratch", "WindowsBuild"));
            Directory.CreateDirectory(outputDirectory);
            var outputPath = Path.Combine(outputDirectory, "CoC-Clear.exe");
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = outputPath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None,
            });

            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException($"Windows build failed: {report.summary.result}");
            }
        }
    }
}
