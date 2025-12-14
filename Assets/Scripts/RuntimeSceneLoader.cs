using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

/// <summary>
/// クリックでシーン遷移（Button.onClick 不使用）
/// </summary>
public class RuntimeSceneLoader : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] public string SceneName;

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"[RuntimeSceneLoader] Clicked. SceneName={SceneName}");

        if (string.IsNullOrEmpty(SceneName))
        {
            Debug.LogError("[RuntimeSceneLoader] SceneName is empty.");
            return;
        }

        SceneManager.LoadScene(SceneName);
    }
}
