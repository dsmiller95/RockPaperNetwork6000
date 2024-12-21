using System;
using System.Collections.Generic;
using System.Linq;
using Dman.Utilities.Logger;
using Unity.Netcode;
using Utils;

public class PlayerState : INetworkSerializable, IEquatable<PlayerState>
{
    private CardId _chosenAction;
    private CardId[] _hand;
    private CardId[] _deck;

    public CardId ChosenAction => _chosenAction;
    public CardId[] Hand => _hand;
    public CardId[] Deck => _deck;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref _chosenAction);
        serializer.SerializeValue(ref _hand);
        serializer.SerializeValue(ref _deck);
    }
    
    public PlayerState WithChosenAction(CardId? action)
    {
        return new PlayerState()
        {
            _chosenAction = action.GetValueOrDefault(),
            _hand = _hand,
            _deck = _deck
        };
    }
    
    public PlayerState WithHand(CardId[] hand)
    {
        return new PlayerState()
        {
            _chosenAction = _chosenAction,
            _hand = hand,
            _deck = _deck
        };
    }
    
    public PlayerState WithDeck(CardId[] deck)
    {
        return new PlayerState()
        {
            _chosenAction = _chosenAction,
            _hand = _hand,
            _deck = deck
        };
    }

    public PlayerState PlayCard(CardId cardToPlay)
    {
        if (_chosenAction == cardToPlay) return this;

        if(cardToPlay == CardId.None)
        {
            // playing None card. take the played card back into my hand.
            return new PlayerState()
            {
                _chosenAction = CardId.None,
                _hand = _hand.Append(_chosenAction).ToArray(),
                _deck = _deck
            };
        }
        
        if (!_hand.Contains(cardToPlay))
        {
            Log.Error("Tried to play a card that was not in the hand");
        }

        var newHand = _hand
            .Where(card => card != cardToPlay);
        if(_chosenAction != CardId.None)
        {
            newHand = newHand.Append(_chosenAction);
        }
            
        return new PlayerState
        {
            _chosenAction = cardToPlay,
            _hand = newHand.ToArray(),
            _deck = _deck
        };
    }

    public (PlayerState, CardId) TakePlayedCard()
    {
        if (_chosenAction == CardId.None) return (this, CardId.None);
        
        return (new PlayerState()
        {
            _chosenAction = CardId.None,
            _hand = _hand,
            _deck = _deck
        }, _chosenAction);
    }
    
    public bool Equals(PlayerState other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return _chosenAction.Equals(other._chosenAction) &&
               _hand.ElementsEqual(other._hand) && 
               _deck.ElementsEqual(other._deck);
    }

    public override bool Equals(object obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((PlayerState)obj);
    }

    public static bool operator ==(PlayerState left, PlayerState right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(PlayerState left, PlayerState right)
    {
        return !Equals(left, right);
    }
}