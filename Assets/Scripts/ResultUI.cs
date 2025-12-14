using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class ResultUI : MonoBehaviour
{
    public TextMeshProUGUI resultTitleText;
    public TextMeshProUGUI detailText;

    void Start()
    {
        var gm = GameManager.Instance;
        if (gm == null) return;
        // タイトル表示
        resultTitleText.text = gm.LastResultSuccess ? "SUCCESS" : "FAIL";

        // 共通情報
        string info = $"Mode: {gm.CurrentMode}\n";
        info += $"Streak: {gm.Streak}\n";
        info += $"Tries: {gm.TotalTries} / Success: {gm.TotalSuccess} / Rate: {(gm.TotalTries > 0 ? (gm.TotalSuccess * 100 / gm.TotalTries) : 0)}%\n";

        if (gm.CurrentMode == GameMode.Experience) {
            info += $"ConfiguredRate: {gm.ConfiguredRatePercent}%\n";
        } else if (gm.CurrentMode == GameMode.Guess) {
            info += $"ActualRate: {gm.ActualRatePercent}%\n";
            info += $"YourGuess: {gm.UserGuessPercent}%\n";
            info += $"Error: {Mathf.Abs(gm.UserGuessPercent - gm.ActualRatePercent)}\n";
        }
        detailText.text = info;
    }

    public void OnClickRetry()
    {
        SceneManager.LoadScene("Game");
    }
    public void OnClickModeSelect()
    {
        SceneManager.LoadScene("ModeSelect");
    }
}

