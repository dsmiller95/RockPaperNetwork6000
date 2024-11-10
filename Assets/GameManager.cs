using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Dman.Utilities;
using Dman.Utilities.Logger;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public enum CombatAction
{
    None,
    Scissors,
    Rock,
    Paper
}

public enum GamePhase
{
    ChoosingActions,
    CountingDown,
    RevealActions,
    RevealWinner,
}

public enum CombatWinner
{
    Player0,
    Player1,
    Draw
}

public enum MyWinState
{
    None,
    MyWin,
    MyLoss,
    Draw
}


//THIS IS THE LOCAL GAME MANAGER
public class GameManager : NetworkBehaviour
{
    public static GameManager GAME_MANAGER;
    static Guid CLIENT_ID = System.Guid.NewGuid();
    public static FixedString64Bytes ClientIdString => new(CLIENT_ID.ToString());
    public static string ClientIdStringShort => CLIENT_ID.ToString().Substring(0, 4);

    public static MyWinState DidIWin()
    {
        var gameManager = GAME_MANAGER;
        if (gameManager.gamePhase.Value != GamePhase.RevealWinner)
        {
            return MyWinState.None;
        }
        
        if (gameManager.lastWinner.Value == CombatWinner.Draw)
        {
            return MyWinState.Draw;
        }
        
        var iAmP0 = gameManager.p1Id.Value == ClientIdString;
        var iAmP1 = gameManager.p2Id.Value == ClientIdString;
        
        if(gameManager.lastWinner.Value == CombatWinner.Player0)
        {
            if(iAmP0) return MyWinState.MyWin;
            if(iAmP1) return MyWinState.MyLoss;
        }
        else if(gameManager.lastWinner.Value == CombatWinner.Player1)
        {
            if(iAmP0) return MyWinState.MyLoss;
            if(iAmP1) return MyWinState.MyWin;
        }

        return MyWinState.None;
    }
    

    public List<PlayerData> playerDirectory;
    
    
    public NetworkVariable<GamePhase> gamePhase = new(GamePhase.ChoosingActions);
    
    public NetworkVariable<FixedString64Bytes> p1Id = new();
    public NetworkVariable<FixedString64Bytes> p2Id = new();
    
    public NetworkVariable<CombatAction> p1Action = new();
    public NetworkVariable<CombatAction> p2Action = new();
    public NetworkVariable<CombatWinner> lastWinner = new();
    
    [Tooltip("In Seconds")]
    public float countdownTime = 1f;
    [Tooltip("In Seconds")]
    public float handRevealTime = 1f;
    [Tooltip("In Seconds")]
    public float winRevealTime = 1f;

    
    private bool TryAddToMatch(FixedString64Bytes id)
    {
        if (p1Id.Value.IsEmpty)
        {
            p1Id.Value = id;
            return true;
        }
        if (p2Id.Value.IsEmpty)
        {
            p2Id.Value = id;
            return true;
        }

        return false;
    }

    /// <summary>
    /// always runs on the server
    /// </summary>
    /// <param name="id"></param>
    /// <param name="action"></param>
    private void SetAction(FixedString64Bytes id, CombatAction action)
    {
        if(!gamePhase.Value.AllowsChangeAction())
        {
            return;
        }
        if(p1Id.Value == id)
        {
            p1Action.Value = action;
        }
        else if(p2Id.Value == id)
        {
            p2Action.Value = action;
        }
    }

    
    private void ForceResolveActions()
    {
        if (p1Action.Value == CombatAction.None || p2Action.Value == CombatAction.None)
        {
            Debug.LogError("ForceResolveActions called with missing actions!");
            return;
        }
        
        var action0 = p1Action.Value;
        var action1 = p2Action.Value;
        
        p1Action.Value = CombatAction.None;
        p2Action.Value = CombatAction.None;
        
        Log.Info("Resolving actions: " + action0 + " vs " + action1);
        
        lastWinner.Value = GetWinner(action0, action1);
        
        Log.Info("Winner: " + lastWinner);
    }

    private CombatWinner GetWinner(CombatAction p1, CombatAction p2)
    {
        return (p1, p2) switch
        {
            (CombatAction.Scissors, CombatAction.Paper) => CombatWinner.Player0,
            (CombatAction.Rock, CombatAction.Scissors) => CombatWinner.Player0,
            (CombatAction.Paper, CombatAction.Rock) => CombatWinner.Player0,
            (CombatAction.Paper, CombatAction.Scissors) => CombatWinner.Player1,
            (CombatAction.Scissors, CombatAction.Rock) => CombatWinner.Player1,
            (CombatAction.Rock, CombatAction.Paper) => CombatWinner.Player1,
            
            _ => CombatWinner.Draw
        };
    }
    
