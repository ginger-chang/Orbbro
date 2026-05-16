using UnityEngine;

public class GameModeManager : MonoBehaviour
{
    private GameController gameController;
    private UIManager uiManager;

    void Awake()
    {
        this.gameController = GameController.Instance;
        this.uiManager = GameObject.FindGameObjectWithTag("UI Manager").GetComponent<UIManager>();
    }

    public void setupGameMode()
    {
        
        if (gameController.mode == GameMode.Classic)
        {
            setupClassicMode();
        } else if (gameController.mode == GameMode.Endless)
        {
            setupEndlessMode();
        } else if (gameController.mode == GameMode.Adventure)
        {
            setupAdventureMode();
        }
    }

    public bool timerEnabled = true;
    public bool levelEnabled = true;

    // ----------------CLASSIC-----------------
    private void setupClassicMode()
    {
        timerEnabled = true;
        levelEnabled = true;
        uiManager.setLevel(true);
        uiManager.setTimer(true);
    }

    // ----------------ENDLESS-----------------
    private void setupEndlessMode()
    {
        timerEnabled = false;
        levelEnabled = false;
        uiManager.setLevel(false);
        uiManager.setTimer(false);
    }

    // ---------------ADVENTURE----------------
    private void setupAdventureMode()
    {
        timerEnabled = true;
        levelEnabled = true;
        uiManager.setLevel(true);
        uiManager.setTimer(true);
    }
}
