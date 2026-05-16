using UnityEngine;
using UnityEngine.Tilemaps;

public class DungeonManager : MonoBehaviour
{
    public int roomCount = 0;
    public int minSize = 0;
    public int maxSize = 0;

    public Tilemap floorTilemap;
    public Tilemap wallTilemap;

    public TileBase[] floorTiles;
    public TileBase[] wallTiles;

    public GameObject frontDoorPrefab;
    public GameObject leftDoorPrefab;
    public GameObject rightDoorPrefab;

    void Start()
    {
        Generate();
    }

    public void Generate()
    {
        if (floorTilemap == null || wallTilemap == null)
            return;

        floorTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();

        DungeonGenerator gen =
            new DungeonGenerator(
                roomCount,
                minSize,
                maxSize,
                floorTiles.Length
            );

        foreach (var entry in gen.floorTileData)
        {
            if (entry.Value < floorTiles.Length)
            {
                floorTilemap.SetTile(
                    (Vector3Int)entry.Key,
                    floorTiles[entry.Value]
                );
            }
        }

        foreach (var entry in gen.wallTileData)
        {
            if (entry.Value < wallTiles.Length)
            {
                wallTilemap.SetTile(
                    (Vector3Int)entry.Key,
                    wallTiles[entry.Value]
                );
            }
        }

        foreach (var door in gen.doorTileData)
        {
            Vector3Int doorCell =
                new Vector3Int(
                    door.position.x,
                    door.position.y,
                    0
                );

            // 문 위치의 벽 타일 제거
            wallTilemap.SetTile(doorCell, null);

            GameObject prefabToSpawn;

            if (door.direction == DoorDirection.Left)
            {
                prefabToSpawn = leftDoorPrefab;
            }
            else if (door.direction == DoorDirection.Right)
            {
                prefabToSpawn = rightDoorPrefab;
            }
            else
            {
                prefabToSpawn = frontDoorPrefab;
            }

            if (prefabToSpawn == null)
                continue;

            GameObject obj = Instantiate(
                prefabToSpawn,
                new Vector3(
                    door.position.x + 0.5f,
                    door.position.y + 0.5f,
                    0
                ),
                Quaternion.identity
            );

            Door doorScript = obj.GetComponent<Door>();

            if (doorScript != null)
            {
                doorScript.doorDirection = door.direction;
            }
        }
    }
}