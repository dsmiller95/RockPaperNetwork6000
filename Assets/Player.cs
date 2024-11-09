using System;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    readonly KeyCode shield = KeyCode.A;
    readonly KeyCode magic = KeyCode.S;
    readonly KeyCode sword = KeyCode.D;

    public bool isOwner;

    void Awake() { }

    void Start()
    {
        if (!IsOwner)
            return;
        Debug.Log("Start from " + GameManager.GetClientIdShort());

        GameManager.GAME_MANAGER.RegisterRpc(GameManager.GetClientId());
    }

    void Update()
    {
        isOwner = IsOwner;

        if (!IsLocalPlayer)
            return;

        if (Input.GetKeyDown(shield))
        {
            Debug.Log(GameManager.GetClientIdShort() + " is sending shielding");
            GameManager.GAME_MANAGER.ShieldRpc(GameManager.GetClientId());
        }
        else if (Input.GetKeyDown(magic))
        {
            Debug.Log(GameManager.GetClientIdShort() + " is sending magicking");
            GameManager.GAME_MANAGER.MagicRpc(GameManager.GetClientId());
        }
        else if (Input.GetKeyDown(sword))
        {
            Debug.Log(GameManager.GetClientIdShort() + " is sending swording");
            GameManager.GAME_MANAGER.SwordRpc(GameManager.GetClientId());
        }
    }
}
