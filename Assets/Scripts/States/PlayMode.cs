public class PlayMode : BaseState
{
    void Entry()
    {
        stateController.gameStateDictionary.Add(Enums.GameState.PlayMode, this);
    }
}