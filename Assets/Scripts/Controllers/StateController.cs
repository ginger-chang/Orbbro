using System.Collections.Generic;

public class StateController
{
    public Enums.GameState currentGameState;

    public Dictionary<Enums.GameState, BaseState> gameStateDictionary;

    public void changeGameState(Enums.GameState gameState)
    {
        // old state exitsx
        gameStateDictionary[this.currentGameState].Exit();
        this.currentGameState = gameState;
        // new state enters
        gameStateDictionary[this.currentGameState].Entry();
    }
}