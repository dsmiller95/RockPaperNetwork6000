using System;
using Unity.Netcode;

public enum PlayerCardType
{
    Scissors,
    Rock,
    Paper
}

public struct CardIdGenerator
{
    private int _nextId;

    public static CardIdGenerator Create() => new() { _nextId = 1};

    public CardId Next()
    {
        return CardId.Create(_nextId++);
    }
}

[Serializable]
public struct CardData : INetworkSerializeByMemcpy, IEquatable<CardData>
{
    public PlayerCardType cardType;
    public CardId cardId;
    
    public bool Equals(CardData other)
    {
        return cardType == other.cardType && cardId.Equals(other.cardId);
    }

    public override bool Equals(object obj)
    {
        return obj is CardData other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine((int)cardType, cardId);
    }

    public static bool operator ==(CardData left, CardData right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(CardData left, CardData right)
    {
        return !left.Equals(right);
    }
}

[Serializable]
public struct CardId : INetworkSerializeByMemcpy, IEquatable<CardId>
{
    public static CardId None => new CardId { _cardId = -1 };
    
    private int _cardId;

    internal static CardId Create(int internalId)
    {
        if (internalId <= 0) throw new InvalidOperationException();
        return new CardId { _cardId = internalId };
    }

    public bool Equals(CardId other)
    {
        return _cardId == other._cardId;
    }

    public override bool Equals(object obj)
    {
        return obj is CardId other && Equals(other);
    }

    public override int GetHashCode()
    {
        return _cardId;
    }

    public static bool operator ==(CardId left, CardId right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(CardId left, CardId right)
    {
        return !left.Equals(right);
    }
}