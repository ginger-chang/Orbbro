using UnityEngine;

public class SkillSelectState : IGameState
{
    private readonly float _resumeTimeLimit;

    public SkillSelectState(float resumeTimeLimit)
    {
        _resumeTimeLimit = resumeTimeLimit;
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
        ctx.StateMachine.ChangeState(new PlayModeState(_resumeTimeLimit, _resumeTimeLimit));
    }
}
