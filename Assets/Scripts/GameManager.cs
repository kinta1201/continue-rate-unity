using UnityEngine;

public enum GameMode
{
    None = 0,
    Experience = 1, // 継続率体感
    Guess = 2       // 継続率当て
}

/// <summary>
/// ゲーム状態・モード管理（見た目不要）
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameMode CurrentMode { get; private set; } = GameMode.None;

    /// <summary>体感モードで設定された継続率(1-100)</summary>
    public int ConfiguredRatePercent { get; private set; } = 50;

    /// <summary>当てモードで内部設定された継続率(1-100)</summary>
    public int ActualRatePercent { get; private set; } = 50;

    public int Streak { get; private set; } = 0;

    public int TotalTries { get; private set; }
    public int TotalSuccess { get; private set; }
    public bool LastResultSuccess { get; private set; }
    public int UserGuessPercent { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetMode(GameMode mode)
    {
        CurrentMode = mode;

        if (mode == GameMode.Guess)
        {
            // 1〜100で内部決定（プレイヤーには非表示）
            ActualRatePercent = Random.Range(1, 101);
        }
    }

    public void SetConfiguredRate(int ratePercent)
    {
        ConfiguredRatePercent = Mathf.Clamp(ratePercent, 1, 100);
    }

    public void StartRun()
    {
        Streak = 0;
    }

    public void SetUserGuess(int percent)
    {
        UserGuessPercent = Mathf.Clamp(percent, 1, 100);
    }

    public void ApplyTrialResult(bool success)
    {
        LastResultSuccess = success;
        TotalTries += 1;
        if (success) TotalSuccess += 1;
        Streak = success ? (Streak + 1) : 0;
    }

    /// <summary>
    /// ドアを開ける試行。成功ならstreakを増やしtrue、失敗ならfalse。
    /// </summary>
    public bool TryOpenDoor()
    {
        int rate = (CurrentMode == GameMode.Experience) ? ConfiguredRatePercent : ActualRatePercent;
        float p = rate / 100f;

        bool success = ProbabilityEngine.IsSuccess(p);
        return success;
    }
}
