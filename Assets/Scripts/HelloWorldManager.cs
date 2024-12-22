using System;
using Unity.Netcode;
using UnityEngine;
using WebMatchShareHelper;

public class HelloWorldManager : MonoBehaviour
{
    private ConnectionManager m_ConnectionManager;
    
    private string m_clientJoinCode = string.Empty;

    void Awake()
    {
        m_ConnectionManager = GetComponent<ConnectionManager>();
    }

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));
        
        if (m_ConnectionManager.IsConnecting)
        {
            GUILayout.Label("Connecting...");
        }
        else if (m_ConnectionManager.IsConnected)
        {
            StatusLabels();
        }
        else
        {
            // not connected or connecting
            StartButtons();
            GUILayout.Label(m_ConnectionManager.ConnectionError);
        }
        
        
        GUILayout.EndArea();
    }

    void StartButtons()
    {
        var sharedCode = ShareHelper.GetShared();
        if (sharedCode != null)
        {
            // a code was shared with this user, automatically join.
            m_ConnectionManager.OnConnectAsClient(sharedCode).Forget();
            return;
        }
        
        if (GUILayout.Button("Host"))
        {
            m_ConnectionManager.OnConnectAsHost().Forget();
        }
        
        GUILayout.Label("Join code:");
        m_clientJoinCode = GUILayout.TextField(m_clientJoinCode);
        if (GUILayout.Button("Join"))
        {
            m_ConnectionManager.OnConnectAsClient(m_clientJoinCode).Forget();
        }
    }

    void StatusLabels()
    {
        var mode = m_ConnectionManager.IsHost ?
            "Host" : "Client";

        GUILayout.Label("Transport: " +
                        NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetType().Name);
        GUILayout.Label("Mode: " + mode);
        var joinCode = m_ConnectionManager.IsHost ? m_ConnectionManager.hostConnectJoinCode : this.m_clientJoinCode;  
        GUILayout.Label("Join code: " + joinCode);
        
        if (GUILayout.Button("Copy join"))
        {
            ShareHelper.Share(joinCode);
        }
    }
}

public static class AwaitableExtensions
{
    public static async void Forget(this Awaitable forgetMe)
    {
        try
        {
            await forgetMe;
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            throw;
        }
    }
    
    public static async void Forget<T>(this Awaitable<T> forgetMe)
    {
        try
        {
            await forgetMe;
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            throw;
        }
    }
}
