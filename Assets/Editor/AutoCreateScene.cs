using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class AutoCreateScene
{
    private const string ScenePath = "Assets/Scenes/Main.unity";

    [InitializeOnLoadMethod]
    private static void EnsureScene()
    {
        if (!File.Exists(ScenePath))
        {
            EditorApplication.delayCall += Build;
        }
    }

    [MenuItem("Tools/HowToFish/Пересоздать сцену")]
    public static void Build()
    {
        Directory.CreateDirectory("Assets/Scenes");

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var root = new GameObject("GameRoot");
        root.AddComponent<GameBootstrap>();

        EditorSceneManager.SaveScene(scene, ScenePath);

        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(ScenePath, true)
        };

        EditorSceneManager.OpenScene(ScenePath);
        Debug.Log("HowToFish: сцена создана в " + ScenePath);
    }
}