using UnityEngine;
using UnityEngine.UI;

public class DisplayResult : MonoBehaviour
{
    public Text displayText;
        
    private void Update()
    {
        displayText.text = GetActionResultDescription() ?? string.Empty;
    }

    private string GetActionResultDescription()
    {
        var gameManager = GameManager.GAME_MANAGER;
        return null;
    }
}