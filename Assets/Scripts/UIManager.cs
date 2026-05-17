using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    private GameController gameController;

    [SerializeField] private GameObject pauseButton;
    [SerializeField] private GameObject revivePopupGO;

    [SerializeField] private GameObject pauseMenuGO;
    [SerializeField] private GameObject mainMenuGO;
    [SerializeField] private GameObject settingsGO;
    [SerializeField] private GameObject gameOverGO;


    void Start()
    {
        pauseMenuGO.SetActive(false);
        RefreshMainMenuButtons();
    }

    void Awake()
    {
        this.gameController = GameController.Instance;
    }

    //------------------GAME MODES-------------------
    [Header("---Game Modes---")]
    [SerializeField] private GameObject timerBarGO;
    [SerializeField] private GameObject backgroundMaskGO;
    [SerializeField] private GameObject levelTextGO;

    [Header("---Main Menu Buttons---")]
    [SerializeField] private TextMeshProUGUI classicButtonText;
    [SerializeField] private TextMeshProUGUI endlessButtonText;
    [SerializeField] private TextMeshProUGUI adventureButtonText;

    //----------------------HEARTS----------------------------

    [Header("---Hearts---")]
    [SerializeField] private GameObject heart1GO;
    [SerializeField] private GameObject heart2GO;
    [SerializeField] private GameObject heart3GO;

    public void UpdateHearts(int remaining)
    {
        heart1GO.SetActive(remaining >= 1);
        heart2GO.SetActive(remaining >= 2);
        heart3GO.SetActive(remaining >= 3);
    }

    //--------------------SKILL SELECT------------------------

    [Header("---Skill Select---")]
    [SerializeField] private GameObject skillSelectGO;
    [SerializeField] private TextMeshProUGUI skill1Text;
    [SerializeField] private TextMeshProUGUI skill2Text;
    [SerializeField] private TextMeshProUGUI skill3Text;
    [SerializeField] private TextMeshProUGUI skillPicksRemainingText; // optional — shows "2 picks left"

    private SkillOffer[] _currentSkillOptions;

    public void ShowSkillSelect(SkillOffer[] skills, int picksRemaining = 1)
    {
        _currentSkillOptions = skills;
        SetSkillText(skill1Text, skills[0]);
        SetSkillText(skill2Text, skills[1]);
        SetSkillText(skill3Text, skills[2]);
        if (skillPicksRemainingText != null)
            skillPicksRemainingText.text = picksRemaining > 1 ? $"{picksRemaining} picks left" : "";
        skillSelectGO.SetActive(true);
        pauseButton.SetActive(false);
    }

    public void HideSkillSelect()
    {
        skillSelectGO.SetActive(false);
        pauseButton.SetActive(true);
    }

    // Called by each skill button via Inspector (pass 0, 1, or 2)
    public void OnSkillSelected(int index) => gameController.SelectSkill(_currentSkillOptions[index]);

    private void SetSkillText(TextMeshProUGUI label, SkillOffer offer)
    {
        label.text = $"<b>{SkillManager.GetDisplayName(offer)}</b>\n<size=70%>{SkillManager.GetDescription(offer)}</size>";
    }

    private void RefreshMainMenuButtons()
    {
        SetModeButtonText(classicButtonText,   "Classic",   GameMode.Classic);
        SetModeButtonText(endlessButtonText,   "Endless",   GameMode.Endless);
        SetModeButtonText(adventureButtonText, "Adventure", GameMode.Adventure);
    }

    private void SetModeButtonText(TextMeshProUGUI label, string modeName, GameMode mode)
    {
        if (label == null) return;
        int hs = ScoreManager.GetHighScore(mode);
        label.text = hs > 0 ? $"{modeName}\n<size=70%>Best: {hs}</size>" : modeName;
    }

    public void SetTimer(bool active)
    {
        timerBarGO.SetActive(active);
    }

    public void SetLevel(bool active)
    {
        backgroundMaskGO.GetComponent<RectMask2D>().enabled = active;
        levelTextGO.SetActive(active);
    }

    //--------------------PAUSE----------------------

    [Header("---Pause---")]
    [SerializeField] private GameObject restartButtonGO;
    [SerializeField] private GameObject homeButtonGO;
    [SerializeField] private TextMeshProUGUI restartText;
    [SerializeField] private TextMeshProUGUI homeText;

    private bool confirmRestart = false;
    private bool confirmHome = false;

    public void Pause()
    {
        pauseButton.SetActive(false);
        gameController.StateMachine.PushState(new PauseState());
        // PauseState.Enter calls ShowPauseMenu
    }

    public void ShowPauseMenu()
    {
        pauseMenuGO.SetActive(true);
        // Ensure pause menu sorts above dynamically-added orb fall canvases (sortingOrder 10/11).
        var canvas = pauseMenuGO.GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = pauseMenuGO.AddComponent<Canvas>();
            pauseMenuGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }
        canvas.overrideSorting = true;
        canvas.sortingOrder = 100;
    }

    public void HidePauseMenu()
    {
        pauseMenuGO.SetActive(false);
    }

    public void PauseHome()
    {
        if (confirmHome == false)
        {
            confirmHome = true;
            homeButtonGO.GetComponent<Image>().color = Color.cornflowerBlue;
            homeText.text = "Home?";
            resetRestartButton();
        }
        else
        {
            Home();
        }
    }

    public void Resume()
    {
        ClearUIForPlayMode();
        gameController.StateMachine.PopState();
    }

    public void PauseRestart()
    {
        if (confirmRestart == false)
        {
            confirmRestart = true;
            restartButtonGO.GetComponent<Image>().color = Color.cornflowerBlue;
            restartText.text = "Restart?";
            resetHomeButton();
        }
        else
        {
            Restart();
        }
    }

    [SerializeField] GameObject settingsBackgroundGO;

    public void PauseSettings()
    {
        Debug.Log("open settings");
        settingsGO.SetActive(true);
        settingsBackgroundGO.SetActive(false);
        ResetToDefaultPauseMenu();
    }

    public void ResetToDefaultPauseMenu()
    {
        resetHomeButton();
        resetRestartButton();
    }

    private void resetHomeButton()
    {
        confirmHome = false;
        homeButtonGO.GetComponent<Image>().color = Color.white;
        homeText.text = "Home";
    }

    private void resetRestartButton()
    {
        confirmRestart = false;
        restartButtonGO.GetComponent<Image>().color = Color.white;
        restartText.text = "Restart";
    }

    //----------------------SETTINGS--------------------------

    public void saveSettings()
    {
        Debug.Log("settings settttt");
        settingsGO.SetActive(false);
    }

    //-----------------------REVIVE---------------------------

    [Header("---Revive---")]
    [SerializeField] private TextMeshProUGUI reviveCountdownText;
    private Coroutine reviveCoroutine;

    public void activateRevivePopup()
    {
        revivePopupGO.SetActive(true);
        reviveCoroutine = StartCoroutine(reviveCountdown());
    }

    private IEnumerator reviveCountdown()
    {
        int seconds = 6;
        while (seconds != 0)
        {
            reviveCountdownText.text = $"Skipping in {seconds}...";
            yield return new WaitForSeconds(1f);
            seconds--;
        }
        revivePopupGO.SetActive(false);
        // Countdown expired — transition to GameOver via the FSM
        gameController.StateMachine.ChangeState(new GameOverState());
    }

    public void ReviveWatchAd()
    {
        Debug.Log("watch an ad");
        StopCoroutine(reviveCoroutine);
        revivePopupGO.SetActive(false);
        gameController.Revive();
    }

    public void ReviveSpendGem()
    {
        Debug.Log("spend some gems");
        StopCoroutine(reviveCoroutine);
        revivePopupGO.SetActive(false);
        gameController.Revive();
    }

    //---------------------GAME OVER--------------------------

    public void GameOver()
    {
        // TODO: tween game over scene
        gameOverGO.SetActive(true);
    }

    public void GameOverRestart()
    {
        gameOverGO.SetActive(false);
        Restart();
    }

    public void GameOverHome()
    {
        gameOverGO.SetActive(false);
        Home();
    }

    //------------------------HOME----------------------------

    public void HomeSettings()
    {
        settingsGO.SetActive(true);
        settingsBackgroundGO.SetActive(true);
    }

    public void ClassicMode()
    {
        gameController.SetGameMode(GameMode.Classic);
        StartGame();
    }

    public void EndlessMode()
    {
        gameController.SetGameMode(GameMode.Endless);
        StartGame();
    }

    public void AdventureMode()
    {
        gameController.SetGameMode(GameMode.Adventure);
        StartGame();
    }

    private void StartGame()
    {
        ClearUIForPlayMode();
        gameController.Start();
        gameController.EnterPlayMode();
    }

    //---------------------HELPERS------------------------

    public void ClearUIForPlayMode()
    {
        mainMenuGO.SetActive(false);
        pauseMenuGO.SetActive(false);
        settingsGO.SetActive(false);
        pauseButton.SetActive(true);
        gameOverGO.SetActive(false);
        revivePopupGO.SetActive(false);
        UpdateHearts(0);
        ResetToDefaultPauseMenu();
    }

    private void Restart()
    {
        ClearUIForPlayMode();
        DOTween.KillAll();
        gameController.KillCoroutines();
        gameController.Start();
        gameController.EnterPlayMode();
    }

    private void Home()
    {
        gameController.KillCoroutines();
        gameController.BoardManager.ClearBoard();
        gameController.StateMachine.ChangeState(new MainMenuState());
        mainMenuGO.SetActive(true);
        pauseButton.SetActive(false);
        UpdateHearts(0);
        RefreshMainMenuButtons();
    }
}
