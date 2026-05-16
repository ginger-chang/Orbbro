using UnityEngine;

public class PlayModeState : IGameState
{
    private float _timeLimit;
    private float _timeRemaining;
    private bool _warningActive;

    private const float WarningThreshold = 5f;

    public PlayModeState(float timeLimit, float timeRemaining)
    {
        _timeLimit = timeLimit;
        _timeRemaining = timeRemaining;
    }

    public void Enter(GameContext ctx)
    {
        Time.timeScale = 1f;
    }

    public void Tick(GameContext ctx)
    {
        if (!ctx.GameModeManager.timerEnabled) return;

        _timeRemaining -= Time.deltaTime;
        _timeRemaining = Mathf.Max(0f, _timeRemaining);
        ctx.ScoreManager.timerBar.value = _timeRemaining / _timeLimit;

        UpdateWarning(ctx);

        if (_timeRemaining <= 0f)
        {
            ctx.GameController.LevelTimeUp();
        }
    }

    public void Exit(GameContext ctx)
    {
        if (_warningActive) ctx.ScoreManager.StopTimerWarning();
    }

    private void UpdateWarning(GameContext ctx)
    {
        GameMode mode = ctx.GameController.mode;
        bool shouldWarn = _timeRemaining < WarningThreshold
            && (mode == GameMode.Classic || mode == GameMode.Adventure);

        if (shouldWarn && !_warningActive)
        {
            _warningActive = true;
            ctx.ScoreManager.StartTimerWarning();
        }
        else if (!shouldWarn && _warningActive)
        {
            _warningActive = false;
            ctx.ScoreManager.StopTimerWarning();
        }
    }

    // GameController reads these when setting up the next level
    public float TimeRemaining => _timeRemaining;
    public float TimeLimit => _timeLimit;

    public void ResetTimer(float timeLimit)
    {
        _timeLimit = timeLimit;
        _timeRemaining = timeLimit;
        _warningActive = false;
    }
}
