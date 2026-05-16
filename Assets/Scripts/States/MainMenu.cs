using UnityEngine;

public class MainMenuState : IGameState
{
    public void Enter(GameContext ctx)
    {
        Time.timeScale = 0f;
    }

    public void Tick(GameContext ctx) { }

    public void Exit(GameContext ctx) { }
}
