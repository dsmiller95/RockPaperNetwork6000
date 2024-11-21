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
        var myWinState = GameManager.GAME_MANAGER.GetMyWinState();
        
        switch (myWinState)
        {
            case MyWinState.MyWin:
                return "You Win!";
            case MyWinState.MyLoss:
                return "You Lose!";
            case MyWinState.Draw:
                return "Draw!";
            case MyWinState.None:
                return "...";
            default:
                return "...";
        }
    }
}