public class ResolvingState : IGameState
{
    public void Enter(GameContext ctx)
    {
        ctx.GameController.StartResolveCoroutine();
    }

    public void Tick(GameContext ctx) { }

    public void Exit(GameContext ctx) { }
}
