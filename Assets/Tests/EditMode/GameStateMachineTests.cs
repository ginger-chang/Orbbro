using NUnit.Framework;

// Stub state that records how many times each method was called.
// Ignores GameContext so tests don't need real MonoBehaviours.
class StubState : IGameState
{
    public readonly string Name;
    public int EnterCount;
    public int ExitCount;
    public int TickCount;

    public StubState(string name) { Name = name; }

    public void Enter(GameContext ctx) => EnterCount++;
    public void Tick(GameContext ctx)  => TickCount++;
    public void Exit(GameContext ctx)  => ExitCount++;
}

class AnotherStubState : IGameState
{
    public void Enter(GameContext ctx) { }
    public void Tick(GameContext ctx)  { }
    public void Exit(GameContext ctx)  { }
}

[TestFixture]
public class GameStateMachineTests
{
    // GameContext with all-null refs — safe as long as stub states ignore it
    private GameContext NullCtx() => new GameContext();

    // ---------------------------------------------------------------
    // Construction
    // ---------------------------------------------------------------

    [Test]
    public void Constructor_EntersInitialState()
    {
        var initial = new StubState("initial");
        var machine = new GameStateMachine(NullCtx(), initial);

        Assert.AreEqual(1, initial.EnterCount, "Enter should be called once on the initial state");
        Assert.AreEqual(0, initial.ExitCount);
    }

    [Test]
    public void Constructor_SetsCurrentStateName()
    {
        var machine = new GameStateMachine(NullCtx(), new StubState("s"));
        Assert.AreEqual("StubState", machine.CurrentStateName);
    }

    // ---------------------------------------------------------------
    // ChangeState
    // ---------------------------------------------------------------

    [Test]
    public void ChangeState_ExitsOldAndEntersNew()
    {
        var a = new StubState("a");
        var b = new StubState("b");
        var machine = new GameStateMachine(NullCtx(), a);

        machine.ChangeState(b);

        Assert.AreEqual(1, a.ExitCount,  "old state should be exited");
        Assert.AreEqual(1, b.EnterCount, "new state should be entered");
    }

    [Test]
    public void ChangeState_UpdatesCurrentState()
    {
        var a = new StubState("a");
        var b = new StubState("b");
        var machine = new GameStateMachine(NullCtx(), a);

        machine.ChangeState(b);

        Assert.IsTrue(machine.IsInState<StubState>());
        Assert.AreSame(b, machine.Current);
    }

    [Test]
    public void ChangeState_UpdatesCurrentStateName()
    {
        var machine = new GameStateMachine(NullCtx(), new StubState("a"));
        machine.ChangeState(new AnotherStubState());

        Assert.AreEqual("AnotherStubState", machine.CurrentStateName);
    }

    // ---------------------------------------------------------------
    // PushState
    // ---------------------------------------------------------------

    [Test]
    public void PushState_ExitsCurrentAndEntersOverlay()
    {
        var a = new StubState("a");
        var overlay = new StubState("overlay");
        var machine = new GameStateMachine(NullCtx(), a);

        machine.PushState(overlay);

        Assert.AreEqual(1, a.ExitCount,       "underlying state should be exited");
        Assert.AreEqual(1, overlay.EnterCount, "overlay state should be entered");
    }

    [Test]
    public void PushState_MakesOverlayCurrent()
    {
        var a = new StubState("a");
        var overlay = new StubState("overlay");
        var machine = new GameStateMachine(NullCtx(), a);

        machine.PushState(overlay);

        Assert.AreSame(overlay, machine.Current);
    }

    // ---------------------------------------------------------------
    // PopState
    // ---------------------------------------------------------------

    [Test]
    public void PopState_ExitsOverlayAndRestoresPrevious()
    {
        var a = new StubState("a");
        var overlay = new StubState("overlay");
        var machine = new GameStateMachine(NullCtx(), a);

        machine.PushState(overlay);
        machine.PopState();

        Assert.AreEqual(1, overlay.ExitCount, "overlay should be exited on pop");
        Assert.AreSame(a, machine.Current,    "previous state should be restored");
    }

    [Test]
    public void PopState_DoesNotReenterRestoredState()
    {
        // This covers the Resolving+Pause bug: resuming must not restart the coroutine
        var underlying = new StubState("underlying");
        var overlay    = new StubState("overlay");
        var machine    = new GameStateMachine(NullCtx(), underlying);

        machine.PushState(overlay);
        int enterCountBeforePop = underlying.EnterCount; // should be 1 (from construction)
        machine.PopState();

        Assert.AreEqual(enterCountBeforePop, underlying.EnterCount,
            "restored state must not have Enter called again — would restart in-flight coroutines");
    }

    [Test]
    public void PopState_UpdatesCurrentStateName()
    {
        var a       = new StubState("a");
        var overlay = new AnotherStubState();
        var machine = new GameStateMachine(NullCtx(), a);

        machine.PushState(overlay);
        machine.PopState();

        Assert.AreEqual("StubState", machine.CurrentStateName);
    }

    // ---------------------------------------------------------------
    // Tick
    // ---------------------------------------------------------------

    [Test]
    public void Tick_ForwardsToCurrentState()
    {
        var state   = new StubState("s");
        var machine = new GameStateMachine(NullCtx(), state);

        machine.Tick();
        machine.Tick();

        Assert.AreEqual(2, state.TickCount);
    }

    [Test]
    public void Tick_DoesNotTickPushedUnderlyingState()
    {
        var a       = new StubState("a");
        var overlay = new StubState("overlay");
        var machine = new GameStateMachine(NullCtx(), a);

        machine.PushState(overlay);
        machine.Tick();

        Assert.AreEqual(0, a.TickCount,       "underlying state must not tick while paused");
        Assert.AreEqual(1, overlay.TickCount, "overlay state should tick");
    }

    // ---------------------------------------------------------------
    // IsInState
    // ---------------------------------------------------------------

    [Test]
    public void IsInState_ReturnsTrueForCurrentType()
    {
        var machine = new GameStateMachine(NullCtx(), new StubState("s"));
        Assert.IsTrue(machine.IsInState<StubState>());
    }

    [Test]
    public void IsInState_ReturnsFalseForOtherType()
    {
        var machine = new GameStateMachine(NullCtx(), new StubState("s"));
        Assert.IsFalse(machine.IsInState<AnotherStubState>());
    }

    [Test]
    public void IsInState_UpdatesAfterChangeState()
    {
        var machine = new GameStateMachine(NullCtx(), new StubState("s"));
        machine.ChangeState(new AnotherStubState());

        Assert.IsFalse(machine.IsInState<StubState>());
        Assert.IsTrue(machine.IsInState<AnotherStubState>());
    }
}
