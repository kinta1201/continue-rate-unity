using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameMode { None, Experience, Guess }

    public GameMode CurrentMode { get; private set; } = GameMode.None;
    public int ConfiguredRate { get; private set; } = 50;
    public int ActualRate { get; private set; } = 50;
    public int Streak { get; private set; } = 0;
    public int UserGuess { get; private set; } = 50;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetMode(GameMode mode)
    {
        CurrentMode = mode;

        if (mode == GameMode.Guess)
        {
            ActualRate = Random.Range(1, 101);
        }
    }

    public void SetConfiguredRate(int rate)
    {
        ConfiguredRate = rate;
    }

    public void SetUserGuess(int guess)
    {
        UserGuess = guess;
    }

    public void ResetStreak()
    {
        Streak = 0;
    }

    /// <summary>
    /// ドアを開ける試行。成功ならStreakを増やし、失敗ならStreakをリセット。
    /// </summary>
    public bool TryOpenDoor()
    {
        int rate = (CurrentMode == GameMode.Experience) ? ConfiguredRate : ActualRate;
        float probability = rate / 100f;

        bool success = ProbabilityEngine.IsSuccess(probability);

        if (success)
        {
            Streak++;
        }
        else
        {
            Streak = 0;
        }

        return success;
    }
}