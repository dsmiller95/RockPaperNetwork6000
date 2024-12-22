using System;


public enum CardPlacement
{
    Deck,
    Hand,
    Played,
    Discard,
}


[Serializable]
public struct CardSlotSpec : IEquatable<CardSlotSpec>
{
    public CardPlacement placement;
    public int index;
    
    public CardSlotSpec(CardPlacement placement, int index)
    {
        this.placement = placement;
        this.index = index;
    }

    public bool Equals(CardSlotSpec other)
    {
        return placement == other.placement && index == other.index;
    }

    public override bool Equals(object obj)
    {
        return obj is CardSlotSpec other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine((int)placement, index);
    }

    public static bool operator ==(CardSlotSpec left, CardSlotSpec right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(CardSlotSpec left, CardSlotSpec right)
    {
        return !left.Equals(right);
    }
}