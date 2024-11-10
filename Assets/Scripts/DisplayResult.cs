using UnityEngine;
using UnityEngine.UI;

public class DisplayResult : MonoBehaviour
{
    public TMPro.TMP_Text displayText;
        
    private void Update()
    {
        displayText.text = GetActionResultDescription() ?? string.Empty;
    }

    private string GetActionResultDescription()
    {
        var gameManager = GameManager.GAME_MANAGER;
        return gameManager.lastWinner.Value.ToString();
    }
}