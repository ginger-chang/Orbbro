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

    public void SetupGameMode()
    {
        
        if (gameController.mode == GameMode.Classic)
        {
            SetupClassicMode();
        } else if (gameController.mode == GameMode.Endless)
        {
            SetupEndlessMode();
        } else if (gameController.mode == GameMode.Adventure)
        {
            SetupAdventureMode();
        }
    }

    public bool timerEnabled = true;
    public bool levelEnabled = true;

    // ----------------CLASSIC-----------------
    private void SetupClassicMode()
    {
        timerEnabled = true;
        levelEnabled = true;
        uiManager.SetLevel(true);
        uiManager.SetTimer(true);
    }

    // ----------------ENDLESS-----------------
    private void SetupEndlessMode()
    {
        timerEnabled = false;
        levelEnabled = false;
        uiManager.SetLevel(false);
        uiManager.SetTimer(false);
    }

    // ---------------ADVENTURE----------------
    private void SetupAdventureMode()
    {
        timerEnabled = true;
        levelEnabled = true;
        uiManager.SetLevel(true);
        uiManager.SetTimer(true);
    }
}
