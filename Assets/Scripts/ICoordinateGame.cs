using System.Collections.Generic;
using JetBrains.Annotations;
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
    
    public PlayerState? GetMyState();
    public UnityEvent OnMyStateChanged { get; }

    public PlayerState? GetOpponentState();
    public UnityEvent OnOpponentStateChanged { get; }

    public PlayerCardType GetCardType(CardId forId);

    /// <summary>
    /// register the current player on the server. must be called after player initialization to
    /// track the player in the lobby
    /// </summary>
    public void Register();
    
    public void PlayCard(CardId cardId);
}
