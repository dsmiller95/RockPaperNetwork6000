using UnityEngine;
using UnityEngine.UI;

public class DisplayAction : MonoBehaviour
{
    public int playerIndex;
    public Text displayText;
        
    private void Update()
    {
        displayText.text = GetPlayerActionDescription() ?? string.Empty;
    }

    private string GetPlayerActionDescription()
    {
        var gameManager = GameManager.GAME_MANAGER;
        return null;
    }
}