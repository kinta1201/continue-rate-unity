# CONTINUE RATE (Unity 2D) - Starter Project

このZIPは「スクリプト一式＋プロジェクト骨格」です。
Unityで開いて、UI（Canvas）を配置・参照をドラッグすれば動きます。

## 1. Unityで開く
Unity Hub → Open → このフォルダを選択。

※Unityのバージョンが違っても、基本的にアップグレードして開けます。

## 2. Scene
Assets/Scenes/Main.unity は未同梱です（シーンYAMLを生成しない方が事故が少ないため）。
空シーンでOKです。

## 3. Hierarchy（Canvas構成）
Canvas (Screen Space - Overlay)
├── TitleScreen
│   ├── TitleText (Text: "CONTINUE RATE")
│   └── StartButton (Button: "スタート")
├── ModeSelectScreen
│   ├── ExperienceButton (Button: "継続率体感モード")
│   ├── GuessButton (Button: "継続率当てモード")
│   └── BackButton (Button: "タイトルへ戻る")  ※任意
├── RateSettingScreen
│   ├── RateSlider (Slider: 1-100, Whole Numbers)
│   ├── RateText (Text: "50%")
│   └── PlayStartButton (Button: "プレイ開始")
├── PlayScreen
│   ├── StreakText (Text: "連続成功：0")
│   ├── DoorButton (Button)
│   │   └── DoorImage (Image)
│   └── InstructionText (Text: "タップして開ける")
└── ResultScreen
    ├── ResultStreakText (Text)
    ├── ResultRateText (Text) ※体感用
    ├── GuessInputGroup (GameObject)
    │   ├── GuessSlider (Slider 1-100)
    │   ├── GuessSliderText (Text "50%")
    │   └── GuessConfirmButton (Button "決定")
    ├── ActualRateText (Text) ※初期非表示
    ├── RetryButton (Button "もう一度")
    └── BackToModeSelectButton (Button "モード選択へ")

## 4. Empty GameObjects
- GameManager（GameManager.cs）
- UIController（UIController.cs）
- DoorController（DoorController.cs）

## 5. Inspectorの参照設定
### UIController
- Screens: Title/Mode/Rate/Play/Result をドラッグ
- Rate Setting: rateSlider/rateText/playStartButton をドラッグ
- Play: streakText をドラッグ
- Result: resultStreakText/resultRateText/retryButton/backToModeSelectButton をドラッグ
- Guess: guessInputGroup/guessSlider/guessSliderText/guessConfirmButton/actualRateText をドラッグ

### DoorController
- doorButton に PlayScreen/DoorButton をドラッグ
- doorView に PlayScreen/DoorButton（DoorView付）をドラッグ
- ui に UIController をドラッグ

### DoorButton
- DoorButton に DoorView.cs をアタッチ
- DoorViewの doorImage に DoorImage をドラッグ

## 6. ButtonのOnClick設定
StartButton → UIController.OnStartButtonClicked
ExperienceButton → UIController.OnExperienceModeSelected
GuessButton → UIController.OnGuessModeSelected
BackButton → UIController.ShowTitle（任意）
PlayStartButton → UIController.OnPlayStartClicked
GuessConfirmButton → UIController.OnGuessConfirmClicked
RetryButton → UIController.OnRetryClicked
BackToModeSelectButton → UIController.OnBackToModeSelectClicked

## 7. 初期Active
TitleScreenだけActive、他はInactive。
GuessInputGroupはInactive、ActualRateTextはInactive推奨（UIControllerで初期化もします）。

---

動かなければ、Consoleの赤エラー（行番号つき）を貼ってください。
