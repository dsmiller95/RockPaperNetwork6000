using Cards;
using Dman.Utilities;
using UnityEngine;

public class CardSlot : MonoBehaviour
{
    public CardSlotSpec slotSpec;
    public bool hidden;
    [SerializeField] private CardId cardInSlot;

    private IBoundCard _boundCard;
    
    private void Awake()
    {
        cardInSlot = CardId.None;
    }

    public void SetCardInSlot(CardId newCard)
    {
        if (cardInSlot == newCard) return;
        cardInSlot = newCard;
        if (_boundCard != null)
        { 
            _boundCard.OnClick -= OnMyCardClicked;
        }
        if(newCard == CardId.None) return;
        
        var registry = SingletonLocator<ICardRegistry>.Instance;
        var boundCard = registry.GetCard(newCard);
        boundCard.OnClick += OnMyCardClicked;
        boundCard.SetDisplay(transform, this.hidden);
    }

    private void OnMyCardClicked()
    {
        GetComponentInParent<PlayerStateBinding>().OnCardClicked(slotSpec.placement, cardInSlot);
    }
}