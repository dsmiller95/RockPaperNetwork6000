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
        Debug.Log("Start from " + NetworkBehaviourId + "," + NetworkObjectId);
    }

    void Update()
    {
        isOwner = IsOwner;

        if (!IsLocalPlayer)
            return;

        if (Input.GetKeyDown(shield))
        {
            Debug.Log(NetworkBehaviourId + "," + NetworkObjectId + " is sending shielding");
            GameManager.GAME_MANAGER.ShieldRpc(NetworkBehaviourId, NetworkObjectId);
        }
        else if (Input.GetKeyDown(magic))
        {
            Debug.Log(NetworkBehaviourId + "," + NetworkObjectId + " is sending magicking");
            GameManager.GAME_MANAGER.MagicRpc(NetworkBehaviourId, NetworkObjectId);
        }
        else if (Input.GetKeyDown(sword))
        {
            Debug.Log(NetworkBehaviourId + "," + NetworkObjectId + " is sending swording");
            GameManager.GAME_MANAGER.SwordRpc(NetworkBehaviourId, NetworkObjectId);
        }
    }
}
