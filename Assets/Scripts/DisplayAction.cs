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
        var action = gameManager.actions[playerIndex];
        switch (action)
        {
            case GameManager.Action.Sword:
                return "Sword";
            case GameManager.Action.Shield:
                return "Shield";
            case GameManager.Action.Magic:
                return "Magic";
            default:
            case GameManager.Action.None:
                return "Waiting for action";
        }
    }
}