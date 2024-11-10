using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public enum CombatAction
{
    None,
    Sword,
    Shield,
    Magic
};

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
    
    
    public NetworkVariable<FixedString64Bytes> p1Id = new();
    public NetworkVariable<FixedString64Bytes> p2Id = new();
    
    public NetworkVariable<CombatAction> p1Action = new();
    public NetworkVariable<CombatAction> p2Action = new();
    public NetworkVariable<CombatWinner> lastWinner = new();
    
    public float lastActionChange = 0;
    [Tooltip("In Seconds")]
    public float timeSinceLastActionToResolve = 1f;

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

    private void SetAction(FixedString64Bytes id, CombatAction action)
    {
        if(p1Id.Value == id)
        {
            p1Action.Value = action;
            lastActionChange = Time.time;
        }
        else if(p2Id.Value == id)
        {
            p2Action.Value = action;
            lastActionChange = Time.time;
        }
    }

    private void TryResolveActions()
    {
        if(p1Action.Value == CombatAction.None || p2Action.Value == CombatAction.None) return;
        var timeSinceLastAction = Time.time - lastActionChange;
        if(timeSinceLastAction < timeSinceLastActionToResolve) return;
        
        
        var action0 = p1Action.Value;
        var action1 = p2Action.Value;
        
        p1Action.Value = CombatAction.None;
        p2Action.Value = CombatAction.None;
        
        Debug.Log("Resolving actions: " + action0 + " vs " + action1);
        
        lastWinner.Value = GetWinner(action0, action1);
        
        Debug.Log("Winner: " + lastWinner);
    }

    private CombatWinner GetWinner(CombatAction p1, CombatAction p2)
    {
        return (p1, p2) switch
        {
            (CombatAction.Sword, CombatAction.Magic) => CombatWinner.Player0,
            (CombatAction.Shield, CombatAction.Sword) => CombatWinner.Player0,
            (CombatAction.Magic, CombatAction.Shield) => CombatWinner.Player0,
            (CombatAction.Magic, CombatAction.Sword) => CombatWinner.Player1,
            (CombatAction.Sword, CombatAction.Shield) => CombatWinner.Player1,
            (CombatAction.Shield, CombatAction.Magic) => CombatWinner.Player1,
            
            _ => CombatWinner.Draw
        };
    }
    
    void Awake()
    {
        ThereCanBeOnlyOne();

        playerDirectory = new List<PlayerData>();
    }

    private void Update()
    {
        if (!this.IsServer) return;
        
        TryResolveActions();
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
        Debug.Log("REGISTERING " + id);

        PlayerData newData = new() { clientId = id };

        playerDirectory.Add(newData);

        TryAddToMatch(id);
    }

    [Rpc(SendTo.Server)]
    public void ShieldRpc(FixedString64Bytes id)
    {
        Debug.Log("Received shielding from " + id);
        SetAction(id, CombatAction.Shield);
    }

    [Rpc(SendTo.Server)]
    public void MagicRpc(FixedString64Bytes id)
    {
        Debug.Log("Received magicking from " + id);
        SetAction(id, CombatAction.Magic);
    }

    [Rpc(SendTo.Server)]
    public void SwordRpc(FixedString64Bytes id)
    {
        Debug.Log("Received swording from " + id);
        SetAction(id, CombatAction.Sword);
    }
}

[System.Serializable]
public struct PlayerData
{
    public FixedString64Bytes clientId;
}
