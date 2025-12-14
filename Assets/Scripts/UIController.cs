using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI画面切替とテキスト更新を担当
/// </summary>
public class UIController : MonoBehaviour
{
    [Header("Screens")]
    public GameObject titleScreen;
    public GameObject modeSelectScreen;
    public GameObject rateSettingScreen;
    public GameObject playScreen;
    public GameObject resultScreen;

    [Header("Rate Setting (Experience)")]
    public Slider rateSlider;
    public Text rateText;
    public Button playStartButton;

    [Header("Play")]
    public Text streakText;

    [Header("Result Common")]
    public Text resultStreakText;
    public Text resultRateText; // 体感モード用「設定した継続率」
    public Button retryButton;
    public Button backToModeSelectButton;

    [Header("Guess Mode UI")]
    public GameObject guessInputGroup;
    public Slider guessSlider;
    public Text guessSliderText;
    public Button guessConfirmButton;
    public Text actualRateText; // 初期非表示

    private bool _guessConfirmed = false;

    private void Start()
    {
        // 初期表示
        ShowOnly(titleScreen);

        if (rateSlider != null)
        {
            rateSlider.minValue = 1;
            rateSlider.maxValue = 100;
            rateSlider.wholeNumbers = true;
            rateSlider.value = 50;
            rateSlider.onValueChanged.AddListener(OnRateSliderChanged);
            OnRateSliderChanged(rateSlider.value);
        }

        if (guessSlider != null)
        {
            guessSlider.minValue = 1;
            guessSlider.maxValue = 100;
            guessSlider.wholeNumbers = true;
            guessSlider.value = 50;
            guessSlider.onValueChanged.AddListener(OnGuessSliderChanged);
            OnGuessSliderChanged(guessSlider.value);
        }

        if (actualRateText != null) actualRateText.gameObject.SetActive(false);
        if (guessInputGroup != null) guessInputGroup.SetActive(false);
    }

    // ---- Screen helpers ----
    private void ShowOnly(GameObject active)
    {
        if (titleScreen != null) titleScreen.SetActive(active == titleScreen);
        if (modeSelectScreen != null) modeSelectScreen.SetActive(active == modeSelectScreen);
        if (rateSettingScreen != null) rateSettingScreen.SetActive(active == rateSettingScreen);
        if (playScreen != null) playScreen.SetActive(active == playScreen);
        if (resultScreen != null) resultScreen.SetActive(active == resultScreen);
    }

    public void ShowTitle() => ShowOnly(titleScreen);

    public void ShowModeSelect() => ShowOnly(modeSelectScreen);

    // ---- Button handlers ----
    public void OnStartButtonClicked()
    {
        ShowModeSelect();
    }

    public void OnExperienceModeSelected()
    {
        GameManager.Instance.SetMode(GameMode.Experience);
        ShowOnly(rateSettingScreen);
    }

    public void OnGuessModeSelected()
    {
        GameManager.Instance.SetMode(GameMode.Guess);
        StartPlay();
    }

    public void OnPlayStartClicked()
    {
        int p = (rateSlider != null) ? Mathf.RoundToInt(rateSlider.value) : 50;
        GameManager.Instance.SetConfiguredRate(p);
        StartPlay();
    }

    public void OnRetryClicked()
    {
        // 同モードで再プレイ。当てモードは新しい継続率にする
        if (GameManager.Instance.CurrentMode == GameMode.Guess)
        {
            GameManager.Instance.SetMode(GameMode.Guess);
        }
        StartPlay();
    }

    public void OnBackToModeSelectClicked()
    {
        ShowModeSelect();
    }

    public void OnGuessConfirmClicked()
    {
        _guessConfirmed = true;
        int guess = (guessSlider != null) ? Mathf.RoundToInt(guessSlider.value) : 50;

        if (actualRateText != null)
        {
            actualRateText.text = $"実際の継続率：{GameManager.Instance.ActualRatePercent}%";
            actualRateText.gameObject.SetActive(true);
        }

        // 予想値は「あなたの予想：XX%」として表示したければ、ここで追加Textを用意して更新してください
        // 今回はスライダー表示だけでOKにしています。
        if (guessConfirmButton != null) guessConfirmButton.interactable = false;
    }

    // ---- Slider handlers ----
    private void OnRateSliderChanged(float v)
    {
        if (rateText != null) rateText.text = $"{Mathf.RoundToInt(v)}%";
    }

    private void OnGuessSliderChanged(float v)
    {
        if (guessSliderText != null) guessSliderText.text = $"{Mathf.RoundToInt(v)}%";
    }

    // ---- Play / Result ----
    private void StartPlay()
    {
        _guessConfirmed = false;

        GameManager.Instance.StartRun();
        UpdateStreak(0);

        // 結果画面の当てUI初期化
        if (guessInputGroup != null) guessInputGroup.SetActive(false);
        if (actualRateText != null) actualRateText.gameObject.SetActive(false);
        if (guessConfirmButton != null) guessConfirmButton.interactable = true;

        ShowOnly(playScreen);
    }

    public void UpdateStreak(int streak)
    {
        if (streakText != null) streakText.text = $"連続成功：{streak}";
    }

    public void ShowResult()
    {
        ShowOnly(resultScreen);

        int streak = GameManager.Instance.Streak;
        if (resultStreakText != null) resultStreakText.text = $"連続成功：{streak}";

        if (GameManager.Instance.CurrentMode == GameMode.Experience)
        {
            if (resultRateText != null)
            {
                resultRateText.gameObject.SetActive(true);
                resultRateText.text = $"設定した継続率：{GameManager.Instance.ConfiguredRatePercent}%";
            }
            if (guessInputGroup != null) guessInputGroup.SetActive(false);
            if (actualRateText != null) actualRateText.gameObject.SetActive(false);
        }
        else // Guess
        {
            if (resultRateText != null) resultRateText.gameObject.SetActive(false);
            if (guessInputGroup != null) guessInputGroup.SetActive(true);
            if (actualRateText != null) actualRateText.gameObject.SetActive(false);
            if (guessConfirmButton != null) guessConfirmButton.interactable = true;
        }
    }

    // ---- PlayScreenアクティブ判定 ----
    public bool IsPlayScreenActive()
    {
        return playScreen != null && playScreen.activeInHierarchy;
    }
}
