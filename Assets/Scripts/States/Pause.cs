using UnityEngine;

public class PauseState : IGameState
{
    public void Enter(GameContext ctx)
    {
        Time.timeScale = 0f;
        ctx.UIManager.ShowPauseMenu();
    }

    public void Tick(GameContext ctx) { }

    public void Exit(GameContext ctx)
    {
        Time.timeScale = 1f;
        ctx.UIManager.HidePauseMenu();
    }
}
