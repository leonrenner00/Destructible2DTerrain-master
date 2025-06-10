using UnityEngine;
using System.Collections.Generic;

/// Drop this on the terrain Sprite.
[RequireComponent(typeof(SpriteRenderer), typeof(PolygonCollider2D))]
public class TerrainController : MonoBehaviour
{
    [Header("Blast Settings")]
    public float pixelsPerUnit = 0;     // MUST match import PPU
    public float colliderDetail = 1f;      // 1 = default; >1 = fewer verts

    SpriteRenderer      sr;
    PolygonCollider2D   poly;
    Texture2D           runtimeTex;

    void Awake()
    {
        sr   = GetComponent<SpriteRenderer>();
        poly = GetComponent<PolygonCollider2D>();

        // Duplicate the texture so we don’t trash the asset
        Texture2D src = sr.sprite.texture;
        runtimeTex    = Instantiate(src);
        runtimeTex.Apply();

        // Re-create the sprite so it points at our runtime texture
        sr.sprite = Sprite.Create(
            runtimeTex,
            new Rect(0,0, runtimeTex.width, runtimeTex.height),
            new Vector2(0.5f,0.5f),
            pixelsPerUnit = GetComponent<SpriteRenderer>().sprite.pixelsPerUnit,
            0,
            SpriteMeshType.Tight);

        RebuildColliderFromSprite();
    }

    /* ───────────────── blast hole ───────────────── */
    public void DestroyCircle(Vector2 worldPos, float radiusWorld)
    {
        // Convert to texture space (origin = centre of sprite)
        Vector2 local = worldPos - (Vector2)transform.position;
        int cx = Mathf.RoundToInt(local.x * pixelsPerUnit + runtimeTex.width  * 0.5f);
        int cy = Mathf.RoundToInt(local.y * pixelsPerUnit + runtimeTex.height * 0.5f);
        int r  = Mathf.RoundToInt(radiusWorld * pixelsPerUnit);

        // Draw transparent circle
        for (int y = -r; y <= r; y++)
        {
            int py = cy + y; if (py < 0 || py >= runtimeTex.height) continue;
            int sqY = y*y;
            for (int x = -r; x <= r; x++)
            {
                if (x*x + sqY > r*r) continue;           // outside circle
                int px = cx + x; if (px < 0 || px >= runtimeTex.width) continue;
                runtimeTex.SetPixel(px, py, Color.clear); // alpha = 0
            }
        }
        runtimeTex.Apply(false, false);

        RebuildColliderFromSprite();
    }

    /* ───────────────── helper ───────────────── */
    void RebuildColliderFromSprite()
    {
        poly.pathCount = 0;                 // wipe old
        int shapeCount = sr.sprite.GetPhysicsShapeCount();

        List<Vector2> verts = new List<Vector2>();
        for (int i = 0; i < shapeCount; i++)
        {
            verts.Clear();
            sr.sprite.GetPhysicsShape(i, verts);

            // optional simplification
            if (colliderDetail > 1f)
                verts = Reduce(verts, colliderDetail);

            poly.pathCount++;
            poly.SetPath(poly.pathCount-1, verts);
        }
    }

    /* --- Ramer-Douglas-Peucker vertex reduction (very small & fast) --- */
    static List<Vector2> Reduce(List<Vector2> pts, float tolerance)
    {
        if (pts.Count < 3) return new List<Vector2>(pts);
        List<Vector2> result = new List<Vector2>();
        ReduceRecursive(pts, 0, pts.Count-1, tolerance*tolerance, result);
        result.Add(pts[pts.Count-1]);
        return result;
    }
    static void ReduceRecursive(List<Vector2> pts,int first,int last,float tolSq,List<Vector2> res)
    {
        float maxDistSq = 0f;
        int   index     = 0;
        Vector2 a = pts[first], b = pts[last];
        for (int i = first+1; i < last; i++)
        {
            float distSq = PerpDistSq(a,b,pts[i]);
            if (distSq > maxDistSq) { maxDistSq = distSq; index = i; }
        }
        if (maxDistSq > tolSq)
        {
            ReduceRecursive(pts, first, index, tolSq, res);
            ReduceRecursive(pts, index, last , tolSq, res);
        }
        else
            res.Add(a);
    }
    static float PerpDistSq(Vector2 a, Vector2 b, Vector2 p)
    {
        float l2 = (b-a).sqrMagnitude;
        return l2 == 0 ? (p-a).sqrMagnitude :
            Vector2.SqrMagnitude( p - (a + Vector2.Dot(p-a, b-a)/l2 * (b-a)) );
    }
}
