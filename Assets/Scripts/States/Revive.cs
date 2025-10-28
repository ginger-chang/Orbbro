public class Revive : BaseState
{
    void Entry()
    {
        stateController.gameStateDictionary.Add(Enums.GameState.Revive, this);
    }
}