using System.Collections.Generic;

public class GameStateMachine
{
    public IGameState Current { get; private set; }
    private readonly Stack<IGameState> _stack = new Stack<IGameState>();
    private readonly GameContext _ctx;

    // Visible in the Unity Inspector via GameController's _inspectorState field
    public string CurrentStateName { get; private set; }

    public GameStateMachine(GameContext ctx, IGameState initialState)
    {
        _ctx = ctx;
        Current = initialState;
        CurrentStateName = Current.GetType().Name;
        Current.Enter(_ctx);
    }

    // Hard transition — no memory of previous state
    public void ChangeState(IGameState newState)
    {
        Current.Exit(_ctx);
        Current = newState;
        CurrentStateName = Current.GetType().Name;
        Current.Enter(_ctx);
    }

    // Overlay — saves current state so PopState can restore it (used for Pause)
    public void PushState(IGameState newState)
    {
        Current.Exit(_ctx);
        _stack.Push(Current);
        Current = newState;
        CurrentStateName = Current.GetType().Name;
        Current.Enter(_ctx);
    }

    // Restore previous state (used for Resume)
    public void PopState()
    {
        Current.Exit(_ctx);
        Current = _stack.Pop();
        CurrentStateName = Current.GetType().Name;
        Current.Enter(_ctx);
    }

    // Called every frame from GameController.Update()
    public void Tick()
    {
        Current?.Tick(_ctx);
    }

    public bool IsInState<T>() where T : IGameState => Current is T;
}
