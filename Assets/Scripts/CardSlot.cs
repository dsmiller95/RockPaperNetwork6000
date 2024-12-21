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
        if (_boundCard != null)
        { 
            _boundCard.OnClick -= OnMyCardClicked;
        }
        var registry = SingletonLocator<ICardRegistry>.Instance;
        var boundCard = registry.GetCard(newCard);
        boundCard.OnClick += OnMyCardClicked;
    }

    private void OnMyCardClicked()
    {
        GetComponentInParent<PlayerStateBinding>().OnCardClicked(slotSpec.placement, cardInSlot);
    }
}