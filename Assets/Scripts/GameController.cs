using UnityEngine;
using System.Collections;
using DG.Tweening;

[RequireComponent(typeof(BoardManager))]
[RequireComponent(typeof(ScoreManager))]
public class GameController : MonoBehaviour
{
    // FSM — _inspectorState mirrors the current state for the Unity Inspector
    public GameStateMachine StateMachine { get; private set; }
    [SerializeField] private string _inspectorState;

    public GameMode mode = GameMode.Classic;

    private AudioManager audioManager;
    private UIManager uiManager;
    private GameModeManager gameModeManager;
    public BoardManager BoardManager { get; private set; }
    private ScoreManager scoreManager;

    private float _savedTimeRemaining;
    private bool _levelAdvancedDuringResolve;

    public enum GameMode { Classic, Endless, Adventure }

    //--------------------STARTING------------------------

    private void Awake()
    {
        audioManager    = GameObject.FindGameObjectWithTag("audio").GetComponent<AudioManager>();
        uiManager       = GameObject.FindGameObjectWithTag("UI Manager").GetComponent<UIManager>();
        gameModeManager = GameObject.FindGameObjectWithTag("Game Mode Manager").GetComponent<GameModeManager>();
        BoardManager    = GetComponent<BoardManager>();
        scoreManager    = GetComponent<ScoreManager>();

        var ctx = new GameContext
        {
            GameController  = this,
            UIManager       = uiManager,
            AudioManager    = audioManager,
            GameModeManager = gameModeManager,
            BoardManager    = BoardManager,
            ScoreManager    = scoreManager
        };
        StateMachine    = new GameStateMachine(ctx, new MainMenuState());
        ctx.StateMachine = StateMachine;

        scoreManager.OnLevelGoalReached += OnLevelGoalReached;
    }

    // Called by Unity on first frame, and again by UIManager when starting/restarting.
    // Does not transition to PlayMode — UIManager calls EnterPlayMode() after.
    public void Start()
    {
        BoardManager.ClearBoard();
        DOTween.KillAll();
        BoardManager.Initialize();

        gameModeManager.setupGameMode();
        scoreManager.ResetForNewGame();
        revived = false;
    }

    public void EnterPlayMode()
    {
        StateMachine.ChangeState(new PlayModeState(scoreManager.CurrentTimeLimit, scoreManager.CurrentTimeLimit));
    }

    void Update()
    {
        StateMachine.Tick();
        _inspectorState = StateMachine.CurrentStateName;
    }

    //----------------------HELPERS-------------------------

    public void killCoroutines() => StopAllCoroutines();

    public void setGameMode(GameMode mode) => this.mode = mode;

    //---------------------LEVEL GOAL-----------------------

    private void OnLevelGoalReached()
    {
        _levelAdvancedDuringResolve = true;
        if (StateMachine.Current is PlayModeState ps)
            ps.ResetTimer(scoreManager.CurrentTimeLimit);
    }

    //---------------------GAME OVER & REVIVAL-----------------------

    private Orb orbInSwap;
    private bool revived;

    public void setOrbInSwap(Orb orb) => orbInSwap = orb;

    // Called by PlayModeState.Tick when the timer hits zero
    public void LevelTimeUp()
    {
        if (orbInSwap != null)
        {
            orbInSwap.curGrid.assignOrb(orbInSwap, "just go there");
            orbInSwap.validDrag = false;
            Resolve();
        }
        else if (!revived)
        {
            revived = true;
            StateMachine.ChangeState(new ReviveState());
        }
        else
        {
            StateMachine.ChangeState(new GameOverState());
        }
    }

    public void Revive()
    {
        Debug.Log("revive!");
        revived = true;
        StateMachine.ChangeState(new PlayModeState(scoreManager.CurrentTimeLimit, scoreManager.CurrentTimeLimit));
    }

    //--------------------RESOLVING-------------------------

    public void Resolve()
    {
        if (StateMachine.Current is PlayModeState ps)
            _savedTimeRemaining = ps.TimeRemaining;

        scoreManager.BeginResolve();
        _levelAdvancedDuringResolve = false;
        StateMachine.ChangeState(new ResolvingState());
    }

    public void StartResolveCoroutine() => StartCoroutine(ResolveEnumerator());

    public IEnumerator ResolveEnumerator()
    {
        yield return new WaitForSeconds(0.1f);
        while (BoardManager.ExistsMatch())
        {
            yield return StartCoroutine(DisappearAllMatches());
        }
        scoreManager.HideComboText();

        float resumeTime = _levelAdvancedDuringResolve
            ? scoreManager.CurrentTimeLimit
            : _savedTimeRemaining;
        _levelAdvancedDuringResolve = false;
        StateMachine.ChangeState(new PlayModeState(scoreManager.CurrentTimeLimit, resumeTime));
    }

    private IEnumerator DisappearAllMatches()
    {
        scoreManager.BeginIteration();
        BoardManager.UpdateMatches();

        for (int matchId = 1; matchId <= BoardManager.NumMatches; matchId++)
        {
            int numOrb = BoardManager.DestroyMatchedOrbs(matchId);
            audioManager.PlayDisappearSFX();
            scoreManager.AddMatchScore(numOrb);
            yield return new WaitForSeconds(0.5f);
        }
        yield return StartCoroutine(BoardManager.FillBoard());
    }
}
