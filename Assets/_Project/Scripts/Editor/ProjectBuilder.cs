using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace HollowGround.Editor
{
    public static class ProjectBuilder
    {
        private const string BuildPath = "Build";
        private const string ExeName = "HollowGround";

        [MenuItem("HollowGround/Build/Build Windows x64 (Development)", false, 300)]
        public static void BuildWindowsDevelopment()
        {
            Build(BuildOptions.Development);
        }

        [MenuItem("HollowGround/Build/Build Windows x64 (Release)", false, 301)]
        public static void BuildWindowsRelease()
        {
            Build(BuildOptions.None);
        }

        private static void Build(BuildOptions options)
        {
            string[] scenes = { "Assets/Scenes/Main Scene.unity" };
            string path = $"{BuildPath}/{ExeName}.exe";

            EditorUserBuildSettings.standaloneBuildSubtarget = StandaloneBuildSubtarget.Player;

            BuildReport report = BuildPipeline.BuildPlayer(scenes, path, BuildTarget.StandaloneWindows64, options);

            if (report.summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"Build succeeded: {report.summary.totalSize} bytes -> {path}");
                EditorUtility.RevealInFinder(path);
            }
            else
            {
                Debug.LogError($"Build failed: {report.summary.result}");
                foreach (var step in report.steps)
                {
                    foreach (var msg in step.messages)
                    {
                        if (msg.type == LogType.Error)
                            Debug.LogError($"  {msg.content}");
                    }
                }
            }
        }

        [MenuItem("HollowGround/Build/Open Build Folder", false, 302)]
        public static void OpenBuildFolder()
        {
            EditorUtility.RevealInFinder($"{BuildPath}/{ExeName}.exe");
        }
    }
}
