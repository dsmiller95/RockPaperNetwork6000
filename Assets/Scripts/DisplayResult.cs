using UnityEngine;
using UnityEngine.UI;

public class DisplayResult : MonoBehaviour
{
    public TMPro.TMP_Text displayText;
    public TMPro.TMP_Text displayWinLog;

    private void Start()
    {
        displayWinLog.text = "";
        GameManager.GAME_MANAGER.OnGameResolved.AddListener(OnGameResolved);
    }

    private void OnGameResolved(MyWinState myWinState)
    {
        var existingText = displayWinLog.text;
        var newRow = GetActionResultDescription(myWinState);
        displayWinLog.text = existingText + "\n" + newRow;
    }

    private void Update()
    {
        displayText.text = GetActionResultDescription(GameManager.GAME_MANAGER.GetMyWinState());
    }
    
    private static string GetActionResultDescription(MyWinState myWinState)
    {
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