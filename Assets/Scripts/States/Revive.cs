public class ReviveState : IGameState
{
    public void Enter(GameContext ctx)
    {
        ctx.UIManager.activateRevivePopup();
    }

    public void Tick(GameContext ctx) { }

    public void Exit(GameContext ctx) { }
}
