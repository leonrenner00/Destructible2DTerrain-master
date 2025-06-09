using UnityEngine;
using Cinemachine;

/// <summary> Keeps the Cinemachine virtual camera locked on whichever
/// soldier is active this turn.  Drop this on the GameManager. </summary>
[RequireComponent(typeof(TurnManager))]
public class CameraFollow : MonoBehaviour
{
    CinemachineVirtualCamera vcam;
    TurnManager gm;

    void Awake()
    {
        gm   = GetComponent<TurnManager>();
        vcam = FindObjectOfType<CinemachineVirtualCamera>();
        if (vcam == null)
            Debug.LogError("No CinemachineVirtualCamera found in scene!");
    }

    // GameManager should call this after it picks the new current soldier
    public void SnapTo(Transform target)
    {
        if (vcam) vcam.Follow = target;
    }

    void Start()
    {
        // lock on to whoever starts first
        SnapTo(gm.player[gm.current].transform);
    }
}