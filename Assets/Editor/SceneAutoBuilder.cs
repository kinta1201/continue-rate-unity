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

    [MenuItem("Tools/AutoBuild/Rebuild All Scenes")]
    public static void RebuildAllScenes()
    {
        EnsureScenesFolder();

        BuildTitleScene();
        BuildSimpleNavScene(ModeSelectSceneName, "Go Game", GameSceneName);
        BuildGameScene(); // ★プロトタイプ版
        BuildSimpleNavScene(ResultSceneName, "Back Title", TitleSceneName);

        ApplyBuildSettings();
        EditorUtility.DisplayDialog("OK", "All scenes rebuilt", "OK");
    }

    private static void ApplyBuildSettings()
    {
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene($"{ScenesFolder}/{TitleSceneName}.unity", true),
            new EditorBuildSettingsScene($"{ScenesFolder}/{ModeSelectSceneName}.unity", true),
            new EditorBuildSettingsScene($"{ScenesFolder}/{GameSceneName}.unity", true),
            new EditorBuildSettingsScene($"{ScenesFolder}/{ResultSceneName}.unity", true),
        };
    }

    // ---------- Scenes ----------

    private static void BuildTitleScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        scene.name = TitleSceneName;

        EnsureEventSystem(scene);
        var canvas = EnsureCanvas(scene);

        CreateHeaderText(canvas.transform, "TITLE");

        var button = CreateButton(canvas.transform, "StartButton", "Start", new Vector2(0, -40));
        var loader = button.gameObject.AddComponent<RuntimeSceneLoader>();
        loader.SceneName = ModeSelectSceneName;

        SaveScene(scene, TitleSceneName);
    }

    private static void BuildSimpleNavScene(string name, string label, string next)
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        scene.name = name;

        EnsureEventSystem(scene);
        var canvas = EnsureCanvas(scene);

        CreateHeaderText(canvas.transform, name.ToUpperInvariant());

        var button = CreateButton(canvas.transform, "NavButton", label, new Vector2(0, -40));
        var loader = button.gameObject.AddComponent<RuntimeSceneLoader>();
        loader.SceneName = next;

        SaveScene(scene, name);
    }

    /// <summary>
    /// Gameシーン：継続率体感/当て を1画面で体験できるプロトタイプUIを自動生成
    /// </summary>
    private static void BuildGameScene()
    {
        Debug.Log("[SceneAutoBuilder] Building Game (Prototype)...");

        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        scene.name = GameSceneName;

        EnsureEventSystem(scene);
        var canvas = EnsureCanvas(scene);

        CreateHeaderText(canvas.transform, "GAME");

        // ---- UI: Mode Dropdown ----
        CreateDropdown(canvas.transform, "ModeDropdown", new Vector2(0, 140), new Vector2(520, 60));

        // ---- UI: Rate Slider + Label ----
        CreateText(canvas.transform, "RateLabel", "設定継続率：80%", new Vector2(0, 70), 26);
        CreateSlider(canvas.transform, "RateSlider", new Vector2(0, 20), new Vector2(520, 40));

        // ---- UI: Door Button ----
        var doorButton = CreateButton(canvas.transform, "DoorButton", "Door", new Vector2(0, -60));
        var doorLabel = doorButton.transform.Find("Text")?.GetComponent<Text>();
        if (doorLabel != null) doorLabel.name = "DoorLabel";

        // ---- UI: Info Text ----
        var infoText = CreateText(canvas.transform, "InfoText",
            "ドアを押して体感しよう。\n成功→次へ / 失敗→ドボン", new Vector2(0, -160), 22);
        infoText.alignment = TextAnchor.UpperCenter;

        // ---- UI: Next/Retry Button ----
        var nextButton = CreateButton(canvas.transform, "NextOrRetryButton", "次のステージへ", new Vector2(0, -260));
        var nextLabel = nextButton.transform.Find("Text")?.GetComponent<Text>();
        if (nextLabel != null) nextLabel.name = "NextOrRetryLabel";

        // ---- Controller (attach to Canvas) ----
        canvas.gameObject.GetComponent<ContinueRateGameScenePrototype>()
var existing = canvas.gameObject.GetComponent<ContinueRateGameScenePrototype>();
if (existing == null)
{
    canvas.gameObject.AddComponent<ContinueRateGameScenePrototype>();
}


        SaveScene(scene, GameSceneName);
    }

    // ---------- Helpers ----------

    private static void EnsureScenesFolder()
    {
        if (!AssetDatabase.IsValidFolder(ScenesFolder))
        {
            Directory.CreateDirectory(ScenesFolder);
            AssetDatabase.Refresh();
        }
    }

    private static void SaveScene(Scene scene, string name)
    {
        EditorSceneManager.SaveScene(scene, $"{ScenesFolder}/{name}.unity");
    }

    private static void EnsureEventSystem(Scene scene)
    {
        if (Object.FindObjectOfType<EventSystem>() != null) return;

        var es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();

        // 入力差でクリック死なないように（両対応）
#if ENABLE_INPUT_SYSTEM
        es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
        es.AddComponent<StandaloneInputModule>();
#endif

        SceneManager.MoveGameObjectToScene(es, scene);
    }

    private static Canvas EnsureCanvas(Scene scene)
    {
        // 既存があれば使う（NewSceneなので基本無いが保険）
        var existing = Object.FindObjectOfType<Canvas>();
        if (existing != null) return existing;

        var go = new GameObject("Canvas");
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        go.AddComponent<CanvasScaler>();
        go.AddComponent<GraphicRaycaster>();
        SceneManager.MoveGameObjectToScene(go, scene);
        return canvas;
    }

    private static void CreateHeaderText(Transform parent, string text)
    {
        var go = new GameObject("HeaderText");
        go.transform.SetParent(parent, false);

        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0, -30);
        rt.sizeDelta = new Vector2(600, 80);

        var t = go.AddComponent<Text>();
        t.text = text;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = 36;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = Color.black;
        t.raycastTarget = false;
    }

    private static Text CreateText(Transform canvas, string name, string text, Vector2 anchoredPos, int fontSize)
    {
        var go = new GameObject(name);
        go.transform.SetParent(canvas, false);

        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = new Vector2(900, 220);

        var uiText = go.AddComponent<Text>();
        uiText.text = text;
        uiText.alignment = TextAnchor.MiddleCenter;
        uiText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        uiText.fontSize = fontSize;
        uiText.color = Color.black;
        uiText.raycastTarget = false;

        return uiText;
    }

    private static Slider CreateSlider(Transform canvas, string name, Vector2 anchoredPos, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(canvas, false);

        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        var slider = go.AddComponent<Slider>();
        slider.minValue = 1;
        slider.maxValue = 100;
        slider.wholeNumbers = true;
        slider.value = 80;

        // Background
        var bg = new GameObject("Background");
        bg.transform.SetParent(go.transform, false);
        var bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.85f, 0.85f, 0.85f, 1f);
        var bgRt = bg.GetComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;

        // Fill Area
        var fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(go.transform, false);
        var faRt = fillArea.AddComponent<RectTransform>();
        faRt.anchorMin = new Vector2(0, 0.25f);
        faRt.anchorMax = new Vector2(1, 0.75f);
        faRt.offsetMin = new Vector2(10, 0);
        faRt.offsetMax = new Vector2(-10, 0);

        // Fill
        var fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        var fillImg = fill.AddComponent<Image>();
        fillImg.color = new Color(0.2f, 0.6f, 0.9f, 1f);
        var fillRt = fill.GetComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero;
        fillRt.anchorMax = Vector2.one;
        fillRt.offsetMin = Vector2.zero;
        fillRt.offsetMax = Vector2.zero;

        // Handle Slide Area
        var handleArea = new GameObject("Handle Slide Area");
        handleArea.transform.SetParent(go.transform, false);
        var haRt = handleArea.AddComponent<RectTransform>();
        haRt.anchorMin = Vector2.zero;
        haRt.anchorMax = Vector2.one;
        haRt.offsetMin = new Vector2(10, 0);
        haRt.offsetMax = new Vector2(-10, 0);

        // Handle
        var handle = new GameObject("Handle");
        handle.transform.SetParent(handleArea.transform, false);
        var handleImg = handle.AddComponent<Image>();
        handleImg.color = new Color(1f, 1f, 1f, 1f);
        var handleRt = handle.GetComponent<RectTransform>();
        handleRt.sizeDelta = new Vector2(24, 24);

        slider.targetGraphic = handleImg;
        slider.fillRect = fillRt;
        slider.handleRect = handleRt;
        slider.direction = Slider.Direction.LeftToRight;

        return slider;
    }

    private static Dropdown CreateDropdown(Transform canvas, string name, Vector2 anchoredPos, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(canvas, false);

        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        var img = go.AddComponent<Image>();
        img.color = new Color(0.9f, 0.9f, 0.9f, 1f);

        var dd = go.AddComponent<Dropdown>();
        dd.targetGraphic = img;

        // Label
        var labelGO = new GameObject("Label");
        labelGO.transform.SetParent(go.transform, false);
        var label = labelGO.AddComponent<Text>();
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = 24;
        label.color = Color.black;
        label.alignment = TextAnchor.MiddleLeft;
        label.text = "継続率体感モード";

        var lrt = labelGO.GetComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero;
        lrt.anchorMax = Vector2.one;
        lrt.offsetMin = new Vector2(16, 6);
        lrt.offsetMax = new Vector2(-16, -6);

        dd.captionText = label;

        // Minimal Template (required by Dropdown)
        var templateGO = new GameObject("Template");
        templateGO.SetActive(false);
        templateGO.transform.SetParent(go.transform, false);
        var templateRT = templateGO.AddComponent<RectTransform>();
        templateRT.pivot = new Vector2(0.5f, 1f);
        templateRT.anchorMin = new Vector2(0, 0);
        templateRT.anchorMax = new Vector2(1, 0);
        templateRT.anchoredPosition = new Vector2(0, -5);
        templateRT.sizeDelta = new Vector2(0, 200);

        templateGO.AddComponent<Image>().color = new Color(0.95f, 0.95f, 0.95f, 1f);
        var scrollRect = templateGO.AddComponent<ScrollRect>();

        var viewport = new GameObject("Viewport");
        viewport.transform.SetParent(templateGO.transform, false);
        var viewportRT = viewport.AddComponent<RectTransform>();
        viewportRT.anchorMin = Vector2.zero;
        viewportRT.anchorMax = Vector2.one;
        viewportRT.offsetMin = Vector2.zero;
        viewportRT.offsetMax = Vector2.zero;
        viewport.AddComponent<Mask>().showMaskGraphic = false;
        viewport.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.02f);

        var content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        var contentRT = content.AddComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0, 1);
        contentRT.anchorMax = new Vector2(1, 1);
        contentRT.pivot = new Vector2(0.5f, 1);
        contentRT.anchoredPosition = Vector2.zero;
        contentRT.sizeDelta = new Vector2(0, 200);

        scrollRect.viewport = viewportRT;
        scrollRect.content = contentRT;

        var item = new GameObject("Item");
        item.transform.SetParent(content.transform, false);
        var itemRT = item.AddComponent<RectTransform>();
        itemRT.anchorMin = new Vector2(0, 1);
        itemRT.anchorMax = new Vector2(1, 1);
        itemRT.pivot = new Vector2(0.5f, 1);
        itemRT.sizeDelta = new Vector2(0, 50);

        item.AddComponent<Image>().color = Color.white;
        item.AddComponent<Toggle>();

        var itemLabelGO = new GameObject("Item Label");
        itemLabelGO.transform.SetParent(item.transform, false);
        var itemLabel = itemLabelGO.AddComponent<Text>();
        itemLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        itemLabel.fontSize = 24;
        itemLabel.color = Color.black;
        itemLabel.alignment = TextAnchor.MiddleLeft;
        itemLabel.text = "Option";

        var ilrt = itemLabelGO.GetComponent<RectTransform>();
        ilrt.anchorMin = Vector2.zero;
        ilrt.anchorMax = Vector2.one;
        ilrt.offsetMin = new Vector2(16, 0);
        ilrt.offsetMax = new Vector2(-16, 0);

        dd.template = templateRT;
        dd.itemText = itemLabel;

        // optionsは ContinueRateGameScenePrototype が Start() で入れる
        return dd;
    }

    private static Button CreateButton(Transform parent, string name, string label, Vector2 pos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);

        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(280, 70);
        rt.anchoredPosition = pos;

        var img = go.AddComponent<Image>();
        img.raycastTarget = true;

        var btn = go.AddComponent<Button>();

        var textGO = new GameObject("Text");
        textGO.transform.SetParent(go.transform, false);

        var trt = textGO.AddComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;

        var tt = textGO.AddComponent<Text>();
        tt.text = label;
        tt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        tt.alignment = TextAnchor.MiddleCenter;
        tt.color = Color.black;
        tt.raycastTarget = false;

        return btn;
    }
}
#endif
