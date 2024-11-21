using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Dman.Utilities;
using Dman.Utilities.Logger;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;


public class GameManager : NetworkBehaviour, ICoordinateGame
{
    public static ICoordinateGame GAME_MANAGER;
    /// <summary>
    /// the client ID is generated once per client, before network connection.
    /// Used to uniquely identify each client in lobbies. 
    /// </summary>
    private static Guid CLIENT_ID = Guid.NewGuid();
    private static FixedString64Bytes ClientIdString => new(CLIENT_ID.ToString());
    private static string ClientIdStringShort => CLIENT_ID.ToString().Substring(0, 4);
    

    [SerializeField] private List<PlayerData> playerDirectory;
    
    [Serializable]
    private struct PlayerData
    {
        public FixedString64Bytes clientId;
    }
    
    public NetworkVariable<GamePhase> gamePhase = new(GamePhase.ChoosingActions);
    
    public NetworkVariable<FixedString64Bytes> p0Id = new();
    public NetworkVariable<FixedString64Bytes> p1Id = new();

    public NetworkVariable<CombatAction> p0Action = new();
    public NetworkVariable<CombatAction> p1Action = new();
    public NetworkVariable<CombatWinner> lastWinner = new();
    
    public UnityEvent onMyActionChanged = new();
    public UnityEvent OnMyActionChanged => onMyActionChanged;
    public UnityEvent onOpponentActionChanged = new();
    public UnityEvent OnOpponentActionChanged => onOpponentActionChanged;
    
    [Tooltip("In Seconds")]
    public float countdownTime = 1f;
    [Tooltip("In Seconds")]
    public float handRevealTime = 1f;
    [Tooltip("In Seconds")]
    public float winRevealTime = 1f;

    private GameUIManager GameUIManager => SingletonLocator<GameUIManager>.Instance; 

    public MyWinState GetMyWinState()
    {
        
        if (gamePhase.Value != GamePhase.RevealWinner)
        {
            return MyWinState.None;
        }
        
        var iAmP0 = IsP0();
        var iAmP1 = IsP1();
        if(!iAmP0 && !iAmP1)
        { // I may not be participating in the game at all
            return MyWinState.None;
        }
        
        switch (lastWinner.Value)
        {
            case CombatWinner.Player0:
                return iAmP0 ? MyWinState.MyWin : MyWinState.MyLoss;
            case CombatWinner.Player1:
                return iAmP1 ? MyWinState.MyWin : MyWinState.MyLoss;
            
            case CombatWinner.Draw:
                return MyWinState.Draw;
            default:
                return MyWinState.None;
        }
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
        
        if (p0Id.Value == ClientIdString)
        {
            return p1Action.Value;
        }
        if (p1Id.Value == ClientIdString)
        {
            return p0Action.Value;
        }

        return null;
    }
    
    public CombatAction? GetMyAction()
    {
        if (IsP0()) return p0Action.Value;
        if (IsP1()) return p1Action.Value;

        return null;
    }
    
    private bool IsP0()
    {
        return p0Id.Value == ClientIdString;
    }
    private bool IsP1()
    {
        return p1Id.Value == ClientIdString;
    }
    
    public void Register()
    {
        var currentPlayerId = ClientIdString;
        Log.Info("REGISTERING " + currentPlayerId);
        RegisterRpc(currentPlayerId);
    }

    [Rpc(SendTo.Server)]
    private void RegisterRpc(FixedString64Bytes id)
    {
        Log.Info("REGISTERING " + id);

        PlayerData newData = new() { clientId = id };

        playerDirectory.Add(newData);

        TryAddToMatch(id);
    }

    public void PlayAction(CombatAction action)
    {
        var currentPlayerId = ClientIdString;
        
        Log.Info($"{currentPlayerId} is sending {action}");
        
        PlayActionServerRpc(action, currentPlayerId);
    }
    
    [Rpc(SendTo.Server)]
    private void PlayActionServerRpc(CombatAction action, FixedString64Bytes id)
    {
        Log.Info($"Received {action} from {id}");
        SetAction(id, action);
    }
    
    private bool TryAddToMatch(FixedString64Bytes id)
    {
        if (p0Id.Value.IsEmpty)
        {
            p0Id.Value = id;
            return true;
        }
        if (p1Id.Value.IsEmpty)
        {
            p1Id.Value = id;
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
        if(p0Id.Value == id)
        {
            p0Action.Value = action;
        }
        else if(p1Id.Value == id)
        {
            p1Action.Value = action;
        }
    }
    
    private void ForceResolveActions()
    {
        if (p0Action.Value == CombatAction.None || p1Action.Value == CombatAction.None)
        {
            Debug.LogError("ForceResolveActions called with missing actions!");
            return;
        }
        
        var action0 = p0Action.Value;
        var action1 = p1Action.Value;
        
        p0Action.Value = CombatAction.None;
        p1Action.Value = CombatAction.None;
        
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
        
        gamePhase.OnValueChanged += OnGamePhaseChanged;
        
        p0Action.OnValueChanged += OnP0ActionChanged;
        p1Action.OnValueChanged += OnP1ActionChanged;
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

    private void OnP0ActionChanged(CombatAction previousvalue, CombatAction newvalue)
    {
        if(IsP0()) onMyActionChanged.Invoke();
        if(IsP1()) onOpponentActionChanged.Invoke();
    }
    private void OnP1ActionChanged(CombatAction previousvalue, CombatAction newvalue)
    {
        if(IsP1()) onMyActionChanged.Invoke();
        if(IsP0()) onOpponentActionChanged.Invoke();
    }
    
    private void OnGamePhaseChanged(GamePhase prevValue, GamePhase newValue)
    {
        GameUIManager.OnGamePhaseChanged(newValue);
        if(newValue == GamePhase.CountingDown)
        {
            GameUIManager.BeginCountdown(countdownTime);
        }
    }

    private void InstanceOnOnConnectionBegin(ConnectionType conType)
    {
        if (conType == ConnectionType.Client) return;

        if (!IsServer)
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
            
            await UniTask.WaitUntil(() => p0Action.Value != CombatAction.None && p1Action.Value != CombatAction.None);
            
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
}
public static class GamePhaseExtensions{

    public static bool AllowsChangeAction(this GamePhase phase)
    {
        return phase is
            GamePhase.ChoosingActions or
            GamePhase.CountingDown;
    }
    
}