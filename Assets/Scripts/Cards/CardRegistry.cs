using System;
using System.Collections.Generic;
using Dman.Utilities;
using UnityEngine;

namespace Cards
{
    public interface ICardRegistry
    {
        public IBoundCard GetCard(CardId withId);
    }

    public interface IBoundCard
    {
        public event System.Action OnClick;
        public void SetDisplay(Transform slot, bool hidden);
    }
    
    [UnitySingleton]
    public class CardRegistry : MonoBehaviour, ICardRegistry
    {
        public CardBinding cardPrefab;
        
        private Dictionary<CardId, CardBinding> existingCards = new();
        
        public IBoundCard GetCard(CardId withId)
        {
            if (withId == CardId.None) throw new InvalidOperationException();
            if (existingCards.TryGetValue(withId, out var existingCard))
            {
                return existingCard;
            }

            var cardType = GameManager.GAME_MANAGER.GetCardType(withId);
            var card = Instantiate(cardPrefab, transform);
            card.Initialize(withId, cardType);
            existingCards[withId] = card;
            
            return card;
        }
    }
}