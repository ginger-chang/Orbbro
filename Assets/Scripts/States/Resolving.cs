public class Resolving : BaseState
{
    void Entry()
    {
        stateController.gameStateDictionary.Add(Enums.GameState.Resolving, this);
    }
}