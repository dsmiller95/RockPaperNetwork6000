using System;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    readonly KeyCode shield = KeyCode.A;
    readonly KeyCode magic = KeyCode.S;
    readonly KeyCode sword = KeyCode.D;
    

    void Start()
    {
        if (!IsOwner)
            return;
        Debug.Log("Start from " + GameManager.ClientIdStringShort);

        GameManager.GAME_MANAGER.RegisterRpc(GameManager.ClientIdString);
    }

    void Update()
    {
        if (!IsLocalPlayer)
            return;

        if (Input.GetKeyDown(shield))
        {
            Debug.Log(GameManager.ClientIdStringShort + " is sending shielding");
            GameManager.GAME_MANAGER.ShieldRpc(GameManager.ClientIdString);
        }
        else if (Input.GetKeyDown(magic))
        {
            Debug.Log(GameManager.ClientIdStringShort + " is sending magicking");
            GameManager.GAME_MANAGER.MagicRpc(GameManager.ClientIdString);
        }
        else if (Input.GetKeyDown(sword))
        {
            Debug.Log(GameManager.ClientIdStringShort + " is sending swording");
            GameManager.GAME_MANAGER.SwordRpc(GameManager.ClientIdString);
        }
    }
}
