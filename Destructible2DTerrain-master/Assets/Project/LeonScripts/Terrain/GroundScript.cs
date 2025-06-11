/*
 * GroundDestructible
 * ------------------
 * • Attach to a SpriteRenderer that uses a readable ARGB32 texture.
 * • When a trigger tagged “Explosion” (with a CircleCollider2D) touches it
 *   the script punches a circular hole, updates the texture, then rebuilds
 *   a PolygonCollider2D so physics matches the new outline.
 *
 * Tested in Unity 2022.3 LTS
 */

using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[DisallowMultipleComponent]
public class GroundScript : MonoBehaviour
{
    [Header("Source texture (must be Read/Write, ARGB32, Clamp)")]
    public Texture2D baseTexture;

    [Header("Pixels-per-unit the sprite will use")]
    public float pixelsPerUnit = 50f;

    /* ───────── private ───────── */
    Texture2D runtimeTex;           // the clone we actually modify
    SpriteRenderer sr;

    int   texW, texH;               // pixel size
    float worldW, worldH;           // sprite size in world units

    /* ───────── initialise ───────── */
    void Awake()
    {
        sr       = GetComponent<SpriteRenderer>();
        runtimeTex = Instantiate(baseTexture);

        if (runtimeTex.format != TextureFormat.ARGB32)
            Debug.LogWarning($"{name}: source must be ARGB32 (is {runtimeTex.format})");
        if (runtimeTex.wrapMode != TextureWrapMode.Clamp)
            Debug.LogWarning($"{name}: wrap mode should be Clamp");

        ApplyTexture();             // sets sprite & initial collider
    }

    /* ───────── PUBLIC API: punch a hole ───────── */
    public void MakeHole(Vector2 worldPos, float radiusWorld)
    {
        Vector2Int c = WorldToPixel(worldPos);
        int r        = Mathf.RoundToInt(radiusWorld * texW / worldW);

        int sqrR = r * r;
        for (int y = -r; y <= r; y++)
        {
            int yy = y * y;
            int py = c.y + y; if (py < 0 || py >= texH) continue;

            for (int x = -r; x <= r; x++)
            {
                if (x * x + yy > sqrR) continue;
                int px = c.x + x; if (px < 0 || px >= texW) continue;

                runtimeTex.SetPixel(px, py, Color.clear);
            }
        }
        runtimeTex.Apply(false, false);
        ApplyTexture();             // refresh sprite + collider
    }

    /* ───────── helper: apply texture & rebuild collider ───────── */
    void ApplyTexture()
    {
        sr.sprite = Sprite.Create(runtimeTex,
                                  new Rect(0, 0, runtimeTex.width, runtimeTex.height),
                                  new Vector2(0.5f, 0.5f),
                                  pixelsPerUnit,
                                  0,
                                  SpriteMeshType.Tight);

        texW = runtimeTex.width;
        texH = runtimeTex.height;
        worldW = sr.bounds.size.x;
        worldH = sr.bounds.size.y;

        // rebuild collider
        Destroy(GetComponent<PolygonCollider2D>());
        gameObject.AddComponent<PolygonCollider2D>();
    }

    /* ───────── helper: map world → pixel coords ───────── */
    Vector2Int WorldToPixel(Vector2 pos)
    {
        var local = pos - (Vector2)transform.position;
        int px = Mathf.RoundToInt(texW * 0.5f + local.x * texW / worldW);
        int py = Mathf.RoundToInt(texH * 0.5f + local.y * texH / worldH);
        return new Vector2Int(px, py);
    }

    /* ───────── explode on trigger ───────── */
    void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.CompareTag("Explosion")) return;
        var circle = col as CircleCollider2D;
        if (!circle) return;

        MakeHole(circle.bounds.center, circle.bounds.extents.x);
        Destroy(col.gameObject, 0.05f);       // tidy the explosion FX
    }
}
