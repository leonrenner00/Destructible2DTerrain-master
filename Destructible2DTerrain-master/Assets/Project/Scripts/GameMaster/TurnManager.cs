using System.Collections;
using UnityEngine;
using TMPro;                     // remove if you don’t use TextMeshPro

public class TurnManager : MonoBehaviour
{
    [Header("Players (exactly 2)")]
    public PlayerMovement[] player = new PlayerMovement[2];

    [Header("UI (optional)")]
    public TMP_Text turnBanner;               // drag a TMP Text, or leave null

    [Header("Timing")]
    public float turnDelay = 1f;              // pause before next turn starts

    /* ── ACCESSOR so other scripts can read whose turn it is ── */
    public int CurrentIndex => current;       // read-only to the outside

    int current = 0;                          // 0 → Player 1, 1 → Player 2
    
    public static TurnManager Instance { get; private set; }
    bool bannerShown;  
    void Awake() => Instance = this; 
    
    void Start()
    {
        if (player == null || player.Length != 2)
        {
            Debug.LogError("TurnManager: need exactly two PlayerMovement refs!");
            enabled = false;
            return;
        }

        foreach (var p in player) if (p) p.enabled = false;
        StartCoroutine(BeginTurnAfterDelay(0f));   // start Player 1 immediately
    }

    /* -------- called by PlayerMovement when it’s done -------- */
    public void EndTurn()
    {
        if (player[current]) player[current].enabled = false;
        current = 1 - current;                       // swap 0 ↔ 1
        StartCoroutine(BeginTurnAfterDelay(turnDelay));
    }

    /* -------- coroutine that actually starts a turn -------- */
    IEnumerator BeginTurnAfterDelay(float delay)
    {
        // show banner right away
        if (turnBanner)
        {
            turnBanner.text    = $"PLAYER {current + 1}'S TURN";
            turnBanner.enabled = true;
        }
        bannerShown = true;  
        yield return new WaitForSeconds(delay);

        // hide banner, enable player
        if (turnBanner) turnBanner.enabled = false;

        var p = player[current];
        if (p == null) { Debug.Log($"Player {1-current+1} wins!"); yield break; }

        p.enabled = true;
        p.BeginTurn();

        /* update MoveBar */
        var bar = FindObjectOfType<MovementBar>();
        if (bar) bar.player = p;

        /* update Cinemachine follow */
        var vcam = FindObjectOfType<Cinemachine.CinemachineVirtualCamera>();
        if (vcam) vcam.Follow = p.transform;
    }
    
    public void HideBannerIfCurrent(PlayerMovement caller)
    {
        if (!bannerShown) return;
        if (player[current] != caller) return;    // ignore other player

        if (turnBanner) turnBanner.enabled = false;
        bannerShown = false;
    }
    
    
}


