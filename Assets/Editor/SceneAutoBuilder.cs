using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using System.IO;

public class SceneAutoBuilder
{
    private static readonly string[] SceneNames = { "Title", "ModeSelect", "Game", "Result" };
    private static readonly string ScenePathRoot = "Assets/Scenes/";

    [MenuItem("Tools/ContinueRate/Generate Scenes")]
    public static void GenerateScenes()
    {
        if (!Directory.Exists(ScenePathRoot))
            Directory.CreateDirectory(ScenePathRoot);

        for (int i = 0; i < SceneNames.Length; i++)
        {
            string sceneName = SceneNames[i];
            string scenePath = ScenePathRoot + sceneName + ".unity";

            var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject eventSystem = EnsureEventSystem();
            GameObject canvas = EnsureCanvas();

            string nextScene = null;
            string buttonText = null;
            if (sceneName == "Title") { nextScene = "ModeSelect"; buttonText = "Start"; }
            else if (sceneName == "ModeSelect") { nextScene = "Game"; buttonText = "Go Game"; }
            else if (sceneName == "Game") { nextScene = null; buttonText = "Door"; }
            else if (sceneName == "Result") { nextScene = "Title"; buttonText = "Back Title"; }

            GameObject buttonGO = CreateButton(canvas.transform, buttonText);

            // スクリプトアタッチ&OnClick設定
            if (sceneName == "Game")
            {
                var door = buttonGO.AddComponent<DoorController>();
                var btn = buttonGO.GetComponent<Button>();
                btn.onClick.RemoveAllListeners();
                UnityEditor.Events.UnityEventTools.AddPersistentListener(btn.onClick, door.GoResult);
            }
            else
            {
                var loader = canvas.GetComponent<SceneLoader>() ?? canvas.AddComponent<SceneLoader>();
                var btn = buttonGO.GetComponent<Button>();
                btn.onClick.RemoveAllListeners();
                UnityEditor.Events.UnityEventTools.AddStringPersistentListener(btn.onClick, loader.Load, nextScene);
            }
            // シーン保存
            EditorSceneManager.SaveScene(newScene, scenePath);
        }
        // Build Settings追加
        UpdateBuildSettings(SceneNames);
        EditorUtility.DisplayDialog("Scene生成完了", "全シーン自動生成・設定が完了しました。", "OK");
    }

    private static GameObject EnsureEventSystem()
    {
        var esObj = GameObject.FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
        if (esObj != null) return esObj.gameObject;
        var go = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));
        return go;
    }
    private static GameObject EnsureCanvas()
    {
        var canvas = GameObject.FindObjectOfType<Canvas>();
        if (canvas != null) return canvas.gameObject;
        var go = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        go.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        go.transform.SetAsLastSibling();
        return go;
    }
    private static GameObject CreateButton(Transform parent, string text)
    {
        var go = new GameObject("Button", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var img = go.GetComponent<Image>();
        img.color = new Color(1, 1, 1, 1);
        var btn = go.GetComponent<Button>();
        // 位置調整
        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(160, 40);
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        // Text生成
        var textGO = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        textGO.transform.SetParent(go.transform, false);
        var txt = textGO.GetComponent<Text>();
        txt.text = text;
        txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        txt.fontSize = 24;
        txt.color = Color.black;
        txt.alignment = TextAnchor.MiddleCenter;
        // Text Stretch
        var textRt = textGO.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero; textRt.anchorMax = Vector2.one;
        textRt.sizeDelta = Vector2.zero;
        return go;
    }
    private static void UpdateBuildSettings(string[] sceneNames)
    {
        var editorScenes = new EditorBuildSettingsScene[sceneNames.Length];
        for (int i = 0; i < sceneNames.Length; i++)
        {
            string scenePath = ScenePathRoot + sceneNames[i] + ".unity";
            editorScenes[i] = new EditorBuildSettingsScene(scenePath, true);
        }
        EditorBuildSettings.scenes = editorScenes;
    }
}
