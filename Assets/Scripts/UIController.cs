using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [Header("Screens")]
    public GameObject titleScreen;
    public GameObject modeSelectScreen;
    public GameObject rateSettingScreen;
    public GameObject playScreen;
    public GameObject resultScreen;

    [Header("Rate Setting")]
    public Slider rateSlider;
    public Text rateText;

    [Header("Play Screen")]
    public Text streakText;

    [Header("Result Screen")]
    public Text resultTitleText;
    public Text resultStreakText;
    public Text resultRateText;
    public GameObject guessInputGroup;
    public Slider guessSlider;
    public Text guessSliderText;
    public Button guessConfirmButton;
    public Text actualRateText;

    [Header("References")]
    public DoorController doorController;

    void Start()
    {
        ShowTitle();

        // --- Rate Slider ---
        if (rateSlider != null)
        {
            // intにしたいならここで固定
            rateSlider.wholeNumbers = true;
            rateSlider.minValue = 1;
            rateSlider.maxValue = 100;

            // 多重登録防止
            rateSlider.onValueChanged.RemoveListener(OnRateSliderChanged);
            rateSlider.onValueChanged.AddListener(OnRateSliderChanged);

            // 初回表示を確実に更新
            SyncRateUIFromGameManager();
        }

        // --- Guess Slider ---
        if (guessSlider != null)
        {
            guessSlider.wholeNumbers = true;
            guessSlider.minValue = 1;
            guessSlider.maxValue = 100;

            guessSlider.onValueChanged.RemoveListener(OnGuessSliderChanged);
            guessSlider.onValueChanged.AddListener(OnGuessSliderChanged);
            OnGuessSliderChanged(guessSlider.value);
        }
    }

    void HideAllScreens()
    {
        titleScreen.SetActive(false);
        modeSelectScreen.SetActive(false);
        rateSettingScreen.SetActive(false);
        playScreen.SetActive(false);
        resultScreen.SetActive(false);
    }

    public void ShowTitle()
    {
        HideAllScreens();
        titleScreen.SetActive(true);
        GameManager.Instance.SetMode(GameManager.GameMode.None);
    }

    public void ShowModeSelect()
    {
        HideAllScreens();
        modeSelectScreen.SetActive(true);
    }

    public void ShowRateSetting()
    {
        HideAllScreens();
        rateSettingScreen.SetActive(true);

        // ここが肝：画面を開いた瞬間に必ず表示更新
        SyncRateUIFromGameManager();
    }

    public void ShowPlay()
    {
        HideAllScreens();
        playScreen.SetActive(true);
        GameManager.Instance.ResetStreak();
        UpdateStreakText();
        doorController.ResetDoor();
    }

    public void ShowResult()
    {
        HideAllScreens();
        resultScreen.SetActive(true);

        resultTitleText.text = "ゲームオーバー";
        resultStreakText.text = $"連続成功：{GameManager.Instance.Streak}回";

        if (GameManager.Instance.CurrentMode == GameManager.GameMode.Experience)
        {
            resultRateText.text = $"設定した継続率：{GameManager.Instance.ConfiguredRate}%";
            resultRateText.gameObject.SetActive(true);
            guessInputGroup.SetActive(false);
            actualRateText.gameObject.SetActive(false);
        }
        else if (GameManager.Instance.CurrentMode == GameManager.GameMode.Guess)
        {
            resultRateText.gameObject.SetActive(false);
            guessInputGroup.SetActive(true);
            actualRateText.gameObject.SetActive(false);
            guessConfirmButton.interactable = true;

            if (guessSlider != null)
            {
                guessSlider.SetValueWithoutNotify(50);
                OnGuessSliderChanged(guessSlider.value);
            }
        }
    }

    public void OnStartButtonClicked()
    {
        ShowModeSelect();
    }

    public void OnExperienceModeSelected()
    {
        GameManager.Instance.SetMode(GameManager.GameMode.Experience);
        ShowRateSetting();
    }

    public void OnGuessModeSelected()
    {
        GameManager.Instance.SetMode(GameManager.GameMode.Guess);
        ShowPlay();
    }

    public void OnPlayStartClicked()
    {
        int rate = Mathf.RoundToInt(rateSlider.value);
        GameManager.Instance.SetConfiguredRate(rate);
        ShowPlay();
    }

    public void OnRetryClicked()
    {
        if (GameManager.Instance.CurrentMode == GameManager.GameMode.Guess)
        {
            GameManager.Instance.SetMode(GameManager.GameMode.Guess);
        }
        ShowPlay();
    }

    public void OnBackToModeSelectClicked()
    {
        ShowModeSelect();
    }

    public void OnGuessConfirmClicked()
    {
        guessConfirmButton.interactable = false;

        int guess = Mathf.RoundToInt(guessSlider.value);
        GameManager.Instance.SetUserGuess(guess);

        actualRateText.text = $"あなたの予想：{GameManager.Instance.UserGuess}%\n実際の継続率：{GameManager.Instance.ActualRate}%";
        actualRateText.gameObject.SetActive(true);
    }

    void OnRateSliderChanged(float value)
    {
        int rate = Mathf.RoundToInt(value);

        // 表示更新
        if (rateText != null)
            rateText.text = $"{rate}%";

        // 常にGameManagerへ反映
        if (GameManager.Instance != null)
            GameManager.Instance.SetConfiguredRate(rate);
    }

    void OnGuessSliderChanged(float value)
    {
        int rate = Mathf.RoundToInt(value);
        if (guessSliderText != null)
        {
            guessSliderText.text = $"{rate}%";
        }
    }

    public void UpdateStreakText()
    {
        streakText.text = $"連続成功：{GameManager.Instance.Streak}";
    }

    // ===== 追加: GameManagerの値をUIへ確実に反映 =====
    private void SyncRateUIFromGameManager()
    {
        if (rateSlider == null || rateText == null) return;

        // GameManagerがいなければ適当に50
        int current = 50;

        if (GameManager.Instance != null)
        {
            // ここは今のGameManagerの変数名に合わせてる（あなたの貼ったUIControllerに合わせてConfiguredRate）
            current = Mathf.Clamp(GameManager.Instance.ConfiguredRate, 1, 100);
        }

        // onValueChangedを発火させずにUI値だけ合わせる → その後に手動で表示更新
        rateSlider.SetValueWithoutNotify(current);
        rateText.text = $"{current}%";
    }
}
