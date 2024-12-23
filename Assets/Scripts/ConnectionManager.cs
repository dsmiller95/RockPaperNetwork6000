﻿using System;
using Dman.Utilities;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public enum ConnectionType
{
    Host,
    Client
}

public interface IConnectionManager
{
    public event Action<ConnectionType> OnConnectionBegin;
}

[UnitySingleton]
public class ConnectionManager : MonoBehaviour, IConnectionManager
{
    string autoSelectRegionName = "auto-select (QoS)";
    
    public string hostConnectJoinCode = string.Empty; 
    
    public async Awaitable Start()
    {
        // Initialize Unity Services
        await UnityServices.InitializeAsync();
    }


    [field: SerializeField]
    public string ConnectionError { get; private set; }
    
    private ConnectionState connectionState = ConnectionState.Disconnected;

    public bool IsConnecting => connectionState == ConnectionState.Connecting ||
                                UnityServices.State == ServicesInitializationState.Initializing;
    public bool IsConnected => connectionState is ConnectionState.ConnectedHost or ConnectionState.ConnectedClient;
    public bool IsDisconnected => connectionState == ConnectionState.Disconnected;
    
    public event Action<ConnectionType> OnConnectionBegin;
    
    public bool IsHost
    {
        get
        {
            if (connectionState != ConnectionState.ConnectedHost) return false;
            if (!NetworkManager.Singleton.IsHost)
            {
                Debug.LogError("Connection state is connected host, but network manager is not host");
                return false;
            }

            return true;
        }
    }

    public enum ConnectionState
    {
        Disconnected,
        Connecting,
        ConnectedHost,
        ConnectedClient,
    }
    
    private IDisposable DisconnectIfStillConnecting()
    {
        return new DisposableAbuse.LambdaDispose(() =>
        {
            if (connectionState == ConnectionState.Connecting)
            {
                connectionState = ConnectionState.Disconnected;
                Debug.LogWarning("Connection could not complete. returning to disconnected state.");
            }
        });
    }
    
    /// <summary>
    /// connect as a host. returns the join code.
    /// </summary>
    /// <returns></returns>
    public async Awaitable<string> OnConnectAsHost()
    {
        if (!IsDisconnected) throw new InvalidOperationException("already connected or connecting");
        connectionState = ConnectionState.Connecting;
        hostConnectJoinCode = string.Empty;
        using var _1 = DisconnectIfStillConnecting();
        
        var playerId = await SignIn();
        
        Debug.Log($"Signed in. Player ID: {playerId}");
        
        
        
        Debug.Log("Host - Creating an allocation. Upon success, I have 10 seconds to BIND to the Relay server that I've allocated.");

        // Determine region to use (user-selected or auto-select/QoS)
        string region = GetRegionOrQosDefault();
        Debug.Log($"The chosen region is: {region ?? autoSelectRegionName}");

        // Set max connections. Can be up to 100, but note the more players connected, the higher the bandwidth/latency impact.
        int maxConnections = 4;

        // Important: Once the allocation is created, you have ten seconds to BIND, else the allocation times out.
        var hostAllocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);//region);
        Debug.Log($"Host Allocation ID: {hostAllocation.AllocationId}, region: {hostAllocation.Region}");
        Debug.Log("Host - Getting a join code for my allocation. I would share that join code with the other players so they can join my session.");
        var joinCode = await GetJoinCode(hostAllocation);
        

        // Extract the Relay server data from the Allocation response.
        var relayServerData = new RelayServerData(hostAllocation, "wss");
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

        NetworkManager.Singleton.StartHost();

        Debug.Log($"Host - Started host successfully, setting to connected host. join code is {joinCode}");
        connectionState = ConnectionState.ConnectedHost;
        hostConnectJoinCode = joinCode;
        OnConnectionBegin?.Invoke(ConnectionType.Host);
        
        return joinCode;
    }

    public async Awaitable OnConnectAsClient(string joinCode)
    {
        if (!IsDisconnected) throw new InvalidOperationException("already connected or connecting");

        if (string.IsNullOrEmpty(joinCode))
        {
            this.ConnectionError = "Join code is empty";
            return;
        }
        
        connectionState = ConnectionState.Connecting;
        using var _1 = DisconnectIfStillConnecting();
        
        var playerId = await SignIn();
        
        Debug.Log($"Signed in. Player ID: {playerId}");

        JoinAllocation playerAllocation = null; 
        try
        {
            playerAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
        }
        catch(RelayServiceException relayException)
        {
            if (relayException.Reason == RelayExceptionReason.InvalidRequest)
            {
                ConnectionError = "Invalid join code";
            }
            else
            {
                ConnectionError = "Failed to join room: " + relayException.Reason.ToString();
            }
            
            Debug.LogError(relayException.Message, this);

            return;
        }
        Debug.Log("Player - Joined Allocation ID: " + playerAllocation.AllocationId);

        // Configure Unity Transport with the Relay server data
        var relayServerData = new RelayServerData(playerAllocation, "wss");
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

        // Start the client
        NetworkManager.Singleton.StartClient();
        
        
        connectionState = ConnectionState.ConnectedClient;
        OnConnectionBegin?.Invoke(ConnectionType.Client);
    }

    
    private async Awaitable<string> SignIn()
    {
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        return AuthenticationService.Instance.PlayerId;
    }
    
    
    string GetRegionOrQosDefault()
    {
        return autoSelectRegionName;
    }
    
    
    /// <summary>
    /// Event handler for when the Get Join Code button is clicked.
    /// </summary>
    private async Awaitable<string> GetJoinCode(Allocation hostAllocation)
    {
        try
        {
            var joinCode = await RelayService.Instance.GetJoinCodeAsync(hostAllocation.AllocationId);
            Debug.Log("Host - Got join code: " + joinCode);
            return joinCode;
        }
        catch (RelayServiceException ex)
        {
            Debug.LogError(ex.Message + "\n" + ex.StackTrace);
            throw;
        }
    }
}