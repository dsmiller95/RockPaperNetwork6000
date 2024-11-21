using UnityEngine.Events;

public interface ICoordinateGame
{
    public MyWinState GetMyWinState();
    /// <summary>
    /// invoked whenever a game resolves (a player wins, or a draw occurs).
    /// Contains information about how the win pertains to the player on the current client.
    /// Typically invoked after the result has been displayed to the player and the next game begins
    /// </summary>
    public UnityEvent<MyWinState> OnGameResolved { get; }
    
    public CombatAction? GetMyAction();
    public UnityEvent OnMyActionChanged { get; }
    
    public CombatAction? GetOpponentAction();
    public UnityEvent OnOpponentActionChanged { get; }

    /// <summary>
    /// register the current player on the server. must be called after player initialization to
    /// track the player in the lobby
    /// </summary>
    public void Register();
    
    /// <summary>
    /// Play the given action for the current player
    /// </summary>
    /// <param name="action"></param>
    public void PlayAction(CombatAction action);
}