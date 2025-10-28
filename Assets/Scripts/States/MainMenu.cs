public class MainMenu : BaseState
{
    void Entry()
    {
        stateController.gameStateDictionary.Add(Enums.GameState.MainMenu, this);
    }
}