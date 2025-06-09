using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class TurnManager : MonoBehaviour
{
    [FormerlySerializedAs("soldiers")] public PlayerController[] player;      // length must be 2
    public int current = 0;                  // 0 = Player 1, 1 = Player 2

    void Start()
    {
        if (player == null || player.Length != 2)
        {
            Debug.LogError("GameManager expects exactly two soldiers in the array!");
            enabled = false;
            return;
        }

        StartTurn();  // kick off the first turn
    }

    public void StartTurn()
    {
        int other = 1 - current;              // flip 0 ↔ 1

        // Handle win condition first in case the previous turn killed someone
        if (player[other] == null)
        {
            Debug.Log($"Player {current + 1} wins!");
            // TODO: load win screen, show UI, etc.
            return;
        }

        // Disable the non-active soldier (guard against null in case of late destruction)
        if (player[other] != null)
            player[other].enabled = false;

        // Enable and initialize the active soldier
        if (player[current] != null)
        {
            player[current].enabled = true;
            player[current].BeginTurn();
        }
        

        // OPTIONAL: notify UI / listeners
        // OnTurnStarted?.Invoke(current);
        Debug.Log($"--- Player {current + 1}'s turn ---"); var mainCam = Camera.main;
        if (!mainCam)
        {
            Debug.LogError("No MainCamera tagged camera in scene!");
            return;
        }

        var vcam = FindObjectOfType<Cinemachine.CinemachineVirtualCamera>();
        if (!vcam)
        {
            Debug.LogError("No CinemachineVirtualCamera found.");
            return;
        }

        if (!mainCam.TryGetComponent(out Cinemachine.CinemachineBrain brain))
            brain = mainCam.gameObject.AddComponent<Cinemachine.CinemachineBrain>();

        if (player[current] == null)
        {
            Debug.LogError("Current soldier slot is empty!");
            return;
        }
        
        vcam.Follow = player[current].transform;
        
    }
    

    public void EndTurn()
    {
        current = 1 - current;   // swap players
        StartTurn();
    }
}
