using System;
using UnityEngine;
using UnityEngine.UI;

public class DisplayAction : MonoBehaviour
{
    public int playerIndex;
    public TMPro.TMP_Text displayText;
        
    private void Update()
    {
        displayText.text = GetPlayerActionDescription() ?? string.Empty;
    }

    private string GetPlayerActionDescription()
    {
        var gameManager = GameManager.GAME_MANAGER;
        var action = playerIndex switch
        {
            0 => gameManager.p1Action.Value,
            1 => gameManager.p2Action.Value,
            _ => throw new ArgumentOutOfRangeException()
        };
        switch (action)
        {
            case CombatAction.Sword:
                return "Sword";
            case CombatAction.Shield:
                return "Shield";
            case CombatAction.Magic:
                return "Magic";
            default:
            case CombatAction.None:
                return "Waiting for action";
        }
    }
}