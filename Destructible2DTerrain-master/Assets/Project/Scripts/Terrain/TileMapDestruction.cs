using UnityEngine;
using UnityEngine.Tilemaps;

<<<<<<< Updated upstream
public class TileMapDestruction : MonoBehaviour
{
    public Tilemap tilemap;       // drag your Tilemap

    // called by your projectile
    public void PunchHole(Vector2 worldPos, float radius)
    {
        BoundsInt area = new BoundsInt(
            Mathf.FloorToInt((worldPos.x - radius) / tilemap.cellSize.x),
            Mathf.FloorToInt((worldPos.y - radius) / tilemap.cellSize.y),
            0,
            Mathf.CeilToInt( 2 * radius / tilemap.cellSize.x),
            Mathf.CeilToInt( 2 * radius / tilemap.cellSize.y),
            1);

        foreach (Vector3Int pos in area.allPositionsWithin)
        {
            Vector3 tileCenter = tilemap.GetCellCenterWorld(pos);
            if (Vector2.Distance(tileCenter, worldPos) <= radius)
                tilemap.SetTile(pos, null);           // pop the tile
        }
        tilemap.CompressBounds();                     // optional
=======
/// Add this to the Tilemap that should be destructible
[RequireComponent(typeof(Tilemap))]
public class TileMapDestruction : MonoBehaviour
{
    Tilemap tilemap;

    void Awake() => tilemap = GetComponent<Tilemap>();

    /// Call this when an explosion happens.
    /// worldPos  – centre of blast (world space)
    /// radiusW   – radius in world units
    public void PunchHole(Vector2 worldPos, float radiusW)
    {
        
        Vector3Int min = tilemap.WorldToCell(worldPos - Vector2.one * radiusW);
        Vector3Int max = tilemap.WorldToCell(worldPos + Vector2.one * radiusW);

        float rSq = radiusW *radiusW;

        for (int y = min.y; y <= max.y; y++)
        for (int x = min.x; x <= max.x; x++)
        {
            Vector3Int cell = new Vector3Int(x, y, 0);
            if (!tilemap.HasTile(cell)) continue;

                
            Vector3 centreW = tilemap.GetCellCenterWorld(cell);   // world-space
            if ((centreW - (Vector3)worldPos).sqrMagnitude <= rSq)
                tilemap.SetTile(cell, null);
        }

        tilemap.CompressBounds();         
>>>>>>> Stashed changes
    }
}