    void Awake()
    {
        ThereCanBeOnlyOne();

        playerDirectory = new List<PlayerData>();
        
        SingletonLocator<IConnectionManager>.Instance.OnConnectionBegin += InstanceOnOnConnectionBegin;
        
        this.gamePhase.OnValueChanged += OnGamePhaseChanged;
    }


    private GameUIManager GameUIManager => SingletonLocator<GameUIManager>.Instance; 
    private void OnGamePhaseChanged(GamePhase prevValue, GamePhase newValue)
    {
        GameUIManager.OnGamePhaseChanged(newValue);
        if(newValue == GamePhase.CountingDown)
        {
            GameUIManager.BeginCountdown(this.countdownTime);
        }
    }

    private void InstanceOnOnConnectionBegin(ConnectionType conType)
    {
        if (conType == ConnectionType.Client) return;

        if (!this.IsServer)
        {
            Log.Error("Expected to be server when not client, but was not server");
            return;
        }
        RunGameServerTiming().Forget();
    }

    private async UniTask RunGameServerTiming()
    {
        while (true)
        {
            gamePhase.Value = GamePhase.ChoosingActions;
            Log.Info("choosing actions!");
            
            await UniTask.WaitUntil(() => p1Action.Value != CombatAction.None && p2Action.Value != CombatAction.None);
            
            gamePhase.Value = GamePhase.CountingDown;
            Log.Info("counting down!");
            
            await UniTask.Delay(TimeSpan.FromSeconds(countdownTime));
            // var countdownBeginTime = Time.time;
            // float timeSinceCountdownStart = 0;
            // while((timeSinceCountdownStart = Time.time - countdownBeginTime) < countdownTime)
            // {
            //     var secondsRemaining = countdownTime - timeSinceCountdownStart;
            //     GameUIManager.SetCountdown(secondsRemaining);
            //     await UniTask.Yield();
            // }
            
            gamePhase.Value = GamePhase.RevealActions;
            Log.Info("revealing actions!");
            
            await UniTask.Delay(TimeSpan.FromSeconds(handRevealTime));
            
            gamePhase.Value = GamePhase.RevealWinner;
            ForceResolveActions();
            Log.Info("revealing winner!");
            
            await UniTask.Delay(TimeSpan.FromSeconds(winRevealTime));
        }
    }

    public CombatAction? GetMyAction()
    {
        if (p1Id.Value == ClientIdString)
        {
            return p1Action.Value;
        }
        if (p2Id.Value == ClientIdString)
        {
            return p2Action.Value;
        }

        return null;
    }
    
    public CombatAction? GetOpponentAction()
    {
        switch (gamePhase.Value)
        {
            case GamePhase.ChoosingActions:
            case GamePhase.CountingDown:
                return null;
            case GamePhase.RevealActions:
            case GamePhase.RevealWinner:
            default:
                break;
        }
        
        if (p1Id.Value == ClientIdString)
        {
            return p2Action.Value;
        }
        if (p2Id.Value == ClientIdString)
        {
            return p1Action.Value;
        }

        return null;
    }

    void ThereCanBeOnlyOne()
    {
        if (GAME_MANAGER == null)
        {
            GAME_MANAGER = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [Rpc(SendTo.Server)]
    public void RegisterRpc(FixedString64Bytes id)
    {
        Log.Info("REGISTERING " + id);

        PlayerData newData = new() { clientId = id };

        playerDirectory.Add(newData);

        TryAddToMatch(id);
    }

    [Rpc(SendTo.Server)]
    public void ShieldRpc(FixedString64Bytes id)
    {
        Log.Info("Received shielding from " + id);
        SetAction(id, CombatAction.Rock);
    }

    [Rpc(SendTo.Server)]
    public void MagicRpc(FixedString64Bytes id)
    {
        Log.Info("Received magicking from " + id);
        SetAction(id, CombatAction.Paper);
    }

    [Rpc(SendTo.Server)]
    public void SwordRpc(FixedString64Bytes id)
    {
        Log.Info("Received swording from " + id);
        SetAction(id, CombatAction.Scissors);
    }
}

[System.Serializable]
public struct PlayerData
{
    public FixedString64Bytes clientId;
}


public static class GamePhaseExtensions{

    public static bool AllowsChangeAction(this GamePhase phase)
    {
        return phase is
            GamePhase.ChoosingActions or
            GamePhase.CountingDown;
    }
    
}