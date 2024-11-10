using Dman.Utilities;
using UnityEngine;
using UnityEngine.UI;

[UnitySingleton]
public class GameUIManager : MonoBehaviour
{
    public GameObject chooseCommandTextUi;
    
    [Tooltip("Used for the last second only")]
    public Gradient countdownGradient;
    public Image countdownImage;
    public TMPro.TMP_Text countdownText;
    public int ticksPerSecond;

    public void OnGamePhaseChanged(GamePhase newPhase)
    {
        chooseCommandTextUi.SetActive(newPhase.AllowsChangeAction());
        
        countdownText.gameObject.SetActive(newPhase == GamePhase.CountingDown);
    }
    
    
    public void SetCountdown(float secondsRemaining)
    {
        var countdown = Mathf.CeilToInt(secondsRemaining * ticksPerSecond);
        countdownText.text = countdown.ToString();

        var gradientRemaining = Mathf.Min(1, secondsRemaining);
        countdownImage.color = countdownGradient.Evaluate(gradientRemaining);
    }
}