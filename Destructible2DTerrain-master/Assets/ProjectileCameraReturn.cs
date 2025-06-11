using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileCameraReturn : MonoBehaviour
{
    void OnDisable()            => TryReset();   // covers SetActive(false)
    void OnDestroy()            => TryReset();   // normal destroy
    void OnBecameInvisible()    => Destroy(gameObject, 0.1f);  // off-screen cleanup

    void TryReset()
    {
        if (CameraFollowTarget.I &&
            CameraFollowTarget.I.IsCurrent(transform))
        {
            CameraFollowTarget.I.ResetToPlayer();
        }
    }
}
