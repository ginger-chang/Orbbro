using UnityEngine;
using System.Collections;
using DG.Tweening;

[RequireComponent(typeof(BoardManager))]
[RequireComponent(typeof(ScoreManager))]
public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }

    // FSM — _inspectorState mirrors the current state for the Unity Inspector
    public GameStateMachine StateMachine { get; private set; }
    [SerializeField] private string _inspectorState;

    public GameMode mode = GameMode.Classic;

    private AudioManager audioManager;
    private UIManager uiManager;
    private GameModeManager gameModeManager;
    public BoardManager BoardManager { get; private set; }
    public ScoreManager ScoreManager { get; private set; }
    public SkillManager SkillManager { get; private set; }

    private float _savedTimeRemaining;
    private bool _levelAdvancedDuringResolve;
    private bool _spawnDiamondOnNextFill;
    private int _pendingSkillSelects;

    //--------------------STARTING------------------------

    private void Awake()
    {
        Instance        = this;
        audioManager    = GameObject.FindGameObjectWithTag("audio").GetComponent<AudioManager>();
        uiManager       = GameObject.FindGameObjectWithTag("UI Manager").GetComponent<UIManager>();
        gameModeManager = GameObject.FindGameObjectWithTag("Game Mode Manager").GetComponent<GameModeManager>();
        BoardManager    = GetComponent<BoardManager>();
        ScoreManager    = GetComponent<ScoreManager>();
        SkillManager    = new SkillManager();

        var ctx = new GameContext
        {
            GameController  = this,
            UIManager       = uiManager,
            AudioManager    = audioManager,
            GameModeManager = gameModeManager,
            BoardManager    = BoardManager,
            ScoreManager    = ScoreManager,
            SkillManager    = SkillManager
        };
        StateMachine    = new GameStateMachine(ctx, new MainMenuState());
        ctx.StateMachine = StateMachine;

        ScoreManager.OnLevelGoalReached += OnLevelGoalReached;
    }

    // Called by Unity on first frame, and again by UIManager when starting/restarting.
    // Does not transition to PlayMode — UIManager calls EnterPlayMode() after.
    public void Start()
    {
        BoardManager.ClearBoard();
        DOTween.KillAll();
        BoardManager.Initialize();

        SkillManager.Reset();
        _pendingSkillSelects = 0;
        gameModeManager.SetupGameMode();
        ScoreManager.ResetForNewGame(mode, SkillManager);
        _heartsUsed = 0;
        _reviveUsed = false;
    }

    public void EnterPlayMode()
    {
        StateMachine.ChangeState(new PlayModeState(ScoreManager.CurrentTimeLimit, ScoreManager.CurrentTimeLimit));
    }

    void Update()
    {
        StateMachine.Tick();
        _inspectorState = StateMachine.CurrentStateName;
    }

    //----------------------HELPERS-------------------------

    public void KillCoroutines()
    {
        StopAllCoroutines();
        BoardManager.StopAllCoroutines();
    }

    public void SetGameMode(GameMode mode) => this.mode = mode;

    public void SelectSkill(SkillOffer offer)
    {
        if (StateMachine.Current is SkillSelectState sss)
        {
            sss.SelectSkill(StateMachine.Ctx, offer);
            if (offer.Type == SkillType.ExtraLife)
                uiManager.UpdateHearts(SkillManager.ExtraLives - _heartsUsed);
        }
    }

    //---------------------LEVEL GOAL-----------------------

    private void OnLevelGoalReached()
    {
        _levelAdvancedDuringResolve = true;
        if (StateMachine.Current is PlayModeState ps)
        {
            ScoreManager.StopTimerWarning();
            ps.ResetTimer(ScoreManager.CurrentTimeLimit);
        }
        if (ScoreManager.CurrentLevel % 5 == 0
            && (mode == GameMode.Classic || mode == GameMode.Adventure))
            _spawnDiamondOnNextFill = true;
        if (mode == GameMode.Adventure && ScoreManager.CurrentLevel % 3 == 0)
            _pendingSkillSelects++;
    }

    //---------------------GAME OVER & REVIVAL-----------------------

    private Orb orbInSwap;
    private int  _heartsUsed;
    private bool _reviveUsed;

    public void SetOrbInSwap(Orb orb) => orbInSwap = orb;

    // Called by PlayModeState.Tick when the timer hits zero
    public void LevelTimeUp()
    {
        if (orbInSwap != null)
        {
            orbInSwap.curGrid.AssignOrb(orbInSwap, "just go there");
            orbInSwap.validDrag = false;
            orbInSwap = null;
            Resolve();
        }
        else if (_heartsUsed < SkillManager.ExtraLives)
        {
            _heartsUsed++;
            uiManager.UpdateHearts(SkillManager.ExtraLives - _heartsUsed);
            StateMachine.ChangeState(new PlayModeState(ScoreManager.CurrentTimeLimit, ScoreManager.CurrentTimeLimit));
        }
        else if (!_reviveUsed)
        {
            _reviveUsed = true;
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
        StateMachine.ChangeState(new PlayModeState(ScoreManager.CurrentTimeLimit, ScoreManager.CurrentTimeLimit));
    }

    //--------------------RESOLVING-------------------------

    public void Resolve()
    {
        if (StateMachine.Current is PlayModeState ps)
            _savedTimeRemaining = ps.TimeRemaining;

        ScoreManager.BeginResolve();
        _levelAdvancedDuringResolve = false;
        StateMachine.ChangeState(new ResolvingState());
    }

    public void StartResolveCoroutine() => StartCoroutine(ResolveEnumerator());

    public IEnumerator ResolveEnumerator()
    {
        yield return new WaitForSeconds(0.1f);
        bool anyMatch = false;
        while (BoardManager.ExistsMatch())
        {
            anyMatch = true;
            yield return StartCoroutine(DisappearAllMatches());
        }
        ScoreManager.HideComboText();

        float timeBonus  = ScoreManager.EndResolve();
        float resumeTime = _levelAdvancedDuringResolve
            ? ScoreManager.CurrentTimeLimit
            : Mathf.Min(_savedTimeRemaining + timeBonus, ScoreManager.CurrentTimeLimit);
        _levelAdvancedDuringResolve = false;

        if (!anyMatch && (mode == GameMode.Classic || mode == GameMode.Adventure))
            resumeTime = Mathf.Max(0f, resumeTime - 2f);

        if (_pendingSkillSelects > 0)
        {
            int picks = _pendingSkillSelects;
            _pendingSkillSelects = 0;
            StateMachine.ChangeState(new SkillSelectState(ScoreManager.CurrentTimeLimit, picks));
        }
        else
        {
            StateMachine.ChangeState(new PlayModeState(ScoreManager.CurrentTimeLimit, resumeTime));
        }
    }

    private IEnumerator DisappearAllMatches()
    {
        ScoreManager.BeginIteration();
        BoardManager.UpdateMatches();

        for (int matchId = 1; matchId <= BoardManager.NumMatches; matchId++)
        {
            var (numOrb, numDiamonds, suit) = BoardManager.DestroyMatchedOrbs(matchId);
            audioManager.PlayDisappearSFX();
            ScoreManager.AddMatchScore(numOrb, suit);
            if (numDiamonds > 0) ScoreManager.CollectDiamonds(numDiamonds);
            yield return new WaitForSeconds(0.5f);
        }
        bool spawnDiamond = _spawnDiamondOnNextFill;
        _spawnDiamondOnNextFill = false;
        yield return StartCoroutine(BoardManager.FillBoard(spawnDiamond));
    }
}
