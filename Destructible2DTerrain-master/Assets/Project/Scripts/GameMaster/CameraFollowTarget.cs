using UnityEngine;
using Cinemachine;

public class CameraFollowTarget : MonoBehaviour
{
    public static CameraFollowTarget I { get; private set; }

    [Tooltip("Drag the scene’s Cinemachine virtual camera here")]
    public CinemachineVirtualCamera vcam;

    Transform player;

    void Awake() => I = this;

    public void SetPlayer(Transform p) { player = p; vcam.Follow = p; }

    public void Follow(Transform t)    => vcam.Follow = t;
    public void ResetToPlayer()        => vcam.Follow = player;

    public bool IsCurrent(Transform t) => vcam && vcam.Follow == t;
}