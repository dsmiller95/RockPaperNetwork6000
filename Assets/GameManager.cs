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

    [Rpc(SendTo.Server)]
    public void MagicRpc(string id)
    {
        Debug.Log("Received magicking from " + id);
        SetAction(id, Action.Magic);
    }

    [Rpc(SendTo.Server)]
    public void SwordRpc(string id)
    {
        Debug.Log("Received swording from " + id);
        
        SetAction(id, Action.Sword);
    }
}

[System.Serializable]
public struct PlayerData
{
    public string clientId;
}
