using UnityEngine.Events;

public interface ICoordinateGame
{
    public MyWinState GetMyWinState();
    
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