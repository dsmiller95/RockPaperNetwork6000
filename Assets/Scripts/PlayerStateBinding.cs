using System;
using System.Collections.Generic;
using System.Linq;
using Dman.Utilities.Logger;
using UnityEngine;

public class PlayerStateBinding : MonoBehaviour
{
    public bool isOpponent;
    
    public CardSlot[] cardSlots;

    private void Start()
    {
        cardSlots = GetComponentsInChildren<CardSlot>();
        if (isOpponent)
        {
            GameManager.GAME_MANAGER.OnOpponentStateChanged.AddListener(OnStateChanged);
        }
        else
        {
            GameManager.GAME_MANAGER.OnMyStateChanged.AddListener(OnStateChanged);
        }
    }

    public void OnCardClicked(CardPlacement placement, CardId id)
    {
        if (isOpponent) return;
        if (id == CardId.None)
        {
            Log.Error("Clicked on empty card");
            return;
        }

        switch (placement)
        {
            case CardPlacement.Hand:
                GameManager.GAME_MANAGER.PlayCard(id);
                break;
            case CardPlacement.Played:
                break;
            case CardPlacement.Deck:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(placement), placement, null);
        }
    }

    private void OnStateChanged()
    {
        var newState = isOpponent ?
            GameManager.GAME_MANAGER.GetOpponentState() :
            GameManager.GAME_MANAGER.GetMyState();
        if (newState == null)
        {
            Log.Error("No state found");
            return;
        }
        Log.Info($"State changed to\n{newState}");

        var played = GetBySpec(CardPlacement.Played, 0);
        played.SetCardInSlot(newState.ChosenAction);

        var hand = GetAllIn(CardPlacement.Hand).ToList();
        for (int i = 0; i < hand.Count; i++)
        {
            var inHand = newState.Hand.Length > i ? newState.Hand[i] : CardId.None;
            hand[i].SetCardInSlot(inHand);
        }
        
        var deck = GetAllIn(CardPlacement.Deck).ToList();
        for (int i = 0; i < deck.Count; i++)
        {
            var inDeck = newState.Deck.Length > i ? newState.Deck[i] : CardId.None;
            deck[i].SetCardInSlot(inDeck);
        }
    }

    
    private IEnumerable<CardSlot> GetAllIn(CardPlacement placement)
    {
        var slots =  cardSlots
            .Where(x => x.slotSpec.placement == placement)
            .OrderBy(x => x.slotSpec.index)
            .ToList();
        for (int i = 0; i < slots.Count(); i++)
        {
            if(slots[i].slotSpec.index != i)
            {
                Log.Error($"Card slot index mismatch: {slots[i].slotSpec.index} != {i}");
            }
        }
        return slots;
    }
    
    private CardSlot GetBySpec(CardPlacement placement, int index)
    {
        return GetBySpec(new CardSlotSpec(placement, index));
    }
    
    private CardSlot GetBySpec(CardSlotSpec spec)
    {
        return cardSlots.SingleOrDefault(x => x.slotSpec == spec);
    }
}