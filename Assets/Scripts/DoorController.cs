using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// クリック受付 → GameManagerへ通知 → Resultシーンへ遷移
/// </summary>
public class DoorController : MonoBehaviour
{
    [Header("References")]
    public Button doorButton;
    public DoorView doorView;
    public UIController ui;

    private void Awake()
    {
        if (doorButton != null)
        {
            doorButton.onClick.RemoveAllListeners(); // 重複防止
            doorButton.onClick.AddListener(OnDoorClicked);
        }
    }

    public void OnDoorClicked()
    {
        Debug.Log($"[OnDoorClicked] {gameObject.name} instanceID={GetInstanceID()} time={Time.time}");
        if (GameManager.Instance == null) return;

        // PlayScreenがアクティブでなければ無視
        if (ui == null || !ui.IsPlayScreenActive()) return;

        bool success = GameManager.Instance.TryOpenDoor();
        GameManager.Instance.ApplyTrialResult(success);

        // Resultシーンへ
        SceneManager.LoadScene("Result");
    }
}
