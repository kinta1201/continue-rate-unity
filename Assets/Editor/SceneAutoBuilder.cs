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

        BuildTitleScene();
        BuildNavScene(ModeSelectSceneName, "Go Game", GameSceneName);
        BuildGameScene();
        BuildNavScene(ResultSceneName, "Back Title", TitleSceneName);

        ApplyBuildSettings();

        EditorUtility.DisplayDialog("Scene生成完了", "全シーン自動生成・上書きが完了しました。", "OK");
        Debug.Log("[SceneAutoBuilder] Rebuild All Scenes completed.");
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
        Debug.Log("[SceneAutoBuilder] Build Settings updated.");
    }

    [MenuItem("Tools/AutoBuild/Open Title Scene")]
    public static void OpenTitleScene()
    {
        var path = GetScenePath(TitleSceneName);
        EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
    }

    // -----------------------------
    // Scene Builders
    // -----------------------------

    private static void BuildTitleScene()
    {
        Debug.Log("[SceneAutoBuilder] Building Title...");
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        scene.name = TitleSceneName;

        EnsureEventSystem(scene);
        var canvas = EnsureCanvas(scene);

        CreateHeaderText(canvas.transform, "TITLE");

        var button = CreateButton(canvas.transform, "Start", new Vector2(0, -40));

        // ★ OnClickは使わない：クリックは RuntimeSceneLoader(IPointerClickHandler) が拾う
        var loader = button.gameObject.GetComponent<RuntimeSceneLoader>() ??
                     button.gameObject.AddComponent<RuntimeSceneLoader>();
        loader.SceneName = ModeSelectSceneName;

        SaveScene(scene, TitleSceneName);
    }

    private static void BuildNavScene(string sceneName, string buttonLabel, string nextSceneName)
    {
        Debug.Log($"[SceneAutoBuilder] Building {sceneName}...");
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        scene.name = sceneName;

        EnsureEventSystem(scene);
        var canvas = EnsureCanvas(scene);

        CreateHeaderText(canvas.transform, sceneName.ToUpperInvariant());

        var button = CreateButton(canvas.transform, buttonLabel, new Vector2(0, -40));

        var loader = button.gameObject.GetComponent<RuntimeSceneLoader>() ??
                     button.gameObject.AddComponent<RuntimeSceneLoader>();
        loader.SceneName = nextSceneName;

        SaveScene(scene, sceneName);
    }

    private static void BuildGameScene()
    {
        Debug.Log("[SceneAutoBuilder] Building Game...");
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        scene.name = GameSceneName;

        EnsureEventSystem(scene);
        var canvas = EnsureCanvas(scene);

        CreateHeaderText(canvas.transform, "GAME");

        var button = CreateButton(canvas.transform, "Door", new Vector2(0, -40));

        var loader = button.gameObject.GetComponent<RuntimeSceneLoader>() ??
                     button.gameObject.AddComponent<RuntimeSceneLoader>();
        loader.SceneName = ResultSceneName;

        SaveScene(scene, GameSceneName);
    }

    // -----------------------------
    // Helpers
    // -----------------------------

    private static void EnsureScenesFolder()
    {
        if (!AssetDatabase.IsValidFolder(ScenesFolder))
        {
            Directory.CreateDirectory(ScenesFolder);
            AssetDatabase.Refresh();
        }
    }

    private static string GetScenePath(string sceneName) => $"{ScenesFolder}/{sceneName}.unity";

    private static void SaveScene(Scene scene, string sceneName)
    {
        string path = GetScenePath(sceneName);
        bool ok = EditorSceneManager.SaveScene(scene, path);
        if (!ok) Debug.LogError($"[SceneAutoBuilder] Failed to save scene: {path}");
        else Debug.Log($"[SceneAutoBuilder] Scene saved: {path}");
    }

    private static void EnsureEventSystem(Scene scene)
    {
        // NewSceneMode.Singleなので基本はこれでOK
        if (Object.FindObjectOfType<EventSystem>() != null) return;

        var es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();

        // 入力システム差でクリックが死なないように分岐（両対応）
#if ENABLE_INPUT_SYSTEM
        es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#elif ENABLE_LEGACY_INPUT_MANAGER
        es.AddComponent<StandaloneInputModule>();
#else
        es.AddComponent<StandaloneInputModule>();
#endif

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
        uiText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
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
        image.raycastTarget = true;

        // Buttonは見た目用に残してOK（OnClickは使わない）
        var button = buttonGO.AddComponent<Button>();

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
        uiText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        uiText.fontSize = 28;
        uiText.color = Color.black;
        uiText.raycastTarget = false;

        return button;
    }
}
#endif
