using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager GAME_MANAGER;

    void Awake()
    {
        ThereCanBeOnlyOne();
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
    public void ShieldRpc(ushort networkBehaviourId, ulong networkObjectId)
    {
        Debug.Log("Received shielding from " + networkBehaviourId + ", " + networkObjectId);
    }

    [Rpc(SendTo.Server)]
    public void MagicRpc(ushort networkBehaviourId, ulong networkObjectId)
    {
        Debug.Log("Received magicking from " + networkBehaviourId + ", " + networkObjectId);
    }

    [Rpc(SendTo.Server)]
    public void SwordRpc(ushort networkBehaviourId, ulong networkObjectId)
    {
        Debug.Log("Received swording from " + networkBehaviourId + ", " + networkObjectId);
    }
}
