using System;
using UnityEngine;
using UnityEngine.UI;


public class DisplayAction : MonoBehaviour
{
    public bool isOpponent;
    public TMPro.TMP_Text displayText;
    
    private void Update()
    {
        displayText.text = GetPlayerActionDescription() ?? string.Empty;
    }

    private string GetPlayerActionDescription()
    {
        var gameManager = GameManager.GAME_MANAGER;
        var action = isOpponent ? gameManager.GetOpponentAction() : gameManager.GetMyAction();
        switch (action)
        {
            case CombatAction.Scissors:
                return "Scissors";
            case CombatAction.Rock:
                return "Rock";
            case CombatAction.Paper:
                return "Paper";
            case null:
                return "?";
            default:
            case CombatAction.None:
                return "Waiting for action";
        }
    }
}