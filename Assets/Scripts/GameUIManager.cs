using System;
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
    
    private float? countdownEnd;

    public void OnGamePhaseChanged(GamePhase newPhase)
    {
        chooseCommandTextUi.SetActive(newPhase.AllowsChangeAction());
        
        countdownText.gameObject.SetActive(newPhase == GamePhase.CountingDown);
    }
    
    public void BeginCountdown(float secondsRemaining)
    {
        countdownEnd = Time.time + secondsRemaining;
    }

    private void Update()
    {
        if (!countdownEnd.HasValue) return;
        
        var timeRemaining = countdownEnd.Value - Time.time;
        if (timeRemaining <= 0)
        {
            countdownEnd = null;
            return;
        }
        
        SetCountdownUi(timeRemaining);
    }
    
    private void SetCountdownUi(float secondsRemaining)
    {
        var countdown = Mathf.CeilToInt(secondsRemaining * ticksPerSecond);
        countdownText.text = countdown.ToString();

        var gradientRemaining = Mathf.Min(1, secondsRemaining);
        countdownImage.color = countdownGradient.Evaluate(gradientRemaining);;
    }
}