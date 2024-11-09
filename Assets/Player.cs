using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    UnityEngine.KeyCode shield = KeyCode.A;
    UnityEngine.KeyCode magic = KeyCode.S;
    UnityEngine.KeyCode sword = KeyCode.D;

    void Awake()
    {
        Debug.Log("Hi from " + NetworkBehaviourId);
    }

    void Update()
    {
        if (Input.GetKeyDown(shield))
        {
            Debug.Log(NetworkBehaviourId + " is sending shielding");
            GameManager.GAME_MANAGER.ShieldRpc(NetworkBehaviourId);
        }
        else if (Input.GetKeyDown(magic))
        {
            Debug.Log(NetworkBehaviourId + " is sending magicking");
            GameManager.GAME_MANAGER.MagicRpc(NetworkBehaviourId);
        }
        else if (Input.GetKeyDown(sword))
        {
            Debug.Log(NetworkBehaviourId + " is sending swording");
            GameManager.GAME_MANAGER.SwordRpc(NetworkBehaviourId);
        }
    }
}
