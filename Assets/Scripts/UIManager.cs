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
    }

    void Awake()
    {
        this.gameController = GameObject.FindGameObjectWithTag("game controller").GetComponent<GameController>();
    }

    //------------------GAME MODES-------------------
    [Header("---Game Modes---")]
    [SerializeField] private GameObject timerBarGO;
    [SerializeField] private GameObject backgroundMaskGO;
    [SerializeField] private GameObject levelTextGO;

    public void setTimer(bool active)
    {
        timerBarGO.SetActive(active);
    }

    public void setLevel(bool active)
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
        clearUIForPlayMode();
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
        gameController.setGameMode(GameMode.Classic);
        StartGame();
    }

    public void EndlessMode()
    {
        gameController.setGameMode(GameMode.Endless);
        StartGame();
    }

    public void AdventureMode()
    {
        gameController.setGameMode(GameMode.Adventure);
        StartGame();
    }

    private void StartGame()
    {
        clearUIForPlayMode();
        gameController.Start();
        gameController.EnterPlayMode();
    }

    //---------------------HELPERS------------------------

    public void clearUIForPlayMode()
    {
        mainMenuGO.SetActive(false);
        pauseMenuGO.SetActive(false);
        settingsGO.SetActive(false);
        pauseButton.SetActive(true);
        gameOverGO.SetActive(false);
        revivePopupGO.SetActive(false);
        ResetToDefaultPauseMenu();
    }

    private void Restart()
    {
        clearUIForPlayMode();
        DOTween.KillAll();
        gameController.killCoroutines();
        gameController.Start();
        gameController.EnterPlayMode();
    }

    private void Home()
    {
        gameController.killCoroutines();
        gameController.BoardManager.ClearBoard();
        gameController.StateMachine.ChangeState(new MainMenuState());
        mainMenuGO.SetActive(true);
        pauseButton.SetActive(false);
    }
}
