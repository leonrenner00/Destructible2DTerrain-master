using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionScript : MonoBehaviour
{
<<<<<<< Updated upstream:Destructible2DTerrain-master/Assets/Project/Scripts/Players/ExplosionScript.cs
    [SerializeField] float blastRadius = 1.5f;
    [SerializeField] GroundScript terrain;
    [SerializeField] TileMapDestruction terrain1;
    private void Start()
    {
        // 1. Damage terrain
       // terrain.MakeHole(transform.position, blastRadius);
        terrain1.PunchHole(transform.position, blastRadius);
=======
    [SerializeField] float blastRadius = 4f;
    [SerializeField] TileMapDestruction terrain;
    private void Start()
    {
        // 1. Damage terrain
        terrain.PunchHole(transform.position, blastRadius);

>>>>>>> Stashed changes:Destructible2DTerrain-master/Assets/Project/Scripts/ExplosionScript.cs
        // 2. Knock back physics bodies
        foreach (Collider2D col in Physics2D.OverlapCircleAll(transform.position, blastRadius))
        {
            if (col.attachedRigidbody)
            {
                Vector2 dir = (Vector2)(col.transform.position - transform.position).normalized;
                float force = Mathf.Lerp(8, 0, dir.magnitude / blastRadius);
                col.attachedRigidbody.AddForce(dir * force, ForceMode2D.Impulse);
            }
        }

        // 3. Destroy the FX object after one frame
        Destroy(gameObject);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected() =>
        Gizmos.DrawWireSphere(transform.position, blastRadius);
#endif
}
