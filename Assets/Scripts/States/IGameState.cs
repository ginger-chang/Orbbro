public interface IGameState
{
    void Enter(GameContext ctx);
    void Tick(GameContext ctx);
    void Exit(GameContext ctx);
}
