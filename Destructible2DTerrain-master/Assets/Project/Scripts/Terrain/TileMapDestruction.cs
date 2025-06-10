using UnityEngine;
using UnityEngine.Tilemaps;

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
    }
}