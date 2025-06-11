using UnityEngine;
using UnityEngine.UI;

/// Attach to the MoveBar Image in the Canvas.
public class MovementBar : MonoBehaviour
{
    public PlayerMovement player;      // set at runtime by TurnManager
    Image fill;

    void Awake() => fill = GetComponent<Image>();

    void Update()
    {
        if (!player || !fill) return;
        fill.fillAmount = player.RemainingMoveRatio;
    }
}

<<<<<<< Updated upstream
=======

>>>>>>> Stashed changes
