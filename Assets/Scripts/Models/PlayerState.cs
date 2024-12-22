using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dman.Utilities.Logger;
using Unity.Netcode;
using Utils;

public class PlayerState : INetworkSerializable, IEquatable<PlayerState>
{
    private CardId _chosenAction;
    private CardId[] _hand;
    
    /*
     * deck and discard are ordered:
     * 0 at the bottom, last card on top.
     * For a deck with A on the bottom, then B and then C, the array would be:
     * [A, B, C].
     * Draw operations pop the last element off the array.
     */
    private CardId[] _deck;
    private CardId[] _discard;

    public CardId ChosenAction => _chosenAction;
    public CardId[] Hand => _hand;
    public CardId[] Deck => _deck;
    public CardId[] Discard => _discard;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref _chosenAction);
        serializer.SerializeValue(ref _hand);
        serializer.SerializeValue(ref _deck);
        serializer.SerializeValue(ref _discard);
    }
    
    private PlayerState WithChosenAction(CardId? action)
    {
        return new PlayerState()
        {
            _chosenAction = action.GetValueOrDefault(),
            _hand = _hand,
            _deck = _deck,
            _discard = _discard,
        };
    }
    
    private PlayerState WithHand(CardId[] hand)
    {
        return new PlayerState()
        {
            _chosenAction = _chosenAction,
            _hand = hand,
            _deck = _deck,
            _discard = _discard,
        };
    }
    
    private PlayerState WithDeck(CardId[] deck)
    {
        return new PlayerState()
        {
            _chosenAction = _chosenAction,
            _hand = _hand,
            _deck = deck,
            _discard = _discard,
        };
    }
    private PlayerState WithDiscard(CardId[] discard)
    {
        return new PlayerState()
        {
            _chosenAction = _chosenAction,
            _hand = _hand,
            _deck = _deck,
            _discard = discard,
        };
    }
    
    public static PlayerState CreateNew(IEnumerable<CardId> deck, int handSize, System.Random rng)
    {
        var deckArr = deck.ToArray();
        deckArr.ShuffleInPlace(rng);
        var hand = deckArr.Take(handSize).ToArray();
        var newDeck = deckArr.Skip(handSize).ToArray();
        return new PlayerState()
        {
            _chosenAction = CardId.None,
            _hand = hand,
            _deck = newDeck,
            _discard = Array.Empty<CardId>(),
        };
    }

    public PlayerState PlayCard(CardId cardToPlay)
    {
        if (_chosenAction == cardToPlay) return this;

        if(cardToPlay == CardId.None)
        {
            // placing None card into the chosen action slot.
            // take the played card back into my hand.
            return this.WithChosenAction(CardId.None)
                .WithHand(_hand.Append(_chosenAction).ToArray());
        }
        
        if (!_hand.Contains(cardToPlay))
        {
            Log.Error("Tried to play a card that was not in the hand");
            return this;
        }

        var newHand = _hand
            .Where(card => card != cardToPlay);
        if(_chosenAction != CardId.None)
        {
            newHand = newHand.Append(_chosenAction);
        }
        
        return this.WithChosenAction(cardToPlay)
            .WithHand(newHand.ToArray());
    }

    public (PlayerState, CardId) TakePlayedCard()
    {
        if (_chosenAction == CardId.None) return (this, CardId.None);

        return (this.WithChosenAction(CardId.None), _chosenAction);
    }
    
    public PlayerState DiscardPlayedCard()
    {
        if (_chosenAction == CardId.None) return this;

        return this.WithChosenAction(CardId.None)
            .WithDiscard(_discard.Append(_chosenAction).ToArray());
    }
    
    public PlayerState DrawCard()
    {
        if (_deck.Length == 0)
        {
            Log.Error("Tried to draw a card with no cards left in the deck");
            return this;
        }

        var newHand = _hand.Append(_deck[^1]);
        return this.WithHand(newHand.ToArray())
            .WithDeck(_deck.SkipLast(1).ToArray());
    }
    
    
    public override string ToString()
    {
        var result = new StringBuilder();
        result.Append($"Chosen action: {_chosenAction}, ");
        result.Append($"Hand: {_hand.Length}, ");
        result.Append($"Deck: {_deck.Length}");
        return result.ToString();
    }

    #region Equality members
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
    #endregion
}