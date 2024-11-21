using System;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    readonly KeyCode rock = KeyCode.A;
    readonly KeyCode paper = KeyCode.S;
    readonly KeyCode scissors = KeyCode.D;
    
    void Start()
    {
        if (!IsOwner) return;
        GameManager.GAME_MANAGER.Register();
    }

    void Update()
    {
        if (!IsLocalPlayer)
            return;

        var playedActionMaybe = GetPlayedAction();
        if (playedActionMaybe is {} playedAction)
        {
            GameManager.GAME_MANAGER.PlayAction(playedAction);
        }
    }
    
    private CombatAction? GetPlayedAction()
    {
        if (Input.GetKeyDown(rock))
        {
            return CombatAction.Rock;
        }
        else if (Input.GetKeyDown(paper))
        {
            return CombatAction.Paper;
        }
        else if (Input.GetKeyDown(scissors))
        {
            return CombatAction.Scissors;
        }

        return null;
    }
}
