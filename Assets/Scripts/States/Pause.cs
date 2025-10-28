public class Pause : BaseState
{
    void Entry()
    {
        stateController.gameStateDictionary.Add(Enums.GameState.Pause, this);
    }
}