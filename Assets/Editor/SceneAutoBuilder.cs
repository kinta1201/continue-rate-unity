// Assets/Editor/SceneAutoBuilder.cs
// Unity 2022.3 LTS
// 冪等：Rebuildを何回実行しても同一のシーン構成を再生成する

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class SceneAutoBuilder
{
    private const string ScenesFolder = "Assets/Scenes";
    private const string TitleSceneName = "Title";
    private const string ModeSelectSceneName = "ModeSelect";
    private const string GameSceneName = "Game";
    private const string ResultSceneName = "Result";

    private static readonly string[] SceneNamesInOrder =
    {
        TitleSceneName,
        ModeSelectSceneName,
        GameSceneName,
        ResultSceneName
    };

    [MenuItem("Tools/AutoBuild/Rebuild All Scenes")]
    public static void RebuildAllScenes()
    {
        EnsureScenesFolder();

        // Title
        BuildTitleScene();

        // ModeSelect
        BuildSimpleNavScene(ModeSelectSceneName, "Go Game", GameSceneName);

        // Game
        BuildSimpleNavScene(GameSceneName, "To Result", ResultSceneName);

        // Result
        BuildSimpleNavScene(ResultSceneName, "Back Title", TitleSceneName);

        ApplyBuildSettings();

        Debug.Log("[SceneAutoBuilder] Rebuild All Scenes done.");
    }

    [MenuItem("Tools/AutoBuild/Apply Build Settings")]
    public static void ApplyBuildSettings()
    {
        var scenes = new EditorBuildSettingsScene[SceneNamesInOrder.Length];
        for (int i = 0; i < SceneNamesInOrder.Length; i++)
        {
            string path = GetScenePath(SceneNamesInOrder[i]);
            scenes[i] = new EditorBuildSettingsScene(path, true);
        }

        EditorBuildSettings.scenes = scenes;
        Debug.Log("[SceneAutoBuilder] Build Settings updated (Scenes In Build applied).");
    }

    [MenuItem("Tools/AutoBuild/Open Title Scene")]
    public static void OpenTitleScene()
    {
        string path = GetScenePath(TitleSceneName);
        if (!File.Exists(path))
        {
            Debug.LogError($"[SceneAutoBuilder] Title scene not found: {path}. Run Rebuild All Scenes first.");
            return;
        }
        EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
    }

    // ------------------------
    // Scene Builders
    // ------------------------

    private static void BuildTitleScene()
    {
        // 重要：新規シーンを作り直す（冪等 & 生成漏れ防止）
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        scene.name = TitleSceneName;

        // 必ずUIを生成（今回の不具合の根絶）
        EnsureEventSystem(scene);
        var canvas = EnsureCanvas(scene);

        // 見出し
        CreateHeaderText(canvas.transform, "TITLE");

        // Button: Start -> ModeSelect
        var button = CreateButton(canvas.transform, "Start", new Vector2(0, -40));
        WireButtonToLoadScene(button, ModeSelectSceneName);

        SaveActiveSceneAs(TitleSceneName);
        Debug.Log("[SceneAutoBuilder] Title scene built with Canvas/EventSystem/Button and saved.");
    }

    private static void BuildSimpleNavScene(string sceneName, string buttonLabel, string nextSceneName)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        scene.name = sceneName;

        EnsureEventSystem(scene);
        var canvas = EnsureCanvas(scene);

        CreateHeaderText(canvas.transform, sceneName.ToUpperInvariant());

        var button = CreateButton(canvas.transform, buttonLabel, new Vector2(0, -40));
        WireButtonToLoadScene(button, nextSceneName);

        SaveActiveSceneAs(sceneName);
        Debug.Log($"[SceneAutoBuilder] {sceneName} scene built and saved.");
    }

    // ------------------------
    // UI Helpers
    // ------------------------

    private static void EnsureScenesFolder()
    {
        if (!AssetDatabase.IsValidFolder(ScenesFolder))
        {
            Directory.CreateDirectory(ScenesFolder);
            AssetDatabase.Refresh();
            Debug.Log($"[SceneAutoBuilder] Created folder: {ScenesFolder}");
        }
    }

    private static string GetScenePath(string sceneName)
        => $"{ScenesFolder}/{sceneName}.unity";

    private static void SaveActiveSceneAs(string sceneName)
    {
        string path = GetScenePath(sceneName);

        // 既存があっても上書き（冪等）
        bool ok = EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), path);
        if (!ok)
        {
            Debug.LogError($"[SceneAutoBuilder] Failed to save scene: {path}");
        }
    }

    private static void EnsureEventSystem(Scene scene)
    {
        // シーン内に既に存在するなら作らない
        if (Object.FindObjectOfType<EventSystem>() != null) return;

        var es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
        es.AddComponent<StandaloneInputModule>();
        SceneManager.MoveGameObjectToScene(es, scene);
    }

    private static Canvas EnsureCanvas(Scene scene)
    {
        var existing = Object.FindObjectOfType<Canvas>();
        if (existing != null) return existing;

        var canvasGO = new GameObject("Canvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        SceneManager.MoveGameObjectToScene(canvasGO, scene);
        return canvas;
    }

    private static void CreateHeaderText(Transform canvas, string text)
    {
        var go = new GameObject("HeaderText");
        go.transform.SetParent(canvas, false);

        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0, -30);
        rt.sizeDelta = new Vector2(600, 80);

        var uiText = go.AddComponent<Text>();
        uiText.text = text;
        uiText.alignment = TextAnchor.MiddleCenter;
        uiText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        uiText.fontSize = 36;
        uiText.color = Color.black;
    }

    private static Button CreateButton(Transform canvas, string label, Vector2 anchoredPos)
    {
        var buttonGO = new GameObject("Button");
        buttonGO.transform.SetParent(canvas, false);

        var rt = buttonGO.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = new Vector2(280, 70);

        var image = buttonGO.AddComponent<Image>();
        image.color = new Color(0.9f, 0.9f, 0.9f, 1f);

        var button = buttonGO.AddComponent<Button>();

        // 子Text
        var textGO = new GameObject("Text");
        textGO.transform.SetParent(buttonGO.transform, false);

        var trt = textGO.AddComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;

        var uiText = textGO.AddComponent<Text>();
        uiText.text = label;
        uiText.alignment = TextAnchor.MiddleCenter;
        uiText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        uiText.fontSize = 28;
        uiText.color = Color.black;

        return button;
    }

    private static void WireButtonToLoadScene(Button button, string sceneName)
    {
        // ランタイムで SceneManager.LoadScene(sceneName) を呼ぶだけのシンプルな実装に固定
        // ※ ここは「SceneNavigator」等に変えたいなら後で統一する
        var go = button.gameObject;
        var loader = go.GetComponent<RuntimeSceneLoader>();
        if (loader == null) loader = go.AddComponent<RuntimeSceneLoader>();
        loader.SceneName = sceneName;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(loader.Load);
    }
}

// ランタイム用コンポーネント（Editor生成物に貼り付けるためEditorフォルダ外に置きたいが、
// まずは不具合修正優先で同ファイル内定義 → 後で Assets/Scripts/Runtime に移動可）
public class RuntimeSceneLoader : MonoBehaviour
{
    public string SceneName;

    public void Load()
    {
        if (string.IsNullOrEmpty(SceneName))
        {
            Debug.LogError("[RuntimeSceneLoader] SceneName is empty.");
            return;
        }
        UnityEngine.SceneManagement.SceneManager.LoadScene(SceneName);
    }
}
#endif
