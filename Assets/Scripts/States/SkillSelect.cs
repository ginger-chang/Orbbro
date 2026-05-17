using UnityEngine;

public class SkillSelectState : IGameState
{
    private readonly float _resumeTimeLimit;
    private readonly int   _remainingPicks;

    public SkillSelectState(float resumeTimeLimit, int picks = 1)
    {
        _resumeTimeLimit = resumeTimeLimit;
        _remainingPicks  = picks;
    }

    public void Enter(GameContext ctx)
    {
        Time.timeScale = 0f;
        var skills = ctx.SkillManager.GetRandomSkills(3);
        ctx.UIManager.ShowSkillSelect(skills);
    }

    public void Tick(GameContext ctx) { }

    public void Exit(GameContext ctx)
    {
        Time.timeScale = 1f;
        ctx.UIManager.HideSkillSelect();
    }

    public void SelectSkill(GameContext ctx, SkillOffer offer)
    {
        ctx.SkillManager.Apply(offer);
        ctx.ScoreManager.OnSkillApplied(offer);

        if (_remainingPicks > 1)
            ctx.StateMachine.ChangeState(new SkillSelectState(_resumeTimeLimit, _remainingPicks - 1));
        else
            ctx.StateMachine.ChangeState(new PlayModeState(_resumeTimeLimit, _resumeTimeLimit));
    }
}
