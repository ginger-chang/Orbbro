public class GameOverState : IGameState
{
    public void Enter(GameContext ctx)
    {
        ctx.UIManager.GameOver();
    }

    public void Tick(GameContext ctx) { }

    public void Exit(GameContext ctx) { }
}
