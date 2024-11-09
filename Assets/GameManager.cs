using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

//THIS IS THE LOCAL GAME MANAGER
public class GameManager : NetworkBehaviour
{
    public static GameManager GAME_MANAGER;
    static Guid CLIENT_ID = System.Guid.NewGuid();
    static string clientIdString;
    static string clientIdStringShort;

    public List<PlayerData> playerDirectory;

    public string[] playerIds = new string[2];

    public Winner lastWinner;

    public enum Action
    {
        None,
        Sword,
        Shield,
        Magic
    };

    public Action[] actions = new Action[2];

    void Awake()
    {
        ThereCanBeOnlyOne();

        playerDirectory = new List<PlayerData>();
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

    public static string GetClientIdShort()
    {
        clientIdStringShort ??= CLIENT_ID.ToString().Substring(0, 4);
        return clientIdString;
    }

    public static string GetClientId()
    {
        clientIdString ??= CLIENT_ID.ToString();
        return clientIdString;
    }

    [Rpc(SendTo.Server)]
    public void RegisterRpc(string id)
    {
        Debug.Log("REGISTERING " + id);

        PlayerData newData = new() { clientId = id };

        playerDirectory.Add(newData);

        if (string.IsNullOrEmpty(playerIds[0]))
        {
            playerIds[0] = id;
        }
        else if (string.IsNullOrEmpty(playerIds[1]))
        {
            playerIds[1] = id;
        }
    }

    [Rpc(SendTo.Server)]
    public void ShieldRpc(string id)
    {
        Debug.Log("Received shielding from " + id);
        SetAction(id, Action.Shield);
        TryResolveActions();
    }

    [Rpc(SendTo.Server)]
    public void MagicRpc(string id)
    {
        Debug.Log("Received magicking from " + id);
        SetAction(id, Action.Magic);
        TryResolveActions();
    }

    [Rpc(SendTo.Server)]
    public void SwordRpc(string id)
    {
        Debug.Log("Received swording from " + id);
        
        SetAction(id, Action.Sword);
        TryResolveActions();
    }
    
    

    int? IndexOfPlayer(string id)
    {
        var i = Array.IndexOf(playerIds, id);
        if (i == -1) return null;
        return i;
    }
    
    void SetAction(string id, Action action)
    {
        var i = IndexOfPlayer(id);
        if (i == null) return;
        actions[i.Value] = action;
    }
    
    private void TryResolveActions()
    {
        if (actions[0] == Action.None || actions[1] == Action.None) return;
        
        var action0 = actions[0];
        var action1 = actions[1];
        
        actions[0] = Action.None;
        actions[1] = Action.None;
        
        Debug.Log("Resolving actions: " + action0 + " vs " + action1);
        
        var winner = GetWinner(action0, action1);
        
        Debug.Log("Winner: " + winner);
        
        
    }

    public enum Winner
    {
        Player0,
        Player1,
        Draw
    }

    private Winner GetWinner(Action p1, Action p2)
    {
        return (p1, p2) switch
        {
            (Action.Sword, Action.Magic) => Winner.Player0,
            (Action.Shield, Action.Sword) => Winner.Player0,
            (Action.Magic, Action.Shield) => Winner.Player0,
            (Action.Magic, Action.Sword) => Winner.Player1,
            (Action.Sword, Action.Shield) => Winner.Player1,
            (Action.Shield, Action.Magic) => Winner.Player1,
            
            _ => Winner.Draw
        };
    }
}

[System.Serializable]
public struct PlayerData
{
    public string clientId;
}
