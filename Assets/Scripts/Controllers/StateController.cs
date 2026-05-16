using System.Collections.Generic;
using UnityEngine;

public class GameStateMachine
{
    public IGameState Current { get; private set; }
    private readonly Stack<IGameState> _stack = new Stack<IGameState>();
    private readonly GameContext _ctx;

    // Visible in the Unity Inspector via GameController's serialized field
    public Enums.GameState CurrentStateName { get; private set; }

    public GameStateMachine(GameContext ctx, IGameState initialState)
    {
        _ctx = ctx;
        Current = initialState;
        Current.Enter(_ctx);
    }

    // Hard transition — no memory of previous state
    public void ChangeState(IGameState newState)
    {
        Current.Exit(_ctx);
        Current = newState;
        UpdateStateName();
        Current.Enter(_ctx);
    }

    // Overlay — saves current state so PopState can restore it (used for Pause)
    public void PushState(IGameState newState)
    {
        Current.Exit(_ctx);
        _stack.Push(Current);
        Current = newState;
        UpdateStateName();
        Current.Enter(_ctx);
    }

    // Restore previous state (used for Resume)
    public void PopState()
    {
        Current.Exit(_ctx);
        Current = _stack.Pop();
        UpdateStateName();
        Current.Enter(_ctx);
    }

    // Called every frame from GameController.Update()
    public void Tick()
    {
        Current?.Tick(_ctx);
    }

    public bool IsInState<T>() where T : IGameState => Current is T;

    private void UpdateStateName()
    {
        CurrentStateName = Current switch
        {
            PlayModeState   => Enums.GameState.PlayMode,
            ResolvingState  => Enums.GameState.Resolving,
            PauseState      => Enums.GameState.Pause,
            MainMenuState   => Enums.GameState.MainMenu,
            ReviveState     => Enums.GameState.Revive,
            GameOverState   => Enums.GameState.GameOver,
            _               => CurrentStateName
        };
    }
}
