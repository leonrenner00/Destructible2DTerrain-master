using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MovementBar : MonoBehaviour
{
    public PlayerController player;      // set at runtime by TurnManager
    Image fill;

    void Awake() => fill = GetComponent<Image>();

    void Update()
    {
        if (!player || !fill) return;
        fill.fillAmount = player.RemainingMoveRatio;
    }
}
