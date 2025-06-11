using UnityEngine;

/// Spawn one of these and parent it to the projectile.
/// Destroy it when the projectile ends. When the token
/// dies it snaps the camera back to the player.
public class CameraReturnToken : MonoBehaviour
{
    void OnDestroy()
    {
        if (CameraFollowTarget.I)
            CameraFollowTarget.I.ResetToPlayer();
    }
}