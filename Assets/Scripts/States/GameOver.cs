public class GameOver : BaseState
{
    void Entry()
    {
        stateController.gameStateDictionary.Add(Enums.GameState.GameOver, this);
    }
}